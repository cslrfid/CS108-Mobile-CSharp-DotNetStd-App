using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageInventorynScan : TabbedPage
    {
        public PageInventorynScan()
        {
            InitializeComponent();

            if (BleMvxApplication._inventoryEntryPoint != 0)
            {
                var pages = Children.GetEnumerator();
                pages.MoveNext(); // First page
                pages.MoveNext(); // Second page
                CurrentPage = pages.Current;
            }
        }
    }
}
