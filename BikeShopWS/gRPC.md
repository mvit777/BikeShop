## Shift from Rest to gRPC
 
 (TODO: Add list of required grpc packages for both client and server)
 
 ### (UPDATE) the temporarily aborted shift from Rest to gRPC
After a week of protobuff-fever I decided, not without regret, to step back to REST. While the server part was quite easy to implement, and I'll keep it for a later stage, the client part implementation showed up some not-so-easy-solvable problems that weren't fixed by the othewise excellent [AutoMapper](https://automapper.org/) package. 
In particular, handling of null values and mapping of "grpc entities" (don't know if this term makes sense) and some missing types made me think twice. 
While [Code first project](https://docs.microsoft.com/en-us/aspnet/core/grpc/code-first?view=aspnetcore-5.0) might come to rescue it has some drawbacks on its own.
Moreover, I suspect the naive implementation of my domain models and Mongo entities played a role in the above problems. Definetly time for a refactor and more study/planning before a second attempt. However, I'm definetly not giving up. I'll soon branch the project and keep trying. In the meanwhile, I moved the documentation of first attempt here and I'll keep updating it. At least the server part will co-exist without problems.

## The project
since for the moment I want to keep co-existing both Api, REST and gRPC, I'm going to simply add gRPC on top of the existing [BikeShopWS](https://github.com/mvit777/BikeShop/tree/master/BikeShopWS) which I'm already publishing on my local IIS. 

According to [MS docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/supported-platforms?view=aspnetcore-5.0) ```IIS gRPC support requires .NET 5 and Windows 10 Build 20300.1000 or later```. 
My aging Laptop is on Windows 10 build 19043 and not fully ready to upgrade to Windows 11. The only channel selectable on [Window Insider](https://blogs.windows.com/windows-insider/2021/10/19/releasing-windows-10-build-19044-1319-21h2-to-release-preview-channel/) shows the most recent build is currently 19044. So for the moment I'm out of lack with IIS. The quickest way I found to swap from IIS to Kestrel is the following:

Fire up PowerShell ISE (Powershell ISE is handy because it allows to open a multi-tab terminal), issue the command ```$profile``` to find out where the profile file is supposed to go.
```powershell
PS C:\Windows\System32> $profile
C:\Users\<YOUR_USER>\OneDrive\Documents\WindowsPowerShell\Microsoft.PowerShellISE_profile.ps1

```
create the above file 
```ps
New-Item -Path $PROFILE.CurrentUserCurrentHost -Type file -Force
```
and add a few custom commands
```powershell
# start kestrel 
function runBikeWs{
    Set-Location "C:\<YOUR_PATH_TO>\BikeShopWS\"
    dotnet run
}
# start gRPCUI
function debugBikeWs{
    grpcui localhost:5001
}
# compile protos for BikeShop wasm
function compileProtos{
    stopBikeWs
    Set-Location "C:\Users\Marcello\source\repos\Blazor\BikeShop\"
    dotnet run
}
function stopBikeWs{
    Get-Process -Name *dotnet* | Stop-Process
}
```
close and re-open Powershell ISE.
Now when I want to compile, start and let BikeShopWS run, I simply issue:

```powershell
runBikeWs
```
which runs BikeShopWS on ports http 5000 and https 5001. In development no further certificate is needed because it is using the one of the REST api
```powershell
Compilazione...
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\Marcello\source\repos\Blazor\BikeShopWS

```
I temporarily stop the service because is finally time to add gRPC support. Following [docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-5.0), the first step is to add the NuGet meta-package  ```Grpc.AspNetCore```.

Next step is adding a gRPC service. I want to replicate the ```/bikes** GET url``` which returns the list of bikes from MongoDB. To do so I created a ```Protos/``` folder at project's root level and a ```bike.proto``` file inside the folder.
```proto
syntax = "proto3";

import "google/protobuf/timestamp.proto";
import "google/protobuf/empty.proto";

option csharp_namespace = "BikeShopWS.Protos";

package bike;

// service definition.
service Bikes {
  rpc GetBikes (google.protobuf.Empty) returns (GetBikesResponse); 
}

message GetBikesResponse {
  repeated EntityMongoBike bikeEntities = 1;
}

message EntityBikeOption{
    string Name=1;
    string Description=2;
    int32 Price=3;
}

message EntityBike{
        string Brand=1;
        string Model=2;
        int32 Price=3;
        int32 BasePrice=4;
        bool isStandard=5;
        string Description=6;
}

message EntityMongoBike {
        string Id=1;
        int32 TotalPrice=2;
        bool IsStandard=3;
        repeated EntityBikeOption SelectedOptions=4;
        EntityBike Bike =5;
}

```
then I registered the new proto file in ```BikeShowWs.csproj```
```xml
<ItemGroup>
    <Protobuf Include="Protos\bike.proto" GrpcServices="Server" />
</ItemGroup>
```
and finally I created a ```Services/``` folder with a ```BikeService.cs``` inside.
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BikeDistributor.Domain.Entities;
using BikeDistributor.Infrastructure.services;
using BikeShopWS.Infrastructure;
using BikeShopWS.Protos;   //mind
using Google.Protobuf.WellKnownTypes; //mind
using Grpc.Core; //mind
using Microsoft.Extensions.Logging;
using MV.Framework.providers;

namespace GrpcBike
{
    #region snippet
    public class BikesService : Bikes.BikesBase
    {
        private readonly ILogger<BikesService> _logger;
        private MongoServiceInstanceRegister _register;
        private readonly MongoBikeService _bikeService;
        private WsConfig _config;
        private IMapper _mapper;
        public BikesService(ILogger<BikesService> logger, MongoServiceInstanceRegister register,WsConfig config)
        {
            _logger = logger;
            _register = register;
            _config = config;
            _bikeService = (MongoBikeService)_register.GetServiceInstance("MongoBikeService", _config.GetMongoServiceIdentity("MongoBikeService"));
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MongoEntityBike, EntityMongoBike>();
                cfg.CreateMap<BikeOption, EntityBikeOption>();
                cfg.CreateMap<IBike, EntityBike>();
            });
            _mapper = mapperConfiguration.CreateMapper();
        }

        public override async Task<GetBikesResponse> GetBikes(Empty request, ServerCallContext context)
        {
            List<MongoEntityBike> mebs = await _bikeService.Get();
            var response = new GetBikesResponse();
            List<EntityMongoBike> bikes = _mapper.Map<List<EntityMongoBike>>(mebs);
            response.BikeEntities.AddRange(bikes);
            
            return response;
        }
    }
    #endregion
}
```
the step of mapping "complex" objects caught me a bit unprepared and I'm not fully satisfied with the solution I found so far. I'll soon have a look at C# tooling to automatise the process and check if I'm on the right track. Anyway it seems to work.

The last step is enabling gRPC in ```Startup.cs``` 
(TODO: this code while working does not use the right grpc pacakge. FIX IT with the actual code)
```csharp
//(..code omitted..)
using GrpcBike;

namespace BikeShopWS
{
    public class Startup
    {
        //(...code omitted...)

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //I already enabled CORS for the rest API. Should work fine. In production you might want a less liberal policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });//it has to be here before everything
           // (...code omitted ..)
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
                options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
            });
            services.AddGrpcReflection();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
          // (..code omitted..)
            app.UseEndpoints(endpoints =>
            {
                // Communication with gRPC endpoints must be made through a gRPC client.
                // To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909
                endpoints.MapGrpcService<BikesService>();
                if (env.IsDevelopment())
                {
                    endpoints.MapGrpcReflectionService();// this is an extra nuGet package to facilitate service auto-discover for gRPCUI
                }
            });
           
        }
    }
}
```
We can now test our gRPC BikeService. To do so, we are using a standalone tool called [gRPCUI](https://github.com/fullstorydev/grpcui), which is something like Postman for REST.
This tool uses the go language. If you don't have golang installed on your box you can [download it here](https://golang.org/dl/). The installer should also create an entry in the Path env variable pointing to the golang bin/ folder. Open up powershell ISE and execute this command:
```
go install github.com/fullstorydev/grpcui/cmd/grpcui@latest
```
Now we open another tab in powershell ISE. In the first tab we start our service with those commands we have sticked in our powershell profile

![Start the WS](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/ps1.png)

and in the second tab we start gRPCUI

![Start the debugger](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/ps2.png)

the latter command opens up a browser at http://127.0.0.1:59137/ where our web tester shows up auto-discovering our gRPC service

![Service Discovered](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/gRPCUI_1.png)

request sent..(don't be fooled by the response json format, it is there for readability. The real format is binary)

![request sent](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/gRPCUI_2.png)

Time to go back into the Blazor app and add the gRPC client

(...more to come..)

## Related links
- [gRPC](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-5.0) an absolute must-explore alternative to Rest services
  - [Sanderson's Blog](https://blog.stevensanderson.com/2020/01/15/2020-01-15-grpc-web-in-blazor-webassembly/)
  - [Newton's Annoucement](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-5.0)
  - [10 Steps to Replace REST Services with gRPC-Web](https://www.syncfusion.com/blogs/post/10-steps-to-replace-rest-services-with-grpc-web-in-blazor-webassembly.aspx)
  - [Adding gRPC to an existing service](https://docs.microsoft.com/en-us/aspnet/core/grpc/aspnetcore?view=aspnetcore-5.0&tabs=visual-studio#add-grpc-services-to-an-aspnet-core-app)
  - [grpcurl](https://github.com/fullstorydev/grpcurl/releases) A command line tool curl alike to test gRPC services.
   - [gRPC for .NET configuration](https://docs.microsoft.com/en-us/aspnet/core/grpc/configuration?view=aspnetcore-5.0)
  - [All in One](https://docs.microsoft.com/en-us/aspnet/core/grpc/httpapi?view=aspnetcore-5.0) 
    >gRPC HTTP API is an experimental extension for ASP.NET Core that creates RESTful JSON APIs for gRPC services. Once configured, gRPC HTTP API allows apps to call gRPC    services with familiar HTTP concepts

