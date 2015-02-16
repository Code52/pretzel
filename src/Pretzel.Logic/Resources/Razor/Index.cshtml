---
layout : layout
title : SiteName
---
@model Pretzel.Logic.Templating.Context.PageContext

<ul class="posts">
@{
    var numberPosts = 5;
}
    @for (var i = 0; i < numberPosts && i < Model.Site.Posts.Count; i++)
    {
        var post = Model.Site.Posts[i];
        <li>
            <div class="idea">
                @if (i == 0 && post.Layout == "post")
                {
                    <h1><a href="@post.Url">@post.Title</a></h1>
                    <div class="postdate">@post.Date.ToString("d MMM, yyyy")
                        <ul>
                            @foreach(var tag in post.Tags)
                            {
                                <li><a href="/tag/@tag">@tag</a></li>
                            }
                        </ul>
                    </div>
                    @Raw(post.Content)
                    <br />
                    <a href="@post.Url#disqus_thread">Comments</a>
                }
                else
                {
                    <h2><a class="postlink" href="@post.Url">@post.Title</a></h2>
                    <div class="postdate">@post.Date.ToString("d MMM, yyyy")
                        <ul>
                            @foreach (var tag in post.Tags)
                            {
                                <li><a href="/tag/@tag">@tag</a></li>
                            }
                        </ul>
                    </div>
                    @Raw(post.Content)
                    <a href="@post.Url#disqus_thread">Comments</a>
                }
            </div>
        </li>
    }
</ul>

<h3>OLDER</h3>
<ul class="postArchive">
@foreach(var post in Model.Site.Posts.Skip(numberPosts))
{
    <li>
        <span class="olderpostdate">@post.Date.ToString("d m")</span> <a class="postlink" href="@post.Url">@post.Title</a>
    </li>
}
</ul>

<script type="text/javascript">
//<![CDATA[
(function () {
    var links = document.getElementsByTagName('a');
    var query = '?';
    for (var i = 0; i < links.length; i++) {
        if (links[i].href.indexOf('#disqus_thread') >= 0) {
            query += 'url' + i + '=' + encodeURIComponent(links[i].href) + '&';
        }
    }
    document.write('<script charset="utf-8" type="text/javascript" src="http://disqus.com/forums/DISQUS_NAME/get_num_replies.js' + query + '"></' + 'script>');
})();
//]]>
</script>