using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Api
{
    public sealed class ResetDatabase
    {
        public TokenRequest Token { get; set;}
    }

    public sealed class TokenRequest
    {
        public string APIKey { get; set; }
        public string Token { get; set; }
        public string Url { get; set; }
    }

}
