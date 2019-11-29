

using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace SDKTemplate
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "CS108 UWP WEDGE Demo 2.1.3";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Search and Select CS108", ClassType=typeof(Scenario1_Discovery) },
            new Scenario() { Title="Connect CS108 and Scan Barcode/RFID", ClassType=typeof(Scenario4_Barcode) },
        };

        public string SelectedBleDeviceId;
        public string SelectedBleDeviceName = "No device selected";
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
