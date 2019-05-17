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
    public class ViewModelSpecialFunctionsMenu : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public ICommand OnMultiBankInventoryButtonCommand { protected set; get; }
        public ICommand OnPhaseChannelInventoryButtonCommand { protected set; get; }
        public ICommand OnPeriodicReadButtonCommand { protected set; get; }
        public ICommand OnUCODEDNAButtonCommand { protected set; get; }
        public ICommand OnRFMicroButtonCommand { protected set; get; }
        public ICommand OnBlockWriteButtonCommand { protected set; get; }


        public ViewModelSpecialFunctionsMenu (IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnMultiBankInventoryButtonCommand = new Command(OnMultiBankInventoryButtonClicked);
            OnPhaseChannelInventoryButtonCommand = new Command(OnPhaseChannelInventoryButtonClicked);
            OnPeriodicReadButtonCommand = new Command(OnPeriodicReadButtonClicked);
            OnUCODEDNAButtonCommand = new Command(OnUCODEDNAButtonClicked);
            OnRFMicroButtonCommand = new Command(OnRFMicroButtonClicked);
            OnBlockWriteButtonCommand = new Command(OnBlockWriteButtonClicked);
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

        void OnBlockWriteButtonClicked()
        {
            ShowViewModel<ViewModelBlockWrite>(new MvxBundle());
        }
    }
}
