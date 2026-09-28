using Assisticant.Fields;

namespace Assisticant.SourceGenerators.Demo
{
    // MODEL: independent variables only. [Observable] properties hold the actual
    // mutable state - nothing here is derived from anything else.
    //
    // No constructor: Person has no immutable fields to establish, so it needs
    // none. Mutable, [Observable]-backed properties are never initialized through
    // a constructor parameter - set initial state with an object initializer at
    // the call site instead: new Person { Name = "Alice", Age = 30 }.
    public partial class Person
    {
        [Observable] public partial string Name { get; set; }
        [Observable] public partial int Age { get; set; }
    }
}
