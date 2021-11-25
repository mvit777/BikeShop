# BikeShop.BlazorComponents
TODO: clean of unused files and ...still work in progress
**(Note: the [recent release of .NET6](https://devblogs.microsoft.com/dotnet/announcing-net-6/) brought a lot of new stuff for Blazor. As a consequence, some of the contents below is not entirely actual. In the next days I'll try to update what is relevant)**

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
===================================WARNING======================================

(section updated to reflect .NET 6 new features. Please scroll down to the MultiSelect component to see a smarter way to init the components)

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
    <link href="BikeShop.styles.css" rel="stylesheet" />
  </head>
  <body>
    (...omitted...)
    
    <script src="https://code.jquery.com/jquery-3.3.1.min.js" integrity="sha256-FgpCb/KJQlLNfOu91ta32o/NMZxltwRo8QtmkMRdAu8=" crossorigin="anonymous"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" integrity="sha384-UO2eT0CpHqdSJQ6hJty5KVphtPhzWj9WO1clHTMGa3JDZwrnQq4sF86dIHNDz0W1" crossorigin="anonymous"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" integrity="sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.1.36/pdfmake.min.js"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.1.36/vfs_fonts.js"></script>
    <script type="text/javascript" src="https://cdn.datatables.net/v/bs4/jszip-2.5.0/dt-1.11.3/b-2.0.1/b-colvis-2.0.1/b-html5-2.0.1/b-print-2.0.1/date-1.1.1/sc-2.0.5/sb-1.3.0/sp-1.4.0/sl-1.3.3/datatables.min.js"></script>

    <!-- <script src="./_content/BikeShop.BlazorComponents/MVComponents.js"></script>-->
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
    this.ToggleToast = function (toast, options) {
        $(toast).toast(options);
    }
    this.JSDataTable = function (table, options) {
        //this will take care we not recreate the table
        if (!$.fn.dataTable.isDataTable(table)) {
            $(table).DataTable(options);
        }
    }
    this.RefreshJSDataTable = function (table, options) {
        $(table).dataTable().fnDestroy();
    }
}).apply(bootstrapNS);
```
==============================END OF UPDATED SECTION =================================

So far this is the only code needed to make Blazor interact with the Bootstrap Modal and JQuery Datatables. 
All the other components I wrapped inside my library can either work without javascript (Ex. Tabs) or can just be activated and interacted by C# only. 
Let's have a closer look...

## Brief description of the components
Imagine we want to build the classic product list table with links for creating/editing/deleting items..
The first component we need is a simple [HTMLTable](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/HtmlTable.cs) that accept a data source and some properties (like id o css class) and its companion [template file](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/HtmlTable.razor).
Since at the start I was not very familiar with components I decided to always have two separate files which makes code a lot cleaner. Right now I regret a bit this choice because stuffing all in the xxxx.razor file in the end is quicker a more compact. Anyway here how it looks externally on some page's code.

### The HTMLTable component
(also featuring the [obbligatory spinning stuff](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Spinner.razor))

*Parental Warning: A lot of code stolen from [Developing a Component Library](https://www.ezzylearning.net/tutorial/a-developers-guide-to-blazor-component-libraries)*
```razor
@page "/somepage"
@inject IConfiguration Configuration;
@inject HttpClient RestClient;
@inject IJSRuntime JSRuntime;
@if (EntityBikes == null)
{
    <Spinner /> <!-- some spinning stuff -->

}
else
{
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
}
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
### The Button component

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

### The Modal component

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
                <DataAnnotationsValidator />
                <ValidationSummary />
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
The interesting parts of the built-in EditForm component are 
- the opening tag property ```EditContext="@EditContext"```
- the ```@bind-Value="ProductModel.XXX"``` property of every field
- the basic data validation applied directly on model class via data annotations
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

### The multipurpose Alert component

**What about asking for confirmation? AKA the delete button**

In the case of the delete button we want the user to confirm the action before going on with the deletion. In this case the bootstrap Alert, wrapped into 
the [Alert component](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Alert.razor) might come handy.
Bootstrap Alert is probably the most straight-forward component in terms of both html-markup and functionality, however we can easily add some interesting stuff:

- ability to use the same component instance on the page for different purposes (Ex. show an info alert while editing, show a confirm alert before deleting)
- optional auto-closing based on configurable duration
- a button to dismiss it at any time

the three points above imply that we change component properties after it is rendered. This technique is strongly discouraged by MS as it can introduce inconsistencies in the render tree. In fact you CANNOT do something like this:
```csharp
MainAlert.HtmlCssClass = "alert-secondary"; //will not compile
```
but you can change it via a method
```csharp
MainAlert.ChangeCssClass("alert-secondary"); //works
```
In my experience, with a little care and a ```StateHasChanged``` call nested in the component it will work smoothly.

the component template looks like this
```csharp
@if (Visible)
{
    <div class="alert @HTMLCssClass" role="alert">
        <button type="button" class="close" aria-label="Close" @onclick="onCloseClicked">
            <span aria-hidden="true">&times;</span>
        </button>
        @ChildContent
    </div>
}
@code {
    private void onCloseClicked() => NotifyTimerElapsed(this, null);
}
```
the code-behind is this:
```csharp
public partial class Alert
    {
        [Parameter]
        public virtual string HTMLId {get; set;}
        [Parameter]
        public virtual string HTMLCssClass { get; set; } = "alert-primary";
        [Parameter]
        public virtual double AutoFade { get; set; } = 0;
        [Parameter]
        public virtual RenderFragment ChildContent { get; set; }
        [Parameter]
        public virtual bool Visible { get; set; } = false;

        private System.Timers.Timer _timer;
        public void ChangeVisible(bool visible, bool executeStateHasChanged = false)
        {
            Visible = visible;
            if (Visible)
            {
                if(AutoFade > 0)
                {
                    _timer = new System.Timers.Timer(AutoFade);
                    _timer.Elapsed += NotifyTimerElapsed;
                    _timer.Enabled = true;
                }
            }
            
        }
        public event Action OnElapsed;
        private void NotifyTimerElapsed(Object source, ElapsedEventArgs e)
        {
            OnElapsed?.Invoke();
            Visible = false;
            _timer.Dispose();
            AutoFade = 0;
            StateHasChanged();
        }
        public void ChangeCssClass(string cssClass, bool executeStateHasChanged = false)
        {
            HTMLCssClass = cssClass;
        }
        public void SetAutoFade(double autofade)
        {
            if(_timer!=null)
                NotifyTimerElapsed(this, null);//reset the timer
            AutoFade = autofade;
        }
    }
```
from the perspective of the developer using the component on some page it looks like this:

```csharp
//(..code omitted..)
<Alert HTMLId="MainAlert" @ref="MainAlert" AutoFade=3000><!-- will auto fade in 3 secs -->
        @message<span>&nbsp;</span>
        @if (showConfirmButton)
        {
            <Button HTMLId="btnConfirmDelete" HTMLCssClass="btn-primary" Icon="oi oi-thumb-up" Label="Delete It!" ClickEventName="BikeList_OnDeleteConfirmed" />
            <br />
        }
</Alert>
//(..code omitted..)
@code {
    private List<MongoEntityBike> EntityBikes;
    //(code omitted)
    private Alert MainAlert;//notice the reference to the component @ref property
    private bool showConfirmButton = false;
    //(code omitted)
    //this will show our alert as an alert-danger with a confirm button and will auto close in 10 secs
    public void SubscribeToDeleteItemClick()
    {
        MessagingCenter.Subscribe<Button, string>(this, "BikeList_deleteItemClick", (sender, value) =>
        {
            showConfirmButton = true;
            message = $"Please confirm you want to delete item {value}";
            MainAlert.ChangeCssClass("alert-danger"); //here we change alert style at runtime
            MainAlert.SetAutoFade(10000); //here we change duration. It will auto fade in 10 secs
            MainAlert.ChangeVisible(true);//change visibility
            //or we can just group in a single method call like this:
            //MainAlert.ShowAsDanger(10000);
            deletableObjId = value;

            StateHasChanged();//probably not required but no roundtrip to the server as it is a wasm application
        });
    }
    //this will show our alert as an alert-info and it will autoclose in 3 secs
    public void SubscribeToEditItemClick()
    {
        MessagingCenter.Subscribe<Button, string>(this, "BikeList_editItemClick", (sender, value) =>
        {
            selectedId = StringHelper.NormaliseStringId(value, "_editButton");
            showConfirmButton = false;
            message = $"You are editing {selectedId}";
            MainAlert.ChangeCssClass("alert-info");
            MainAlert.ChangeVisible(true);
            //or we can just group in a single method call like this:
            //MainAlert.ShowAsInfo(3000);

           // ...code omitted....

            StateHasChanged();//probably not required but no roundtrip to the server as it is a wasm application
        });
    }
```
should we not need auto-closing we just omit the AutoFade property or just set its value to 0.

### the Toast component
After submitting a newly created bike or updating an existing one we want to give the user a visual feedback that something happened. For the purpose we could once again use our alert giving it the alert-success class turning it into a toast-like message. But since recent releases of Bootstrap ship with a proper Toast component this is what you are going to use.
As usual we begin wrapping Bootstrap Toast component in our [own component](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Toast.razor) and [code behind](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/Toast.cs). 

Since I'm sure this component is gonna be used by almost any "page" through out the app, I want to make it globally available. The most straight-forward way I found to do so is putting it somewhere in the ```MainLayout.razor``` and give it an absolute positioning (todo: refine positioning). Note the addition of the ```CascadingValue``` tag that automatically injects a reference to the ```MainLayout``` in any component/page
```razor
@inherits LayoutComponentBase
(...code omitted...)
<div style="position: absolute; width: 350px; height: 90px; top:60px; right: 0;">
    <Toast HTMLId="MainToast" Title="Toast" Message="Message" @ref="MainToast" /><!-- also note the @ref property here -->
</div>
<CascadingValue Value="this"><!-- here the "magic" CascadingValue tag -->
    <div class="page">
        <div class="sidebar shadow">
            <NavMenu />
        </div>

        <div class="main">
            <div class="top-row px-4">
                <UserBox />
            </div>
            <div class="content px-4">
                @Body
            </div>
        </div>
      (..code omitted..)
    </div>
</CascadingValue>
@code{
 //(omitted...)
 public async Task PopulateMainToastAsync(string title, string message, string cssClass, string icon = "oi oi-info")
    {
        MainToast.RefreshComponent(title, message, icon, cssClass);
        await JSRuntime.InvokeVoidAsync("bootstrapNS.ToggleToast", "#MainToast", "show");
        StateHasChanged();
    }
}
```
now in the ```AdminProductList``` we can get the reference by doing this

```razor
@code{
    //code omitted
    [CascadingParameter]
    public MainLayout Layout { get; set; }
    
    private async Task HandleSubmit()
    {
      try{
        //code omitted
        await Layout.PopulateMainToastAsync("Operation result", "bike update!", "alert-success", "oi oi-circle-check");
      }catch(Exception ex){
        await Layout.PopulateMainToastAsync("Operation result", ex.Message, "alert-danger", "oi oi-circle-x");
      }
      
    }
    //code omitted
}
```
As soon I discover a simple method to inject a component into another component dynamically, I'm gonna extend this approach to the 4 different Modals I currently have nested into different components, so that I will piggyback on only one Modal instance as well (Think I spotted something like that during latest .NETConf, but unluckily I can't remember what session it was).

## Taking advantage of Blazor/.NET 6 new features

### The Multi select Double Pane component
The last functionality I need for the admin bikes page is a component to build 1-to-many relations between a bike and the optionals. 
Something that might look like the picture below.

![doublepane simple](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/doublepane-simple.png)

This particular component, in its basic form, is probably not too hard to code in pure C#, but since the focus of this library is piggybacking javascript-interop I will continue down this path.
Bootstrap does not come shipped with such a component (surely there are tons of plugins), however after a quick search on the internet I decided to use this [jQuery multiselect plugin](https://crlcu.github.io/multiselect/). The features I appreciated are these:
- it has many options but very simple to setup
- it is well documented
- it is stable
- it does not force additional css (great plus)
- it has no-dependecies beside jquery
- it does not require any questionable packagemanager to be built (other great plus)

It also gives us the opportunity to explore new and smarter ways of setting up js-interop and lazy-load additional javascript. 
First of all, as it is now, my library force a user to copy & paste the code in ```interop.js```, if we are to re-use the library in many projects any time we add a helper function we need to update every ```interop.js``` in every project. Let's fix it:
- Step 1: Erase all the content in ```interop.js``` we are keeping this file only for specific javascript for the BikeShop project
- Step 2: Add this line of code in ```BikeShop/wwwroot/index.html``` just above the inclusion of ```interop.js```
```html
(code omitted)
 <script src="./_content/BikeShop.BlazorComponents/MVComponents.js"></script><!-- _content is a conventional variable that will be expanded by blazor, don't change it-->
    <script src="js/interop.js"></script> 
</body>
(code omitted)
```
- Step3: Let's now create a ```MVComponents.js``` in the ```wwwwroot/``` folder of the **BikeShop.BlazorComponents** project. Now let's stick in it this updated code:

*BikeShop/BlazorComponents/wwwroot/MVComponents.js*
```csharp
//here can also go javascript initialisers
//define namespace for bootstrap components
var bootstrapNS = {};
//register js namespace for bootstrap components

    (function () {
        this.ToggleModal = function (modal, mode) {
            $(modal).modal(mode);
        }
        this.ToggleToast = function (toast, options) {
            $(toast).toast(options);
        }
        /********************new code for MultiSelect component*********/
        this.MultiSelect = function (multiselect, options) {
            var opt = {
                search: {
                    left: '<input type="text" name="q" class="form-control" placeholder="Search..." />',
                    right: '<input type="text" name="q" class="form-control" placeholder="Search..." />',
                },
                fireSearch: function (value) {
                    return value.length > 3;
                }
            }
            $(multiselect).multiselect(opt);
        }
      
        this.GetSelectedOptions = function (multiselect) {
            var selectedOptions = [];
            $(multiselect + ">option").map(function () {
                selectedOptions.push($(this).val());
            });
            
            return selectedOptions;
        }
        /******************end of MultiSelect component**************/
        this.JSDataTable = function (table, options) {
            if (!$.fn.dataTable.isDataTable(table)) {
                $(table).DataTable(options);
                //this.JSDataTables[table] = table;
            }
        }
        this.RefreshJSDataTable = function (table, options) {
            $(table).dataTable().fnDestroy();
        }
    }).apply(bootstrapNS);


```
- Step4: The last step is letting the library know about this file and export it. In ```BikeShop.BlazorComponents.csproj``` file add this code:
```xml
<ItemGroup>
    <None Include="wwwroot\MVComponents.js" />
</ItemGroup>ù
<ItemGroup>
    <None Include="wwwroot\multiselect.min.js" />
  </ItemGroup>
```
As simple as that now the only code we have to add in new projects is the javascript inclusion at step2. Should we add new helper methods to ```MVComponents.js```, recompiling 
the library project will automatically update every project.

Like I said, for the MultiSelect Double pane component we have to rely on a new external library which is contained in a file called ```multiselect.min.js``` which I already placed in the ```BikeShop.BlazorComponents/wwwroot/```. As you may have noticed, there is no reference to this file in the ```index.html```.
The ```multiselect.min.js``` is 11 kb, it is not that much but if we start adding size to the already slow first load (remember that wasm app needs to download the entire blazor framework on the client on first load) it won't help much. Since it is possible to load additional javascript at runtime, I want the ```multiselect.min.js``` to be loaded only when is needed, which is to say when a MultiSelect component appear first on some page. So I let lazy-load the ```multiselect.min.js``` lib by the MultiSelect component itself with this code.

[MultiSelect.cs](https://github.com/mvit777/BikeShop/blob/master/BikeShop.BlazorComponents/Components/MultiSelect.cs)
```csharp
(...code omitted..)
protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JS.InvokeAsync<IJSObjectReference>(
                  "import", "./_content/BikeShop.BlazorComponents/multiselect.min.js");
                await JS.InvokeVoidAsync("bootstrapNS.MultiSelect", "#" + HTMLId, new object[] { });
            }
            //await JS.InvokeVoidAsync("bootstrapNS.MultiSelect", "#" + HTMLId, new object[] { });
        }
(...code omitted...)
```
This component requires a bit of LINQ gymnic in the parent page/component to pump/pull data in/out and keep in sync with javascript manipulation but it is highly re-usable this way and we have delegated all the move-right, move-left, move-all part to javascript. We were also able to add custom handlers on option click in pure C# without interferring with equivalent handler in javascript. 
Please see the [AdminProductList component](https://github.com/mvit777/BikeShop/blob/master/BikeShop/Shared/Components/admin/AdminProductList.razor) to know what I mean.


## Breaking the Monolith and some refactor
### An honest review at the AdminBikeList component
(..more to come..)
### Getting rid of hardcoded values and configure our components from outside
If you step back to our ```MVComponents.js``` you may notice that almost all the components js initializers take a second parameter ```options``` but we did not really used it.

Ex. my beloved jquery datatables...
```javascript
this.JSDataTable = function (table, options) {
   if (!$.fn.dataTable.isDataTable(table)) {
         $(table).DataTable(options);
   }
}
```
...accept an *options* javascript object that is expected to be passed by Blazor
```razor
protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        await JSRuntime.InvokeVoidAsync("bootstrapNS.JSDataTable", "#BikeList", new object[]{});
    }
```
(More to come)

## The Resulting Stuff (so far)
[Summary of implemented components](https://github.com/mvit777/BikeShop/tree/master/BikeShop.BlazorComponents/Components)
>*The list of products* [(actual source code)](https://github.com/mvit777/BikeShop/blob/master/BikeShop/Shared/Components/admin/AdminProductList.razor)
![List](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/BikeListComplete.png)
>*A pop up for editing a product also featuring an info-alert on the background*
![Prodcut Edit](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/BikeEditPopUp.png)

>*the above info-alert instance transformed at runtime into a confirm panel*
![Asking for confirmation](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/deleteConfirm.png)

>*A toast message pops-up to inform some operation succesfully ended*
![Update success](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/toast.png)

>*The options panel*
![Option Panel](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/OptionPopUp.png)

>*Change user from user box*
![User Box](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/BoxUser.png)
>*List users*
![Users List](https://github.com/mvit777/BikeShop/blob/master/BikeShop/wwwroot/images/docs/UsersList.png)

## Conclusion
I'm no frontend guy and have no significant experience of developing with Angular/React/VueJs, so I'm not very qualified to make a comparison between these tools.
However I must say Blazor is very to fast to learn, especially if you have a background in previous or other MS techs. I would also say that for SPA applications, Blazor can be an interesting competitor. The main problems I see are:
- very little free/opensource ecosystem compared to javascript competitors
- demographics of developers. Young developers tend to favor javascript as it is all the rage these days. Aging developers have been using javascript for years now, they might not be so tempted to learn yet another tool (unless they really dislike javascript).
- first load is too slow. The problem is supposed to be mitigated in next release. Check bottom links for more details

On the positive side, Blazor integrates really smoothly with Bootstrap unlike the three other tools mentioned above. This is a feature not to understimate as Bootstrap it is by far still the most popular css-framework around. 
All in all, I'm satisfied so far with my little library. It is nothing more than a light wrapper around Bootstrap components to automatise some HTML but it seems to work well and was very quick to develop. In the past I once tried to develop such a library in pure javascript and another time with [PHP + Twig](https://twig.symfony.com/).

The javascript/jquery attempt was a complete failure up to the point I abandoned it in a very early stage. It had bugs scattered all around and was bloated from the very start.
Basically doing such a thing requires a non-trivial knowdledge of javascript far beyond my level and my interest in the language itself (javascript is non-existent in the ML field). 
Also the fact that I'm not aware of an existing such a library makes me think it is not a good idea. 

The PHP+Twig was much more successful, in the sense that I re-used it in many projects. The main problem of that solution was that once the serverside was executed, I was left alone yet again with a lot of AJAX setup and DOM manipulation.

In Blazor you just forget about AJAX setup and DOM manipulation, as they are run under the covers. This is another good point for Blazor. 
The aspect of inter-mixed html and code is a very handy but reminds me a bit of old style Wordpress, which I don't like very much. I still have to make a decision about 
shifting towards code-behind file + template file style or mix both. The impression is at some point I will use the former as it favours more consistency even if is a lot more verbose. 
Another good point of Blazor components is their declarative style (just like Webforms, Coldfusion or defunct Structs etc etc)
In fact, if you have a very predictable page structure (like my Datatable List + Edit form) building a code generator to automatise the skeleton of many pages becomes a lot easier.


## Related links
- [Messaging Center](https://github.com/aksoftware98/blazor-utilities) Messaging between unrelated components made it easy. A must-have nuget package. The author is also a very active member of the MS community and features a lot of learning material on his own site at https://ahmadmozaffar.net/Blog and at https://www.youtube.com/channel/UCRs-PO48PbbS0l7bBhbu5CA


- [ezzylearning](https://www.ezzylearning.net/tutorials/blazor): (lot of "inspiration" from following links)
    - [Beginner's guide to Components](https://www.ezzylearning.net/tutorial/a-beginners-guide-to-blazor-components)
    - [Templated Component](https://www.ezzylearning.net/tutorial/a-developers-guide-to-blazor-templated-components)
    - [Developing a Component Library](https://www.ezzylearning.net/tutorial/a-developers-guide-to-blazor-component-libraries)

- [EditForm](https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-5.0) Bible definition
  - [ezzylearning tutorial on forms and validation](https://www.ezzylearning.net/tutorial/a-guide-to-blazor-forms-and-validation)

- [Jon Hilton- Making sense of .NET](https://jonhilton.net/post/) A recent discover for me this site has a lot of material about Blazor. It features a lot of short but inspiring articles on Blazor and .NET. Defintely worth reading
  - [With so many Blazor Component Libraries, which one is best for you?](https://jonhilton.net/choosing-a-blazor-component-library/)
  - [How I organise my Blazor components](https://jonhilton.net/blazor-component-folder-structure/)
  - [Sure, you could write all those Blazor HTTP calls yourself...](https://jonhilton.net/blazor-refit/)
  - [Will .NET 6 fix Blazor Prerendering?](https://jonhilton.net/blazor-prerendering-net6/)
  - [Razor Pages has Components too don't you know!](https://jonhilton.net/razor-pages-components/)
  - [Go faster with your own re-usable Blazor components](https://jonhilton.net/build-your-own-re-usable-blazor-components/)

- [Cold Elm Coders](https://shauncurtis.github.io/articles/) Very different style of writing with regard to the preceding author but equally a must read
  - [A Deep Dive into Blazor Components](https://shauncurtis.github.io/articles/Blazor-Components.html#componentbase) The most in-depth article I found so far about components
  - [Working with CSS in Blazor](https://shauncurtis.github.io/articles/Blazor-CSS.html#getting-started) Same as above but this time CSS is the topic

- [So Why Doesn't Microsoft Provide Its Own Blazor Component Library?](https://visualstudiomagazine.com/articles/2021/08/13/blazor-components.aspx) Don't forget to read the comments at this VisualStudio magazine article. When people take things at heart is a good sign.
- [Telerik Blazor REPL](https://www.telerik.com/blazor-ui/repl) A read–eval–print loop sandbox kindly offered online by Telerik

### Commercial Components libraries
As usual for MS stack there is already a big ecosystem of commercial products backing Blazor. If you want to stay on the safe path, here is a work-in-progress list of commercially supported Components libraries (in no particular order):
- [Telerik](https://demos.telerik.com/blazor-ui)
- [Syncfusion](https://www.syncfusion.com/blazor-components)
- [Blazorise](https://blazorise.com/)
- [DevExpress](https://www.devexpress.com/blazor/)
- [Start Blazoring](https://startblazoring.com/) This project is a bit different, in the sense that is not only a collection of components but it also provides a starter template with a lot of functionality built-in
- (..more to come..)

### Free/OpenSource component libraries
It seems I was wrong about the lack of opensource/free libraries
- [Microsoft.Fast](https://github.com/microsoft/fast-blazor) The Microsoft.Fast.Components.FluentUI package provides a lightweight set of Blazor component wrappers around Microsoft's official FluentUI Web Components
- [Blazored](https://github.com/Blazored) A small collection of not-so-common components worth taking a look
- (..more to come..)
