﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
        ///
        ///&lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;feed xmlns=&quot;http://www.w3.org/2005/Atom&quot;&gt;
        ///  &lt;title&gt;@Model.Site.Title | @Model.Site.Config[&quot;author&quot;]&lt;/title&gt;
        ///  &lt;link href=&quot;@Model.Site.Config[&quot;url&quot;]&quot;/&gt;
        ///  &lt;link type=&quot;application/atom+xml&quot; rel=&quot;self&quot; href=&quot;@Model.Site.Config[&quot;url&quot;]/atom.xml&quot;/&gt;
        ///  &lt;updated&gt;@Model.Site.Time.ToString(&quot;s&quot;)&lt;/updated&gt;
        ///  &lt;id&gt;@Model.Site.Config[&quot;url&quot;]/&lt;/id&gt;
        ///  &lt;author&gt;
        ///    &lt;name&gt;@Model.Site.Config[&quot;author&quot;]&lt;/name&gt;
        ///    &lt;email&gt;@M [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Atom {
            get {
                return ResourceManager.GetString("Atom", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to pretzel: 
        ///    engine: razor
        ///
        ///# Site settings
        ///author: # name of the author
        ///title: # site title
        ///url: # site url
        ///contact: # mail of the author.
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
        ///&lt;head&gt;
        ///    &lt;meta content=&quot;en-au&quot; http-equiv=&quot;Content-Language&quot; /&gt;
        ///    &lt;meta content=&quot;text/html; charset=utf-8&quot; http-equiv=&quot;Content-Type&quot; /&gt;
        ///    &lt;link href=&quot;/rss.xml&quot; type=&quot;application/rss+xml&quot; rel=&quot;alternate&quot; title=&quot;Blog Feed&quot; /&gt;
        ///    &lt;link href=&quot;/atom.xml&quot; type=&quot;application/atom+xml&quot; rel=&quot;alternate&quot; title=&quot;Blog Feed&quot; /&gt;
        ///    &lt;meta name=&quot;viewport&quot; content=&quot;width=device-width, initial-scale=1, maximum-scale=1&quot; /&gt;        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Head {
            get {
                return ResourceManager.GetString("Head", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ---
        ///layout : layout
        ///title : SiteName
        ///---
        ///@model Pretzel.Logic.Templating.Context.PageContext
        ///
        ///&lt;ul class=&quot;posts&quot;&gt;
        ///@{
        ///    var numberPosts = 5;
        ///}
        ///    @for (var i = 0; i &lt; numberPosts &amp;&amp; i &lt; Model.Site.Posts.Count; i++)
        ///    {
        ///        var post = Model.Site.Posts[i];
        ///        &lt;li&gt;
        ///            &lt;div class=&quot;idea&quot;&gt;
        ///                @if (i == 0 &amp;&amp; post.Layout == &quot;post&quot;)
        ///                {
        ///                    &lt;h1&gt;&lt;a href=&quot;@post.Url&quot;&gt;@post.Title&lt;/a&gt;&lt;/h1&gt;
        ///                    &lt;div class=&quot;postdate&quot;&gt;@post.Dat [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Index {
            get {
                return ResourceManager.GetString("Index", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @inherits RazorEngine.Templating.TemplateBase&lt;Pretzel.Logic.Templating.Context.PageContext&gt;
        ///
        ///&lt;!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD XHTML 1.0 Transitional//EN&quot; &quot;http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd&quot;&gt;
        ///&lt;html xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;
        ///@Include(&quot;head.cshtml&quot;)
        ///&lt;body&gt;
        ///    &lt;div id=&quot;container&quot;&gt;
        ///        &lt;div id=&quot;side&quot;&gt;
        ///            &lt;a href=&quot;/&quot; id=&quot;home&quot; title=&quot;home&quot; alt=&quot;home&quot;&gt;&lt;img src=&quot;/img/logo.png&quot; alt=&quot;Site Name&quot; /&gt;&lt;/a&gt;
        ///            &lt;div id=&quot;hometext&quot;&gt;&lt;a href=&quot;/&quot;&gt;@Model.Site.Tit [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Layout {
            get {
                return ResourceManager.GetString("Layout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ---
        ///layout: layout
        ///---
        ///@inherits RazorEngine.Templating.TemplateBase&lt;Pretzel.Logic.Templating.Context.PageContext&gt;
        ///
        ///&lt;div class=&quot;entry-container&quot;&gt;
        ///	&lt;div class=&apos;entry&apos;&gt;
        ///		&lt;h1&gt;@Model.Title&lt;/h1&gt;
        ///		&lt;span class=&quot;postdate&quot;&gt;@Model.Page.Date.ToString(&quot;d MMM, yyyy&quot;)
        ///			@foreach(var tag in Model.Page.Tags)
        ///			{	
        ///                &lt;li&gt;&lt;a href=&quot;/tag/@tag&quot;&gt;@tag&lt;/a&gt;&lt;/li&gt;
        ///			}
        ///		&lt;/span&gt;
        ///		@Raw(Model.Content)
        ///	&lt;/div&gt;
        ///&lt;/div&gt;
        ///&lt;div id=&quot;page-navigation&quot;&gt; 
        ///	&lt;div class=&quot;left&quot;&gt;@if (Model.Previous != null)
        ///         [rest of string was truncated]&quot;;.
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
        ///&lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///&lt;rss version=&quot;2.0&quot; xmlns:atom=&quot;http://www.w3.org/2005/Atom&quot;&gt;
        ///&lt;channel&gt;
        ///    &lt;title&gt;@Model.Site.Title | @Model.Site.Config[&quot;author&quot;]&lt;/title&gt;
        ///    &lt;link&gt;@Model.Site.Config[&quot;url&quot;]&lt;/link&gt;
        ///    &lt;atom:link href=&quot;@Model.Site.Config[&quot;url&quot;]/rss.xml&quot; rel=&quot;self&quot; type=&quot;application/rss+xml&quot; /&gt;
        ///    &lt;description&gt;Personal blog of @Model.Site.Config[&quot;author&quot;]&lt;/description&gt;
        ///    &lt;language&gt;en-us&lt;/language&gt;
        ///   [rest of string was truncated]&quot;;.
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
        ///
        ///&lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot; ?&gt;
        ///&lt;urlset xmlns=&quot;http://www.sitemaps.org/schemas/sitemap/0.9&quot;
        ///        xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot;
        ///        xsi:schemalocation=&quot;http://www.sitemaps.org/schemas/sitemap/0.9
        ///  http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd&quot;&gt;
        ///    &lt;!-- Generated with pretzel --&gt;
        ///
        ///    @foreach (var post in Model.Site.Posts)
        ///    {
        ///        &lt;url&gt;
        ///            &lt;loc&gt;http://domai [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Sitemap {
            get {
                return ResourceManager.GetString("Sitemap", resourceCulture);
            }
        }
    }
}
