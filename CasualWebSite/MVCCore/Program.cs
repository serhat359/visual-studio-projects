using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVCCore.Helpers;
using System.Net;
using System.Net.Http;

namespace MVCCore;

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
        services.AddSingleton<IMemoryCache, MemoryCache>();
        services.AddTransient<CacheHelper>();
        services.AddSingleton<MyTorrentRssHelper>();
        services.AddSingleton<PokemonService>();
        services.AddSingleton<ManhwaService>();
        services.AddSingleton<DataContext>();

        services.AddHttpClient("")
            .ConfigurePrimaryHttpMessageHandler(messageHandler =>
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
                    "text/javascript",
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "application/wasm",
                    "text/json",
            };
        });

        services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        }));

        // Add services to the container.
        services.AddControllersWithViews();
    }

    private static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        // Compression is not needed locally
        // app.UseResponseCompression();

        app.UseCors("MyPolicy");

        app.UseResponseCompression();

        app.UseStaticFiles();

        app.UseRouting();

        //app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            endpoints.MapFallback(async context =>
            {
                var notfoundView = new ViewResult
                {
                    StatusCode = 404,
                    ViewName = "NotFound",
                };
                await notfoundView.ExecuteResultAsync(new ActionContext(context, new RouteData(), new ActionDescriptor()));
            });
        });
    }
}