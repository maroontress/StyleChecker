# InvalidConfig

## Summary

Validate the configuration file `StyleChecker.xml`.

## Description

This is not a rule. The InvalidConfig analyzer validates `StyleChecker.xml`
and reports the errors. Some typical errors are described below.

### &sect; Not a well-formed XML document

> Unexpected end of file has occurred. The following elements are not closed:
> _element_. Line _n_, position _m_.

The _element_ is not closed.

### &sect; Not valid for the Schema

> unexpected node type: Element of the element '_unexpected_'
> (it is expected that the element '_expected_' starts)

The root element is not `config`, or the specified XML namespace is not
`"https://maroontress.com/StyleChecker/config.v1"`.

> unexpected node type: Element of the element '_unexpected_'
> (it is expected that the element '_expected_' ends)

The _unexpected_ element occurred.

### &sect; LongLine element

> invalid integer value of maxLineLength attribute: '...'

The `maxLineLength` attribute of the `LongLine` element does not have an
integer value, or `int` cannot represent the value.

> non-positive integer value of maxLineLength attribute: '...'

The `maxLineLength` attribute of the `LongLine` element has
zero or a negative integer value.

## Code fix

The code fix is not provided.
