using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;
using System.Threading.Tasks;
using LoanToolIoT.Database;
using LoanToolIoT.Model.Sql;
using LoanToolIoT.Model.Web;
using System.Linq;
using LoanToolIoT.Model.Api;

namespace LoanToolIoT.Controllers
{
    [RestController(InstanceCreationType.PerCall)]
    public sealed class LoanController :  ApiController
    {
        [UriFormat("/loan")]
        public IPostResponse LoanCamera([FromContent]DeviceLoan Loan)
        {
            var db = new SqliteDB();
            Loan.ExpireDate = DateTime.Now.Ticks;
            var dbres = db.GetLastLoanInfo(Loan);
            if (dbres == null)
            {
                var username = "loan" + new Random().Next(1000, 9999).ToString();
                var password = RandomString(16);
                Loan.GeneratedPassword = password;
                Loan.GeneratedUsername = username;

                DeviceList loandevice = new DeviceList { SerialNumber = Loan.Mac };
                var loanres = db.GetDevice(loandevice);
                if (loanres == null)
                    return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 2, Message = "Device not found", Status = "Failed" });

                var device = new AxisVapixLib.Device();
                device.Host = loanres.Host;
                device.Username = loanres.Username;
                device.Password = loanres.Password;

                var userres = Task.Run(async () => await device.AddUser(username, password, AxisVapixLib.Device.UserGroups.Admin));
                while (!userres.IsCompleted)
                    Task.Delay(100);
                

                
                     if (!userres.Result)
                     {
                         return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 3, Message = "Failed to create username", Status = "Failed" });
                     }
                     var overlay = new AxisVapixLib.Overlay.Overlay(device);
                     Task.Run(() => overlay.SetTextOverlayAsync(0, AxisVapixLib.Overlay.Overlay.TextOverlayBackgroundColor.White, false, false, AxisVapixLib.Overlay.Overlay.TextColor.Black, AxisVapixLib.Overlay.Overlay.TextOverlayPosition.Top, "Reserved to " + Loan.Email));
                     Loan.ExpireDate = DateTime.Now.AddDays(7).Ticks;
                     Loan.LoanDate = DateTime.Now.Ticks;

                     db.SaveLoanInfo(Loan);

                LoanResponse lr = new LoanResponse();
                lr.DateExpire = DateTime.FromBinary(Loan.ExpireDate).ToString();
                lr.DateExpireTick = Loan.ExpireDate;
                lr.SerialNumber = Loan.Mac;
                lr.Username = username;
                lr.Password = password;
                lr.Host = device.Host;

                     return new PostResponse(PostResponse.ResponseStatus.Created, null, lr);
            }
                var d = new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData() { Code = 1, Message = "Already loaned", Status = "Failed" });
                return d;
        }

        /// <summary>
        /// Return a camera from the loan tool
        /// </summary>
        /// <param name="Loan">Loan Parameter, Email, Password and Serial number needed.</param>
        /// <returns>Json/XML response to the client</returns>
        [UriFormat("/return")]
        public IPostResponse ReturnCamera([FromContent]ReturnRequest Loan)
        {
            var db = new Database.SqliteDB();
            var dev = new DeviceLoan { GeneratedPassword = Loan.Password, Email = Loan.Email, Mac = Loan.SerialNumber };
            var dbdev = db.ValidLoanReturn(dev);
            if(dbdev == null)
            {
                return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 1, Message = "Not your camera" });
            }
            db.UpdateLoanInfo(dbdev);
            var axisdev = new AxisVapixLib.Device();
            var device = db.GetDevice(new DeviceList { SerialNumber = dbdev.Mac });
            if(device != null)
            {
                axisdev.Username = device.Username;
                axisdev.Password = device.Password;
                axisdev.Host = device.Host;
                Task.Run(() => axisdev.FactoryDefault(AxisVapixLib.Device.FactoryDefaultMode.Hard)).Wait();
                return new PostResponse(PostResponse.ResponseStatus.Created, null, new ResponseData { Code = 0, Message = "Ok" });
            }
            return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 3, Message = "Device could not be found in local database" });      
        }

        [UriFormat("/reset")]
        public IGetResponse Reset()
        {
            var db = new SqliteDB();
            db.ResetLoanInfo();
            return null;
        }
        private static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
