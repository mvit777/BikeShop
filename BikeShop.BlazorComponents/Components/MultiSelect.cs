using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    
    //public partial class MultiSelect<TItem> : ComponentBase
    public partial class MultiSelect<TItem, TItem2> //: ComponentBase
    {
        [Parameter]
        public string HTMLId { get; set; }
        [Parameter]
        public string RightPaneId { get; set; } = "multiselect_to";
        [Parameter]
        public IList<TItem> SelectableItems { get; set; } = new List<TItem>();
        [Parameter]
        public IList<TItem2> SelectedItems { get; set; } = new List<TItem2>();
        [Parameter]
        public RenderFragment SelectedItemsTemplate { get; set; }
        [Parameter]
        public RenderFragment SelectableItemsTemplate { get; set; }

        private IJSObjectReference module;
        [Inject]
        IJSRuntime JS { get; set; }

        
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
        
        public async Task<string[]> GetSelected(string multiselectHtmlId)
        {
           string[] res = await JS.InvokeAsync<string[]>("bootstrapNS.GetSelectedOptions", multiselectHtmlId);
           return res;
        }

        public void RefreshComponent(IList<TItem> selectableItems, IList<TItem2> selectedItems)
        {
            SelectableItems = selectableItems;
            SelectedItems = selectedItems;
            StateHasChanged();
        }

    }
}
