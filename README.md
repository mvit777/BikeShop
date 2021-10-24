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
All changes should be made in ```wwwroot/index.html```. It does not come with the companion ```bootstrap.min.js``` since Blazor pictures itself as possible complete javascript 
replacement. That means some components will work as expected (Ex. tabs) some will not. 
As a matter of fact, I love Bootstrap, its components and JQuery and I don't have time/talent to re-invent the wheel so I promptly added ```jquery.min.js``` and ```bootstrap.min.js``` at the bottom, together with a [JQuery Datables](https://www.datatables.net/), which is another component that certainly does take a good amount of solid work to replicate in C#.
Blazor has already a good ecosystem of commercial components but it does not come any close to what javascript can offer in terms of free/opensource ecosystem (and I doubt it will ever do). 
In the end what gets rendered in the browser is again just html + javascript and Blazor sports a jsinterop pipeline which allows for a two-way comunication between external javascript and its own components.
My aim here is to create a [small lib of components](https://github.com/mvit777/BikeShop/tree/master/BikeShop.BlazorComponents) that will automatise the output of parametrisable html structure of some Bootstrap components and the plumbing to external javascript manipulation when needed. Let's see what I achieved so far...[BikeShop.BlazorComponents](https://github.com/mvit777/BikeShop/tree/master/BikeShop.BlazorComponents)

(...more to come...)

Armed with my first set of components, before implementing the full CRUD operations I want to...

## Shift from Rest to gRPC
since for the moment I want to keep co-existing both Api, REST and gRPC, I'm going to simply add gRPC on top of the existing [BikeShopWS](https://github.com/mvit777/BikeShop/tree/master/BikeShopWS) which I'm already publishing on my local IIS. According to [MS docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/supported-platforms?view=aspnetcore-5.0) ```IIS requires .NET 5 and Windows 10 Build 20300.1000 or later```. 
My aging Laptop is on Windows 10 build 19043 and not fully ready to upgrade to Windows 11. The only channel selectable on Window Insider shows the most recent build is currently 19044. So for the moment I'm out of lack with IIS. The quickest way I found to swap from IIS to Kestrel is the following:
Fire up PowerShell ISE (Powershell ISE is handy because it allows to open a multi-tab terminal), issue the command ```$profile``` to find out where the profile file is supposed to go.
```
PS C:\Windows\System32> $profile
C:\Users\<YOUR_USER>\OneDrive\Documents\WindowsPowerShell\Microsoft.PowerShellISE_profile.ps1

```
create the above file and add this custom command
```
function runBikeWs{
    Set-Location "C:\<YOUR_PATH_TO>\BikeShopWS\"
    dotnet run
}
```
close and re-open Powershell ISE.
Now when I want to compile, start and let BikeShopWS run, I simply issue:

```
runBikeWs
```
which run BikeShopWS on ports http 5000 and https 5001. In development no further certificate is needed because it is using the one of the REST api
```
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
I temporarily stop the service because now is finally time to add gRPC support. Following [docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-5.0), the first step is to add the NuGet meta-package  ```Grpc.AspNetCore```. Next step is adding a gRPC service. I want to replicate the **/bikes** GET url which returns the list of bikes from MongoDB.

(...more to come..)
TODO: add screenshot for gRPCUI debugging tool

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

### TOT (Totally Out of Topic) ###
After the Klitschko fight I called Tyson Fury a mediocre boxer that took advantage of an aging champion.
Well, not only he proved me wrong in every fight he fought since that, but last night he proved himself to be probaly one of the best Heavyweight ever.
Why am I writing this since the chances to meet him  in an airport :) are even lower than the guy giving a fuck for my remarks?
...I hate to get it wrong when it comes to boxing forecasts...
