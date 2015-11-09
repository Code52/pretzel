# Contributing to Pretzel

## Getting started

**Getting started with Git and GitHub**

 * [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
 * [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
 * [The simple guide to GIT guide](http://rogerdudler.github.com/git-guide/)

Once you're familiar with Git and GitHub, clone the repository and run the ```.\build.cmd``` script to compile the code and run all the unit tests. You can use this script to test your changes quickly.

## Discussing ideas 

* [JabbR Chatroom](http://jabbr.net/#/rooms/code52)
* [GitHub Issues](https://github.com/Code52/pretzel/issues)

**The functionalities are based as much as possible on existing functionalities in jekyll**

## Coding conventions

We are following as much as possible the [C# coding conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx).  
We prefer spaces over tab for indentation.

## Testing

Don't forget to add some tests for your functionality in the tests suit.  
You can see the result either:
- in Visual Studio or other IDE supporting xUnit2
- by executing build.cmd
- on [AppVeyor](https://ci.appveyor.com/project/laedit/pretzel) after the PR is submitted