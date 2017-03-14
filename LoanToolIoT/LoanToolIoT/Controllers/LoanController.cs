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
using System.Collections.Generic;

namespace LoanToolIoT.Controllers
{
    [RestController(InstanceCreationType.PerCall)]
    public sealed class LoanController
    {
        ApiController ApiController = new ApiController();
        /// <summary>
        /// Loan a camera through the API.
        /// Requires: {"SerialNumber" :"", Email = "", Reason= "", LoanDays = ""}.
        /// if successful, loans a camera for 7 days.
        /// </summary>
        /// <param name="Loan"></param>
        /// <returns></returns>
        [UriFormat("/loan/request")]
        public IPostResponse LoanCamera([FromContent]LoanRequest LoanRequest)
        {
            var Loan = new DeviceLoan { Mac = LoanRequest.SerialNumber, Email = LoanRequest.Email, LoanReason = LoanRequest.Reason };
            if ((LoanRequest.LoanDays == 1 || LoanRequest.LoanDays == 7 || LoanRequest.LoanDays == 14) == false)
            {
                return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 4, Message = "Loan days can only be set to 1,7 or 14", Status = "Failed" });
            }
            var db = new SqliteDB();
            Loan.ExpireDate = DateTime.Now.AddDays(LoanRequest.LoanDays).Ticks;
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
                 
                Loan.LoanDate = DateTime.Now.Ticks;

                db.SaveLoanInfo(Loan);

                EmailController e = new EmailController();
                Task.Run(async () => await e.SendEmail(Loan.Email, "Device Loaned", string.Format("You have successfully loaned\nUrl: {0}\nUsername: {1}\nPassword: {2}, Expires: {3}",device.Host,Loan.GeneratedUsername,Loan.GeneratedPassword,DateTime.FromBinary(Loan.ExpireDate).ToString())));


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
        [UriFormat("/loan/return")]
        public IPostResponse ReturnCamera([FromContent]ReturnRequest Loan)
        {
            var db = new Database.SqliteDB();
            var dev = new DeviceLoan { GeneratedPassword = Loan.Password, Email = Loan.Email, Mac = Loan.SerialNumber };
            var dbdev = db.ValidLoanReturn(dev);
            if(dbdev == null)
            {
                return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 1, Message = "Not your camera" });
            }
            dbdev.Returned = true;
            dbdev.ExpireDate = 0;
            db.UpdateLoanInfo(dbdev);
            var axisdev = new AxisVapixLib.Device();
            var device = db.GetDevice(new DeviceList { SerialNumber = dbdev.Mac });
            if(device != null)
            {
                axisdev.Username = device.Username;
                axisdev.Password = device.Password;
                axisdev.Host = device.Host;
                //Task.Run(() => axisdev.FactoryDefault(AxisVapixLib.Device.FactoryDefaultMode.Hard)).Wait();
                return new PostResponse(PostResponse.ResponseStatus.Created, null, new ResponseData { Code = 0, Message = "Ok" });
            }
            return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 3, Message = "Device could not be found in local database" });      
        }

        [UriFormat("/current_loans")]
        public IPostResponse GetCurrentLoans([FromContent]LoanHistoryRequest Request)
        {
            var db = new SqliteDB();
            //TODO: Check for email..
            var loans = db.GetCurrentLoans();

            if (loans == null || loans.Count == 0)
                return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 1, Message = "No loans found" });

            var returnList = new List<LoanHistoryRespose>();
            foreach(var loan in loans)
            {
                if (loan.ExpireDate > DateTime.Now.Ticks)
                {
                    if (loan.Email == Request.Email)
                    {
                        var dev = db.GetDevice(new DeviceList { SerialNumber = loan.Mac });

                        returnList.Add(new LoanHistoryRespose { ExpireDate = DateTime.FromBinary(loan.ExpireDate).ToString(), ExpireDateTick = loan.ExpireDate, SerialNumber = loan.Mac, Username = loan.GeneratedUsername, Password = loan.GeneratedPassword, Host = dev.Host, Model = dev.Model });
                    }
                }
            }
            if (returnList.Count > 0)
                return new PostResponse(PostResponse.ResponseStatus.Created, null, returnList);
            return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Code = 1, Message = "No loans found" });

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
