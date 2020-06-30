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
    public class ViewModelFocusandFastIDSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public ICommand OnbuttonOKCommand { protected set; get; }

        public Boolean switchFocusIsToggled { get; set; }
        public Boolean switchFastIDIsToggled { get; set; }

        public ViewModelFocusandFastIDSetting(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            switchFocusIsToggled = false;
            switchFastIDIsToggled = false;
            OnbuttonOKCommand = new Command(OnButtonOKClicked);
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

        void OnButtonOKClicked(object ind)
        {
            BleMvxApplication._focus = switchFocusIsToggled;
            BleMvxApplication._fastID = switchFastIDIsToggled;

            ShowViewModel<ViewModelFocusandFastIDInventory>(new MvxBundle());
        }
    }
}
