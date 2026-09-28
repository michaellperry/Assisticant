using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Assisticant.SourceGenerators
{
    /// <summary>
    /// Helpers shared by <see cref="ObservableGenerator"/> (Model layer - independent
    /// variables, backed by Observable&lt;T&gt;) and <see cref="ComputedGenerator"/>
    /// (ViewModel layer - dependent variables derived from an injected Model, backed
    /// by Computed&lt;T&gt;). See the Model/ViewModel separation note on each generator
    /// for why these are two separate attributes rather than one.
    /// </summary>
    internal static class GeneratorSupport
    {
        /// <summary>
        /// Full name of the class-level attribute that opts a ViewModel into an
        /// INotifyPropertyChanged implementation. Defined and emitted by
        /// <see cref="NotifyPropertyChangedGenerator"/>; referenced by name (not by
        /// type reference) from <see cref="ObservableGenerator"/> and
        /// <see cref="ComputedGenerator"/> so all three stay independent generators
        /// that only agree on this string.
        /// </summary>
        public const string NotifyPropertyChangedAttributeFullName = "Assisticant.Fields.NotifyPropertyChangedAttribute";

        public readonly struct TypeFrame
        {
            public readonly string Keyword;
            public readonly string Name;

            public TypeFrame(string keyword, string name)
            {
                Keyword = keyword;
                Name = name;
            }
        }

        /// <summary>
        /// Walks the target's enclosing types, returning the chain (outermost first,
        /// including the target itself last if <paramref name="targetNode"/> is itself
        /// a type declaration - i.e. for a class-level attribute) if every one of them
        /// is declared 'partial', or null (with <paramref name="error"/> set) at the
        /// first one that isn't.
        /// </summary>
        public static List<TypeFrame>? GetPartialTypeChainOrNull(
            SyntaxNode targetNode, string targetName, Location location,
            DiagnosticDescriptor notPartialDescriptor, out Diagnostic? error)
        {
            var typeDecls = new List<TypeDeclarationSyntax>();
            if (targetNode is TypeDeclarationSyntax selfType)
                typeDecls.Add(selfType);
            for (var current = targetNode.Parent; current != null; current = current.Parent)
            {
                if (current is TypeDeclarationSyntax typeDecl)
                    typeDecls.Add(typeDecl);
            }
            typeDecls.Reverse(); // outermost first; the target's own declaration (if any) ends up last

            foreach (var typeDecl in typeDecls)
            {
                if (!typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    error = Diagnostic.Create(notPartialDescriptor, location, typeDecl.Identifier.Text, targetName);
                    return null;
                }
            }

            error = null;
            return typeDecls
                .Select(td => new TypeFrame(td.Keyword.Text, td.Identifier.Text + (td.TypeParameterList?.ToString() ?? "")))
                .ToList();
        }

        public static string GetNamespace(INamedTypeSymbol containingType)
        {
            var containingNamespace = containingType.ContainingNamespace;
            return containingNamespace is { IsGlobalNamespace: false }
                ? containingNamespace.ToDisplayString()
                : "";
        }

        public static System.Text.StringBuilder Indent(System.Text.StringBuilder sb, int level) => sb.Append(' ', level * 4);

        public static string SanitizeForFileName(string name) =>
            new string(name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
    }
}
