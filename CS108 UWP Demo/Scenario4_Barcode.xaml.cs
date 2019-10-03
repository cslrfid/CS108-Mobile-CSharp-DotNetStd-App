using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Input.Preview.Injection;

using Windows.System;

namespace SDKTemplate
{
    // This scenario connects to the device selected in the "Discover
    // GATT Servers" scenario and communicates with it.
    // Note that this scenario is rather artificial because it communicates
    // with an unknown service with unknown characteristics.
    // In practice, your app will be interested in a specific service with
    // a specific characteristic.
    public sealed partial class Scenario4_Barcode : Page
    {
        private MainPage rootPage = MainPage.Current;

        CSLibrary.HighLevelInterface _reader = new CSLibrary.HighLevelInterface();

        bool barcodescanning = false;
        bool rfidscanning = false;

        #region UI Code
        public Scenario4_Barcode()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _reader.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);
            _reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);

            if (string.IsNullOrEmpty(rootPage.SelectedBleDeviceId))
            {
                ButtonConnect.IsEnabled = false;
            }
            else
            {
                ButtonConnect.IsEnabled = true;
            }
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            _reader.barcode.OnCapturedNotify -= new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Linkage_CaptureCompleted);
            _reader.notification.OnKeyEvent -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
            _reader.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);
        }
        #endregion

        private async void TypeText(string KeyIn)
        {
            InputInjector inputInjector = InputInjector.TryCreate();
            foreach (var letter in KeyIn)
            {
                if ((letter >= '0' && letter <= '9') ||
                    (letter >= 'A' && letter <= 'Z') ||
                    (letter >= 'a' && letter <= 'z'))
                {
                    var info = new InjectedInputKeyboardInfo();

                    // change character to virtual key code
                    if (letter >= '0' && letter <= '9')
                        info.VirtualKey = (ushort)letter;
                    else if (letter >= 'a' && letter <= 'z')
                    {
                        int a = (int)letter - 32;
                        info.VirtualKey = (ushort)a;
                    }
                    else if (letter >= 'A' && letter <= 'Z')
                        info.VirtualKey = (ushort)letter;

                    inputInjector.InjectKeyboardInput(new[] { info });
                    await Task.Delay(10);
                }
            }

            {
                var info = new InjectedInputKeyboardInfo();

                // change character to virtual key code
                info.VirtualKey = (ushort)13; // Enter/ Return
                inputInjector.InjectKeyboardInput(new[] { info });
                await Task.Delay(10);
            }

        }

        private async void ButtonConnect_Click()
        {
            if (ButtonConnect.Content.ToString().Substring(0, 1) == "C")
            {
                //Windows.UI.Di


                //DisplayAlert("Alert", "You have been alerted", "OK");

                if (await _reader.ConnectAsync(rootPage.SelectedBleDeviceId))
                {
                    ButtonConnect.Content = "Disconnect";
                    textBlockDisconnectMsg.Text = "AFTER DISCONNECT, IF YOU WANT TO CONNECT AGAIN, YOU MUST GO BACK TO PAGE 1 TO SELECT READER AGAIN";
                }
            }
            else
            {
                _reader.DisconnectAsync();
                ButtonConnect.Content = "Connect reader ";



                //rootPage.Navigate()


                Frame rootFrame = Window.Current.Content as Frame;

                rootFrame.Navigate(typeof(MainPage));


                /*
                            Frame rootFrame = Window.Current.Content as Frame;
                            if (rootFrame == null)
                            {
                                // Create a Frame to act as the navigation context and navigate to the first page
                                rootFrame = new Frame();
                                // Place the frame in the current Window
                                Window.Current.Content = rootFrame;
                            }
                            if (rootFrame.Content == null)
                            {
                                // When the navigation stack isn't restored navigate to the first page,
                                // configuring the new page by passing required information as a navigation
                                // parameter
                                rootFrame.Navigate(typeof(MainPage), e.Arguments);
                            }
                            // Ensure the current window is active
                            Window.Current.Activate();
                */

            }
        }
 
        private async void DisplayNoWifiDialog()
        {
            ContentDialog Dialog = new ContentDialog()
            {
                Title = "No wifi connection",
                Content = "Check connection and try again.",
                CloseButtonText = "Ok"
            };

            //var BTTimer = new CSLibrary.Timer(() => { Dialog.Hide(); }, this, 0, 1000);

            await Dialog.ShowAsync();
        }

        void RFIDreaderstat()
        {
            rootPage.NotifyUser($"CS108 reader ready", NotifyType.ErrorMessage);
        }

        private async void ButtonStart_Click()
        {
            if (!barcodescanning)
            {
                barcodescanning = true;
                _reader.barcode.OnCapturedNotify += new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Linkage_CaptureCompleted);
                _reader.barcode.Start();
            }
            else
            {
                barcodescanning = false;
                _reader.barcode.Stop();
                _reader.barcode.OnCapturedNotify -= new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Linkage_CaptureCompleted);
            }
        }

        void Linkage_CaptureCompleted(object sender, CSLibrary.Barcode.BarcodeEventArgs e)
        {
            switch (e.MessageType)
            {
                case CSLibrary.Barcode.Constants.MessageType.DEC_MSG:
                    {
                        string msg = ((CSLibrary.Barcode.Structures.DecodeMessage)e.Message).pchMessage;

                        Debug.WriteLine(msg);
                        TypeText(msg);

                        barcodescanning = false;
                        BarcodeScanningOff();
                    }
                    break;

            case CSLibrary.Barcode.Constants.MessageType.ERR_MSG:
                    //UpdateUI(null, String.Format("Barcode Returned: {0}", e.ErrorMessage));
                    break;
            }
        }



        // for RFID
        bool firstTime = true;

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING)
                return;


            var msg = e.info.epc.ToString();

            Debug.WriteLine(msg);

            if (firstTime)
            {
                firstTime = false;
                TypeText(msg);

                RFIDInventoryOff();
            }
        }

        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
                {
                    if (e.KeyDown)
                    {
                        if (switchRFID.IsOn)
                        {
                            firstTime = true;
                            RFIDInventoryOn();
                        }

                        if (switchBarcode.IsOn)
                        {
                            BarcodeScanningOn();
                        }
                    }
                    else
                    {
                        RFIDInventoryOff();

                        barcodescanning = false;
                        BarcodeScanningOff();
                    }
                }

            });
        }


        void RFIDInventoryOn ()
        {
            _reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
            _reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;
            _reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_RANGING); // inventory with default setting
        }

        void RFIDInventoryOff()
        {
            _reader.rfid.StopOperation();
            _reader.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
        }

        void BarcodeScanningOn()
        {
            _reader.barcode.OnCapturedNotify += new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Linkage_CaptureCompleted);
            _reader.barcode.Start();
        }

        void BarcodeScanningOff ()
        {
            _reader.barcode.Stop();
            _reader.barcode.OnCapturedNotify -= new EventHandler<CSLibrary.Barcode.BarcodeEventArgs>(Linkage_CaptureCompleted);
        }


        void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e)
        {
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (e.Voltage == 0xffff)
                {
                    textBlockBatteryVoltage.Text = "CS108 Bat. ERROR"; //			3.98v
                }
                else
                {
                    textBlockBatteryVoltage.Text = "CS108 Bat. " + ((double)e.Voltage / 1000).ToString("0.000") + "v"; //			v
                }
            });
        }



    }
}


