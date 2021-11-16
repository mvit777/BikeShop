using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    public partial class MultiSelect : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleMultiSelect;

        //public MultiSelect(IJSRuntime jsRuntime)
        //{
        //    moduleMultiSelect = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
        //       "import", "./_content/BikeShop.BlazorComponents/Components/multiselect.min.js").AsTask());
        //}
        public async ValueTask DisposeAsync()
        {
            if (moduleMultiSelect.IsValueCreated)
            {
                var module = await moduleMultiSelect.Value;
                await module.DisposeAsync();
            }
        }

    }
}
