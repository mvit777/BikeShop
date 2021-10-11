using BikeDistributor.Domain.Models;
using BikeDistributor.Infrastructure.core;
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
using Newtonsoft.Json;
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
            
            var mongoContext = GetMongoContext(config);
            var register = new MongoServiceInstanceRegister();
            //save services in the register
            foreach(string sp in mongoContext.MongoSettings.Services)
            {
                string className = sp;
                var service = MongoServiceFactory.GetMongoService(mongoContext, className);
                register.SetServiceInstance(service, className);
                //services.AddScoped<IMongoService>(sp => service);
            }
            services.AddScoped(r=>register);
            config.DefaultMongoSettings = mongoContext.MongoSettings;
            services.AddScoped(c=>config);
            //foreach(string bsontype in mongoContext.MongoSettings.BsonTypes)
            //{
                
            //}
        }

        private MongoDBContext GetMongoContext(WsConfig config)
        {
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
