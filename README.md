# BikeShop
A thrown away blazor wasm client for the bikeDistributor library

This is actually my first try at building an wasm blazor app. 
I was a bit skeptical at start, I expected a similar experience as building webapp with old days webforms. 
In fact the programming model is very much the same but a lot smoother. 
You get a strong Déjà vu  feeling and start thinking that "...this is how webforms should have been back in the days..."

Blazor is very young and I don't think it has a very large pool of potential users besides those developers 
who are not in love with javascript.

As far as I tested (which is not much) I noticed:
- project folder structure is clear and very customisable
- routing is a breeze. You just assign url directly in the blazor partial
- templating and site wire-framing is truly quick
- services registration and DI are very similar to an aspnetcoreMVC project but a bit more messy 

got service registration working but ran into....

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
In fact the Program.cs bootstrap of an wasmp app has this code by default:

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
that is shipped by default with any recent .NET/NetCore release and I will probably use it in place of RestSharp. 
Apart from being the only choice available, it favors ease of use (just like RestSharp) and features a lot async methods by default.

Since my BikeShopWS is located at localhost:8021 all I had to do is create another instance pointing it to the ws url and register it as service as well
```csharp
var restBaseUrl = builder.Configuration.GetSection("BikeShopWS").GetValue<string>("baseUrl", "");
var restClient = new HttpClient { BaseAddress = new Uri(restBaseUrl) };
builder.Services.AddScoped(RestClient => restClient);
```
now to retrieve items from the database from a blazor page it is simple as this (sort of):
```csharp
... (omitted)
@inject HttpClient RestClient;
... (omitted)
@code {
private List<MongoEntityBike> EntityBikes;
protected override async Task OnInitializedAsync()
 {
    var jsonResponse = await RestClient.GetStringAsync("/bike");
    EntityBikes = JsonUtils.GetListFromJArrayBikeEntities(jsonResponse);
 }
}
```
or even simplier if your entity can deserialize without any helper method
```csharp
EntityBikes = await RestClient.GetFromJsonAsync<List<MongoEntityBike>>("/bike");
```
(...more to come...)
