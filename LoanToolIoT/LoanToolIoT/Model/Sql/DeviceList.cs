using SQLite.Net.Attributes;
namespace LoanToolIoT.Model.Sql
{
    public sealed class DeviceList
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Model { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirmwareVersion { get; set; }
        public string DeviceType { get; set; }
        public string SerialNumber { get; set; }
        public string Architecture { get; set; }
        public string Host { get; set; }
        public bool Activated { get; set; }
    }
}
