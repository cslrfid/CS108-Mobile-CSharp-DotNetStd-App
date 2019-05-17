using System;

using Xamarin.Forms;

namespace BLE.Client.Pages
{
	public partial class PageGeiger
	{
		static uint _rssi;

		public PageGeiger()
		{
			InitializeComponent();
        }

        public async void entryPowerCompleted(object sender, EventArgs e)
        {
            uint value;

            try
            {
                value = uint.Parse(entryPower.Text);
                if (value < 0 || value > 300)
                    throw new System.ArgumentException("Value not valid", "tagPopulation");
                entryPower.Text = value.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Value not valid!!!", "OK");
                entryPower.Text = "300";
            }
        }

    }
}
