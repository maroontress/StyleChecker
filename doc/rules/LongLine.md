# LongLine

## Summary

Avoid a long line.

## Description

In default a line must be less than 80 columns, but the length can
be configured with `StyleChecker.xml`.

For example, if you would like a line to be less than 100 columns,
edit `StyleChecker.xml` file as follows:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<config xmlns="https://maroontress.com/StyleChecker/config.v1">
  ⋮
  <LongLine maxLineLength="100"/>
  ⋮
</config>
```

## Code fix

The code fix is not provided.
