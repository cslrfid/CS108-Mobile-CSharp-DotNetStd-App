using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(BLE.Client.UWP.Version_UWP))]
namespace BLE.Client.UWP
{
    public class Version_UWP : IAppVersion
    {
        public string GetVersion()
        {
            //return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
            return "";
        }
        public int GetBuild()
        {
            //return int.Parse(NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString());
            return 0;
        }
    }
}