# ByteOrderMark

## Summary

Remove a Byte Order Mark (BOM) of the file encoded in UTF-8.

## Description

This rule reports diagnostic information of the following files starting with
a UTF-8 BOM.

- C# source files that the project contains (except auto-generated files)
- files specified with the configuration file `StyleChecker.xml`

You can specify the files to check with the configuration file
`StyleChecker.xml`. For example, if you would like to check files whose name
matches `*.Designer.cs` in the any directory of the project, edit
`StyleChecker.xml` file as follows:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<config xmlns="https://maroontress.com/StyleChecker/config.v1">
  ⋮
  <ByteOrderMark>
    <files glob="**/*.Designer.cs" />
  </ByteOrderMark>
  ⋮
</config>
```

The `ByteOrderMark` element can have `files` elements zero or more times
as its child elements, and the *pattern*, which matches the names of the files
to be checked, can be specified to the `glob` attribute's value of the `files`
element.

The path separator in the pattern must be a slash ('`/`') character
regardless of the platform. The directory names `.` and `..` are not
interpreted specially (that is, `.` and `..` do not mean the current and
parent directory, respectively). So, for example, the pattern `foo/../bar/baz.cs`
does not match `bar/baz.cs`. The pattern matching is performed
with the relative path to the project root, so if the pattern starts with
a slash, no files match it.

The pattern can contain an asterisk ('`*`') character as a wildcard,
which matches any character other than a slash zero or more times.
It can also contain a double asterisk ('`**`'), which represents as follows:

- if the pattern equals `**`, it matches all files in the project root
  directory and subdirectories.
- if the pattern ends with `/**` (a slash followed by a double asterisk),
  the subpattern `/**` matches all files in the directory and subdirectories.
- if the pattern starts with `**/` (a double asterisk followed by a slash),
  the subpattern `**/` matches the project root directory and subdirectories.
  For example, `**/foo` matches `foo`, `bar/foo` and `bar/baz/foo`.
- if the pattern contains `/**/`, the subpattern `/**/` matches a slash,
  the directories and subdirectories. For example, `foo/**/bar` matches
  `foo/bar`, `foo/baz/bar` and `foo/baz/qux/bar`.

## Code fix

The code fix is not provided.
