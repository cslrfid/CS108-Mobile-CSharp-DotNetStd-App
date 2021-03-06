﻿/*
Copyright (c) 2018 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

using System.Windows.Input;
using Xamarin.Forms;

using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class ViewModelMaxduraControls : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public ICommand OnMultiBankInventoryButtonCommand { protected set; get; }
        public ICommand OnPhaseChannelInventoryButtonCommand { protected set; get; }
        public ICommand OnPeriodicReadButtonCommand { protected set; get; }
        public ICommand OnUCODEDNAButtonCommand { protected set; get; }
        public ICommand OnRFMicroButtonCommand { protected set; get; }
        public ICommand OnXerxesButtonCommand { protected set; get; }
        public ICommand OnBlockWriteButtonCommand { protected set; get; }
        public ICommand OnReadButtonCommand { protected set; get; }
        public ICommand OnCS83045ButtonCommand { protected set; get; }
        public ICommand OnCS9010ButtonCommand { protected set; get; }
        public ICommand OnTagFocusandFastIDButtonCommand { protected set; get; }
        public ICommand OnCTESIUSTempButtonCommand { protected set; get; }
        public ICommand OnEM4152ButtonCommand { protected set; get; }


        

        public ViewModelMaxduraControls(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnMultiBankInventoryButtonCommand = new Command(OnMultiBankInventoryButtonClicked);
            OnPhaseChannelInventoryButtonCommand = new Command(OnPhaseChannelInventoryButtonClicked);
            OnPeriodicReadButtonCommand = new Command(OnPeriodicReadButtonClicked);
            OnUCODEDNAButtonCommand = new Command(OnUCODEDNAButtonClicked);
            OnRFMicroButtonCommand = new Command(OnRFMicroButtonClicked);
            OnXerxesButtonCommand = new Command(OnXerxesButtonClicked);
            OnBlockWriteButtonCommand = new Command(OnBlockWriteButtonClicked);
            OnReadButtonCommand = new Command(OnReadButtonClicked);
            OnCS83045ButtonCommand = new Command(OnCS83045ButtonClicked);
            OnCS9010ButtonCommand = new Command(OnCS9010ButtonClicked);
            OnTagFocusandFastIDButtonCommand = new Command(OnTagFocusandFastIDButtonClicked);
            OnCTESIUSTempButtonCommand = new Command(OnCTESIUSTempButtonClicked);
            OnEM4152ButtonCommand = new Command(OnEM4152ButtonClicked);
        }

        public override void Resume()
        {
            base.Resume();

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
        }

        void OnMultiBankInventoryButtonClicked()
        {
            ShowViewModel<ViewModelMultiBankInventorySetting>(new MvxBundle());
        }

        void OnPhaseChannelInventoryButtonClicked()
        {
            ShowViewModel<ViewModelPhaseChannelInventory>(new MvxBundle());
        }

        void OnPeriodicReadButtonClicked()
        {
            ShowViewModel<ViewModelPeriodicRead>(new MvxBundle());
        }

        void OnUCODEDNAButtonClicked()
        {
            ShowViewModel<ViewModelUCODEDNA>(new MvxBundle());
        }

        void OnRFMicroButtonClicked()
        {
            ShowViewModel<ViewModelRFMicroSetting>(new MvxBundle());
        }

        void OnXerxesButtonClicked()
        {
            ShowViewModel<ViewModelAxzonSetting>(new MvxBundle());
        }

        void OnBlockWriteButtonClicked()
        {
            ShowViewModel<ViewModelBlockWrite>(new MvxBundle());
        }

        void OnReadButtonClicked()
        {
            ShowViewModel<ViewModelRead>(new MvxBundle());
        }

        void OnCS83045ButtonClicked()
        {
            ShowViewModel<ViewModelCS83045Setting>(new MvxBundle());
        }

        void OnCS9010ButtonClicked()
        {
            ShowViewModel<ViewModelCS9010Inventory>(new MvxBundle());
        }

        void OnTagFocusandFastIDButtonClicked()
        {
            ShowViewModel<ViewModelFocusandFastIDSetting>(new MvxBundle());
        }

        void OnCTESIUSTempButtonClicked()
        {
            ShowViewModel< ViewModelCTESIUSTempInventory>(new MvxBundle());
        }

        void OnEM4152ButtonClicked()
        {
            ShowViewModel<ViewModelEM4152Inventory>(new MvxBundle());
        }
    }
}
