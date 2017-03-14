using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace LoanToolIoT.Cron
{
    public sealed class CameraReset
    {
        Database.SqliteDB db = new Database.SqliteDB();
        private bool forceStop = false;

        public CameraReset()
        {

        }

        public bool ForceStop { get => forceStop; set => forceStop = value; }

        public void StartTasks()
        {
            Task.Run(async () =>
            {
                while(ForceStop == false)
                {
                    await RunCron();
                    await Task.Delay(TimeSpan.FromMinutes(15));
                }
                forceStop = false;
            });
        }

        /// <summary>
        /// Runs the cron command and removes the devices..
        /// </summary>
        private async Task RunCron()
        {
            Debug.WriteLine("Starting Tasks");
            var loans = db.GetCurrentLoans();
            if (loans == null)
                return;
            if (loans.Count == 0)
                return;

            foreach(var loan in loans)
            {
                if (loan.ExpireDate < DateTime.Now.Ticks)
                {
                    var dbdevice = db.GetDevice(new Model.Sql.DeviceList { SerialNumber = loan.Mac });
                    var device = new AxisVapixLib.Device
                    {
                        Host = dbdevice.Host,
                        Username = dbdevice.Username,
                        Password = dbdevice.Password
                    };
                    Debug.WriteLine("Factory defaulting " + dbdevice.SerialNumber);
                    Task.Run(() => device.FactoryDefault(AxisVapixLib.Device.FactoryDefaultMode.Hard)).Wait();
                }
            }

        }
    }
}
