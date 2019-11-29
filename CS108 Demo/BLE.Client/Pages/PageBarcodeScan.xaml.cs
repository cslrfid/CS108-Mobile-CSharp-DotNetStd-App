using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageBarcodeScan
    {
        public PageBarcodeScan()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Icon = new FileImageSource();
                this.Icon.File = "find-barcode-icon-90058-30x30.png";
            }
        }
    }
}
