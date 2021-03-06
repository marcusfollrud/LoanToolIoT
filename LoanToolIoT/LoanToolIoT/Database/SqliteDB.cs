﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanToolIoT.Model.Sql;
using System.IO;
using SQLite.Net;
using Windows.Foundation;
using System.Diagnostics;

namespace LoanToolIoT.Database
{
    public sealed class SqliteDB
    {
        string path;
        SQLiteConnection conn;

        public SqliteDB()
        {
            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
        }

        public void CreateTables()
        {
            conn.CreateTable<DeviceList>();
            conn.CreateTable<LoanLog>();
            conn.CreateTable<DeviceHostList>();
            conn.CreateTable<DeviceLoan>();
            conn.CreateTable<Settings>();
        }

        #region Devices
        public IReadOnlyList<DeviceList> GetDevices()
        {
            return conn.Table<DeviceList>().ToList();
        }

        public DeviceList GetDevice(DeviceList Device)
        {
            return conn.Find<DeviceList>(x => x.SerialNumber == Device.SerialNumber);
        }

        public bool SaveDevice(DeviceList Device)
        {
            if(Device.ID > 0)
            {
                var ret = conn.Update(Device);
                if (ret > 0)
                    return true;
                return false;
            }
            else
            {
                var ret = conn.Insert(Device);
                if (ret > 0)
                    return true;
                return false;
            }
        }
#endregion
        #region Host functions
        public IReadOnlyList<DeviceHostList> GetHostLists()
        {
            return conn.Table<DeviceHostList>().ToList();
        }

        public bool SaveHost(DeviceHostList Host)
        {
            if(Host.ID > 0)
            {
                //We're updating
                var ret = conn.Update(Host);
                if(ret > 0)
                {
                    return true;
                }
                return false;
            }
            else
            {
                var ret = conn.Insert(Host);
                if (ret > 0)
                    return true;
                return false;
                    
            }
        }
        public void DeleteHost(int id)
        {
            conn.Delete<DeviceHostList>(id);
        }
#endregion

        #region Loan Functions
        public DeviceLoan GetLastLoanInfo(DeviceLoan Device)
        {
            return conn.Find<DeviceLoan>(x => x.Mac == Device.Mac && x.ExpireDate > Device.ExpireDate);
        }

        public DeviceLoan ValidLoanReturn(DeviceLoan Device)
        {
            return conn.Table<DeviceLoan>().Where(x => x.Mac == Device.Mac && x.Email == Device.Email && x.GeneratedPassword == Device.GeneratedPassword).OrderByDescending(xx => xx.ID).FirstOrDefault();
        }

        public IReadOnlyList<DeviceLoan> GetCurrentLoans()
        {
            return conn.Table<DeviceLoan>().Where(x => x.ExpireDate > 0 && x.Returned == false).ToList();
        }

        public void SaveLoanInfo(DeviceLoan Device)
        {
            conn.Insert(Device);
        }

        public void UpdateLoanInfo(DeviceLoan Device)
        {
            conn.Update(Device);
        }

        public void ResetLoanInfo()
        {
            conn.DeleteAll<DeviceLoan>();
        }
        #endregion
        #region Settings
        public Settings GetSetting(Settings Setting)
        {
            return conn.Table<Settings>().Where(x => x.Name == Setting.Name).FirstOrDefault();
        }

        public bool SaveSetting(Settings Setting)
        {
            var currentSetting = this.GetSetting(Setting);
            if (currentSetting != null)
            {
                currentSetting.Value = Setting.Value;
                var ret = conn.Update(currentSetting);
                if (ret > 0)
                    return true;
                return false;
            }
            else
            {
                var ret = conn.Insert(Setting);
                if (ret > 0)
                    return true;
                return false;
            }
        }
        public void DeleteSetting(Settings Setting)
        {
            conn.Delete(Setting);
        }

#endregion
    }
}
