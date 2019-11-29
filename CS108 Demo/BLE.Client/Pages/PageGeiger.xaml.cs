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

            if (BleMvxApplication._config.RFID_DBm)
            {
                sliderThreshold.Minimum = -90;
                sliderThreshold.Maximum = -10;
                sliderThreshold.Value = -47;
            }
            else
            {
                sliderThreshold.Minimum = 17;
                sliderThreshold.Maximum = 97;
                sliderThreshold.Value = 60;
            }
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

        void sliderThresholdValueChanged(object sender, EventArgs e)
        {
            labelThreshold.Text = ((int)(sliderThreshold.Value)).ToString();
        }
    }
}
