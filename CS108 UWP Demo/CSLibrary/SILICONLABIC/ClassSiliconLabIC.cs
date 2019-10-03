using System;

using CSLibrary.Barcode;
using CSLibrary.Barcode.Constants;
using CSLibrary.Barcode.Structures;

namespace CSLibrary
{
    public partial class SiliconLabIC
    {
        public event EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs> OnAccessCompleted;

        public bool _firmwareOlderT108 = false;

        private byte[] _version = new byte[3];

        public string _serailNumber = null;

        // RFID event code
        private class DOWNLINKCMD
		{
			public static readonly byte[] GETVERSION = { 0xB0, 0x00 };
            public static readonly byte[] GETSERIALNUMBER = { 0xB0, 0x04 };
        }

        private HighLevelInterface _deviceHandler;

        internal SiliconLabIC(HighLevelInterface handler)
		{
			_deviceHandler = handler;
		}

        internal HighLevelInterface.BTWAITCOMMANDRESPONSETYPE ProcessDataPacket (byte [] data)
        {
            uint pktType = (uint)(data[8] << 8 | data[9]);

            switch (pktType)
            {
                case 0xb000:    // version
                    Array.Copy(data, 10, _version, 0, 3);
                    if (_version[0] < 0x01)
                        _firmwareOlderT108 = true;
                    else if (_version[0] == 0x01 && _version[1] == 0x00 && _version[2] < 0x08)
                        _firmwareOlderT108 = true;

                    return HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;

                case 0xb004:    // serial number

                    _serailNumber = System.Text.Encoding.UTF8.GetString(data, 10, 13);

                    if (OnAccessCompleted != null)
                    {

                        try
                        {
                            Events.OnAccessCompletedEventArgs args = new Events.OnAccessCompletedEventArgs(_serailNumber, Constants.AccessCompletedCallbackType.SERIALNUMBER);

                            OnAccessCompleted(this, args);
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    return HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.BTAPIRESPONSE;
                    break;
            }

            return 0;
        }

        internal void GetVersion ()
		{
            _deviceHandler.SendAsync(0, 3, DOWNLINKCMD.GETVERSION, null, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }

        public UInt32 GetFirmwareVersion ()
        {
            UInt32 value = (UInt32)(_version[0] << 16 | _version[1] << 8 | _version[2]);

            return value;
        }

        public void GetSerialNumber ()
        {
            _deviceHandler.SendAsync(0, 3, DOWNLINKCMD.GETSERIALNUMBER, new byte [1], HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE);
        }

        public void ClearEventHandler()
        {
            OnAccessCompleted = delegate { };
        }
    }
}
