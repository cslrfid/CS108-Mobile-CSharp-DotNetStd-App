using System;

using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class PageMainMenu
    {
        public PageMainMenu()
        {
            InitializeComponent();
            this.Title = "CS108 RFID Reader (C# " + DependencyService.Get<IAppVersion>().GetVersion() + ")";
        }
    }
}