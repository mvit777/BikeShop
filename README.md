# BikeShop
A thrown away blazor wasm client for consuming the [BikeDistributor library](https://github.com/mvit777/BikeDistributor)

**(Note: the recent release of .NET6 brought a lot of new stuff for Blazor. As a consequence, some of the contents below is not entirely actual. In the next days I'll try to update what is relevant)**

This is actually my first try at building an [wasm blazor app](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-6.0). 
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

## Deploying the BikeShop webservice on a Linux server ##
If you can enjoy an active Azure subscription, the [App Service](https://docs.microsoft.com/en-us/azure/app-service/overview) is all you need.
I had the pleasure to use it for a paid project some time ago and it is truly great, integration with visual studio publish feature is also great.
Basically you will see the magic of .NET being a no-bullshit multi-platform development enviroment in just a bunch of clicks on both Visualstudio and Azure with no hassle at all.

For local testing I will instead use:
### plain [VirtualBox](https://www.oracle.com/virtualization/technologies/vm/downloads/virtualbox-downloads.html)
Once you have a VirtualBox, an Ubuntu Server 21.04 LTS guest with [guest additions](https://www.virtualbox.org/manual/ch04.html) installed, you might want to 
create a shared folder following this [excellent guide](https://gist.github.com/estorgio/0c76e29c0439e683caca694f338d4003), the guide is for outdated 18.04 but works smoothly with no modifications for 21.04.
Once you completed every step you should end-up with an auto-mounting folder called ```shared``` on the windows host and a mountpoint equally named on the Ubuntu guest.
If you also completed the last step, the Apache ```wwwroot``` should be sym-linked to the aforementioned ```shared``` folder.

The next step is installing the .netcore runtime on the Ubuntu guest. More information and options can be found on [MS site here](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu) but basically all you need to do is this:
```bash
#download and compile package
wget https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# and then install only the runtime since we are not developing on linux but just publishing
sudo apt-get update;
sudo apt-get install -y apt-transport-https && sudo apt-get update && sudo apt-get install -y aspnetcore-runtime-6.0

```
After this step we create a folder to contain our application and activate required modules for apache to act as a proxy
```bash
mkdir /home/<your username>/shared/bikews
sudo a2enmod proxy ssl headers
```
we now step back to VisualStudio and publish our app to the newly created folder bikews which is reachable from visualstudio since it is a shared folder
but before this step we have to instruct the webservice to use a proxy. To do so, we add this line at the very top of the ```Configure``` method in the ```Startup.cs``` file.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env){
 app.UseForwardedHeaders(new ForwardedHeadersOptions
 {
   ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
  });
 #(omitted code..)
}
```
Next, we can publish the webservice the shared folder. Since we have installed the .netcore runtime on the server, we can choose it as target.
Now we can configure an apache Virtualhost to act as entry point for our application. 
```bash 
sudo /etc/apache2/sites-available/bikews.conf
```
and stick this code in it. All requests to ```dev.bikews.com``` will be forwarded to ```http://127.0.0.1:5000```
```bash 
<VirtualHost *:*>
    RequestHeader set "X-Forwarded-Proto" expr=%{REQUEST_SCHEME}
</VirtualHost>

<VirtualHost *:80>
    ProxyPreserveHost On
    ProxyPass / http://127.0.0.1:5000/
    ProxyPassReverse / http://127.0.0.1:5000/
    ServerName dev.bikews.com
    ErrorLog ${APACHE_LOG_DIR}bikews-error.log
    CustomLog ${APACHE_LOG_DIR}bikews-access.log common
</VirtualHost>
```
Since ```dev.bikews.com``` is not a registered and existing domain we have to trick both the host machine and the ubuntu guest to believe it exists, to do so
we add this line to ```/etc/hosts``` on the linux guest
```
127.0.0.1 dev.bikews.com
```
and this line on ```C:\Windows\System32\drivers\etc\hosts```
```
# the following ip is the ip of the linux guest
192.168.1.134   dev.bikews.com
```
the last step is creating a service to manage the Kestrel process (the application server). 
For this purpose we create a service definition file
```bash
sudo nano /etc/systemd/system/kestrel-bikews.service
```
and slap this configuration inside it
```
[Unit]
Description=The bikeshop webservice

[Service]
WorkingDirectory=/var/www/html/bikews
ExecStart=/usr/share/dotnet/dotnet /var/www/html/bikews/BikeShopWS.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=bikeshopws
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production 

[Install]
WantedBy=multi-user.target
```
and then enable and start it
```bash
sudo systemctl enable kestrel-bikews.service
sudo systemctl start kestrel-bikews.service
# to check the status: should show "running"
sudo systemctl status kestrel-bikews.service
```
(...more to come...)


### Docker + VirtualBox
#### an alternative way to using a shared folder
Let's build a powershell script to compile and deploy via scp directly on a linux box created with the buzz-of-the-jour
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
- [Telerik .NET Web, Desktop & Mobile Products Webinar](https://www.telerik.com/campaigns/wb-telerik-r1-2022-release-webinar-web-desktop-mobile-products) Wednesday, February 2 I 11:00 am – 1:00 pm ET
- [Uno Conf 2021](https://unoconf.com/) November 30, 2021
>The Uno Platform is a UI Platform for building single-codebase applications for Windows, Web/WebAssembly, iOS, macOS, Android and Linux.
>It allows C# and Windows XAML code to run on all target platforms, while allowing you control of every pixel.
- [Women in tech 2021](https://womenintechfestivalglobal.com/womenintechfestivalglobal2021/en/page/home) November 22-23, 2021
- [Windows Office Hours](https://techcommunity.microsoft.com/t5/windows-events/windows-office-hours-november-18-2021/ev-p/2870707) November 18, 8-9a.m.
- [The Future of .NET](https://www.telerik.com/campaigns/wb-the-future-of-dotnet-webinar) Monday, November 15, 2021
- [NET Conf 2021](https://www.dotnetconf.net/) November 9, 2021

