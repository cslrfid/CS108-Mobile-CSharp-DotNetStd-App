using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLE.Client.Pages
{
    public partial class PageRFMicroSetting
    {
        string[] _tagTypeOptions = { "Magnus S2", "Magnus S3" };
        string[] _powerOptions = { "Low (16dBm)", "Mid (23dBm)", "High (30dBm)", "Cycle Power by Trigger Button", "Follow system Setting" };
        string[] _targetOptions = { "A", "B", "Toggle A/B"};
        string[] _indicatorsProfileOptions = { "Hot temperature", "Cold temperature", "Moisture detection" };
        string[] _sensorTypeOptions = { "Sensor Code", "Temperature" };
        string[] _sensorCodeUnitOptions = { "code", "%" };
        string[] _sensorCodeS2UnitOptions = { "code", "%", "RAW", "Range Allocation" };
        string[] _temperatureUnitOptions = { "code", "ºF", "ºC" };
        int[] _minOCRSSIs = { 0, 5, 10, 10 };
        int [] _maxOCRSSIs = { 21, 18, 21, 21 };
        string[] _thresholdComparisonOptions = { ">", "<" };
        int[] _thresholdValueOptions = { 100, -1, 58 };
        string [] _thresholdColorOptions = { "Red", "Blue"};

        public PageRFMicroSetting()
        {
            InitializeComponent();

            buttonTagType.Text = _tagTypeOptions[1];
            buttonPower.Text = _powerOptions[2];
            buttonTarget.Text = _targetOptions[2];
            SetIndicatorsProfile(0);

            entryMinWet.Text = "0";
            entryMaxWet.Text = "15";
            entryMinDamp.Text = "15";
            entryMaxDamp.Text = "21";
            entryMinDry.Text = "21";
            entryMaxDry.Text = "31";
        }

        protected override void OnAppearing()
        {
            buttonOK.RemoveBinding(Button.CommandProperty);
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            buttonOK.RemoveBinding(Button.CommandProperty);
            base.OnDisappearing();
        }

        public async void buttonTagTypeClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Tag Type", "Cancel", null, _tagTypeOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonTagType.Text = answer;

                switch (Array.IndexOf(_tagTypeOptions, buttonTagType.Text))
                {
                    case 0: // S2
                        SetIndicatorsProfile(2);
                        buttonIndicatorsProfile.IsEnabled = false;
                        buttonSensorType.IsEnabled = false;
                        break;

                    case 1: // S3
                        buttonIndicatorsProfile.IsEnabled = true;
                        buttonSensorType.IsEnabled = true;
                        break;

                    default: // Xerxes


                        break;
                }
            }
        }

        public async void buttonPowerClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Power", "Cancel", null, _powerOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonPower.Text = answer;
            }
        }

        public async void buttonTargetClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Target", "Cancel", null, _targetOptions);

            if (answer != null && answer !="Cancel")
                buttonTarget.Text = answer;
        }

        public async void buttonSensorTypeClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Sensor Type", "Cancel", null, _sensorTypeOptions);

            if (answer != null && answer !="Cancel")
            {
                SetSensorType((uint)Array.IndexOf(_sensorTypeOptions, answer));
            }
        }

        public async void buttonIndicatorsProfileClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Indicators Profile", "Cancel", null, _indicatorsProfileOptions);

            if (answer != null && answer !="Cancel")
            {
                SetIndicatorsProfile((uint)Array.IndexOf(_indicatorsProfileOptions, answer));
            }
        }

        public async void buttonSensorUnitClicked(object sender, EventArgs e)
        {
            string answer;

            if (buttonSensorType.Text == _sensorTypeOptions[0])
            {
                if (Array.IndexOf(_tagTypeOptions, buttonTagType.Text) == 0)
                {
                    answer = await DisplayActionSheet("Sensor Unit", "Cancel", null, _sensorCodeS2UnitOptions);
                }
                else
                    answer = await DisplayActionSheet("Sensor Unit", "Cancel", null, _sensorCodeUnitOptions);
            }
            else
                answer = await DisplayActionSheet("Sensor Unit", "Cancel", null, _temperatureUnitOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonSensorUnit.Text = answer;
            }
        }

        public async void buttonSensorUnitPropertyChanged(object sender, EventArgs e)
        {
            if (buttonTagType != null)
            {
                if (Array.IndexOf(_tagTypeOptions, buttonTagType.Text) == 0)
                {
                    if (Array.IndexOf(_sensorCodeS2UnitOptions, buttonSensorUnit.Text) == 3)
                    {
                        entryMinOCRSSI.Text = _minOCRSSIs[2].ToString();
                        entryMaxOCRSSI.Text = _maxOCRSSIs[2].ToString();

                        stacklayoutMinWet.IsVisible = true;
                        stacklayoutMaxWet.IsVisible = true;
                        stacklayoutMinDamp.IsVisible = true;
                        stacklayoutMaxDamp.IsVisible = true;
                        stacklayoutMinDry.IsVisible = true;
                        stacklayoutMaxDry.IsVisible = true;

                    }
                    else
                    {
                        stacklayoutMinWet.IsVisible = false;
                        stacklayoutMaxWet.IsVisible = false;
                        stacklayoutMinDamp.IsVisible = false;
                        stacklayoutMaxDamp.IsVisible = false;
                        stacklayoutMinDry.IsVisible = false;
                        stacklayoutMaxDry.IsVisible = false;

                    }
                }
            }
        }

        public async void buttonThresholdComparisonClicked(object sender, EventArgs e)
        {
            string answer = await DisplayActionSheet("Threshold Comparison", "Cancel", null, _thresholdComparisonOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonThresholdComparison.Text = answer;
            }
        }

        public async void buttonThresholdColorClicked(object sender, EventArgs e)
        {
            string answer = await DisplayActionSheet("Threshold Color", "Cancel", null, _thresholdColorOptions);

            if (answer != null && answer !="Cancel")
            {
                buttonThresholdColor.Text = answer;
            }
        }

        public async void ButtonOK_Clicked(object sender, EventArgs e)
        {
            BleMvxApplication._rfMicro_TagType = Array.IndexOf(_tagTypeOptions, buttonTagType.Text);
            BleMvxApplication._rfMicro_Power = Array.IndexOf(_powerOptions, buttonPower.Text);
            BleMvxApplication._rfMicro_Target = Array.IndexOf(_targetOptions, buttonTarget.Text); 
            BleMvxApplication._rfMicro_SensorType = Array.IndexOf(_sensorTypeOptions, buttonSensorType.Text);
            switch (BleMvxApplication._rfMicro_SensorType)
            {
                case 0:
                    BleMvxApplication._rfMicro_SensorUnit = Array.IndexOf(_sensorCodeS2UnitOptions, buttonSensorUnit.Text);
                    if (BleMvxApplication._rfMicro_SensorUnit != 0)
                        BleMvxApplication._rfMicro_SensorUnit += 2;
                    break;
                default:
                    BleMvxApplication._rfMicro_SensorUnit = Array.IndexOf(_temperatureUnitOptions, buttonSensorUnit.Text);
                    break;
            }
            BleMvxApplication._rfMicro_minOCRSSI = int.Parse(entryMinOCRSSI.Text);
            BleMvxApplication._rfMicro_maxOCRSSI = int.Parse(entryMaxOCRSSI.Text);
            BleMvxApplication._rfMicro_thresholdComparison = Array.IndexOf(_thresholdComparisonOptions, buttonThresholdComparison.Text);
            BleMvxApplication._rfMicro_thresholdValue = int.Parse(entryThresholdValue.Text);
            BleMvxApplication._rfMicro_thresholdColor = buttonThresholdColor.Text;

            BleMvxApplication._rfMicro_MinWet = int.Parse(entryMinWet.Text);
            BleMvxApplication._rfMicro_MaxWet = int.Parse(entryMaxWet.Text);
            BleMvxApplication._rfMicro_MinDamp = int.Parse(entryMinDamp.Text);
            BleMvxApplication._rfMicro_MaxDamp = int.Parse(entryMaxDamp.Text);
            BleMvxApplication._rfMicro_MinDry = int.Parse(entryMinDry.Text);
            BleMvxApplication._rfMicro_MaxDry = int.Parse(entryMaxDry.Text);

            buttonOK.SetBinding(Button.CommandProperty, new Binding("OnOKButtonCommand"));
            buttonOK.Command.Execute(1);
            buttonOK.RemoveBinding(Button.CommandProperty);
        }

        bool SetIndicatorsProfile(uint index)
        {
            switch (index)
            {
                case 0:
                    buttonIndicatorsProfile.Text = _indicatorsProfileOptions[0];
                    SetSensorType(1);
                    buttonSensorUnit.Text = _temperatureUnitOptions[1];
                    buttonThresholdComparison.Text = _thresholdComparisonOptions[0];
                    entryThresholdValue.Text = _thresholdValueOptions[0].ToString();
                    buttonThresholdColor.Text = _thresholdColorOptions[0];
                    break;
                case 1:
                    buttonIndicatorsProfile.Text = _indicatorsProfileOptions[1];
                    SetSensorType(1);
                    buttonSensorUnit.Text = _temperatureUnitOptions[2];
                    buttonThresholdComparison.Text = _thresholdComparisonOptions[1];
                    entryThresholdValue.Text = _thresholdValueOptions[1].ToString();
                    buttonThresholdColor.Text = _thresholdColorOptions[1];
                    break;
                case 2:
                    buttonIndicatorsProfile.Text = _indicatorsProfileOptions[2];
                    SetSensorType(0);
                    buttonSensorUnit.Text = _sensorCodeUnitOptions[1];
                    buttonThresholdComparison.Text = _thresholdComparisonOptions[0];
                    entryThresholdValue.Text = _thresholdValueOptions[2].ToString();
                    buttonThresholdColor.Text = _thresholdColorOptions[1];
                    break;
                default:
                    return false;
            }

            return true;
        }

        bool SetSensorType(uint index)
        {
            if (index >= _sensorTypeOptions.Length)
                return false;

            buttonSensorType.Text = _sensorTypeOptions[index];
            entryMinOCRSSI.Text = _minOCRSSIs[index].ToString();
            entryMaxOCRSSI.Text = _maxOCRSSIs[index].ToString();

            switch (index)
            {
                case 0:
                    buttonSensorUnit.Text = _sensorCodeUnitOptions[0];
                    break;
                default:
                    buttonSensorUnit.Text = _temperatureUnitOptions[2];
                    break;
            }

            return true;
        }
    }
}
