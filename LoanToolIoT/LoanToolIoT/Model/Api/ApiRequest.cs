using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Api
{
    public sealed class ApiRequest
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public string Secret { get; set; }
        public string ApiKey { get; set; }
    }
}
