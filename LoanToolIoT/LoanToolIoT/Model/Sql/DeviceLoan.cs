using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Sql
{
    public sealed class DeviceLoan
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get;set;}
        public string Mac { get; set; }
        public long LoanDate { get; set; }
        public long ExpireDate { get; set; }
        public string LoanReason { get; set; }
        public string Email { get; set; }
        public string GeneratedUsername { get; set; }
        public string GeneratedPassword { get; set; }
        public bool Returned { get; set; }
    }
}
