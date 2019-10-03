using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLE.Client.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PageXerxesSetting
	{
        string[] _tagTypeOptions = { "Xerxes" };

        public PageXerxesSetting ()
		{
			InitializeComponent ();

            buttonTagType.Text = _tagTypeOptions[0];
            entryDelay.Text = "15";
        }

        public async void ButtonOK_Clicked(object sender, EventArgs e)
        {
            BleMvxApplication._xerxes_delay = int.Parse(entryDelay.Text);

            buttonOK.SetBinding(Button.CommandProperty, new Binding("OnOKButtonCommand"));
            buttonOK.Command.Execute(1);
            buttonOK.RemoveBinding(Button.CommandProperty);
        }
    }
}