using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Assisticant.SourceGenerators.GeneratorSupport;

namespace Assisticant.SourceGenerators
{
    /// <summary>
    /// ViewModel layer. Turns an <c>[Computed]</c>-decorated method into a read-only
    /// property - backed by <c>Computed&lt;T&gt;</c> for a scalar return type, or by a
    /// live <c>ObservableCollection&lt;T&gt;</c> for an <c>IEnumerable&lt;T&gt;</c>
    /// return type (see the "Collection mode" section below) - Assisticant's dependent
    /// variables.
    ///
    /// <b>Model/ViewModel separation:</b> by default a ViewModel holds no
    /// <c>Observable&lt;T&gt;</c> of its own (see <see cref="ObservableGenerator"/>'s
    /// doc comment) - it takes one or more Models through its constructor, the same
    /// way a View takes a ViewModel through its DataContext/<c>ForView.Wrap</c> - and
    /// derives everything it exposes from them. This is a strong default, not a hard
    /// rule: a ViewModel can also carry its own [Observable] state for things no Model
    /// would ever care about (e.g. "is this row selected"), and a [Computed] property
    /// can just as well depend on one of those as on an injected Model's property -
    /// dependency tracking doesn't care which class or layer an Observable/Computed
    /// lives in; that's purely a convention this generator doesn't enforce.
    ///
    ///     public partial class PersonViewModel
    ///     {
    ///         private readonly Person _person; // injected Model
    ///         public PersonViewModel(Person person) =&gt; _person = person;
    ///
    ///         [Computed] private string ComputeGreeting() =&gt; $"Hello, {_person.Name}!";
    ///     }
    ///
    /// becomes:
    ///
    ///     partial class PersonViewModel
    ///     {
    ///         private readonly Computed&lt;string&gt; __greetingField = new Computed&lt;string&gt;(ComputeGreeting);
    ///         public string Greeting =&gt; __greetingField.Value;
    ///     }
    ///
    /// The method must take no parameters (it becomes the Func&lt;T&gt; passed to
    /// Computed&lt;T&gt;'s constructor, or the source enumerable a collection-mode
    /// property re-synchronizes from). Because a property can't share its name with
    /// the method that computes it, the property name is derived by stripping a
    /// leading "Compute" or "Get" prefix (or given explicitly via
    /// [Computed("PropertyName")]).
    ///
    /// Add <c>[NotifyPropertyChanged]</c> to the class (see
    /// <see cref="NotifyPropertyChangedGenerator"/>) to have each scalar [Computed]
    /// property also raise <c>PropertyChanged</c> (asynchronously, via
    /// <c>UpdateScheduler</c>) whenever its backing <c>Computed&lt;T&gt;</c>
    /// invalidates. Collection-mode properties don't participate in this - see below.
    ///
    /// <b>Collection mode.</b> When the method returns <c>IEnumerable&lt;T&gt;</c> (or
    /// any type implementing it for exactly one T - excluding <c>string</c>, which
    /// technically implements <c>IEnumerable&lt;char&gt;</c>, mirroring the exclusion
    /// the reflection-based <c>Metas/MemberMeta.cs</c> already makes), the generated
    /// property is instead a live, strongly-typed
    /// <c>System.Collections.ObjectModel.ObservableCollection&lt;T&gt;</c> that's
    /// synchronized - not replaced - on every recompute:
    ///
    ///     [Computed] private IEnumerable&lt;PersonViewModel&gt; ComputePeople()
    ///         =&gt; _people.Select(p =&gt; new PersonViewModel(p));
    ///
    /// becomes (see <see cref="GeneratorSupport"/> for CollectionSynchronizer&lt;T&gt;):
    ///
    ///     private ObservableCollection&lt;PersonViewModel&gt;? __peopleField;
    ///     private Computed? __peopleSentryField;
    ///     public ObservableCollection&lt;PersonViewModel&gt; People { get { ...; return __peopleField; } }
    ///
    /// The property's identity never changes across recomputes - only its contents do,
    /// via minimal Add/Remove/Move mutations that raise real
    /// <c>INotifyCollectionChanged</c> events, with per-item identity preserved by a
    /// <c>RecycleBin&lt;T&gt;</c> exactly the way <c>Collections/ComputedList.cs</c>
    /// already preserves it (so e.g. an item's own [Observable] "IsSelected" survives
    /// a recompute).
    ///
    /// This targets a real <c>ObservableCollection&lt;T&gt;</c> - not a custom
    /// <c>INotifyCollectionChanged</c> implementation, and not <c>ComputedList&lt;T&gt;</c>
    /// itself, which implements neither - because .NET MAUI's <c>CollectionView</c> is
    /// confirmed (independent of anything in this repo) to not reliably subscribe to
    /// <c>INotifyCollectionChanged</c> on an arbitrary custom collection class; only
    /// on <c>ObservableCollection&lt;T&gt;</c> itself
    /// (https://github.com/dotnet/maui/issues/29284, open as of this writing). The
    /// diff algorithm mirrors Assisticant's own WPF proxy
    /// (<c>Metas/ListSlot.cs</c>/<c>Metas/CollectionItem.cs</c>), generalized from an
    /// untyped <c>ObservableCollection&lt;object&gt;</c> to a strongly-typed one.
    ///
    /// This is a prototype - see the caveats on <see cref="ObservableGenerator"/>,
    /// which apply here too.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class ComputedGenerator : IIncrementalGenerator
    {
        private const string ComputedAttributeFullName = "Assisticant.Fields.ComputedAttribute";

        private const string AttributeSource = @"// <auto-generated/>
#nullable enable

namespace Assisticant.Fields
{
    /// <summary>
    /// Generates a read-only property - backed by <see cref=""Computed{T}""/> for a
    /// scalar return type, or by a live, incrementally-synchronized
    /// <see cref=""System.Collections.ObjectModel.ObservableCollection{T}""/> for an
    /// <c>IEnumerable&lt;T&gt;</c> return type. Belongs on ViewModel classes only,
    /// deriving from an injected Model; see [Observable] for Models.
    ///
    /// Apply it to a parameterless method:
    ///
    ///     [Computed] private string ComputeGreeting() =&gt; $""Hello, {_person.Name}!"";
    ///     [Computed] private IEnumerable&lt;PersonViewModel&gt; ComputePeople() =&gt; _people.Select(p =&gt; new PersonViewModel(p));
    ///
    /// generates a property named by stripping the method's leading ""Compute""/""Get""
    /// prefix (here, ""Greeting""/""People""), or by the name given to the attribute:
    /// [Computed(""PropertyName"")]. The containing type (and every enclosing type, if
    /// nested) must be declared 'partial'.
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class ComputedAttribute : global::System.Attribute
    {
        public ComputedAttribute() { }

        public ComputedAttribute(string propertyName) => PropertyName = propertyName;

        public string? PropertyName { get; }
    }

    /// <summary>
    /// Applies a newly computed sequence to a live
    /// <see cref=""System.Collections.ObjectModel.ObservableCollection{T}""/> with
    /// minimal Add/Remove/Move mutations instead of a full Clear+rebuild, preserving
    /// per-item identity via <see cref=""Assisticant.RecycleBin{T}""/> so a recycled
    /// item's own state (e.g. an [Observable] ""IsSelected"") survives across
    /// recomputes. Generated collection-mode [Computed] code calls this instead of
    /// hand-rolling the diff; the algorithm mirrors Assisticant's own WPF proxy
    /// (Metas/ListSlot.cs, Metas/CollectionItem.cs), generalized from an untyped
    /// ObservableCollection&lt;object&gt; to a strongly-typed one. Not intended to be
    /// called directly.
    /// </summary>
    public static class CollectionSynchronizer<T>
    {
        private sealed class Slot : global::System.IDisposable
        {
            private readonly global::System.Collections.ObjectModel.ObservableCollection<T> _collection;
            private readonly T _item;
            private bool _inCollection;

            public Slot(global::System.Collections.ObjectModel.ObservableCollection<T> collection, T item, bool inCollection)
            {
                _collection = collection;
                _item = item;
                _inCollection = inCollection;
            }

            public void Dispose()
            {
                if (_inCollection)
                    _collection.Remove(_item);
            }

            public void EnsureInCollection(int index)
            {
                if (!_inCollection)
                {
                    _collection.Insert(index, _item);
                    _inCollection = true;
                }
                else if (!global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(_collection[index], _item))
                {
                    _collection.Remove(_item);
                    _collection.Insert(index, _item);
                }
            }

            public override int GetHashCode() => _item == null ? 0 : _item.GetHashCode();

            public override bool Equals(object? obj) =>
                obj is Slot other && global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(_item, other._item);
        }

        public static void Synchronize(
            global::System.Collections.ObjectModel.ObservableCollection<T> collection,
            global::System.Collections.Generic.IEnumerable<T>? newItems)
        {
            var slots = new global::System.Collections.Generic.List<Slot>();

            // Dump the collection's current contents into a recycle bin, keyed by
            // item identity (Equals/GetHashCode), then extract - in the NEW order -
            // either the matching old Slot (preserving position-tracking state) or a
            // brand new one for each incoming item. Anything left in the bin at the
            // end of the `using` block is disposed, which removes it from the
            // collection - this is the same trick RecycleBin's own doc comment
            // describes: ""it disposes old objects that are no longer in use"".
            using (var bin = new global::Assisticant.RecycleBin<Slot>())
            {
                foreach (var oldItem in collection)
                    bin.AddObject(new Slot(collection, oldItem, inCollection: true));

                if (newItems != null)
                    foreach (var item in newItems)
                        slots.Add(bin.Extract(new Slot(collection, item, inCollection: false)));
            }

            // Now that membership is settled, fix up ordering: insert new items and
            // move misplaced ones into their correct position, left to right.
            int index = 0;
            foreach (var slot in slots)
            {
                slot.EnsureInCollection(index);
                ++index;
            }
        }
    }
}
";

        private static readonly DiagnosticDescriptor TypeNotPartialDiagnostic = new DiagnosticDescriptor(
            id: "ASSISTICANT101",
            title: "Containing type must be partial",
            messageFormat: "Type '{0}' must be declared 'partial' to use [Computed] on method '{1}'",
            category: "Assisticant.SourceGenerators",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MethodHasParametersDiagnostic = new DiagnosticDescriptor(
            id: "ASSISTICANT102",
            title: "Method must have no parameters",
            messageFormat: "Method '{0}' is decorated with [Computed] but declares parameters; it becomes a Func<T> with no arguments",
            category: "Assisticant.SourceGenerators",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MethodReturnsVoidDiagnostic = new DiagnosticDescriptor(
            id: "ASSISTICANT103",
            title: "Method must return a value",
            messageFormat: "Method '{0}' is decorated with [Computed] but returns void; it must return the computed value",
            category: "Assisticant.SourceGenerators",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor CannotDeriveNameDiagnostic = new DiagnosticDescriptor(
            id: "ASSISTICANT104",
            title: "Cannot derive a property name",
            messageFormat: "Cannot derive a property name for method '{0}'; rename it with a 'Compute' or 'Get' prefix (e.g. 'Compute{0}'), or specify one explicitly with [Computed(\"PropertyName\")]",
            category: "Assisticant.SourceGenerators",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly string[] StrippablePrefixes = { "Compute", "Get" };

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
                ctx.AddSource("ComputedAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

            IncrementalValuesProvider<MethodResult> results = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    ComputedAttributeFullName,
                    predicate: static (node, _) => node is MethodDeclarationSyntax,
                    transform: static (ctx, ct) => Analyze(ctx, ct));

            context.RegisterSourceOutput(results.Collect(), static (spc, all) => Execute(spc, all));
        }

        private static MethodResult Analyze(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
        {
            var method = (IMethodSymbol)ctx.TargetSymbol;
            var location = ctx.TargetNode.GetLocation();

            var typeChain = GetPartialTypeChainOrNull(ctx.TargetNode, method.Name, location, TypeNotPartialDiagnostic, out var typeError);
            if (typeChain == null)
                return MethodResult.Error(typeError!);

            if (method.Parameters.Length > 0)
                return MethodResult.Error(Diagnostic.Create(MethodHasParametersDiagnostic, location, method.Name));

            if (method.ReturnsVoid)
                return MethodResult.Error(Diagnostic.Create(MethodReturnsVoidDiagnostic, location, method.Name));

            string? candidateName = GetExplicitPropertyName(method) ?? DerivePropertyName(method.Name);
            if (string.IsNullOrEmpty(candidateName) || candidateName == method.Name)
                return MethodResult.Error(Diagnostic.Create(CannotDeriveNameDiagnostic, location, method.Name));

            string propertyName = candidateName!;
            var backingFieldName = "__" + char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1) + "Field";
            var ns = GetNamespace(method.ContainingType);

            var collectionElementType = GetEnumerableElementType(method.ReturnType);
            if (collectionElementType != null)
            {
                var elementTypeDisplay = collectionElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return MethodResult.CollectionSuccess(ns, typeChain, method.Name, propertyName, elementTypeDisplay, backingFieldName);
            }

            var scalarTypeDisplay = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var notifiesPropertyChanged = method.ContainingType.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == NotifyPropertyChangedAttributeFullName);

            return MethodResult.ScalarSuccess(ns, typeChain, method.Name, propertyName, scalarTypeDisplay, backingFieldName, notifiesPropertyChanged);
        }

        /// <summary>
        /// Returns T if <paramref name="returnType"/> is (or implements, for exactly
        /// one T) <c>IEnumerable&lt;T&gt;</c> - excluding <c>string</c>, which
        /// technically implements <c>IEnumerable&lt;char&gt;</c> but is a scalar for
        /// this generator's purposes, mirroring <c>Metas/MemberMeta.cs</c>'s existing
        /// `MemberType != typeof(string)` exclusion. Returns null for a scalar type.
        /// </summary>
        private static ITypeSymbol? GetEnumerableElementType(ITypeSymbol returnType)
        {
            if (returnType.SpecialType == SpecialType.System_String)
                return null;

            if (returnType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } self)
                return self.TypeArguments[0];

            foreach (var iface in returnType.AllInterfaces)
            {
                if (iface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                    return iface.TypeArguments[0];
            }

            return null;
        }

        private static string? GetExplicitPropertyName(IMethodSymbol method)
        {
            var attribute = method.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == ComputedAttributeFullName);
            if (attribute is { ConstructorArguments.Length: > 0 } &&
                attribute.ConstructorArguments[0].Value is string explicitName &&
                !string.IsNullOrWhiteSpace(explicitName))
            {
                return explicitName;
            }
            return null;
        }

        private static string? DerivePropertyName(string methodName)
        {
            foreach (var prefix in StrippablePrefixes)
            {
                if (methodName.Length > prefix.Length && methodName.StartsWith(prefix, StringComparison.Ordinal))
                    return methodName.Substring(prefix.Length);
            }
            return null;
        }

        private static void Execute(SourceProductionContext spc, ImmutableArray<MethodResult> all)
        {
            foreach (var result in all)
            {
                if (result.Diagnostic is { } diagnostic)
                    spc.ReportDiagnostic(diagnostic);
            }

            var successes = all.Where(r => r.Diagnostic == null).ToList();

            var groups = successes.GroupBy(r => r.Namespace + "|" + string.Join("+", r.TypeChain!.Select(t => t.Name)));

            foreach (var group in groups)
            {
                var first = group.First();
                var source = RenderType(first.Namespace, first.TypeChain!, group.ToList());
                var hintName = string.Join(".", first.TypeChain!.Select(t => SanitizeForFileName(t.Name))) + ".Computed.g.cs";
                spc.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
            }
        }

        private static string RenderType(string ns, List<TypeFrame> typeChain, List<MethodResult> members)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// Generated by Assisticant.SourceGenerators.ComputedGenerator from [Computed] methods.");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            bool hasNamespace = !string.IsNullOrEmpty(ns);
            int indent = 0;

            if (hasNamespace)
            {
                sb.Append("namespace ").Append(ns).AppendLine();
                sb.AppendLine("{");
                indent = 1;
            }

            foreach (var frame in typeChain)
            {
                Indent(sb, indent).Append("partial ").Append(frame.Keyword).Append(' ').Append(frame.Name).AppendLine();
                Indent(sb, indent).AppendLine("{");
                indent++;
            }

            // The PropertyChanged event, the interface's presence on the class, and the
            // __RaisePropertyChanged helper called below are all owned by
            // NotifyPropertyChangedGenerator (see its doc comment for why this generator
            // doesn't declare them itself even when `notifies` is true here).
            foreach (var member in members)
            {
                if (member.IsCollection)
                    RenderCollectionMember(sb, indent, member);
                else
                    RenderScalarMember(sb, indent, member);
                sb.AppendLine();
            }

            for (int i = typeChain.Count - 1; i >= 0; i--)
            {
                indent--;
                Indent(sb, indent).AppendLine("}");
            }

            if (hasNamespace)
                sb.AppendLine("}");

            return sb.ToString();
        }

        private static void RenderScalarMember(StringBuilder sb, int indent, MethodResult member)
        {
            // A field initializer can't reference an instance method (CS0236 -
            // "this" isn't available yet), so the Computed<T> is constructed
            // lazily, on first access, instead of via a field initializer or a
            // generated constructor (which would need to run no matter which of
            // the user's own constructors was called).
            Indent(sb, indent)
                .Append("private global::Assisticant.Fields.Computed<").Append(member.ElementType).Append(">? ")
                .Append(member.BackingFieldName).AppendLine(";");

            if (member.NotifiesPropertyChanged)
            {
                // Raising PropertyChanged is deferred through UpdateScheduler rather
                // than invoked directly from the Invalidated handler, even though
                // this event carries no value to read: Observable<T>.Value's setter
                // raises invalidation (which cascades into this Computed<T>'s
                // Invalidated) BEFORE storing the new value, and some UI dispatchers
                // (documented on UpdateScheduler.Initialize) run queued work inline
                // when already on the UI thread. Deferring - the same way
                // ComputedSubscription and the WPF proxy's MemberSlot already do -
                // guarantees the eventual re-read sees the new value regardless of
                // what the host binding infrastructure's own dispatch does.
                Indent(sb, indent)
                    .Append("public ").Append(member.ElementType).Append(' ').Append(member.PropertyName)
                    .Append(" => (").Append(member.BackingFieldName)
                    .Append(" ??= __Create").Append(member.PropertyName).AppendLine("Field()).Value;");
                Indent(sb, indent)
                    .Append("private global::Assisticant.Fields.Computed<").Append(member.ElementType).Append("> __Create")
                    .Append(member.PropertyName).AppendLine("Field()");
                Indent(sb, indent).AppendLine("{");
                Indent(sb, indent + 1)
                    .Append("var computed = new global::Assisticant.Fields.Computed<").Append(member.ElementType).Append(">(")
                    .Append(member.MethodName).AppendLine(");");
                Indent(sb, indent + 1)
                    .Append("computed.Invalidated += () => global::Assisticant.UpdateScheduler.ScheduleUpdate(() => __RaisePropertyChanged(\"")
                    .Append(member.PropertyName).AppendLine("\"));");
                Indent(sb, indent + 1).AppendLine("return computed;");
                Indent(sb, indent).AppendLine("}");
            }
            else
            {
                Indent(sb, indent)
                    .Append("public ").Append(member.ElementType).Append(' ').Append(member.PropertyName)
                    .Append(" => (").Append(member.BackingFieldName)
                    .Append(" ??= new global::Assisticant.Fields.Computed<").Append(member.ElementType).Append(">(")
                    .Append(member.MethodName).Append(")).Value;").AppendLine();
            }
        }

        private static void RenderCollectionMember(StringBuilder sb, int indent, MethodResult member)
        {
            var collectionType = "global::System.Collections.ObjectModel.ObservableCollection<" + member.ElementType + ">";
            var sentryFieldName = member.BackingFieldName.Substring(0, member.BackingFieldName.Length - "Field".Length) + "SentryField";

            // The ObservableCollection<T> instance itself is never replaced across
            // recomputes - only its contents are, via CollectionSynchronizer<T> - so a
            // binding that reads this property once (e.g. MAUI's CollectionView.ItemsSource)
            // keeps seeing live updates without ever needing PropertyChanged for this
            // property itself. See this file's doc comment for why the target is a real
            // ObservableCollection<T> rather than a custom INotifyCollectionChanged type.
            Indent(sb, indent).Append("private ").Append(collectionType).Append("? ").Append(member.BackingFieldName).AppendLine(";");
            Indent(sb, indent).AppendLine("private global::Assisticant.Computed? " + sentryFieldName + ";");
            Indent(sb, indent).Append("public ").Append(collectionType).Append(' ').Append(member.PropertyName).AppendLine();
            Indent(sb, indent).AppendLine("{");
            Indent(sb, indent + 1).AppendLine("get");
            Indent(sb, indent + 1).AppendLine("{");
            Indent(sb, indent + 2).Append("if (").Append(member.BackingFieldName).AppendLine(" == null)");
            Indent(sb, indent + 2).AppendLine("{");
            Indent(sb, indent + 3).Append(member.BackingFieldName).Append(" = new ").Append(collectionType).AppendLine("();");
            Indent(sb, indent + 3).Append(sentryFieldName).Append(" = new global::Assisticant.Computed(() => global::Assisticant.Fields.CollectionSynchronizer<")
                .Append(member.ElementType).Append(">.Synchronize(").Append(member.BackingFieldName).Append(", ").Append(member.MethodName).AppendLine("()));");
            // The dispatcher registered with UpdateScheduler.Initialize is exactly what
            // MAUI needs here too: CollectionView throws if ItemsSource is mutated off
            // the UI thread, and this defers every Add/Remove/Move CollectionSynchronizer<T>
            // performs through that same dispatcher - no separate mechanism required.
            Indent(sb, indent + 3).Append(sentryFieldName)
                .AppendLine(".Invalidated += () => global::Assisticant.UpdateScheduler.ScheduleUpdate(() => " + sentryFieldName + "!.OnGet());");
            Indent(sb, indent + 2).AppendLine("}");
            Indent(sb, indent + 2).Append(sentryFieldName).AppendLine("!.OnGet();");
            Indent(sb, indent + 2).Append("return ").Append(member.BackingFieldName).AppendLine(";");
            Indent(sb, indent + 1).AppendLine("}");
            Indent(sb, indent).AppendLine("}");
        }

        private sealed class MethodResult
        {
            public string Namespace = "";
            public List<TypeFrame>? TypeChain;
            public string MethodName = "";
            public string PropertyName = "";
            public string ElementType = "";
            public string BackingFieldName = "";
            public bool NotifiesPropertyChanged;
            public bool IsCollection;
            public Diagnostic? Diagnostic;

            public static MethodResult ScalarSuccess(
                string ns, List<TypeFrame> typeChain, string methodName, string propertyName, string elementType,
                string backingFieldName, bool notifiesPropertyChanged) =>
                new MethodResult
                {
                    Namespace = ns,
                    TypeChain = typeChain,
                    MethodName = methodName,
                    PropertyName = propertyName,
                    ElementType = elementType,
                    BackingFieldName = backingFieldName,
                    NotifiesPropertyChanged = notifiesPropertyChanged,
                    IsCollection = false,
                };

            public static MethodResult CollectionSuccess(
                string ns, List<TypeFrame> typeChain, string methodName, string propertyName, string elementType,
                string backingFieldName) =>
                new MethodResult
                {
                    Namespace = ns,
                    TypeChain = typeChain,
                    MethodName = methodName,
                    PropertyName = propertyName,
                    ElementType = elementType,
                    BackingFieldName = backingFieldName,
                    IsCollection = true,
                };

            public static MethodResult Error(Diagnostic diagnostic) =>
                new MethodResult { Diagnostic = diagnostic };
        }
    }
}
