﻿using System;
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
    public class ViewModelSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;


        public ViewModelSetting(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        public override void Resume()
        {
            base.Resume();

            BleMvxApplication._reader.siliconlabIC.OnAccessCompleted += new EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs>(OnAccessCompletedEvent);
        }

        public override void Suspend()
        {
            BleMvxApplication._reader.siliconlabIC.OnAccessCompleted -= new EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs>(OnAccessCompletedEvent);

            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        void OnAccessCompletedEvent(object sender, CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                switch (e.type)
                {
                    case CSLibrary.SiliconLabIC.Constants.AccessCompletedCallbackType.SERIALNUMBER:
                        _userDialogs.Alert("Serial Number : " + (string)e.info);
                        break;
                }
            });
        }
    }
}
