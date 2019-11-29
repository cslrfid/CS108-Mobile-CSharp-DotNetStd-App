using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions.Contracts;

using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace BLE.Client.ViewModels
{
    public class ViewModelCS83045Setting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entryTempOffsetText { get; set; }
        public string entryTemp1THUnderText { get; set; }
        public string entryTemp1THOverText { get; set; }
        public string entryTemp1THCountText { get; set; }
        public string entryTemp2THUnderText { get; set; }
        public string entryTemp2THOverText { get; set; }
        public string entryTemp2THCountText { get; set; }
        public string entryLogIntervalText { get; set; }
        public ICommand OnOKButtonCommand { protected set; get; }

        public ViewModelCS83045Setting(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            entryTempOffsetText = "-20";
            entryLogIntervalText = "1";
            entryTemp1THUnderText = "-10";
            entryTemp1THOverText = "40";
            entryTemp1THCountText = "1";
            entryTemp2THUnderText = "-10";
            entryTemp2THOverText = "40";
            entryTemp2THCountText = "1";
            OnOKButtonCommand = new Command(OnOKButtonClicked);
        }

        public override void Resume()
        {
            base.Resume();
        }

        public override void Suspend()
        {
            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        void OnOKButtonClicked(object ind)
        {
            BleMvxApplication._coldChain_TempOffset = int.Parse(entryTempOffsetText);
            BleMvxApplication._coldChain_Temp1THUnder = int.Parse(entryTemp1THUnderText);
            BleMvxApplication._coldChain_Temp1THOver = int.Parse(entryTemp1THOverText);
            BleMvxApplication._coldChain_Temp1THCount = int.Parse(entryTemp1THCountText);
            BleMvxApplication._coldChain_Temp2THUnder = int.Parse(entryTemp2THUnderText);
            BleMvxApplication._coldChain_Temp2THOver = int.Parse(entryTemp2THOverText);
            BleMvxApplication._coldChain_Temp2THCount = int.Parse(entryTemp2THCountText);
            BleMvxApplication._coldChain_LogInterval = int.Parse(entryLogIntervalText);

            ShowViewModel<ViewModelCS83045Inventory>(new MvxBundle());
        }
    }
}
