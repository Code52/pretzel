# In development


# 0.2.0

## Breaking changes
- [#189](https://github.com/Code52/pretzel/pull/189): Handle bool in yaml
In a YAML frontmatter like the following:
``` text
-------
active: true
-------
``` 
Recognize that the value of "active" is a boolean and convert it in the datas for DotLiquid so that it is possible to use it in a template like that:
```cs
{% if page.active == true %}
```
Like in Jekyll.
 
But this will cause a breaking change if anyone have a
```cs
{% if page.active == 'true' %}
```
in a template.
 
## Features
- [#181](https://github.com/Code52/pretzel/pull/181): Added new Jekyll filters and tags by [switchspan](https://github.com/switchspan)
    - DateToLongStringFilter
    - DateToStringFilter
    - NumberOfWordsFilter
    - CgiEscapeFilter
    - UriEscapeFilter
    - CommentBlock
    - PostUrlBlock
 
- [#185](https://github.com/Code52/pretzel/pull/185): Allow all *.md, *.mkd, *.mkdn, *.mdown and *.markdown files to be processed
 
- Add a cleantarget argument which deletes the target directory (_site), like Jekyll does by default
- Improve less compilation
- Add include and exclude configuration features, like in [Jekyll](http://jekyllrb.com/docs/configuration/#global-configuration)
- [#189](https://github.com/Code52/pretzel/pull/189): Add support for category in page permalinks by [dkarzon](https://github.com/dkarzon)
- [#186](https://github.com/Code52/pretzel/pull/186): site.title can be valorized in _config.yaml and page.ig is generated for every posts and pages

## Fixes
- Fix issue where transforms aren't processed during taste => fix the .less not compiled during taste for example
- [#194](https://github.com/Code52/pretzel/pull/194): Fix issue where the style wasn't used for WebSequenceDiagram: now with "@@sequence mscgen", the style will be "mscgen"