<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# LongLine

<div class="horizontal-scroll">

![LongLine][fig-LongLine]

</div>

## Summary

Avoid a long line.

## Default severity

Warning

## Description

In default, a line must be less than 80 columns, but the length can
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

[fig-LongLine]:
  https://maroontress.github.io/StyleChecker/images/LongLine.png
