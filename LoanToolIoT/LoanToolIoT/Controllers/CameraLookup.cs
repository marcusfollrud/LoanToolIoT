using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System.Diagnostics;
using Windows.Foundation;
using AxisVapixLib;
using AxisVapixLib.Parameter;
using LoanToolIoT.Model.Sql;
using System.Net;

namespace LoanToolIoT.Controllers
{
    [RestController(InstanceCreationType.Singleton)]
    public sealed class CameraLookup
    {
        
        [UriFormat("/find")]
        public IAsyncOperation<IGetResponse> findCameras()
        {

            //Start with finding the host list to parse
            Database.SqliteDB db = new Database.SqliteDB();
            //Get current passwords.
            var validPasswordString = db.GetSetting(new Settings { Name = "validCameraPasswords" });
            string[] validPassword;
            if (validPasswordString == null)
                validPassword = new string[] { "pass", "adp2013" };
            else
                validPassword = validPasswordString.Value.Split(new char[] { ',' });
            validPasswordString = null;
            var hosts = db.GetHostLists();
            /// Value is intered like:
            /// IP Start
            /// IP End
            /// Port Start
            /// Port End
            if(hosts.Count == 0)
            {
                return Task.FromResult<IGetResponse>(new GetResponse(GetResponse.ResponseStatus.OK, hosts)).AsAsyncOperation();
            }

            List<string> connectionUrls = new List<string>();
            foreach (DeviceHostList item in hosts)
            {
                var ips = new IPRange(item.IPRange);
                foreach(IPAddress ip in ips.GetAllIP())
                {
                    var startPort = item.StartPort;
                    while (startPort <= item.EndPort)
                    {
                        connectionUrls.Add(ip.ToString() + ":" + startPort);
                        startPort++;
                    }
                }
            }


            //Ip addresses scoped.
            foreach(string url in connectionUrls)
            {
                foreach (string password in validPassword)
                {
                    Debug.WriteLine(url);
                    AxisVapixLib.Device dev = new AxisVapixLib.Device
                    {
                        Host = url,
                        Username = "root",
                        Password = password,
                    };
                    dev.CredentialCache = new NetworkCredential("root", password);

                    var valid = ValidateDevice(dev);
                    while (valid.Status != AsyncStatus.Completed)
                    {
                        Task.Delay(10).Wait();
                    }

                    if (valid.GetResults())
                    {


                        //Connection exists. Check if it's an Axis camera.
                        Parameter p = new Parameter(dev);

                        List<AxisParameter> parameters = new List<AxisParameter>();
                        parameters.Add(new AxisParameter() { Name = "Properties.System.SerialNumber" });
                        parameters.Add(new AxisParameter() { Name = "Properties.Firmware.Version" });
                        parameters.Add(new AxisParameter() { Name = "Brand.ProdShortName" });
                        parameters.Add(new AxisParameter() { Name = "Brand.ProdType" });
                        parameters.Add(new AxisParameter() { Name = "Properties.System.Architecture" });



                        Debug.WriteLine("Testing Password " + password);
                        dev.Password = password;
                        bool succeed = false;
                        var receivedParameter = GetParameters(p, parameters);
                        while (receivedParameter.Status != AsyncStatus.Completed)
                        {
                            Task.Delay(10).Wait();
                        }
                        if (receivedParameter.GetResults() != null)
                        {

                            DeviceList device = new DeviceList();
                            foreach (AxisParameter Params in receivedParameter.GetResults())
                            {
                                switch (Params.Name)
                                {
                                    case "Properties.System.SerialNumber":
                                        device.SerialNumber = Params.Value;
                                        break;
                                    case "Properties.Firmware.Version":
                                        device.FirmwareVersion = Params.Value;
                                        break;
                                    case "Brand.ProdShortName":
                                        device.Model = Params.Value;
                                        break;
                                    case "Properties.System.Architecture":
                                        device.Architecture = Params.Value;
                                        break;
                                    case "Brand.ProdType":
                                        device.DeviceType = Params.Value;
                                        break;
                                }
                            }
                            Debug.WriteLine("Password in cache " + dev.CredentialCache.Password);
                            device.Username = dev.CredentialCache.UserName;
                            device.Password = dev.CredentialCache.Password;
                            device.Host = dev.Host;
                            Debug.WriteLine(string.Format("Found Device: {0} - {1} ({2}) - {3} {4}", device.Model, device.SerialNumber, dev.Host, dev.CredentialCache.UserName, dev.CredentialCache.Password));
                            var dbdevice = db.GetDevice(device);

                            if (dbdevice == null)
                            {
                                //Insert
                                db.SaveDevice(device);
                                Debug.WriteLine(string.Format("Inserting device: {0} {1} {2}", device.SerialNumber, device.Model, device.Host));

                                //Unknown device. Factory default.
                                Debug.WriteLine("Factory defaulting " + device.SerialNumber);
                                Task.Run(() => dev.FactoryDefault(Device.FactoryDefaultMode.Hard));

                            }
                            else
                            {
                                //Update
                                device.ID = dbdevice.ID;
                                db.SaveDevice(device);
                                Debug.WriteLine(string.Format("Updating device: {0} {1} {2}", device.SerialNumber, device.Model, device.Host));
                            }
                            dbdevice = null;
                            succeed = true;
                        }
                        if (!succeed)
                        {
                            Debug.WriteLine("Could not connect to " + dev.Host);
                        }
                        else
                        {
                            dev.ResetHTTPClient();
                            break;
                        }
                        //TODO ADD Password checking
                        //TODO CREATE TABLE FOR POSSIBLE PASSWORDS


                    }
                    dev.ResetHTTPClient();
                }
                //Reset the HTTP client to allow connections to a new IP.
                
            }

            /*Debug.WriteLine("Find cameras");
            AxisVapixLib.Device dev = new AxisVapixLib.Device
            {
                Host = "96.89.207.211:5014",
                Username = "root",
                Password = "adp2013",

            };

            var valid = ValidateDevice(dev);
            while(valid.Status != AsyncStatus.Completed)
            {
                Task.Delay(1000).Wait();
            }
            var result = valid.GetResults();

            return Task.FromResult<IGetResponse>(new GetResponse(GetResponse.ResponseStatus.OK, result)).AsAsyncOperation();
            //var test = AsyncInfoFactory Task.FromResult<bool>(dev.ValidateConnect()).AsAsyncOperation<bool>(); //Task.FromResult<bool>(await dev.ValidateConnect()).AsAsyncOperation();
            */
            return null;
        }

        private IAsyncOperation<bool> ValidateDevice(AxisVapixLib.Device Device)
        {
            try
            {
                return Task.Run(async () => await Device.ValidateConnect()).AsAsyncOperation<bool>();
            }
            catch(WebException ex)
            {
                Debug.WriteLine(ex.Message);
                return Task.Run(() => false).AsAsyncOperation<bool>();
            }
        }

        private IAsyncOperation<List<AxisParameter>> GetParameters(Parameter Parameter, List<AxisParameter> ParameterList)
        {
            return Task.Run(async () => await Parameter.GetParametersAsync(ParameterList)).AsAsyncOperation<List<AxisParameter>>();
        }

        /// <summary>
        /// Ip Range class for detecting the different IP ranges
        /// Found on StackOverFlow
        /// http://stackoverflow.com/questions/4172677/c-enumerate-ip-addresses-in-a-range/
        /// </summary>
        internal class IPRange
        {
            public IPRange(string ipRange)
            {
                if (ipRange == null)
                    throw new ArgumentNullException();

                if (!TryParseCIDRNotation(ipRange) && !TryParseSimpleRange(ipRange))
                    throw new ArgumentException();
            }

            public IEnumerable<IPAddress> GetAllIP()
            {
                int capacity = 1;
                for (int i = 0; i < 4; i++)
                    capacity *= endIP[i] - beginIP[i] + 1;

                List<IPAddress> ips = new List<IPAddress>(capacity);
                for (int i0 = beginIP[0]; i0 <= endIP[0]; i0++)
                {
                    for (int i1 = beginIP[1]; i1 <= endIP[1]; i1++)
                    {
                        for (int i2 = beginIP[2]; i2 <= endIP[2]; i2++)
                        {
                            for (int i3 = beginIP[3]; i3 <= endIP[3]; i3++)
                            {
                                ips.Add(new IPAddress(new byte[] { (byte)i0, (byte)i1, (byte)i2, (byte)i3 }));
                            }
                        }
                    }
                }

                return ips;
            }

            /// <summary>
            /// Parse IP-range string in CIDR notation.
            /// For example "12.15.0.0/16".
            /// </summary>
            /// <param name="ipRange"></param>
            /// <returns></returns>
            private bool TryParseCIDRNotation(string ipRange)
            {
                string[] x = ipRange.Split('/');

                if (x.Length != 2)
                    return false;

                byte bits = byte.Parse(x[1]);
                uint ip = 0;
                String[] ipParts0 = x[0].Split('.');
                for (int i = 0; i < 4; i++)
                {
                    ip = ip << 8;
                    ip += uint.Parse(ipParts0[i]);
                }

                byte shiftBits = (byte)(32 - bits);
                uint ip1 = (ip >> shiftBits) << shiftBits;

                if (ip1 != ip) // Check correct subnet address
                    return false;

                uint ip2 = ip1 >> shiftBits;
                for (int k = 0; k < shiftBits; k++)
                {
                    ip2 = (ip2 << 1) + 1;
                }

                beginIP = new byte[4];
                endIP = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    beginIP[i] = (byte)((ip1 >> (3 - i) * 8) & 255);
                    endIP[i] = (byte)((ip2 >> (3 - i) * 8) & 255);
                }

                return true;
            }

            /// <summary>
            /// Parse IP-range string "12.15-16.1-30.10-255"
            /// </summary>
            /// <param name="ipRange"></param>
            /// <returns></returns>
            private bool TryParseSimpleRange(string ipRange)
            {
                String[] ipParts = ipRange.Split('.');

                beginIP = new byte[4];
                endIP = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    string[] rangeParts = ipParts[i].Split('-');

                    if (rangeParts.Length < 1 || rangeParts.Length > 2)
                        return false;

                    beginIP[i] = byte.Parse(rangeParts[0]);
                    endIP[i] = (rangeParts.Length == 1) ? beginIP[i] : byte.Parse(rangeParts[1]);
                }

                return true;
            }

            private byte[] beginIP;
            private byte[] endIP;
        }
    }
}