
using CatalogService.Api.Extensions;
using CatalogService.Api.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace CatalogService.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection Services)
        {

            //Services.AddScoped<IIdentityService, IdentityServer.Application.Services.IdentityService>(); //identity service consul registration

            Services.AddControllers();
            Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CatalogService.Api", Version = "v1"});
            });

            //Dışarıya vereceğimiz image lerin hangi klasörde olduklarını söyleyebilmke için kullanacağız.
            Services.Configure<CatalogSettings>(Configuration.GetSection("CatalogSettings"));
            Services.ConfigureDbContext(Configuration);

            Services.ConfigureConsul(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CatalogService.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.RegisterWithConsul(lifetime);
        }
    }
}
