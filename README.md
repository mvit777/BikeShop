# BikeShop
A thrown away blazor wasm client for consuming the [BikeDistributor library](https://github.com/mvit777/BikeDistributor)

**(Note: the recent release of .NET6 brought a lot of new stuff for Blazor. As a consequence, some of the contents below is not entirely actual. In the next days I'll try to update what is relevant)**

This is actually my first try at building an [wasm blazor app](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-5.0). 
I was a bit skeptical at start, I expected a similar experience as building webapp with old days webforms. 
In fact the programming model (*) is very much the same but a lot smoother. 
You get a strong Déjà vu  feeling and start thinking that "...this is how webforms should have been back in the days..."

(*) after some more testing I realised this remark applies more to [Blazor Server than Blazor wasm](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-5.0)

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
(this only applies to blazor wasm not blazor server) but it means http is the only protocol supported, which in turn means 
the whole System.Net namespace (with the exception of System.Net.Http) is not supported which in turn means no direct connection to Databases or sending mails unless I 
put up a webservice backend. As a cherry on the pie my crafted-with-love Config class accept only a path in the constructor (lol).
Next time I'll make sure to better read the [docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-6.0).
Jokes apart, most of the [MV.Framework](https://github.com/mvit777/BikeDistributor/tree/master/MV.Framework) stuff will be used by the webservice, so it is not useless.

## a nice findout: the HttpClient ##
Being forced to add a Webservice to fill the gaps in the blazor app, I installed the RestSharp package (which is extremly popular these days), built a thin wrapper around it and stuffed everything into the BikeDistributor library. 
That was only to realise that also the RestSharp package unexpectedly relies on System.Net and therefore is also `not supported by the platform`. (this seems to have changed lately)
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

What I would like to reach at some point is the following...
```csharp
var response = await bc.GetBikesAsync(new Google.Protobuf.WellKnownTypes.Empty());
EntityBikes = m.Map<List<MongoEntityBike>>(response.BikeEntities);
```

In the meanwhile I want to move on the topic of templating and components.

## More Details on: Templating & Components ##
Blazor comes bundled with the Bootstrap css (in my case I found 4.3.1). It is not in the latest version but it is obviously trivial to point to the latest version or change the css framework or remove any css framework and start from scratch.
All changes should be made in ```wwwroot/index.html```. 

It does not come with the companion ```bootstrap.min.js``` since Blazor pictures itself as possible complete javascript 
replacement. That means some components will work as expected (Ex. tabs), while some will not (Ex. Modal).

As a matter of fact, I love Bootstrap, its components and JQuery and I don't have time/talent to re-invent the wheel so I promptly added ```jquery.min.js``` and ```bootstrap.min.js``` at the bottom of ```wwwroot/index.html```, together with [JQuery Datables](https://www.datatables.net/), which is another component that certainly does take a good amount of solid work to replicate in C# (please see bottom links for an on-going very valuable attempt).

Blazor has already a rich ecosystem of commercial components but it does not come any close to what javascript can offer in terms of free/opensource ecosystem (and I doubt it will ever do). 
In the end what gets rendered in the browser is again just html + javascript and Blazor sports a jsinterop pipeline which allows for a two-way comunication between external javascript and its own components.
My aim here is to create a [small lib of components](https://github.com/mvit777/BikeShop/tree/master/BikeShop.BlazorComponents) that will automatise the output of parametrisable html structure of some Bootstrap components and the plumbing to external javascript manipulation when needed. Let's see what I achieved so far...[BikeShop.BlazorComponents](https://github.com/mvit777/BikeShop/tree/master/BikeShop.BlazorComponents)

(...[more details](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/README.md)...)

Armed with my first set of components, before implementing the full CRUD operations I want to talk about...

## the temporarily aborted shift [from Rest to gRPC](https://github.com/mvit777/BikeShop/blob/master/BikeShopWS/gRPC.md)
After a [week of protobuff-fever](https://github.com/mvit777/BikeShop/blob/master/BikeShopWS/gRPC.md) I decided, not without regret, to step back to REST. While the server part was quite easy to implement, and I'll keep it for a later stage, the client part implementation showed up some not-so-easy-solvable problems that weren't fixed by the othewise excellent [AutoMapper](https://automapper.org/) package. 
In particular, handling of null values and mapping of "grpc entities" and their very hard formal testability (don't know if this term makes sense) and some missing types made me think twice. 
While [Code first project](https://docs.microsoft.com/en-us/aspnet/core/grpc/code-first?view=aspnetcore-5.0) might come to rescue it has some drawbacks on its own.
Moreover, I suspect the naive implementation of my domain models and Mongo entities played a role in the above problems. Definetly time for a refactor and more study/planning before a second attempt. However, I'm definetly not giving up. I'll soon branch the project and keep trying. In the meanwhile, I moved the documentation of first attempt [here](https://github.com/mvit777/BikeShop/blob/master/BikeShopWS/gRPC.md) and I'll keep updating it. At least the server part will co-exist without problems.

## Backend
- Product Catalog builder
- Stats
- Order management and basic invoice
- Customers data
- Stock and Shipping
- Impersonation and accounts
- Customer behaviors lab
(...more to come..)

## Frontend
### Shopping cart
This is supposed to be the interesting part of the application as I'm planning it to be also the playground for the ai-training
(...more to come..)
### Bringin AI to the table
(...more to come...)

## Telemetry
(...more to come...)

## Last Paragraph: a quick note about the BikeShop WS ##
(...more to come...)

### Related and inspiring links ###
- [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-5.0) An introduction to Blazor
- [ezzylearning.net](https://www.ezzylearning.net/tutorials/blazor) has an entire section devoted to Blazor. I especially liked the tutorials on components
- [Blazor University](https://blazor-university.com/) is a very coincise and quick reference at every aspect of Blazor
- [Blazor Developer Italiani](https://blazordev.it/) a site entirely devoted to Blazor in the italian language. It features detailed and in-depth tutorials on the topic.
- [RestClient](https://github.com/MelbourneDeveloper/RestClient.Net) an alternative to default HttpClient package. I find it very interesting because it actually supports a lot of protocols not only http. It would be a nice addition in my own [MV.Framework library](https://github.com/mvit777/BikeDistributor/tree/master/MV.Framework)
- [Refit](https://reactiveui.github.io/refit/) Yet another alternative to default HttpClient

- [Messaging Center](https://github.com/aksoftware98/blazor-utilities) Messaging between unrelated components made it easy. A must-have nuget package. The author is also a very active member of the MS community and features a lot of learning material on his own site at https://ahmadmozaffar.net/Blog and at https://www.youtube.com/channel/UCRs-PO48PbbS0l7bBhbu5CA
- [DataTable.Blazor](https://github.com/JustinWilkinson/DataTables.Blazor) A very valuable effort to port JQuery DataTables entirely in C#
- [Implementing MVVM](https://www.syncfusion.com/blogs/post/mvvm-pattern-in-blazor-for-state-management.aspx) A must-read article

- [List of Commercial Components libs](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/README.md#commercial-components-libraries) (This anchor link does not seem to work properly, just scroll at the bottom of the linked .md)

## News
- [Telerik .NET Web, Desktop & Mobile Products Webinar](Wednesday, February 2 I 11:00 am – 1:00 pm ET) 90+ truly native Blazor components, 90+ components in the updated web design kits for Figma
- [Uno Conf 2021](https://unoconf.com/) November 30, 2021
>The Uno Platform is a UI Platform for building single-codebase applications for Windows, Web/WebAssembly, iOS, macOS, Android and Linux.
>It allows C# and Windows XAML code to run on all target platforms, while allowing you control of every pixel.
- [Women in tech 2021](https://womenintechfestivalglobal.com/womenintechfestivalglobal2021/en/page/home) November 22-23, 2021
- [Windows Office Hours](https://techcommunity.microsoft.com/t5/windows-events/windows-office-hours-november-18-2021/ev-p/2870707) November 18, 8-9a.m.
- [The Future of .NET](https://www.telerik.com/campaigns/wb-the-future-of-dotnet-webinar) Monday, November 15, 2021
- [NET Conf 2021](https://www.dotnetconf.net/) November 9, 2021

