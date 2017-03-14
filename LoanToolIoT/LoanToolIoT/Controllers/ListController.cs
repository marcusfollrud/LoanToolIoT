using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;
using System.Threading.Tasks;
using LoanToolIoT.Database;

namespace LoanToolIoT.Controllers
{
    [RestController(InstanceCreationType.PerCall)]
    public sealed class ListController
    {

        [UriFormat("/camera_list")]
        public IGetResponse GetCameraList()
        {
            //Todo: List Cameras from database.
            var db = new SqliteDB();
            var devices = db.GetDevices();
            for (int i = 0; i < devices.Count; i++)
            {
                devices[i].Username = "hidden";
                devices[i].Password = "hidden";
            }

            return new GetResponse(GetResponse.ResponseStatus.OK,
                devices
                );
        }
    }
}
