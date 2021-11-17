using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    
    public partial class MultiSelect : ComponentBase
    {
        [Parameter]
        public string HTMLId { get; set; }

        private IJSObjectReference module;
        [Inject]
        IJSRuntime JS { get; set; }

        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JS.InvokeAsync<IJSObjectReference>(
                  "import", "./_content/BikeShop.BlazorComponents/multiselect.min.js");
            }
            await JS.InvokeVoidAsync("bootstrapNS.MultiSelect", "#" + HTMLId, new object[] { });
        }
        

    }
}
