[![AppVeyor build status](https://ci.appveyor.com/api/projects/status/rp8nq03r4b7mdecf?svg=true)](https://ci.appveyor.com/project/laedit/pretzel)
[![Coverity Scan Build Status](https://scan.coverity.com/projects/3949/badge.svg)](https://scan.coverity.com/projects/3949)
[![Coverage Status](https://coveralls.io/repos/Code52/pretzel/badge.svg)](https://coveralls.io/r/Code52/pretzel)
[![Chocolatey](https://img.shields.io/chocolatey/v/pretzel.svg?style=flat)](https://chocolatey.org/packages/pretzel/) [![Nuget](https://img.shields.io/nuget/v/Pretzel.Logic.svg)](https://www.nuget.org/packages/Pretzel.Logic/)


## Pretzel

A simple, pluggable site generation tool for .NET developers and Windows users (with Mono support planned I think).

Pretzel follows the same conventions as [Jekyll](https://github.com/jekyll/jekyll) and should be compatible with basic Jekyll websites. If you are not familiar with Jekyll, have a read at [http://jekyllrb.com/docs/usage/](http://jekyllrb.com/docs/usage/).

### Usage

The principal commands are the following but more informations are available on the [wiki](https://github.com/Code52/pretzel/wiki)

**Create** is used to create the folder structure for a new site:

    pretzel create
    
If the site should be at a specific folder, this can be specified as a parameter:

    pretzel create C:\path\to\folder

**Bake** is used to generate a site based on the contents of a folder.

To scan the current directory for a website and detect the content to process, run:

    pretzel bake 

To scan a specific folder and parse the contents, run:

    pretzel bake C:\path\to\folder

To explicitly specify the input - we should support inferring the input based on the files found anyway - run:

    pretzel bake --engine liquid
    
**Taste** is for testing a site locally - make a change, and pretzel should handle regenerating the page when a file changes.

To test a site locally, run:

    pretzel taste 

To specify the port to serve the site from (default will be 4000), run:

    pretzel taste --port 5000

To stop the site, press q.

*More information on the [wiki](https://github.com/Code52/pretzel/wiki)*.

### Contributing

All contributions are welcome, please read our [contributing guide](CONTRIBUTING.md).

### Attribution
 “[Pretzel](https://thenounproject.com/noun/pretzel/#icon-No555)” symbol by Edward Boatman on [The Noun Project](https://thenounproject.com/), from The Noun Project collection.
