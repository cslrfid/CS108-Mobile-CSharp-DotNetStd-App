using Xamarin.Forms;

namespace BLE.Client.Pages
{
	public partial class PageSetting : TabbedPage
	{
        public PageSetting()
        {
            InitializeComponent();

            switch (BleMvxApplication._reader.rfid.GetModelName())
            {
                default:
                    this.Children.RemoveAt(2);
                    break;

                case "CS463":
                    this.Children.RemoveAt(3);
                    break;
            }


        }
    }
}
