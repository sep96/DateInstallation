
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Globalization;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Linq;
using System.Management;

namespace main
{
    class Program
    {
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern Int32 MsiGetProductInfo(string product, string property, [Out] StringBuilder valueBuf, ref Int32 len);

        static void Main(string[] args)
        {
            // Fitst Soulution
            // Show List Excist in 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall' Current User
            var key = GetUninstallRegistryKeyByProductName("AnyDesk");
            var version = key.GetValue("DisplayVersion");
            // Second Soulution
            // Show List Excist in 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall' LocalMachine
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (Microsoft.Win32.RegistryKey key2 = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                foreach (string subkey_name in key2.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key2.OpenSubKey(subkey_name))
                    {
                        var ss = subkey.GetValue("DisplayName");
                        Console.WriteLine(subkey.GetValue("DisplayName"));
                    }
                }
            }
            // Third Soulution
            // Use Win32_Product To Show All installed Program. (BEST WAY) Slowest way!
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_Product");
            foreach (ManagementObject mo in mos.Get())
            {
                if (mo.Path.ToString() != "" )
                {
                    if (mo["Name"].ToString() == "Setup")
                    {
                        Console.WriteLine(mo["Name"]);
                        var dateInstallation = FromDate((mo["InstallDate"].ToString()));
                        if((DateTime.Now - dateInstallation).Days > 30)
                        {
                            Console.WriteLine("Expired");
                        }
                    }
                }
            }
            Console.ReadKey();
        }
        public static DateTime FromDate(string SerialDate)
        {
            var year = Convert.ToInt32(SerialDate.Substring(0, 4));
            var mon = Convert.ToInt32(SerialDate[4].ToString() + SerialDate[5].ToString());
            var day = Convert.ToInt32(SerialDate[6].ToString() + SerialDate[7].ToString());
            try
            {
                var date = new DateTime(year, mon, day);
                return date;
            }
            catch(Exception ss)
            {
                return DateTime.Now;
            }
        }
        private static RegistryKey GetUninstallRegistryKeyByProductName(string productName)
        {
            var subKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            if (subKey == null)
                return null;
            var temp = subKey.GetSubKeyNames();
            foreach (var name in subKey.GetSubKeyNames())
            {
                var application = subKey.OpenSubKey(name, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.QueryValues | RegistryRights.ReadKey | RegistryRights.SetValue);
                if (application == null)
                    continue;
                foreach (var appKey in application.GetValueNames().Where(appKey => appKey.Equals("DisplayName")))
                {
                    if (application.GetValue(appKey).Equals(productName))
                        return application;
                    break;
                }
            }
            return null;
        }

    }
}
