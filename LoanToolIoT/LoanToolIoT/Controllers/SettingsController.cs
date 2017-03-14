using LoanToolIoT.Model.Api;
using LoanToolIoT.Model.Sql;
using LoanToolIoT.Model.Web;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Controllers
{
    public sealed class SettingsController
    {
        ApiController ap = new ApiController();
        Database.SqliteDB db = new Database.SqliteDB();

        [UriFormat("/settings/save")]
        public IPostResponse SaveSetting([FromContent]SettingsRequest Setting)
        {
            var validate = ap.ValidateAPI(Setting.API.Token, Setting.API.ApiKey, "/settings/save");
            if (validate.Code == 0)
            {
                db.SaveSetting(Setting.Setting);
                return new PostResponse(PostResponse.ResponseStatus.Created, null, new ResponseData { Code = 0, Message = "Setting saved" });
            }
            else
            {
                return new PostResponse(PostResponse.ResponseStatus.Conflict, null, validate);
            }
        }

        [UriFormat("/settings/get")]
        public IGetResponse GetSetting([FromContent]SettingsRequest Setting)
        {
            var validate = ap.ValidateAPI(Setting.API.Token, Setting.API.ApiKey, "/settings/get");
            if (validate.Code == 0)
            {
                var set = db.GetSetting(Setting.Setting);
                return new GetResponse(GetResponse.ResponseStatus.OK, null, set);
            }
            else
            {
                return new GetResponse(GetResponse.ResponseStatus.NotFound, null, validate);
            }
        }
    }
}
