// Licensed to the siocore Foundation under one or more agreements.
// The siocore Foundation licenses this file to you under the GNU General Public License v3.0 license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.IdentityModel.Tokens;
using Sio.Cms.Lib;
using Sio.Cms.Lib.Services;
using RewriteRules;
using System.IO;
using System.Text;

namespace Sio.Cms.Web
{
    public partial class Startup
    {
        protected void ConfigRoutes(IApplicationBuilder app)
        {
            if (SioService.GetConfig<bool>("IsRewrite"))
            {
                using (StreamReader apacheModRewriteStreamReader =
            File.OpenText("ApacheModRewrite.txt"))
                using (StreamReader iisUrlRewriteStreamReader =
                    File.OpenText("IISUrlRewrite.xml"))
                {
                    var options = new RewriteOptions()
                        .AddRedirect("redirect-rule/(.*)", "redirected/$1")
                        .AddRewrite(@"^rewrite-rule/(\d+)/(\d+)", "rewritten?var1=$1&var2=$2",
                            skipRemainingRules: true)
                        .AddApacheModRewrite(apacheModRewriteStreamReader)
                        .AddIISUrlRewrite(iisUrlRewriteStreamReader)
                        .Add(MethodRules.RedirectXMLRequests);
                    //.Add(new RedirectImageRequests(".png", "/png-images"))
                    //.Add(new RedirectImageRequests(".jpg", "/jpg-images"));

                    app.UseRewriter(options);
                }
            //    app.Run(context => context.Response.WriteAsync(
            //$"Rewritten or Redirected Url: " +
            //$"{context.Request.Path + context.Request.QueryString}"));
            }
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{culture=" + SioService.GetConfig<string>(SioConstants.ConfigurationKeyword.DefaultCulture) + "}/{area:exists}/{controller=Portal}/{action=Init}");
                routes.MapRoute(
                    name: "alias",
                    template: "{culture=" + SioService.GetConfig<string>(SioConstants.ConfigurationKeyword.DefaultCulture) + "}/{seoName}");
                routes.MapRoute(
                   name: "page",
                   template: "{culture=" + SioService.GetConfig<string>(SioConstants.ConfigurationKeyword.DefaultCulture) + "}/{seoName}");
                routes.MapRoute(
                    name: "file",
                    template: "{culture=" + SioService.GetConfig<string>(SioConstants.ConfigurationKeyword.DefaultCulture) + "}/portal/file");
                routes.MapRoute(
                    name: "article",
                    template: "{culture=" + SioService.GetConfig<string>(SioConstants.ConfigurationKeyword.DefaultCulture) + "}/article/{id}/{seoName}");
                routes.MapRoute(
                    name: "product",
                    template: @"{culture=" + SioService.GetConfig<string>(SioConstants.ConfigurationKeyword.DefaultCulture) + @"}/product/{seoName}");
            });
        }

    }
}
