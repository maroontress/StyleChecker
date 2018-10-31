# AssignmentToParameter

## Summary

Assignment to parameters must be avoided.

## Description

This rule reports diagnostic information of assignment to the parameters
passed _by value_ (except _by reference_). Note that [1]:

> Do not confuse the concept of passing _by reference_ with the concept of
> reference types. The two concepts are not the same. A method parameter can be
> modified by `ref` regardless of whether it is a value type or a reference
> type. There is no boxing of a value type when it is passed _by reference_.

Those who are unfamiliar with C# often assign a new value to the parameters
passed _by value_, with the intention that the assignment will be reflected in
the caller's variables. This rule allows them to immediately notice that
themselves are confused or misled.

## Code fix

The code fix is not provided.

## Example

### Diagnostic

```csharp
public void Method(int value)
{
    value = 0;
}
```

## See also

- [_Remove Assignments To Parameters_](https://refactoring.com/catalog/removeAssignmentsToParameters.html) [2]

- [_ParameterAssignment_](http://checkstyle.sourceforge.net/config_coding.html#ParameterAssignment) [3]

  > Disallows assignment of parameters.
  >
  > Rationale: Parameter assignment is often considered poor programming
  > practice. Forcing developers to declare parameters as final is often
  > onerous. Having a check ensure that parameters are never assigned would
  > give the best of both worlds.

- [_FinalParameters_](http://checkstyle.sourceforge.net/config_misc.html#FinalParameters) [3]

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

[1] [Microsoft, _ref keyword (C# Reference)_](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/ref)

[2] [Fowler, Martin, et al. _Refactoring: improving the design of existingcode._ Addison-Wesley Professional, 1999.](https://books.google.com/books?hl=en&lr=&id=UTgFCAAAQBAJ&oi=fnd&pg=PR7&dq=related:vnwrAmPEMgzFtM:scholar.google.com/&ots=WhUS8DZwaj&sig=VA7mXR3Ug6dn1uhQStZTVKYfSUw)

[3] [CheckStyle](http://checkstyle.sourceforge.net/)