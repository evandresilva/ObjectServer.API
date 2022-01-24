using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using tusdotnet;
using tusdotnet.Helpers;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;
using Microsoft.AspNetCore.Http.Features;
using System.Text;
using System.IO;
using ObjectServer.API.ServiceInterfaces;
using ObjectServer.API.Services;
using System;
using tusdotnet.Models.Expiration;

namespace ObjectServer.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            DataLakeService = new LakeFsService(configuration);
            TusFileIdProvider = new LakeFsService(configuration);

        }

        public IConfiguration Configuration { get; }
        public IDataLakeService DataLakeService { get; }
        public ITusFileIdProvider TusFileIdProvider { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddCors();
            //services.AddScoped<IDataLakeService, LakeFsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use((context, next) =>
            {
                // Default limit was changed some time ago. Should work by setting MaxRequestBodySize to null using ConfigureKestrel but this does not seem to work for IISExpress.
                // Source: https://github.com/aspnet/Announcements/issues/267
                context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = int.MaxValue;
                return next.Invoke();
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors(builder => builder
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowAnyOrigin()
                   .WithExposedHeaders(CorsHelper.GetExposedHeaders()));
            var tusFiles = Path.Combine(env.ContentRootPath, "tusfiles");
            if (!Directory.Exists(tusFiles))
                Directory.CreateDirectory(tusFiles);

            var tusBuffer = new TusDiskBufferSize(4096);
            app.UseTus(httpContext => new DefaultTusConfiguration
            {
                Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(20)),
                Store = new TusDiskStore(tusFiles, true, tusBuffer, TusFileIdProvider),
                UrlPath = "/objectServer/file",
                MaxAllowedUploadSizeInBytes = int.MaxValue,
                Events = new Events
                {
                    //Upload completion event callback
                    OnFileCompleteAsync = async ctx =>
                    {
                        //Get upload file
                        var file = await ctx.GetFileAsync();
                        //var extensionName = Path.GetExtension(fileName);
                        ////Upload to LakeFs

                        var stream = await file.GetContentAsync(ctx.CancellationToken);
                        var result = await DataLakeService.UploadAsync(stream, file.Id);
                        //temp
                        if (!result.Success)
                        {
                            ctx.HttpContext.Response.StatusCode = (int)result.StatusCode;
                            await ctx.HttpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(result.Message));
                            return;
                        }
                    }
                },
            });
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
