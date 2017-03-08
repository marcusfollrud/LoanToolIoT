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
using LoanToolIoT.Database;
using LoanToolIoT.Model;
using LoanToolIoT.Model.Sql;
using LoanToolIoT.Model.Web;

namespace LoanToolIoT.Controllers
{
    [RestController(InstanceCreationType.PerCall)]
    public sealed class HostController
    {
        SqliteDB db = new SqliteDB();
        /// <summary>
        /// Lists the Host that are in the database
        /// </summary>
        /// <returns></returns>
        [UriFormat("/hosts/list")]
        public IGetResponse ListHosts()
        {
            return new GetResponse(GetResponse.ResponseStatus.OK,db.GetHostLists());
        }

        /// <summary>
        /// Adds a host to the Database
        /// </summary>
        /// <param name="Host"></param>
        /// <returns></returns>
        [UriFormat("/hosts/save")]
        public IPostResponse SaveHost([FromContent]DeviceHostList Host)
        {
            //TODO: Add checks for valid hosts.
            if (db.SaveHost(Host))
                return new PostResponse(PostResponse.ResponseStatus.Created, null, new ResponseData { Status = "Host added." });
            return new PostResponse(PostResponse.ResponseStatus.Conflict, null, new ResponseData { Status = "Host failed to add." });
        }

        [UriFormat("/hosts/delete/{id}")]
        public IGetResponse DeleteHost(int id)
        {
            db.DeleteHost(id);
            return new GetResponse(GetResponse.ResponseStatus.OK, null, new ResponseData { Status = "OK" });
        }
    }
}
