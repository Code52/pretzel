[![TeamCity build status](http://teamcity.ginnivan.net/app/rest/builds/buildType:(id:bt52)/statusIcon)](http://teamcity.ginnivan.net/viewType.html?buildTypeId=bt52&guest=1)  |  [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/rp8nq03r4b7mdecf?svg=true)](https://ci.appveyor.com/project/laedit/pretzel)  |  [![Coverity Scan Build Status](https://scan.coverity.com/projects/3949/badge.svg)](https://scan.coverity.com/projects/3949)


## Pretzel

A simple, pluggable site generation tool for .NET developers and Windows users (with Mono support planned I think)

Pretzel follows the same convensions as  [Jekyll](https://github.com/mojombo/jekyll) and should be compatible with basic Jekyll websites. If you are not familiar with Jekyll, have a read at [http://jekyllrb.com/docs/usage/](http://jekyllrb.com/docs/usage/)

### Usage

We are working on a number of features for the initial release of Pretzel, which are represented by the commands **create**, **bake** and **taste**. 

**Create** is used to create the folder structure for a new site.

    pretzel create
    
If the site should be at a specific folder, this can be specified as a parameter:

    pretzel create C:\path\to\folder

**Bake** is used to generate a site based on the contents of a folder.

To scan the current directory for a website and detect the content to process, run:

    pretzel bake 

To scan a specific folder and parse the contents, run:

    pretzel bake C:\path\to\folder

To explicitly specify the input - we should support inferring the input based on the files found anyway - run:

    pretzel bake --engine jekyll
    

**Taste** is for testing a site locally - make a change, and pretzel should handle regenerating the page when a file changes.

To test a site locally, run:

	pretzel taste 

To specify the port to serve the site from (default will be 4000), run:

    pretzel taste --port 5000

To stop the site, press q.

### Getting started

**Getting started with Git and GitHub**

 * [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
 * [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
 * [The simple gude to GIT guide](http://rogerdudler.github.com/git-guide/)

Once you're familiar with Git and GitHub, clone the repository and run the ```.\build.cmd``` script to compile the code and run all the unit tests. You can use this script to test your changes quickly.

### Discussing ideas 

* [Trello Board](https://trello.com/board/pretzel/4f25ffb3dbbed1ab5a4f0f5a) - add ideas, or claim an idea and start working on it!
* [JabbR Chatroom](http://jabbr.net/#/rooms/code52) - discuss things in real-time with people all over the world!

### Contributing

All PRs are welcome but don't forget the tests with your new shiny feature.

### Links
- [ColorCode Fork](https://github.com/csainty/ColorCode) - Switch from inline html to stylesheet approach 


### Attribution
 “[Pretzel](http://thenounproject.com/noun/pretzel/#icon-No555)” symbol by Edward Boatman on [The Noun Project](http://www.thenounproject.com/), from The Noun Project collection.
