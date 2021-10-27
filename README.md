# BikeShop
A thrown away blazor wasm client for consuming the [BikeDistributor library](https://github.com/mvit777/BikeDistributor)

This is actually my first try at building an [wasm blazor app](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-5.0). 
I was a bit skeptical at start, I expected a similar experience as building webapp with old days webforms. 
In fact the programming model (*) is very much the same but a lot smoother. 
You get a strong Déjà vu  feeling and start thinking that "...this is how webforms should have been back in the days..."

(*) after some more testing I realised this remark applies more to Blazor Server than Blazor wasm

Blazor is very young and I don't think it has a very large pool of potential users besides those developers 
who are not in love with javascript.

As far as I tested (which is not much) I noticed:
- project folder structure is clear and very customisable
- routing is a breeze. You just assign urls directly in the blazor partial
- templating and site wire-framing is truly quick
- services registration and DI are very similar to an aspnetcoreMVC project but a bit more messy 

In the meanwhile I got service registration working but ran into....

## my first big gotcha ##
After a a day of unsuccesful trying to connect to a MongoDb server I fully understood the implications of this sentence on Microsoft docs
`
...code is executing in the browser sandbox ....
`
this applies only to blazor wasm (not blazor server) but it means http is the only protocol that works which in turn means 
the whole System.Net namespace (with the exception of System.Net.Http) is not supported which in turn means no direct connection to Databases or sending mails unless I 
put up a webservice backend. That makes wasm not exactly the perfect candidate for testing the portability of my library...
Next time I'll make sure to better read the docs

## a nice findout: the HttpClient ##
Being forced to add a (bit redundant) Webservice to fill the gaps in the blazor app, I installed the RestSharp package (which is extremly popular these days), built a thin wrapper around it and stuffed everything into the BikeDistributor library. 
That was only to realise that also the RestSharp package unexpectedly relies on System.Net and therefore is also `not supported by the platform`.
After a brief search on the internet and a look at Program.cs I spotted the solution:
In fact the Program.cs bootstrap of an wasm app has this code by default:

```csharp
//instantiates the HttpClient and registers it as a service pointing the base url of the app itself
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
builder.Services.AddScoped(localClient => http);
//as weird as it looks this is the way it retrieves the appsettings.json configuration file
using var response = await http.GetAsync("appsettings.json");
using var stream = await response.Content.ReadAsStreamAsync();
builder.Configuration.AddJsonStream(stream);
```
In essence, the [HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0) is a relatively new class (at least for me) 
that is shipped by default with any recent .NET/NetCore release and I will probably use it in place of RestSharp for my next projects. 
Apart from being the only choice available, it favors ease of use (just like RestSharp) and features a lot of async methods by default.

Since my BikeShopWS is located at localhost:8021 all I had to do is create another instance pointing it to the ws url and register it as service as well
```csharp
var restBaseUrl = builder.Configuration.GetSection("BikeShopWS").GetValue<string>("baseUrl", "");
var restClient = new HttpClient { BaseAddress = new Uri(restBaseUrl) };
builder.Services.AddScoped(RestClient => restClient);
```
now to retrieve items from the database from a blazor page is as simple as this:
```csharp
... (omitted)
@inject HttpClient RestClient;
... (omitted)
@code {
private List<MongoEntityBike> EntityBikes;
protected override async Task OnInitializedAsync()
 {
    var jsonResponse = await RestClient.GetStringAsync("/bikes");
    EntityBikes = JsonUtils.DeserializeMongoEntityBikeList(jsonResponse);
 }
}
```
or even simplier if your entity can deserialize without any helper method
```csharp
EntityBikes = await RestClient.GetFromJsonAsync<List<MongoEntityBike>>("/bikes");
```
While resolving a few minor but annoying issues with serialization/deserialization of my objects, I realised that Blazor supports [gRPC](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-5.0) out-of-the-box (sort of).
An in-depth explanation of what it is and why it is good thing can be found in the links section at the bottom of this page. Apparently it is also possible 
to add support for gRPC to an existing Rest-WS api. I'll detail all the process required in a later paragraph as soon as I implement all the steps. 
In the meanwhile I want to move on the topic of templating and components.

## More Details on: Templating & Components ##
Blazor comes bundled with the Bootstrap css (in my case I found 4.3.1). It is not in the latest version but it is obviously trivial to point to the latest version or change the css framework or remove any css framework and start from scratch.
All changes should be made in ```wwwroot/index.html```. 

It does not come with the companion ```bootstrap.min.js``` since Blazor pictures itself as possible complete javascript 
replacement. That means some components will work as expected (Ex. tabs), while some will not (Ex. Modal).

As a matter of fact, I love Bootstrap, its components and JQuery and I don't have time/talent to re-invent the wheel so I promptly added ```jquery.min.js``` and ```bootstrap.min.js``` at the bottom of ```wwwroot/index.html```, together with [JQuery Datables](https://www.datatables.net/), which is another component that certainly does take a good amount of solid work to replicate in C#.

Blazor has already a rich ecosystem of commercial components but it does not come any close to what javascript can offer in terms of free/opensource ecosystem (and I doubt it will ever do). 
In the end what gets rendered in the browser is again just html + javascript and Blazor sports a jsinterop pipeline which allows for a two-way comunication between external javascript and its own components.
My aim here is to create a [small lib of components](https://github.com/mvit777/BikeShop/tree/master/BikeShop.BlazorComponents) that will automatise the output of parametrisable html structure of some Bootstrap components and the plumbing to external javascript manipulation when needed. Let's see what I achieved so far...[BikeShop.BlazorComponents](https://github.com/mvit777/BikeShop/tree/master/BikeShop.BlazorComponents)

(...[more details](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/README.md)...)

Armed with my first set of components, before implementing the full CRUD operations I want to...

## Shift from Rest to gRPC
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
//start kestrel 
function runBikeWs{
    Set-Location "C:\<YOUR_PATH_TO>\BikeShopWS\"
    dotnet run
}
//start gRPCUI
function debugBikeWs{
    grpcui localhost:5001
}
//recompile protos for BikeShop wasm
function compileProtos{
    Set-Location "C:\Users\Marcello\source\repos\Blazor\BikeShop\"
    dotnet run
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
the step of mapping a "complex" object caught me a bit unprepared and I'm not fully satisfied with the solution I found so far. I'll soon have a look at C# tooling to automatise the process and check if I'm on the right track. Anyway it seems to work.

The last step is enabling gRPC in ```Startup.cs``` 

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



## Last Paragraph: a quick note about the BikeShop WS ##
(...more to come...)

### Related and inspiring links ###
- [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-5.0) An introduction to Blazor
- [ezzylearning.net](https://www.ezzylearning.net/tutorials/blazor) has an entire section devoted to Blazor. I especially liked the tutorials on components
- [Blazor University](https://blazor-university.com/) is a very coincise and quick reference at every aspect of Blazor
- [Blazor Developer Italiani](https://blazordev.it/) a site entirely devoted to Blazor in the italian language. It features detailed and in-depth tutorials on the topic.
- [RestClient](https://github.com/MelbourneDeveloper/RestClient.Net) an alternative to default HttpClient package. I find it very interesting because it actually supports a lot of protocols not only http. It would be a nice addition in my own [MV.Framework library](https://github.com/mvit777/BikeDistributor/tree/master/MV.Framework)
- [gRPC](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-5.0) an absolute must-explore alternative to Rest services
  - [Sanderson's Blog](https://blog.stevensanderson.com/2020/01/15/2020-01-15-grpc-web-in-blazor-webassembly/)
  - [Newton's Annoucement](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-5.0)
  - [10 Steps to Replace REST Services with gRPC-Web](https://www.syncfusion.com/blogs/post/10-steps-to-replace-rest-services-with-grpc-web-in-blazor-webassembly.aspx)
  - [Adding gRPC to an existing service](https://docs.microsoft.com/en-us/aspnet/core/grpc/aspnetcore?view=aspnetcore-5.0&tabs=visual-studio#add-grpc-services-to-an-aspnet-core-app)
  - [grpcurl](https://github.com/fullstorydev/grpcurl/releases) A command line tool curl alike to test gRPC services.
   - [gRPC for .NET configuration](https://docs.microsoft.com/en-us/aspnet/core/grpc/configuration?view=aspnetcore-5.0)
  - [All in One](https://docs.microsoft.com/en-us/aspnet/core/grpc/httpapi?view=aspnetcore-5.0) 
    >gRPC HTTP API is an experimental extension for ASP.NET Core that creates RESTful JSON APIs for gRPC services. Once configured, gRPC HTTP API allows apps to call gRPC    services with familiar HTTP concepts

- [Messaging Center](https://github.com/aksoftware98/blazor-utilities) Messaging between unrelated components made it easy. A must-have nuget package. The author is also a very active member of the MS community and features a lot of learning material on his own site at https://ahmadmozaffar.net/Blog and at https://www.youtube.com/channel/UCRs-PO48PbbS0l7bBhbu5CA
- [DataTable.Blazor](https://github.com/JustinWilkinson/DataTables.Blazor) A very valuable effort to port JQuery DataTables entirely in C#
- [Implementing MVVM](https://www.syncfusion.com/blogs/post/mvvm-pattern-in-blazor-for-state-management.aspx) A must-read article from the same author of 10 Steps to Replace REST Services with gRPC-Web
- [List of Commercial Components libs](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/README.md#commercial-components-libraries) (This anchor link does not seem to work properly, just scroll at the bottom of the linked .md)

## News
[Microsoft Ignite](https://myignite.microsoft.com/home) November 2–4, 2021

