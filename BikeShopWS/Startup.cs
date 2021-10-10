using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.services;
using BikeShopWS.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MV.Framework.interfaces;
using MV.Framework.providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeShopWS
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

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });//it has to be here before everything
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BikeShopWS", Version = "v1" });
            });
            var config = new WsConfig(@"./appsettings.json");
            services.AddSingleton(Config => config);
            var mongoContext = GetMongoContext(config);
            var bs = new MongoBikeService(mongoContext);
            services.AddScoped<IMongoService>(BS => bs);       
        }

        private MongoDBContext GetMongoContext(WsConfig config)
        {
            //var mongoUrl = "mongodb+srv://tr_mongouser2:jX9lnzMHo80P39fW@cluster0.i90tq.mongodb.net/?authSource=admin";
            //var mongoDb = "BikeDb";//MongoSettings["dbName"];
            ////var mongoUrl = Configuration.GetSection("Mongo").GetValue<string>("url");
            ////var mongoDb = Configuration.GetSection("Mongo").GetValue<string>("dbName");
            //var mongoServicesNs = "BikeDistributor.Infrastructure.services";//MongoSettings["servicesNamespace"];

            //return new MongoDBContext(mongoUrl, mongoDb);
            var mongosettings = config.GetClassObject<MongoSettings>("Mongo");
            return new MongoDBContext(mongosettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BikeShopWS v1"));
            }


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAll"); //it has to be here between routing and auth

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            BsonClassMap.RegisterClassMap<Bike>();
            BsonClassMap.RegisterClassMap<BikeVariant>();
            BsonClassMap.RegisterClassMap<BikeOption>();
        }
    }
}
