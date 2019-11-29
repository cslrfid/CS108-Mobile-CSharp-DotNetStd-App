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
    public class ViewModelXerxesSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public ICommand OnOKButtonCommand { protected set; get; }

        public ViewModelXerxesSetting(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

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
            if (ind != null)
                if ((int)ind == 1)
                    ShowViewModel<ViewModelXerxesInventory>(new MvxBundle());
        }
    }
}
