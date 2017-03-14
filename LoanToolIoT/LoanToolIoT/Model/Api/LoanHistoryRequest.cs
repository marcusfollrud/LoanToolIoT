using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Api
{
    public sealed class LoanHistoryRequest
    {
        public string Email { get; set; }
    }

    public sealed class LoanHistoryRespose
    {
        public string SerialNumber { get; set; }
        public long ExpireDateTick { get; set; }
        public string ExpireDate { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Model { get; set; }
    }
}
