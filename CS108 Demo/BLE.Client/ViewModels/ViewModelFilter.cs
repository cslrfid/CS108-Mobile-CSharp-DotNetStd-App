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
	public class ViewModelFilter : BaseViewModel
	{
		private readonly IUserDialogs _userDialogs;

		private IDevice _device;
		private System.Threading.Tasks.Task<IService> _services;


		public ViewModelFilter(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
		{
			_userDialogs = userDialogs;
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
	}
}
