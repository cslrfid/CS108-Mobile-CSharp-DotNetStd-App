using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLE.Client.Pages
{
	public partial class PagePreFilter
	{
		public PagePreFilter()
		{
			InitializeComponent();

			entryMaskData.Text = BleMvxApplication._PREFILTER_MASK_EPC;
			entryMaskOffset.Text = BleMvxApplication._PREFILTER_MASK_Offset.ToString();
            switchEnableFilter.IsToggled = BleMvxApplication._PREFILTER_Enable;
		}

		public async void btnOKClicked(object sender, EventArgs e)
		{
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            BleMvxApplication._PREFILTER_MASK_EPC = entryMaskData.Text;
			BleMvxApplication._PREFILTER_MASK_Offset = uint.Parse(entryMaskOffset.Text);
            BleMvxApplication._PREFILTER_Enable = switchEnableFilter.IsToggled;

            BleMvxApplication.SaveConfig();
			//await this.Navigation.PopAsync();
		}
    }
}
