using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using TimeSharerApi.Interfaces;
using TimeSharerApi.Services;
using TimeSharerApi.Models;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.AspNetCore.Cors;
namespace TimeSharerApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.Configure<DatabaseSettings>(
            Configuration.GetSection(nameof(DatabaseSettings)));

            services.AddSingleton<IDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            services.AddCors();
            services.AddControllers();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(name: "v1", info: new OpenApiInfo
                {
                    Title = "TimeSharer API",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "Gareth Whitley",
                        Email = "me@garethwhitley.online",
                        Url = new System.Uri("https://github.com/gwhitdev")
                    }
                });
            });

            services.AddScoped<IVolunteerService, VolunteerService>();
            services.AddScoped<IOrganisationService, OrganisationService>();
            services.AddScoped<IOpportunityService, OpportunityService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            */
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyMethod());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json",
                    "TimeSharer API Version 1");
                c.SupportedSubmitMethods(new[]
                {
                    SubmitMethod.Get, SubmitMethod.Post,
                    SubmitMethod.Put, SubmitMethod.Delete
               });
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
