# InvalidConfig

## Summary

Validate the configuration file `StyleChecker.xml`.

## Description

This is not a rule. The InvalidConfig analyzer validates `StyleChecker.xml`
and reports parse errors.

> Configuration file "StyleChecker.xml" is not valid

The configuration file is not a well-formed XML document.

> The namespace of the root element is not unnamed

The namespace of the root element `config` must be unnamed.

> The root element is not config element

The root element must be `config`.

> The value of maxLineLength attribute is invalid

The `maxLineLength` attribute must has a positive integer value.

## Code fix

The code fix is not provided.
