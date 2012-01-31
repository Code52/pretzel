using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;
using System.IO;
using System.Text.RegularExpressions;
using Pretzel.Logic.Extensions;

namespace Pretzel
{
    class Program
    {
       
        static void Main(string[] args)
        {
            var text = @"---
                        layout: post
                        title: This is a test jekyll document
                        description: TEST ALL THE THINGS
                        date: 2012-01-30
                        tags : 
                        - test
                        - alsotest
                        - lasttest
                        ---

                        ##Test

                        This is a test of YAML parsing";

            var header = text.YamlHeader();

        }


    }
}
