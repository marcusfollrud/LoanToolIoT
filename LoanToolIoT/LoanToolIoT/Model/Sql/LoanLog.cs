using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
namespace LoanToolIoT.Model.Sql
{
    public sealed class LoanLog
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Email { get; set; }
        public string Reason { get; set; }
        public string GeneratedUsername  { get; set; }
        public string GeneratedPassword { get; set; }
        public long TimeStamp { get; set; }
    }
}
