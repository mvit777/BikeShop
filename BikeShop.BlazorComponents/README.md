# BikeShop.BlazorComponents
TODO: clean of unused files and ...still work in progress
## Install
Once you have imported the .dll into your Blazor project, just add a line at the bottom of _Imports.razor

```csharp
(...omitted..)
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
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
    <link href="BikeShop.styles.css" rel="stylesheet" />
  </head>

```

## Brief description of the components

## The Finished Product (so far)

## Related links
[Messaging Center](https://github.com/aksoftware98/blazor-utilities) Messaging between unrelated components made it easy. A must-have nuget package. The author is also a very active member of the MS community and features a lot of learning material on his own site at https://ahmadmozaffar.net/Blog and at https://www.youtube.com/channel/UCRs-PO48PbbS0l7bBhbu5CA
