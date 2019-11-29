using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageXerxesInventory
    {
        bool pageView = true;

		public PageXerxesInventory()
		{
			InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-RFID Tag-104-30x30.png";
            }
        }

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
            var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");

            if (answer)
            {
				BLE.Client.ViewModels.ViewModelXerxesInventory.RFMicroTagInfoViewModel Items = (BLE.Client.ViewModels.ViewModelXerxesInventory.RFMicroTagInfoViewModel)e.SelectedItem;

				BleMvxApplication._SELECT_EPC = Items.DisplayName;
            }
        }

		~PageXerxesInventory()
        {
            pageView = false;
        }
	}
}
