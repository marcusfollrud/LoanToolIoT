using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Sql
{
    /// <summary>
    /// Settings class for Options.
    /// </summary>
    public sealed class Settings
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
