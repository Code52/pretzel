# 0.7.1 (2017/05/09)

 - [#321](https://github.com/Code52/pretzel/issues/321) - Pretzel 0.7.0 uses white text for output irregardless of console background +fix

Commits: 5db0c6ec24...5db0c6ec24


# 0.7.0 (2017-03-18)

 - [#319](https://github.com/Code52/pretzel/pull/319) - Site with support of html_pages contributed by Roland Bär ([rolandbaer](https://github.com/rolandbaer))
 - [#318](https://github.com/Code52/pretzel/issues/318) - Chocolatey install - Get-BinRoot deprecated

Commits: b7afc2ced8...8b677201a1


# 0.6.0 (2017-01-29)

 - [#317](https://github.com/Code52/pretzel/pull/317) - Support for :slug in permalinks contributed by Thomas Freudenberg ([thoemmi](https://github.com/thoemmi)) +Enhancement
 - Fix rss/atom feeds and add site settings in config +Fix
 - Add more info on "Error converting markdown" +Enhancement
 - Cleaning of Tracing/Logger - Logger is now deprecated +Breaking-change

Commits: 8ffc7d6444...35f9a0491d


# 0.5.0 (2016-10-29)

 - [#314](https://github.com/Code52/pretzel/pull/314) - Fix the files are rendered too much time contributed by Jérémie Bertrand ([laedit](https://github.com/laedit))
 - [#313](https://github.com/Code52/pretzel/pull/313) - Minor typos contributed by Stephen Moon ([s-moon](https://github.com/s-moon))
 - [#310](https://github.com/Code52/pretzel/pull/310) - Update FileContentProvider.cs contributed by mark van tilburg ([markvantilburg](https://github.com/markvantilburg))
 - [#309](https://github.com/Code52/pretzel/pull/309) - Update packages.config contributed by mark van tilburg ([markvantilburg](https://github.com/markvantilburg))
 - [#306](https://github.com/Code52/pretzel/pull/306) - Added support for specifying frontmatter defaults in _config.yml contributed by Taco Ditiecher ([tditiecher](https://github.com/tditiecher))
 - [#304](https://github.com/Code52/pretzel/pull/304) - Added DateToStringFilter to the LiquidEngine. contributed by Taco Ditiecher ([tditiecher](https://github.com/tditiecher)) +fix
 - [#298](https://github.com/Code52/pretzel/pull/298) - Replace Firefly with Nowin contributed by E.Z. Hart ([hartez](https://github.com/hartez))
 - [#297](https://github.com/Code52/pretzel/pull/297) - Prevent adding of duplicate tags during PreProcess() in RazorSiteEngine contributed by E.Z. Hart ([hartez](https://github.com/hartez))
 - [#296](https://github.com/Code52/pretzel/issues/296) - ArgumentException when modifying a page while taste is running +fix
 - [#164](https://github.com/Code52/pretzel/issues/164) - Exception while navigating between pages during tasting +fix
 - [#147](https://github.com/Code52/pretzel/issues/147) - Sporadically crashes on "Taste" caused by 'Cannot access a disposed object.' +fix
 - [#102](https://github.com/Code52/pretzel/issues/102) - Crash when browsing a site hosted in Pretzel taste +fix

Commits: 1e49a174fc...a0634fdcec


# 0.4.0 (2016-01-23)

##  Breaking changes
 - [#287](https://github.com/Code52/pretzel/pull/287) - Accessible configuration contributed by Jérémie Bertrand ([laedit](https://github.com/laedit))

## Fixes
 - [#294](https://github.com/Code52/pretzel/pull/294) - Fix failing build on dev machine configured for NuGet V3 contributed by Tim Murphy ([TimMurphy](https://github.com/TimMurphy))
 - [#293](https://github.com/Code52/pretzel/issues/293) - Broken razor template inheritance
 - [#292](https://github.com/Code52/pretzel/pull/292) - Fix on Razor based pages not being renamed to .html contributed by Kees Schollaart ([keesschollaart81](https://github.com/keesschollaart81))


## Features
 - [#288](https://github.com/Code52/pretzel/pull/288) - [RFC] new extension point `IBeforeProcessingTransform` contributed by Thomas Freudenberg ([thoemmi](https://github.com/thoemmi))
 - [#284](https://github.com/Code52/pretzel/pull/284) - New command : create new post contributed by Keuvain ([k94ll13nn3](https://github.com/k94ll13nn3))

Commits: [4d8fcaa1a0...142be2a212](https://github.com/Code52/pretzel/compare/4d8fcaa1a0...142be2a212)


# 0.3.0

## Breaking changes
- [#199](https://github.com/Code52/pretzel/issues/199) - PostUrl should be a Liquid Tag
It should now be used like this
```
{% post_url post-title.md %}
```
- [#266](https://github.com/Code52/pretzel/issues/266) - Merging of dependencies to Pretzel.exe make developing plugins a pain
Pretzel is no longer one unique exe file but a exe, a dll Pretzel.Logic and all of the dependecies dll.
All existing plugins must be recompiled with referencing only Pretzel.Logic.

## Features
- [#200](https://github.com/Code52/pretzel/pull/200) - Single category support for liquid posts contributed by Andrey Akinshin ([AndreyAkinshin](https://github.com/AndreyAkinshin))
- [#202](https://github.com/Code52/pretzel/pull/202) - Add Excerpt feature for pages contributed by Andrey Akinshin ([AndreyAkinshin](https://github.com/AndreyAkinshin))
- [#203](https://github.com/Code52/pretzel/pull/203) - Support for 404 pages contributed by Damian Karzon ([dkarzon](https://github.com/dkarzon))
- [#157](https://github.com/Code52/pretzel/pull/157) - Add support to load dlls in "_plugins" path contributed by Miguel Román ([miguelerm](https://github.com/miguelerm))
- [#206](https://github.com/Code52/pretzel/pull/206) - Code block support contributed by Andrey Akinshin ([AndreyAkinshin](https://github.com/AndreyAkinshin))
- [#213](https://github.com/Code52/pretzel/pull/213) - Adds .htm extension for layouts contributed by Wesley Alcoforado ([wesleyalcoforado](https://github.com/wesleyalcoforado))
- [#216](https://github.com/Code52/pretzel/pull/216) - Support to other default pages contributed by Wesley Alcoforado ([wesleyalcoforado](https://github.com/wesleyalcoforado))
- [#165](https://github.com/Code52/pretzel/pull/165) - Make category inherit Drop (so it can be used in templates) contributed by Jordan Wallwork ([jordanwallwork](https://github.com/jordanwallwork))
- [#218](https://github.com/Code52/pretzel/issues/218) - Add --safe parameter to disable custom plugins
- [#220](https://github.com/Code52/pretzel/pull/220) - Add source and destination options contributed by Jérémie Bertrand ([laedit](https://github.com/laedit))
- [#221](https://github.com/Code52/pretzel/pull/221) - New command, 'Hungry' contributed by ([vikingcode](https://github.com/vikingcode))
- [#225](https://github.com/Code52/pretzel/issues/225) - Update permalinks to jekyll's level
- [#229](https://github.com/Code52/pretzel/pull/229) - Add Scriptcs plugins support contributed by Jérémie Bertrand ([laedit](https://github.com/laedit))
- [#233](https://github.com/Code52/pretzel/pull/233) - Better support for exclude config setting contributed by Damian Karzon ([dkarzon](https://github.com/dkarzon))
- [#241](https://github.com/Code52/pretzel/issues/241) - Should support --version switch
- [#244](https://github.com/Code52/pretzel/issues/244) - Extensionless permalinks
- [#253](https://github.com/Code52/pretzel/pull/253) - Run on Mono contributed by Thiago 'Jedi' Abreu ([thiagoabreu](https://github.com/thiagoabreu))
- [#259](https://github.com/Code52/pretzel/pull/259) - Changed date guessing heuristic to use last modification time of posts instead of current time, if no better data is available. contributed by Gábor Gergely ([kodfodrasz](https://github.com/kodfodrasz))
- [#260](https://github.com/Code52/pretzel/pull/260) - Refactor logging contributed by Gábor Gergely ([kodfodrasz](https://github.com/kodfodrasz))
- [#274](https://github.com/Code52/pretzel/pull/274) - Added option to only use categories found in posts's frontmatter by Thomas Freudenberg ([thoemmi](https://github.com/thoemmi))
- [#274](https://github.com/Code52/pretzel/pull/274) - added option only_frontmatter_categories contributed by Thomas Freudenberg ([thoemmi](https://github.com/thoemmi))
- [#277](https://github.com/Code52/pretzel/pull/277) - allow System.IO.Abstractions in ScriptCs contributed by Thomas Freudenberg ([thoemmi](https://github.com/thoemmi))

## Fixes
- [#198](https://github.com/Code52/pretzel/issues/198) - Liquid tag/filter with underscore doesn't works in markdown files
- [#201](https://github.com/Code52/pretzel/issues/201) - Custom metadatas in pages are not available from site.pages in liquid
- [#212](https://github.com/Code52/pretzel/issues/212) - If a post has invalid categories, it won't render contributed by ([vikingcode](https://github.com/vikingcode))
- [#178](https://github.com/Code52/pretzel/pull/178) - Create command fixes contributed by Gábor Gergely ([kodfodrasz](https://github.com/kodfodrasz))
- [#223](https://github.com/Code52/pretzel/issues/223) - Tag doesn't inherit from Drop
- [#228](https://github.com/Code52/pretzel/issues/228) - page.url contains broken in latest master version
- [#235](https://github.com/Code52/pretzel/pull/235) - Update FileContentProvider.cs contributed by mark van tilburg ([markvantilburg](https://github.com/markvantilburg))
- [#240](https://github.com/Code52/pretzel/issues/240) - Error when baking brand new razor template site
- [#246](https://github.com/Code52/pretzel/issues/246) - created site doesn't process liquid tags in posts when shown on homepage
- [#249](https://github.com/Code52/pretzel/pull/249) - Fix for page.url when paginating contributed by Thomas Freudenberg ([thoemmi](https://github.com/thoemmi))
- [#250](https://github.com/Code52/pretzel/pull/250) - Inconsistent page.date handling contributed by Thomas Freudenberg ([thoemmi](https://github.com/thoemmi))
- [#255](https://github.com/Code52/pretzel/pull/255) - Fix pagination when paginate_link points to a directory contributed by Gábor Gergely ([kodfodrasz](https://github.com/kodfodrasz))
- [#257](https://github.com/Code52/pretzel/pull/257) - Reading namespaces from attributes contributed by Thiago 'Jedi' Abreu ([thiagoabreu](https://github.com/thiagoabreu))
- [#263](https://github.com/Code52/pretzel/pull/263) - Enhanced front matter contributed by Gábor Gergely ([kodfodrasz](https://github.com/kodfodrasz))
- [#265](https://github.com/Code52/pretzel/pull/265) - Fix regenerating site for change in excluded files while tasting contributed by Gábor Gergely ([kodfodrasz](https://github.com/kodfodrasz))
- [#269](https://github.com/Code52/pretzel/pull/269) - Update Rss.liquid contributed by mark van tilburg ([markvantilburg](https://github.com/markvantilburg))
- [#271](https://github.com/Code52/pretzel/pull/271) - Added missing next and previous items in the page Hash in the Liquid engine contributed by Keuvain ([k94ll13nn3](https://github.com/k94ll13nn3))
- [#273](https://github.com/Code52/pretzel/pull/273) - Fix post_url tag contributed by Jérémie Bertrand ([laedit](https://github.com/laedit))

Commits: [1436ac1f52...d6fa9b28b6](https://github.com/Code52/pretzel/compare/1436ac1f52...d6fa9b28b6)


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
