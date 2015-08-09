﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pretzel.Logic.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Razor {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Razor() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Pretzel.Logic.Properties.Razor", typeof(Razor).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ---
        ///layout: post
        ///title: About
        ///comments: false
        ///---
        ///
        ///New Site  
        ///  
        ///##To Do  
        ///  
        ///Edit this Markdown to add an about page.
        /// </summary>
        internal static string About {
            get {
                return ResourceManager.GetString("About", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ---
        ///layout: nil
        ///---
        ///@model Pretzel.Logic.Templating.Context.PageContext
        ///&lt;?xml version=&quot;1.0&quot; ?&gt;
        ///@{
        ///    var sitename = Model.Site.Config.ContainsKey(&quot;sitename&quot;) ? (string)Model.Site.Config[&quot;sitename&quot;] : &quot;Site Name&quot;;
        ///    var domain = Model.Site.Config.ContainsKey(&quot;domain&quot;) ? (string)Model.Site.Config[&quot;domain&quot;] : &quot;http://domain.local&quot;;
        ///    var author = Model.Site.Config.ContainsKey(&quot;author&quot;) ? (string)Model.Site.Config[&quot;author&quot;] : &quot;Author&quot;;
        ///    var email = Model.Site.Config.ContainsKey(&quot;email&quot;) ? (stri [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Atom {
            get {
                return ResourceManager.GetString("Atom", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to pretzel:
        ///    engine: razor
        ///sitename: Site Name
        ///domain: http://domain.local.
        /// </summary>
        internal static string Config {
            get {
                return ResourceManager.GetString("Config", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to --- 
        ///layout: post
        ///title: &quot;My First Post&quot;
        ///author: &quot;Author&quot;
        ///comments: true
        ///---
        ///
        ///## Hello world...
        ///
        ///Code:
        ///
        ///```cs
        ///static void Main() 
        ///{
        ///    Console.WriteLine(&quot;Hello World!&quot;);
        ///}
        ///```
        ///
        ///This is my first post on the site!.
        /// </summary>
        internal static string FirstPost {
            get {
                return ResourceManager.GetString("FirstPost", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @inherits RazorEngine.Templating.TemplateBase&lt;Pretzel.Logic.Templating.Context.PageContext&gt;
        ///@{
        ///    var language = Model.Site.Config.ContainsKey(&quot;language&quot;) ? Model.Site.Config[&quot;language&quot;] : &quot;en-au&quot;;
        ///}
        ///&lt;head&gt;
        ///    &lt;meta content=&quot;@language&quot; http-equiv=&quot;Content-Language&quot; /&gt;
        ///    &lt;meta content=&quot;text/html; charset=utf-8&quot; http-equiv=&quot;Content-Type&quot; /&gt;
        ///    &lt;link href=&quot;/rss.xml&quot; type=&quot;application/rss+xml&quot; rel=&quot;alternate&quot; title=&quot;Blog Feed&quot; /&gt;
        ///    &lt;link href=&quot;/atom.xml&quot; type=&quot;application/atom+xml&quot; rel=&quot;alternate [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Head {
            get {
                return ResourceManager.GetString("Head", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ---
        ///layout : main
        ///title : SiteName
        ///---
        ///.
        /// </summary>
        internal static string Index {
            get {
                return ResourceManager.GetString("Index", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @inherits RazorEngine.Templating.TemplateBase&lt;Pretzel.Logic.Templating.Context.PageContext&gt;
        ///@{
        ///    var sitename = Model.Site.Config.ContainsKey(&quot;sitename&quot;) ? (string)Model.Site.Config[&quot;sitename&quot;] : &quot;Site Name&quot;;
        ///}
        ///&lt;!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD XHTML 1.0 Transitional//EN&quot; &quot;http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd&quot;&gt;
        ///&lt;html xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;
        ///@Include(&quot;head.cshtml&quot;)
        ///&lt;body&gt;
        ///    &lt;div id=&quot;container&quot;&gt;
        ///        &lt;div id=&quot;side&quot;&gt;
        ///            &lt;a href=&quot;/&quot; id=&quot;home&quot; title=&quot;ho [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Layout {
            get {
                return ResourceManager.GetString("Layout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @inherits RazorEngine.Templating.TemplateBase&lt;Pretzel.Logic.Templating.Context.PageContext&gt;
        ///
        ///@{
        ///    Layout = &quot;layout.cshtml&quot;;
        ///    var dateformat = Model.Site.Config.ContainsKey(&quot;dateformat&quot;) ? (string)Model.Site.Config[&quot;dateformat&quot;] : &quot;d MMM, yyyy&quot;;
        ///    var disqus_shortname = Model.Site.Config.ContainsKey(&quot;disqus_shortname&quot;) ? (string)Model.Site.Config[&quot;disqus_shortname&quot;] : &quot;NAME&quot;;
        ///    var numberPosts = 5;
        ///}
        ///
        ///@section body {
        ///    &lt;ul class=&quot;posts&quot;&gt;
        ///        @for (var i = 0; i &lt; numberPosts &amp;&amp; i &lt; M [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Main {
            get {
                return ResourceManager.GetString("Main", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @inherits RazorEngine.Templating.TemplateBase&lt;Pretzel.Logic.Templating.Context.PageContext&gt;
        ///
        ///@{
        ///    Layout = &quot;layout.cshtml&quot;;
        ///    var dateformat = Model.Site.Config.ContainsKey(&quot;dateformat&quot;) ? (string)Model.Site.Config[&quot;dateformat&quot;] : &quot;d MMM, yyyy&quot;;
        ///    var disqus_shortname = Model.Site.Config.ContainsKey(&quot;disqus_shortname&quot;) ? (string)Model.Site.Config[&quot;disqus_shortname&quot;] : &quot;NAME&quot;;
        ///}
        ///
        ///@section body {
        ///    &lt;div class=&quot;entry-container&quot;&gt;
        ///        &lt;div class=&apos;entry&apos;&gt;
        ///            &lt;h1&gt;@Model.Title&lt;/h1&gt;
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Post {
            get {
                return ResourceManager.GetString("Post", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ---
        ///layout: nil
        ///---
        ///@model Pretzel.Logic.Templating.Context.PageContext
        ///&lt;?xml version=&quot;1.0&quot; ?&gt;
        ///@{
        ///    var sitename = Model.Site.Config.ContainsKey(&quot;sitename&quot;) ? (string)Model.Site.Config[&quot;sitename&quot;] : &quot;Site Name&quot;;
        ///    var domain = Model.Site.Config.ContainsKey(&quot;domain&quot;) ? (string)Model.Site.Config[&quot;domain&quot;] : &quot;http://domain.local&quot;;
        ///    var author = Model.Site.Config.ContainsKey(&quot;author&quot;) ? (string)Model.Site.Config[&quot;author&quot;] : &quot;Author&quot;;
        ///    var email = Model.Site.Config.ContainsKey(&quot;email&quot;) ? (stri [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Rss {
            get {
                return ResourceManager.GetString("Rss", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ---
        ///layout: nil
        ///---
        ///@model Pretzel.Logic.Templating.Context.PageContext
        ///&lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot; ?&gt;
        ///@{
        ///    var domain = Model.Site.Config.ContainsKey(&quot;domain&quot;) ? (string)Model.Site.Config[&quot;domain&quot;] : &quot;http://domain.local&quot;;
        ///}
        ///&lt;urlset xmlns=&quot;http://www.sitemaps.org/schemas/sitemap/0.9&quot;
        ///        xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot;
        ///        xsi:schemalocation=&quot;http://www.sitemaps.org/schemas/sitemap/0.9
        ///  http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd&quot;&gt;
        ///    &lt;!--  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Sitemap {
            get {
                return ResourceManager.GetString("Sitemap", resourceCulture);
            }
        }
    }
}
