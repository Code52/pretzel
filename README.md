## Pretzel

A simple, pluggable site generation tool for .NET developers and Windows users (with Mono support planned I think)

### Usage

Initially, we plan to support two modes - **bake** and **taste**. **Bake** is for a once-off generation of the site, whereas **taste** is for testing a site locally - make a change, and pretzel should handle regenerating the page when a file changes.

To scan the current directory for a website and detect the content to process, run:

    pretzel bake 

To scan a specific folder and parse the contents, run:

    pretzel bake C:\path\to\folder

To explicitly specify the input - we should support inferring the input based on the files found anyway - run:

    pretzel bake --engine jekyll
    

To test a site locally (we plan to use [Firefly](https://github.com/loudej/firefly)), run:

	pretzel taste 

To specify the port to serve the site from (default will be 4000), run:

    pretzel taste --port 5000


### Getting started

**Getting started with Git and GitHub**

 * [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
 * [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
 * [The simple gude to GIT guide](http://rogerdudler.github.com/git-guide/)

Once you're familiar with Git and GitHub, clone the repository and run the ```.\build.cmd``` script to compile the code and run all the unit tests. You can use this script to test your changes quickly.

### Discussing ideas 

* [Trello Board](https://trello.com/board/pretzel/4f25ffb3dbbed1ab5a4f0f5a) - add ideas, or claim an idea and start working on it!
* [JabbR Chatroom](http://jabbr.net/#/rooms/code52) - discuss things in real-time with people all over the world!

### Links
- [ColorCode Fork](https://github.com/csainty/ColorCode) - Switch from inline html to stylesheet approach