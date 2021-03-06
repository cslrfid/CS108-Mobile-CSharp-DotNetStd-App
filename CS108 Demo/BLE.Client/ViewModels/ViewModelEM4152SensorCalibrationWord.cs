﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;

namespace BLE.Client.ViewModels
{
    public class ViewModelEM4152SensorCalibrationWord : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPCText { get; set; }
        public string entrySelectedPWDText { get; set; }
        public string labelSensorCalibrationWordText { get; set; }
        public ICommand ButtonReadCommand { protected set; get; }
        public ICommand ButtonWriteCommand { protected set; get; }


        public ViewModelEM4152SensorCalibrationWord(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            ButtonReadCommand = new Command(ButtonReadClick);
            ButtonWriteCommand = new Command(ButtonWriteClick);

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
            SetEvent(true);
        }

        public override void Resume()
        {
            base.Resume();
            SetEvent(true);
        }

        public override void Suspend()
        {
            SetEvent(false);
            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            entrySelectedEPCText = BleMvxApplication._SELECT_EPC;
            entrySelectedPWDText = "00000000";
            labelSensorCalibrationWordText = "0000";

            RaisePropertyChanged(() => entrySelectedEPCText);
            RaisePropertyChanged(() => entrySelectedPWDText);
            RaisePropertyChanged(() => labelSensorCalibrationWordText);
        }

        private void SetEvent(bool enable)
        {
            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.ClearEventHandler();

            if (enable)
            {
                // RFID event handler
                BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            }
        }

        public async void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                if (e.access == CSLibrary.Constants.TagAccess.READ)
                {
                    switch (e.bank)
                    {
                        case CSLibrary.Constants.Bank.USER:
                            if (e.success)
                            {
                                UInt16 value = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToUshorts()[0];

                                labelSensorCalibrationWordText = value.ToString("X04");
                                RaisePropertyChanged(() => labelSensorCalibrationWordText);

                                _userDialogs.ShowSuccess("Read Sucess");
                            }
                            else
                            {
                                _userDialogs.ShowError("Read Fail!!!");
                            }
                            break;
                    }
                }

                if (e.access == CSLibrary.Constants.TagAccess.WRITE)
                {
                    switch (e.bank)
                    {
                        case CSLibrary.Constants.Bank.USER:
                            if (e.success)
                            {
                                _userDialogs.ShowSuccess("Write Sucess");
                            }
                            else
                            {
                                _userDialogs.ShowError("Write Fail!!!");
                            }
                            break;
                    }
                }
            });
        }

        void TagSelected()
        {
            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPCText);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
        }

        void ButtonReadClick()
        {
            RaisePropertyChanged(() => entrySelectedEPCText);
            RaisePropertyChanged(() => entrySelectedPWDText);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagReadUser.accessPassword = Convert.ToUInt32(entrySelectedPWDText, 16);
            BleMvxApplication._reader.rfid.Options.TagReadUser.offset = 0x122; // m_readAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagReadUser.count = 1; // m_readAllBank.WordUser;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
        }

        void ButtonWriteClick(object ind)
        {
            if (ind == null)
                return;
            if ((int)ind != 1)
                return;

            UInt16[] value = new UInt16[1];

            RaisePropertyChanged(() => entrySelectedEPCText);
            RaisePropertyChanged(() => entrySelectedPWDText);
            RaisePropertyChanged(() => labelSensorCalibrationWordText);

            value[0] = Convert.ToUInt16(labelSensorCalibrationWordText, 16);

            //Write User Bank 0x120
            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = Convert.ToUInt32(entrySelectedPWDText, 16);
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = 0x122; // m_readAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = 1; // m_readAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = value;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
        }

    }
}
