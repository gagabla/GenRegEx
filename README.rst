
GenRegEx
===========================================================

GenRegEx is a generic regular expression matching
engine implemented in C#. It can work on sequences
of any kind of tokens, you only have to provide a
helper object, that can decide whether two tokens
are equal or not.

Once a pattern has been created, matching is very
fast, in some cases one million times as fast as
the native regular expressions in C#.


Features
-----------------------------------------------------------

The GenRegEx engine does only implement a small subset
of the regular expression functionallity you might be
used to from other string-only-implementations like
`PCRE`_. The following features are available:

- Matching single tokens of any kind
- Tie up a sequence of tokens to a group
- Repetition of tokens or groups (zero or more (*), one or
  more (+))
- Optional tokens or groups (can occur once (?))
- Greedy/not greedy repetition
- Match from start (^), match till end ($)
- Building patterns by code
- Parsing patterns from string, rendering patterns as
  string
  

Background
-----------------------------------------------------------

This implementation is heavily inspired by the article
`Regular Expression Matching\: the Virtual Machine
Approach`_ from Russ Cox. The pattern is compiled into
a program (a sequence of instructions) that are then beeing
processed by a virtual processor simulating multiple
threads for the different possibilies to match the pattern.


Todos
-----------------------------------------------------------

- Supply a neat interface (using IEnumerator)  
- Collect group-matching information and deliver it in the
  resulting match
- Support explicit repetitions ({N}, {,N}, {N,} and {X,Y})
- Support alternatives (A | B)
- Support token classes ([A B])




.. _PCRE: http://www.pcre.org/
.. _Regular Expression Matching\: the Virtual Machine Approach: http://swtch.com/~rsc/regexp/regexp2.html