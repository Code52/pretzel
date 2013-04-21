using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gate.Middleware;
using System.Threading.Tasks;
using System.IO;

namespace Owin
{
    internal static class HeadSuppressionExtensions
    {
        public static IAppBuilder UseHeadSuppression(this IAppBuilder builder)
        {
            return builder.UseType<HeadSuppression>();
        }
    }
}

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // This middleware can be used to suppress output incorrectly written by other middleware or applications for HEAD requests.
    internal class HeadSuppression
    {
        private AppFunc nextApp;

        public HeadSuppression(AppFunc nextApp)
        {
            this.nextApp = nextApp;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var req = new Request(env);
            if ("HEAD".Equals(req.Method, StringComparison.OrdinalIgnoreCase))
            {
                env[OwinConstants.ResponseBody] = Stream.Null;
            }

            return nextApp(env);
        }
    }
}
