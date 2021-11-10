using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.BlazorComponents.Components
{
    public partial class HtmlTable<TItem> //: IDisposable
    {
        [Parameter]
        public string HTMLId { get; set; }
        
        [Parameter]
        public RenderFragment HeaderTemplate { get; set; }

        [Parameter]
        public RenderFragment<TItem> RowTemplate { get; set; }

        [Parameter]
        public RenderFragment FooterTemplate { get; set; }

        [Parameter]
        public IReadOnlyList<TItem> Items { get; set; }

        public void RefreshComponent(IReadOnlyList<TItem> items)
        {
            Items = items;
        }
        //public void Dispose()
        //{
        //    GC.SuppressFinalize(this);
        //}
    }
}
