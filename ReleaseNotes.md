# vNext

 - [#218](https://github.com/Code52/pretzel/issues/218) - Add --safe parameter to disable custom plugins +Enhancement
 - [#178](https://github.com/Code52/pretzel/pull/178) - Create command fixes contributed by Gábor Gergely ([kodfodrasz](https://github.com/kodfodrasz)) +fix
 - [#165](https://github.com/Code52/pretzel/pull/165) - Make category inherit Drop (so it can be used in templates) contributed by Jordan Wallwork ([jordanwallwork](https://github.com/jordanwallwork)) +Enhancement
 - [#216](https://github.com/Code52/pretzel/pull/216) - Support to other default pages contributed by Wesley Alcoforado ([wesleyalcoforado](https://github.com/wesleyalcoforado)) +Enhancement
 - [#213](https://github.com/Code52/pretzel/pull/213) - Adds .htm extension for layouts contributed by Wesley Alcoforado ([wesleyalcoforado](https://github.com/wesleyalcoforado)) +Enhancement
 - [#212](https://github.com/Code52/pretzel/issues/212) - If a post has invalid categories, it won't render contributed by ([vikingcode](https://github.com/vikingcode)) +fix
 - [#206](https://github.com/Code52/pretzel/pull/206) - Code block support contributed by Andrey Akinshin ([AndreyAkinshin](https://github.com/AndreyAkinshin)) +Feature
 - [#157](https://github.com/Code52/pretzel/pull/157) - Add support to load dlls in "_plugins" path contributed by Miguel Román ([miguelerm](https://github.com/miguelerm)) +Enhancement
 - [#203](https://github.com/Code52/pretzel/pull/203) - Support for 404 pages contributed by Damian Karzon ([dkarzon](https://github.com/dkarzon)) +Enhancement
 - [#202](https://github.com/Code52/pretzel/pull/202) - Add Excerpt feature for pages contributed by Andrey Akinshin ([AndreyAkinshin](https://github.com/AndreyAkinshin)) +Enhancement
 - [#201](https://github.com/Code52/pretzel/issues/201) - Custom metadatas in pages are not available from site.pages in liquid +fix
 - [#200](https://github.com/Code52/pretzel/pull/200) - Single category support for liquid posts contributed by Andrey Akinshin ([AndreyAkinshin](https://github.com/AndreyAkinshin)) +Enhancement
 - [#199](https://github.com/Code52/pretzel/issues/199) - PostUrl should be a Liquid Tag +breaking change
 It should now be used like this
 ```
 {% post_url post-title.md %}
 ```
 - [#198](https://github.com/Code52/pretzel/issues/198) - Liquid tag/filter with underscore doesn't works in markdown files +fix

Commits: [1436ac1f52...2162dbc1f3](https://github.com/Code52/pretzel/compare/1436ac1f52...2162dbc1f3)


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

Commits: ...
