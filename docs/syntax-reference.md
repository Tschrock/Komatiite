Komatiite Lava Syntax Reference
===============================

## Number Literals

#### Integers
```
1
-1
```

#### Decimals
```
3.14
```

## String Literals

#### Double Quoted
```
"Hello World"
```

#### Single Quoted
```
'Hello World'
```

#### Multi-line Strings
```
"Line 1
Line 2"
```

#### Escaped Quotes*
```
'Sasha\'s World'
"The \"Best\" Night Ever"
```

#### Special Characters*
```
"Line 1\nLine 2"
```

#### Interpolated Strings (Only in Shortcodes)
```
'I like {{ food }}'
```

## Comparisons

#### Equals
```
thing1 == thing2
```

#### Not Equal
```
thing1 != thing2
thing1 <> thing2
```

#### Less Than
```
thing1 < thing2
```

#### Greater Than
```
thing1 > thing2
```

#### Less Than or Equal
```
thing1 <= thing2
```

#### Greater Than or Equal
```
thing1 >= thing2
```

## Conditions

#### Contains
```
thing1 contains thing2
```

#### Starts With
```
thing1 startswith thing2
```

#### Ends With
```
thing1 endswith thing2
```

#### Has Key
```
thing1 haskey thing2
```

#### Has Value
```
thing1 hasvalue thing2
```

## Object Navigation

#### Properties
```
thing.property
thing["property"]
```

#### Array Indexes
```
things[0]
```

#### Dictionary Keys
```
things["dirt"]
```

## Special Properties

#### First Item
```
things.first
```

#### Last Item
```
things.last
```

#### Item Count
```
things.size
```

## Filters

#### Basic
```
{{ thing | Filter }}
```

#### Chaining
```
{{ thing | Filter1 | Filter2 }}
```

#### Parameters
```
{{ thing | Filter1:"thing",3,value }}
```

## Tags

#### Basic
```
{% dothing %}
```

#### Block
```
{% dothing %}
	content
{% enddothing %}
```

#### Parameters
```
{% dothing test:"boop" count:4 with:somevalue %}
```

## Shortcodes

#### Basic
```
{[ dothing ]}
```

#### Block
```
{[ dothing ]}
	content
{[ enddothing ]}
```

#### Single Quoted String Parameters
```
{[ dothing test:'boop' ]}
```

#### Interpolated String Parameters
```
{[ dothing test:'I like {{ thing }}' ]}
```

#### Other Parameters*
```
{[ dothing test:"boop" count:4 with:somevalue ]}
```


## Other

#### Whitespace Trimming
```
Hello {{ person -}}    !
Super        {{- thing }}
```

#### Variable Inception
```
{{ [variableName] }}
```

#### Shorthand Comments**
```
{# This is a comment #}
```

#### Shorthand Raw Text**
```
{{{ This is raw text where "{{" and "}}" are meaningless. }}}
```

\* These are extensions to the original Lava syntax. They are not compatible with original Lava and are exclusive to Komatiite.

\*\* These are technically available in original Lava but are either hidden, disabled, or broken in it's default implementation. That means they are not compatible with the original Lava engine and will only work with Komatiite.

