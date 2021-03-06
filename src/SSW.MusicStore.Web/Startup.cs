﻿using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.PlatformAbstractions;

namespace SSW.MusicStore.Web
{
	public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IApplicationEnvironment environment)
        {
            app.UseIISPlatformHandler(); // Used for Active Directory (Windows Authentication) and can be removed.

            // Route all unknown requests to app root
            app.Use(async (context, next) =>
            {
                await next();

                // If there's no available file and the request doesn't contain an extension, we're probably trying to access a page.
                // Rewrite request to use app root
                if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
                {
                    context.Request.Path = "/app/index.html";
                    await next();
                }
            });

            // Serve wwwroot as root
            app.UseFileServer();

            var nodeModulesPath =
                Path.Combine(environment.ApplicationBasePath, "node_modules");

            createFolderIfItDoesNotExist(nodeModulesPath);

            // Serve /node_modules as a separate root (for packages that use other npm modules client side)
            app.UseFileServer(new FileServerOptions()
            {
                // Set root of file server
                FileProvider = new PhysicalFileProvider(nodeModulesPath),
                // Only react to requests that match this path
                RequestPath = "/node_modules", 
                // Don't expose file system
                EnableDirectoryBrowsing = false
            });
        }

        private static void createFolderIfItDoesNotExist(string nodeModulesPath)
        {
            bool exists = System.IO.Directory.Exists(nodeModulesPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(nodeModulesPath);
        }
    }
}
