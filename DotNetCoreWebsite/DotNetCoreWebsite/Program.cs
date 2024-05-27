using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http;

namespace DotNetCoreWebsite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services);

            var app = builder.Build();

            Configure(app, app.Environment);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<FileNameHelper>();
            services.AddSingleton<CoreEncryption>(serviceProvider => new CoreEncryption(serviceProvider.GetService<IConfiguration>().GetSection("EncryptionKey").Value));
            services.AddHttpClient("")
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                    new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        UseDefaultCredentials = true,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
                    });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = new[] {
                    "text/plain",
                    "text/css",
                    "application/javascript",
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "text/json",
                };
            });

            services.AddControllersWithViews();
        }

        private static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseResponseCompression();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "torrent",
                    pattern: "torrent/{id1}/{id2}/",
                    defaults: new { controller = "Home", action = "Torrent" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
