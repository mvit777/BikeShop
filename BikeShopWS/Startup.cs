using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MV.Framework.providers;
using BikeDistributor.Domain.Models;
using BikeShopWS.Infrastructure;
using GrpcBike;
using Microsoft.AspNetCore.HttpOverrides;

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
            services.AddScoped(c=>config);//not really needed atm
            //adding gRpc 23/10/2021
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
                options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
            });
            services.AddGrpcReflection();
            
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
            /*required for linux publishing as we use apache as a proxy */
            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            //});
            /*end required*/
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseGrpcWeb(); // Must be added between UseRouting and UseEndpoints
            
            app.UseCors("AllowAll"); //it has to be here between routing and auth

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseEndpoints(endpoints =>
            {
                // Communication with gRPC endpoints must be made through a gRPC client.
                // To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909
                //endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<BikesService>().EnableGrpcWeb();
                if (env.IsDevelopment())
                {
                    endpoints.MapGrpcReflectionService();
                }
            });
            BsonClassMap.RegisterClassMap<Bike>();
            BsonClassMap.RegisterClassMap<BikeVariant>();
            BsonClassMap.RegisterClassMap<BikeOption>();
        }
    }
}
