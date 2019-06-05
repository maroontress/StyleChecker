# NotDesignedForExtension

## Summary

A class must be designed for inheritance, or else be prohibited from it.

## Description

This is something like the C# version of
[_DesignForExtension check_][design-for-extension-checkstyle]
of Checkstyle\[[1](#ref1)\].
This rule reports diagnostic information of classes that have
a non-empty overridable method or property as follows:

- The `virtual` method which is `public` or `protected`
and implementation of which is not empty
- The `virtual` property which is `public` or `protected`

This rule prevents
[_the fragile base class problem_][fragile-base-class] \[[2](#ref2)\].
And it also prevents [_Call Super_][call-super] \[[2](#ref2)\],
_neutralizing_ a call to the overridden method
of the base class, that is, invoking `base`._Method_(...) in C#.

## Code fix

The code fix is not provided. Refactor the `virtual` method to be
empty or `abstract` one, for example, using the
[_Template Method Pattern_][template-method-pattern]
\[[3](#ref3)\].
And then, change the `override` method (this is, the method
overriding the method of the base class) to be `sealed`, or change
the class to be `sealed`.

## Example

### Diagnostic

```csharp
public class BaseClass
{
    // A virtual method must be empty or be changed to be abstract.
    public virtual void Method()
    {
        DoSomething();
    }
}

public class DerivedClass
{
    // An overriding method must be sealed or empty.
    public override void Method()
    {
        PerformAnotherAction();
    }
}
```

## See also

- [_DesignForExtension_][design-for-extension-checkstyle] \[[1](#ref1)\]

  > Rationale: This library design style protects superclasses against being
  > broken by subclasses. The downside is that subclasses are limited in their
  > flexibility, in particular they cannot prevent execution of code in the
  > superclass, but that also means that subclasses cannot corrupt the state
  > of the superclass by forgetting to call the superclass's method.

- [_Fragile base class_][fragile-base-class] \[[2](#ref2)\]

  > The **fragile base class problem** is a fundamental architectural problem
  > of object-oriented programming systems where base classes (superclasses)
  > are considered “fragile” because seemingly safe modifications to a base
  > class, when inherited by the derived classes, may cause the derived
  > classes to malfunction. The programmer cannot determine whether a base
  > class change is safe simply by examining in isolation the methods of the
  > base class.

- [_Call super_][call-super] \[[2](#ref2)\]

  > **Call super** is a code smell or anti-pattern of some object-oriented
  > programming languages. Call super is a design pattern in which a
  > particular class stipulates that in a derived subclass, the user is
  > required to override a method and call back the overridden function
  > itself at a particular point. The overridden method may be intentionally
  > incomplete, and reliant on the overriding method to augment its
  > functionality in a prescribed manner. However, the fact that the
  > language itself may not be able to enforce all conditions prescribed on
  > this call is what makes this an anti-pattern.

## References

<a id="ref1"></a>
[1] [Checkstyle][checkstyle]

<a id="ref2"></a>
[2] [Wikipedia][wikipedia]

<a id="ref3"></a>
[3] [Gamma, E. and Helm, R. and Johnson, R. and Vlissides, J.
_Design Patterns: Elements of Reusable Object-Oriented Software_.
Reading, Mass: Addison-Wesley, 1994.][book-design-patterns]

[call-super]:
  https://en.wikipedia.org/wiki/Call_super
[fragile-base-class]:
  https://en.wikipedia.org/wiki/Fragile_base_class
[template-method-pattern]:
  https://en.wikipedia.org/wiki/Template_method_pattern
[design-for-extension-checkstyle]:
  http://checkstyle.sourceforge.net/config_design.html#DesignForExtension
[wikipedia]:
  https://en.wikipedia.org/wiki/
[checkstyle]:
  http://checkstyle.sourceforge.net/
[book-design-patterns]:
  https://books.google.com/books/about/Design_Patterns.html?id=6oHuKQe3TjQC
