using System;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        CSLibrary.Timer BTTimer;

        private SiliconLabIC _handleSiliconLabIC = null;
        private RFIDReader _handlerRFIDReader = null;
        private BarcodeReader _handleBarCodeReader = null;
        private Notification _handleNotification = null;
        private BluetoothIC _handleBluetoothIC = null;

        public HighLevelInterface()
        {
            BTTimer = new Timer(TimerFunc, this, 0, 1000);

            BLE_Init();

            _handleSiliconLabIC = new SiliconLabIC(this);
            _handlerRFIDReader = new RFIDReader(this);
            _handleBarCodeReader = new BarcodeReader(this);
            _handleNotification = new Notification(this);
            _handleBluetoothIC = new BluetoothIC(this);
        }

        ~HighLevelInterface()
        {
            BTTimer.Cancel();
        }

        /// <summary>
        /// One second routine
        /// </summary>
        /// <param name="o"></param>
        void TimerFunc(object o)
        {
            /*
            if (_readerState == READERSTATE.READYFORDISCONNECT || _readerState == READERSTATE.DISCONNECT)
            {
                BTTimer.Cancel();
                _sendBuffer.Clear();
                _NeedCommandResponseType = BTWAITCOMMANDRESPONSETYPE.NOWAIT;
                _readerState = READERSTATE.DISCONNECT;
                return;
            }
            */
            BLERWEngineTimer();
            return;
        }
    }
}
