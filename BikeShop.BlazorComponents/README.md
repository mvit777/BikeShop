# BikeShop.BlazorComponents
TODO: clean of unused files and ...still work in progress
## Install
Make sure to install the excellent nuget-package [Messaging Center](https://github.com/aksoftware98/blazor-utilities) at solution level
```
Install-Package AKSoftware.Blazor.Utilities
```
Once you have imported the .dll into your Blazor project, just add the two lines at the bottom of _Imports.razor

```csharp
//(...omitted..)
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using AKSoftware.Blazor.Utilities
@using BikeShop.BlazorComponents.Components
```
Now navigate to the wwwroot folder and add a file interop.js or whatever name it suits you. Make sure to include datatables.css in the head tag in index.html
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
now stick this code in the wwwroot/js/interop.js file

```javascript
//define namespace for bootstrap components
var bootstrapNS = {};
//register the helper functions in the namespace for bootstrap components
(function () {
    this.ToggleModal = function (modal, mode) {
        $(modal).modal(mode);
    }
    this.JSDataTable = function (table, options) {
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

## The Finished Product (so far)

## Related links
[Messaging Center](https://github.com/aksoftware98/blazor-utilities) Messaging between unrelated components made it easy. A must-have nuget package. The author is also a very active member of the MS community and features a lot of learning material on his own site at https://ahmadmozaffar.net/Blog and at https://www.youtube.com/channel/UCRs-PO48PbbS0l7bBhbu5CA
