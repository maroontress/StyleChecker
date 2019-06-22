# AssignmentToParameter

![AssignmentToParameter][fig-AssignmentToParameter]

## Summary

Assignment to parameters must be avoided.

## Default severity

Warning

## Description

This rule reports diagnostic information of assignment to the parameters
passed _by value_ (except _by reference_). Note that
\[[1](#ref1)\]:

> Do not confuse the concept of passing _by reference_ with the concept of
> reference types. The two concepts are not the same. A method parameter can be
> modified by `ref` regardless of whether it is a value type or a reference
> type. There is no boxing of a value type when it is passed _by reference_.

Those who are unfamiliar with C# often assign a new value to the parameters
passed _by value_, with the intention that the assignment will be reflected in
the caller's variables. This rule allows them to immediately notice that
they confused or misled themselves.

The diagnostic for a parameter passed _by value_ is reported when the method
does as follows:

- Assign a new value to it
- Increment or decrement it
- Pass it to any method as the `ref` or `out` parameter.

## Code fix

The code fix is not provided.

## Example

### Diagnostic

```csharp
public int Method(int length)
{
    if (length < 0)
    {
        length = 0;
    }
    length += 1;

    ++length;
    --length;

    OtherMethod(ref length);
    AnotherMethod(out length);
    ⋮
```

## See also

- [_Remove Assignments To Parameters_][remove-assignments-to-parameters]
  \[[2](#ref2)\]

- [_ParameterAssignment_][parameter-assignment-checkstyle] \[[3](#ref3)\]

  > Disallows assignment of parameters.
  >
  > Rationale: Parameter assignment is often considered poor programming
  > practice. Forcing developers to declare parameters as final is often
  > onerous. Having a check ensure that parameters are never assigned would
  > give the best of both worlds.

- [_FinalParameters_][final-parameters-checkstyle] \[[3](#ref3)\]

  > Check that parameters for methods, constructors, and catch blocks are
  > final. Interface, abstract, and native methods are not checked: the final
  > keyword does not make sense for interface, abstract, and native method
  > parameters as there is no code that could modify the parameter.
  >
  > Rationale: Changing the value of parameters during the execution of the
  > method's algorithm can be confusing and should be avoided. A great way to
  > let the Java compiler prevent this coding style is to declare parameters
  > final.

## References

<a id="ref1"></a>
[1] [Microsoft, _ref keyword (C# Reference)_][ref-keyword-microsoft]

<a id="ref2"></a>
[2] [Fowler, Martin, et al. _Refactoring: improving the design of existing
code._ Addison-Wesley Professional, 1999.][book-refactoring]

<a id="ref3"></a>
[3] [Checkstyle][checkstyle]

[final-Parameters-checkstyle]:
  http://checkstyle.sourceforge.net/config_misc.html#FinalParameters
[parameter-assignment-checkstyle]:
  http://checkstyle.sourceforge.net/config_coding.html#ParameterAssignment
[remove-assignments-to-parameters]:
  https://refactoring.com/catalog/removeAssignmentsToParameters.html
[ref-keyword-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/ref
[book-refactoring]:
  https://books.google.com/books?hl=en&lr=&id=UTgFCAAAQBAJ&oi=fnd&pg=PR7&dq=related:vnwrAmPEMgzFtM:scholar.google.com/&ots=WhUS8DZwaj&sig=VA7mXR3Ug6dn1uhQStZTVKYfSUw
[checkstyle]:
  http://checkstyle.sourceforge.net/
[fig-AssignmentToParameter]:
  https://maroontress.github.io/StyleChecker/images/AssignmentToParameter.png
