using System;

using CSLibrary.Barcode;
using CSLibrary.Barcode.Constants;
using CSLibrary.Barcode.Structures;

namespace CSLibrary
{
	public partial class BarcodeReader
    {
        public enum STATE
        {
            UNKNOWN,            // unknown
            READY,              // hardware exists
            NOTVALID           // hardware fail
        }

        private class DOWNLINKCMD
        {
            public static readonly byte[] BARCODEPOWERON = { 0x90, 0x00 };
            public static readonly byte[] BARCODEPOWEROFF = { 0x90, 0x01 };
            public static readonly byte[] BARCODESCANTRIGGER = { 0x90, 0x02 };
            public static readonly byte[] BARCODERAWDATA = { 0x90, 0x03 };
        }

        private HighLevelInterface _deviceHandler;

        STATE _state = STATE.NOTVALID;
        public STATE state { get { return _state; } }


        internal BarcodeReader(HighLevelInterface handler)
		{
			_deviceHandler = handler;
		}

        public void ClearEventHandler()
        {
            //OnCapturedNotify = delegate { };
            OnCapturedNotify = null;
        }

        /// <summary>
        /// Receive BarCode packet
        /// </summary>
        /// <param name="recvData"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        internal bool DeviceRecvData(byte[] recvData)
		{
            if (recvData[2] <= 3) // if not barcode
            {
                if (recvData[2] == 3 && recvData[10] == 0x06) // if return success
                    return true;

                return false;
            }

            switch (recvData[10])
            {
                case 0x02: // prefix1
                    switch (recvData[14])
                    {
                        case 0x34: // Query ESN
                            _state = STATE.READY;
                            break;

                        default:
                            return false;
                    }
                    break;

                default: // Barcode scan data
                    if (OnCapturedNotify != null)
                    {
                        Barcode.Structures.DecodeMessage decodeInfo = new Barcode.Structures.DecodeMessage();       // Decode message structure.

                        decodeInfo.pchMessage = System.Text.Encoding.UTF8.GetString(recvData, 10, recvData[2] - 3).TrimStart();

                        FireCaptureCompletedEvent(new BarcodeEventArgs(MessageType.DEC_MSG, decodeInfo));
                    }
                    break;
            }

            return true;
		}

        public event EventHandler<CSLibrary.Barcode.BarcodeEventArgs> OnCapturedNotify;

        private BarcodeState m_state = BarcodeState.IDLE;
        // Helper for marshalling execution to GUI thread
        private object synlock = new object();

        private void TellThemCaptureCompleted(BarcodeEventArgs e)
        {
            if (OnCapturedNotify != null)
            {
                OnCapturedNotify (_deviceHandler, e);
            }
        }

        private void FireCaptureCompletedEvent(BarcodeEventArgs e)
        {
            TellThemCaptureCompleted(e);
        }


        /// <summary>
        /// Start to capture barcode, until stop is sent.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            try
            {
                if (_state == STATE.NOTVALID)
                    return false;

                _deviceHandler.SendAsync(0, 1, DOWNLINKCMD.BARCODERAWDATA, barcodecmd_ContinueMode, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
                //_deviceHandler.BARCODEPowerOn();
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stop capturing
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            bool rc = true;

            try
            {
                //_deviceHandler.BARCODEPowerOff();
                _deviceHandler.SendAsync(0, 1, DOWNLINKCMD.BARCODERAWDATA, barcodecmd_StopContinue, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
                //FireStateChangedEvent(BarcodeState.STOPPING);
            }
            catch (System.Exception ex)
            {
                rc = false;
            }
            return rc;
        }

        readonly byte[] barcodecmd_TriggerMode = new byte[] { 0x1b, 0x31 };     // Start Trigger Mode
        readonly byte[] barcodecmd_ContinueMode = new byte[] { 0x1b, 0x33 };    // Start Continue Scan Mode
        readonly byte[] barcodecmd_StopContinue = new byte[] { 0x1b, 0x30 };    // Stop Continue Scan Mode
        readonly byte[] barcodecmd_SysModeEnter = new byte[] { 0x6e, 0x6c, 0x73, 0x30, 0x30, 0x30, 0x36, 0x30, 0x31, 0x30, 0x3b };  // Enter Engineer Mode
        readonly byte[] barcodecmd_PermContinueMode = new byte[] { 0x6e, 0x6c, 0x73, 0x30, 0x33, 0x30, 0x32, 0x30, 0x32, 0x30, 0x3b };  
        readonly byte[] barcodecmd_PermTriggerMode = new byte[] { 0x6e, 0x6c, 0x73, 0x30, 0x33, 0x30, 0x32, 0x30, 0x30, 0x30, 0x3b };
        readonly byte[] barcodecmd_ScanCycleTime30000 = new byte[] { 0x6e, 0x6c, 0x73, 0x30, 0x33, 0x31, 0x33, 0x30, 0x30, 0x30, 0x3d, 0x33, 0x30, 0x30, 0x30, 0x30, 0x3b };
        readonly byte[] barcodecmd_SysModeExit = new byte[] { 0x6e, 0x6c, 0x73, 0x30, 0x30, 0x30, 0x36, 0x30, 0x30, 0x30, 0x3b };   // Exit Engineer Mode
        readonly byte[] barcodecmd_QueryESN = new byte[] { 0x7e, 0x00, 0x00, 0x05, 0x33, 0x48, 0x30, 0x32, 0x30, 0xB3 };


        internal void CheckHWValid()
        {
            _state = STATE.NOTVALID;
            _deviceHandler.SendAsync(CSLibrary.HighLevelInterface.DEVICEID.Barcode, DOWNLINKCMD.BARCODERAWDATA, barcodecmd_QueryESN, CSLibrary.HighLevelInterface.BTCOMMANDTYPE.Validate, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.DATA1);
        }

        // public barcode function
        public void FactoryReset()
        {
            //_deviceHandler.BARCODEPowerOn();
            _deviceHandler.SendAsync(0, 1, DOWNLINKCMD.BARCODERAWDATA, barcodecmd_SysModeEnter, CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1);
            _deviceHandler.SendAsync(0, 1, DOWNLINKCMD.BARCODERAWDATA, barcodecmd_ScanCycleTime30000, CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1);
            _deviceHandler.SendAsync(0, 1, DOWNLINKCMD.BARCODERAWDATA, barcodecmd_PermTriggerMode, CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1);
            _deviceHandler.SendAsync(0, 1, DOWNLINKCMD.BARCODERAWDATA, barcodecmd_SysModeExit, CSLibrary.HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA1);
            //_deviceHandler.BARCODEPowerOff();
        }

    }
}
