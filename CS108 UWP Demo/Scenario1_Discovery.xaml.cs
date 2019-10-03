

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SDKTemplate
{
    // This scenario uses a DeviceWatcher to enumerate nearby Bluetooth Low Energy devices,
    // displays them in a ListView, and lets the user select a device and pair it.
    // This device will be used by future scenarios.
    // For more information about device discovery and pairing, including examples of
    // customizing the pairing process, see the DeviceEnumerationAndPairing sample.
    public sealed partial class Scenario1_Discovery : Page
    {
        private MainPage rootPage = MainPage.Current;

        private ObservableCollection<BluetoothLEDeviceDisplay> KnownDevices = new ObservableCollection<BluetoothLEDeviceDisplay>();
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();

        private bool _scanning = false;

        #region UI Code
        public Scenario1_Discovery()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CSLibrary.DeviceFinder.Stop();

            // Save the selected device's ID for use in other scenarios.
            var bleDeviceDisplay = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;
            if (bleDeviceDisplay != null)
            {
                rootPage.SelectedBleDeviceId = bleDeviceDisplay.Id;
                rootPage.SelectedBleDeviceName = bleDeviceDisplay.Name;
            }
        }

        private void EnumerateButton_Click()
        {
            if (!_scanning)
            {
                _scanning = true;
                CSLibrary.DeviceFinder.OnSearchCompleted += DeviceWatcher_Added;
                CSLibrary.DeviceFinder.SearchDevice();
                EnumerateButton.Content = "Stop searching";
                rootPage.NotifyUser($"Device watcher started.", NotifyType.StatusMessage);
            }
            else
            {
                _scanning = false;
                CSLibrary.DeviceFinder.Stop();
                CSLibrary.DeviceFinder.OnSearchCompleted -= DeviceWatcher_Added;
                EnumerateButton.Content = "Start searching";
                rootPage.NotifyUser($"Device watcher stopped.", NotifyType.StatusMessage);
            }
        }

        private async void DeviceWatcher_Added(object sender, object deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    CSLibrary.DeviceFinder.DeviceFinderArgs dfa = (CSLibrary.DeviceFinder.DeviceFinderArgs)deviceInfo;
                    CSLibrary.DeviceFinder.DeviceInfomation di = (CSLibrary.DeviceFinder.DeviceInfomation)dfa.Found;
                    DeviceInformation ndi = (DeviceInformation)di.nativeDeviceInformation;

                    Debug.WriteLine(String.Format("Added {0}{1}", di.ID, di.deviceName));

                    // Make sure device isn't already present in the list.
                    if (FindBluetoothLEDeviceDisplay(ndi.Id) == null)
                    {
                        if (di.deviceName != string.Empty)
                        {
                            // If device has a friendly name display it immediately.
                            KnownDevices.Add(new BluetoothLEDeviceDisplay(ndi));
                        }
                        else
                        {
                            // Add it to a list in case the name gets updated later. 
                            UnknownDevices.Add(ndi);
                        }
                    }
                }
            });
        }

        private BluetoothLEDeviceDisplay FindBluetoothLEDeviceDisplay(string id)
        {
            foreach (BluetoothLEDeviceDisplay bleDeviceDisplay in KnownDevices)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }

        #endregion
    }
}