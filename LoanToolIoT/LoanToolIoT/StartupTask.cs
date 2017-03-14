using Restup.Webserver.File;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;
using Windows.ApplicationModel.Background;
using LoanToolIoT.Controllers;
using LoanToolIoT.Database;
// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace LoanToolIoT
{

    public sealed class StartupTask : IBackgroundTask
    {
        private HttpServer _httpServer;
        private BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            SqliteDB db = new SqliteDB();
            db.CreateTables(); //Tables should only be created during startup.
            var restRouteHandler = new RestRouteHandler();


            //Register the controllers
            restRouteHandler.RegisterController<ListController>();
            restRouteHandler.RegisterController<CameraLookup>();
            restRouteHandler.RegisterController<HostController>();
            restRouteHandler.RegisterController<LoanController>();



            var configuration = new HttpServerConfiguration()
                .ListenOnPort(8800)
                .RegisterRoute("api", restRouteHandler)
                .RegisterRoute(new StaticFileRouteHandler(@"Web"))
                .EnableCors(); // allow cors requests on all origins
            //  .EnableCors(x => x.AddAllowedOrigin("http://specificserver:<listen-port>"));

            var httpServer = new HttpServer(configuration);
            _httpServer = httpServer;

            //Start background cron.
            var cron = new Cron.CameraReset();
            cron.StartTasks();


            await httpServer.StartServerAsync();
        }
    }
}
