# BikeShop.BlazorComponents
TODO: clean of unused files and ...still work in progress
## Install
Make sure to install the excellent nuget-package [Messaging Center](https://github.com/aksoftware98/blazor-utilities) at solution level
```
Install-Package AKSoftware.Blazor.Utilities
```
Once you also have imported the BikeShop.BlazorComponents.dll into your Blazor project, just add the two lines at the bottom of _Imports.razor

```razor
//(...omitted..)
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using AKSoftware.Blazor.Utilities
@using BikeShop.BlazorComponents.Components
```
Now navigate to the ```wwwroot``` folder and add a file ```interop.js``` or whatever name it suits you. Make sure to include datatables.css in the head tag in index.html
together with jquery.js, bootstrap.min.js, datatables.min.js (in this order) before the closing </body>

*wwwroot/index.html*
```html
<html>
  <head>
    (...omitted...)
    <!-- Bootstrap CSS -->
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" integrity="sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T" crossorigin="anonymous"> 
    <!-- datatables -->
    <link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/v/bs4/dt-1.11.3/b-2.0.1/b-colvis-2.0.1/b-html5-2.0.1/cr-1.5.4/date-1.1.1/fc-4.0.0/fh-3.2.0/kt-2.6.4/r-2.2.9/rg-1.1.3/rr-1.2.8/sc-2.0.5/datatables.min.css" />
    <!-- local -->
    <link href="css/app.css" rel="stylesheet" />
    <!-- <link href="BikeShop.styles.css" rel="stylesheet" /> -->
  </head>
  <body>
    (...omitted...)
    
   <script src="https://code.jquery.com/jquery-3.3.1.slim.min.js" integrity="sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo" crossorigin="anonymous"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" integrity="sha384-UO2eT0CpHqdSJQ6hJty5KVphtPhzWj9WO1clHTMGa3JDZwrnQq4sF86dIHNDz0W1" crossorigin="anonymous"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" integrity="sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>
    <script type="text/javascript" src="https://cdn.datatables.net/v/bs4/dt-1.11.3/b-2.0.1/b-colvis-2.0.1/b-html5-2.0.1/cr-1.5.4/date-1.1.1/fc-4.0.0/fh-3.2.0/kt-2.6.4/r-2.2.9/rg-1.1.3/rr-1.2.8/sc-2.0.5/datatables.min.js"></script>
    <script src="js/interop.js"></script>
</body>
```
now stick this code in the ```wwwroot/js/interop.js``` file

```javascript
//define namespace for bootstrap components
var bootstrapNS = {};
//register the helper functions in the namespace for bootstrap components
(function () {
    this.ToggleModal = function (modal, mode) {
        $(modal).modal(mode);
    }
    this.JSDataTable = function (table, options) {
        //this will take care we not recreate the table
        if (!$.fn.dataTable.isDataTable(table)) {
            $(table).DataTable(options);
        }
    }
}).apply(bootstrapNS);
```
So far this is the only code needed to make Blazor interact with the Bootstrap Modal and JQuery Datatables. 
All the other components I wrapped inside my library can either work without javascript (Ex. Tabs) or can just be activated and interacted by C# only. 
Let's have a closer look...

## Brief description of the components
Imagine we want to build the classic product list table with links for creating/editing/deleting items..
The first component we need is a simple [HTMLTable](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/HtmlTable.cs) that accept a data source an some properties (like id o css class) and its companion [template file](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/HtmlTable.razor).
Since at the start I was not very familiar with components I decided to always have two separate files which makes code a lot cleaner. Right now I regret a bit this choice because stuffing all in the xxxx.razor file in the end is quicker a more compact. Anyway here how it looks externally on some page's code.

*Parental Warning: A lot of code stolen from [Developing a Component Library](https://www.ezzylearning.net/tutorial/a-developers-guide-to-blazor-component-libraries)*
```razor
@page "/somepage"
@inject IConfiguration Configuration;
@inject HttpClient RestClient;
@inject IJSRuntime JSRuntime;

<HtmlTable Items="EntityBikes" Context="EntityBike" HTMLId="BikeList">
        <HeaderTemplate>
            <th>Model</th>
            <th>Brand</th>
            <th>Type</th>
            <th>Tot. Price</th>
        </HeaderTemplate>
        <RowTemplate>
            <td>@EntityBike.Bike.Model</td>
            <td>@EntityBike.Bike.Brand</td>
            <td>
                @(EntityBike.Bike.isStandard ? "Standard" : "Custom")
            </td>
            <td>@EntityBike.TotalPrice</td>
        </RowTemplate>
    </HtmlTable>

@code{
private List<MongoEntityBike> EntityBikes;
protected override async Task OnInitializedAsync()
 {
    EntityBikes = await RestClient.GetFromJsonAsync<List<MongoEntityBike>>("/bikes");
 }
}
```
this will output a normal html table, to turn it into a JQuery Datable we have to call the function we have set in previous paragraph into the interop.js file
```razor
(..omitted..)
@code{
private List<MongoEntityBike> EntityBikes;
protected override async Task OnInitializedAsync()
 {
    EntityBikes = await RestClient.GetFromJsonAsync<List<MongoEntityBike>>("/bikes");
 }
 protected async override Task OnAfterRenderAsync(bool firstRender)
    {

        await JSRuntime.InvokeVoidAsync("bootstrapNS.JSDataTable", "#BikeList", new object[] { });
    }
}
```
Next we want to add an edit button for every row. Here comes in play the [Button component](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Button.razor) and yet again [his associated class](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Button.cs)
So we add it in our table definition on somepage:
```razor
@page "/somepage"
@inject IConfiguration Configuration;
@inject HttpClient RestClient;
@inject IJSRuntime JSRuntime;

<HtmlTable Items="EntityBikes" Context="EntityBike" HTMLId="BikeList">
        <HeaderTemplate>
            (...omitted...)
            <th>Tot. Price</th>
            <th>Actions</th>
        </HeaderTemplate>
        <RowTemplate>
           (...omitted...)
            <td>@EntityBike.TotalPrice</td>
            <td>
            <Button HTMLId="@EntityBike.Id" HTMLCssClass="btn-primary btn-sm" Icon="oi oi-pencil" Label="EDIT" ClickEventName="BikeList_editIemClick" />
            </td>
        </RowTemplate>
</HtmlTable>
    (...omitted...)
```
which results in our blue edit button. If we take a closer look the most relevant properties are ```HTMLId```, to retrieve the entity we want to edit, and the ```ClickEventName``` which will broadcast the Event ```BikeList_editIemClick``` globally (courtesy of the afore-mentioned [Messaging Center](https://github.com/aksoftware98/blazor-utilities)) and can be consumed by any component on the page.

I named the ```ClickEventName``` as ```BikeList_editIemClick``` but it can have any name. If have you looked into [button associated class](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Button.cs) you might have noticed this method:

*Button.cs*
```csharp
(...omitted..)
public virtual void SendMessage(){
    string valueToSend = HTMLId;
    MessagingCenter.Send(this, ClickEventName, valueToSend);
}
```
which gets triggered by the ```@onclick="SendMessage"``` handler that I sticked on the html button inside the component template

*Button.razor*
```razor
<button type="button" class="btn @HTMLCssClass" id="@HTMLId"  @onclick="SendMessage" @onclick:preventDefault="PreventDefault">
    @if (Icon != "") {
    <span class="@Icon">&nbsp;</span>
    }    
     @Label
</button>
```
A very cool feature of Messaging Center is that it does not need to be instantiated nor injected somewhere nor registered in a service. It is just there ready to be used everywhere you need it, be it a page, a component or a class. And to provide any part of the system a mean to comunicate. 
Now, we just have to register one or more parts of our system to listen and react when the above-mentioned ```BikeList_editIemClick``` event gets broadcasted.
Let's say we want to recieve the event on the List of products and open a popup to edit the clicked item. To do so we have first to subscribe our page to the event:

```razor
@page "/somepage"
(..omitted..)
@code{
private List<MongoEntityBike> EntityBikes;

public void SubscribeToEditItemClick()
    {
        MessagingCenter.Subscribe<Button, string>(this, "BikeList_editIemClick", (sender, value) =>
        {
        // Do actions against the value
        selectedId = value;
        //we retrieve the full object from our existing list without a trip to the database
        var MongoEntity = EntityBikes.AsQueryable<MongoEntityBike>().Where(x => x.Id == selectedId).SingleOrDefault();
        //we tell the bootstrp modal to show up
        JSRuntime.InvokeVoidAsync("bootstrapNS.ToggleModal", "#EditBikeModal", "show");
        // If the value is updating the component make sure to call StateHasChanged
        StateHasChanged();
        });
    }


protected override async Task OnInitializedAsync()
 {
    EntityBikes = await RestClient.GetFromJsonAsync<List<MongoEntityBike>>("/bikes");
    SubscribeToEditItemClick();//we subscribe to event here
 }
 
 (...omitted...)
```
Note that I call the ```SubscribeToEditItemClick``` at the end of the ```OnInitializedAsync()``` routine, I have not ye investigated the topic but it seems trying to register the same event more than once is smoothly managed by Messaging Center itself, no checks seem to be required.

The last step is adding the [Modal Component](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Modal.cs) to the page.
```razor
@page "/somepage"
@inject IConfiguration Configuration;
@inject HttpClient RestClient;
@inject IJSRuntime JSRuntime;
(..omitted..)
<!-- HIDDEN EDIT MODAL -->
    <Modal HTMLId="EditBikeModal" HeaderTitle="EDIT" HTMLCssClass="modal-md" ShowFooter="false">
        <HeaderTemplate>
            <h5 class="modal-title" id="editBikeModalH5"><span class="oi oi-pencil"></span> Editing Bike... @selectedId</h5>
            <span class="rounded-circle  light-purple-bg" style="background-color: white;">
                <button type="button" class="close" @onclick="CloseEditBikeModal" data-dismiss="modal" aria-label="Close" style="margin-right: -2px;">
                    <span aria-hidden="true">&times;</span>
                </button>
            </span>
        </HeaderTemplate>
        <ChildContent>
           here goes the content of the modal
        </ChildContent>
    </Modal>
<HtmlTable Items="EntityBikes" Context="EntityBike" HTMLId="BikeList">
        <HeaderTemplate>
            (...omitted...)
            <th>Tot. Price</th>
            <th>Actions</th>
        </HeaderTemplate>
        <RowTemplate>
           (...omitted...)
            <td>@EntityBike.TotalPrice</td>
            <td>
            <Button HTMLId="@EntityBike.Id" HTMLCssClass="btn-primary btn-sm" Icon="oi oi-pencil" Label="EDIT" ClickEventName="BikeList_editIemClick" />
            </td>
        </RowTemplate>
</HtmlTable>
 
    (...omitted...)
```
Should we need a larger Modal, we just set the ```HTMLCssClass``` to something like ```modal-xl```.

Inside the ```ChildContent``` tag of the modal we can now put an [EditForm](https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-5.0).
EditForm is a very useful **built-in component of Blazor** which alleviates the *tedium* of designing and binding a form. Let's see how we can use it
```razor
(...omitted.)
<!-- HIDDEN EDIT MODAL -->
    <Modal HTMLId="EditBikeModal" HeaderTitle="EDIT" HTMLCssClass="modal-md" ShowFooter="false">
        <HeaderTemplate>
            <h5 class="modal-title" id="editBikeModalH5"><span class="oi oi-pencil"></span> Editing Bike... @selectedId</h5>
            <span class="rounded-circle  light-purple-bg" style="background-color: white;">
                <button type="button" class="close" @onclick="CloseEditBikeModal" data-dismiss="modal" aria-label="Close" style="margin-right: -2px;">
                    <span aria-hidden="true">&times;</span>
                </button>
            </span>
        </HeaderTemplate>
        <ChildContent>
           <EditForm EditContext="@EditContext" class="row p-3">
                <div class="col-md-6 mb-3">
                    <label for="Model">Model</label>
                    <InputText id="Model" @bind-Value="ProductModel.Model" class="form-control" />
                </div>
                <div class="col-md-6 mb-3">
                    <label for="BasePrice">Price</label>
                    <InputNumber id="BasePrice" @bind-Value="ProductModel.BasePrice" class="form-control" />
                </div>
                <div class="col-md-12 mb-3">
                    <label for="Description">Description</label>
                    <InputTextArea id="Description" @bind-Value="ProductModel.Description" class="form-control" />
                </div>
                <div class="col-12 mb-3">
                    <div class="form-check">
                        <InputCheckbox id="IsStandard" @bind-Value="ProductModel.isStandard" class="form-check-input" />
                        <label class="form-check-label" for="IsStandard">
                            Standard
                        </label>
                    </div>
                </div>
                <div class="col-12 mb-3">
                    <button type="submit" class="btn btn-primary" @onclick="SaveProduct">Submit</button>
                </div>
            </EditForm>
        </ChildContent>
    </Modal>
    (...omitted...)
```
The interesting parts of the EditForm component are 
- the opening tag property ```EditContext="@EditContext"```
- the ```@bind-Value="ProductModel.XXX"``` property of every field
- the final handler ```@onclick="SaveProduct"``` on the submit button. 

The rest is just a regular form.
The [EditContext](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.components.forms.editcontext?view=aspnetcore-5.0) recieves a model object and takes care 
of tracking which fields are modified and field validation (you can read much detailed infos on the link I provided to MS Docs).
We already had a handler in place which is triggerd when some edit button is clicked. Now it is time to add the missing parts...
```razor
(..omitted code..)
@code{
private List<MongoEntityBike> EntityBikes;
private Bike ProductModel = new Bike(); // WE CREATE AN EMPTY Bike INSTANCE MAINLY TO HANDLE THE INSERT NEW CASE
private EditContext EditContext; // WE DECLARE AN EditContext

//we have registered this handler in the OnTaskInitialzed routine
public void SubscribeToEditItemClick()
    {
        MessagingCenter.Subscribe<Button, string>(this, "BikeList_editIemClick", (sender, value) =>
        {
        // Do actions against the value
        selectedId = value;
        //we retrieve the full object from our existing list without a trip to the database
        var MongoEntity = EntityBikes.AsQueryable<MongoEntityBike>().Where(x => x.Id == selectedId).SingleOrDefault();
        ProductModel = MongoEntity.Bike; //WE ASSIGN ProductModel THE SELECTED ITEM
        //we tell the bootstrp modal to show up
        JSRuntime.InvokeVoidAsync("bootstrapNS.ToggleModal", "#EditBikeModal", "show");
        // If the value is updating the component make sure to call StateHasChanged
        StateHasChanged();
        });
    }

protected override async Task OnInitializedAsync()
    {

        EditContext = new EditContext(ProductModel); // WE ASSIGN THE MODEL TO THE EditContext
        //(...omitted code..)
        SubscribeToEditItemClick(); // WE SUBSCRIBE TO THE BikeList_editIemClick  EVENT 
        //(...omitted code..)
    }
```
Now the Modal should show with all fields populated. Finally, we have to add the ```SaveProduct``` handler which mainly consists of sending 
the modified Product to the BikeShopWS in order to store it in the database.

(More to come)

**What about asking for confirmation? AKA the delete button**

In the case of the delete button we want the user to confirm the action before going on with the deletion. In this case the bootstrap Alert, wrapped into 
the [Alert component](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Alert.razor) might come handy.
Let's see how....
(More to come)

## The Resulting Stuff (so far)
>*The list of products* [(actual source code)](https://github.com/mvit777/BikeShop/blob/master/BikeShop/Shared/Components/admin/AdminProductList.razor)
![List](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/BikeListComplete.png)
>*A pop up for editing a product*
![Prodcut Edit](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/BikeEditPopUp.png)

>*Change user from user box*
![User Box](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/BoxUser.png)
>*List users*
![Users List](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/UsersList.png)

## Conclusion
I'm no frontend guy and have no significant experience of developing with Angular/React/VueJs, so I'm not very qualified to make a comparison between these tools.
However I must say Blazor is very to fast to learn, especially if you have a background in previous or other MS techs. I would also say that for SPA applications, Blazor can be an interesting competitor. The main problems I see are:
- very little free/opensource ecosystem compared to javascript competitors
- demographics of developers. Young developers tend to favor javascript as it is all the rage these days. Aging developers have been using javascript for years now, they might not be so tempted to learn yet another tool (unless they really dislike javascript).

On the positive side, Blazor integrates really smoothly with Bootstrap unlike the three other tools mentioned above. This is a feature not to understimate. 
All in all, I'm satisfied so far with my little library. It is nothing more than a light wrapper around Bootstrap components to automatise some HTML but it seems to work well and was very quick to develop. In the past I once tried to develop such a library in pure javascript and another time with PHP + Twig.
The javascript/jquery attempt was a complete failure up to the point I abandoned it in a very early stage. It had bugs scattered all around and was bloated from the very start.
Basically doing such a thing requires a non-trivial knowdledge of javascript far beyond my level. Also the fact that I'm not aware of an existing such a library makes me think it is not a good idea. The PHP+Twig was much more successful, in the sense that I re-used it in many projects. The main problem of that solution was that once the serverside was executed, I was left alone yet again with a lot of AJAX setup and DOM manipulation.
In Blazor you just forget about AJAX setup and DOM manipulation, as they are run under the covers. This is another good point for Blazor. 
The aspect of inter-mixed html and code is a very handy but reminds me a bit of old style Wordpress, which I don't like very much. I still have to make a decision about 
shifting towards code-behind file + template file style or mix both. The impression is at some point I will use the former as it favours more consistency even if is a lot more verbose. 
Another good point of Blazor components is their declarative style (just like Webforms, Coldfusion or defunct Struct etc etc)
In fact, if you have a very predictable page structure (like my Datatable List + Edit form) building a code generator to automatise the skeleton of many pages becomes a lot easier.


## Related links
- [Messaging Center](https://github.com/aksoftware98/blazor-utilities) Messaging between unrelated components made it easy. A must-have nuget package. The author is also a very active member of the MS community and features a lot of learning material on his own site at https://ahmadmozaffar.net/Blog and at https://www.youtube.com/channel/UCRs-PO48PbbS0l7bBhbu5CA


- [ezzylearning](https://www.ezzylearning.net/tutorials/blazor): (lot of "inspiration" from following links)
    - [Beginner's guide to Components](https://www.ezzylearning.net/tutorial/a-beginners-guide-to-blazor-components)
    - [Templated Component](https://www.ezzylearning.net/tutorial/a-developers-guide-to-blazor-templated-components)
    - [Developing a Component Library](https://www.ezzylearning.net/tutorial/a-developers-guide-to-blazor-component-libraries)

- [EditForm](https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-5.0) Bible definition
  - [ezzylearning tutorial on forms and validation](https://www.ezzylearning.net/tutorial/a-guide-to-blazor-forms-and-validation)

### Commercial Components libraries
As usual for MS stack there is already a big ecosystem of commercial products backing Blazor. If you want to stay on the safe path, here is a work-in-progress list of commercially supported Components libraries:
- [Telerik](https://demos.telerik.com/blazor-ui)
- [Syncfusion](https://www.syncfusion.com/blazor-components)
- (..more to come..)
