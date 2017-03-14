using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Api
{
    public sealed class LoanResponse
    {
        public string SerialNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DateExpire { get; set; }
        public long DateExpireTick { get; set; }
        public string Host { get; set; }
    }

    public sealed class LoanRequest
    {
        public string SerialNumber { get; set; }
        public string Email { get; set; }
        public string Reason { get; set; }
        public int LoanDays { get; set; }
    }
}
