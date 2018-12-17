# LongLine

## Summary

Avoid a long line.

## Description

In default a line must be less than 80 columns, but the length can
be configured with `StyleChecker.xml`.

For example, if you would like a line to be less than 100 columns,
edit `StyleChecker.xml` file as follows:

```xml
<config>
  ...
  <LongLine maxLineLength="100"/>
  ...
</config>
```

## Code fix

The code fix is not provided.
