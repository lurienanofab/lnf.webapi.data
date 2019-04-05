using LNF.Impl.DependencyInjection.Web;
using Microsoft.Owin;
using Owin;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartup(typeof(LNF.WebApi.Data.Startup))]

namespace LNF.WebApi.Data
{
    /// <summary>
    /// This class must be local to the application or there is an issue with routing when IIS resets.
    /// </summary>
    public class Startup : ApiOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            var ioc = new IOC();
            ServiceProvider.Current = ioc.Resolver.GetInstance<IProvider>();

            base.Configuration(app);

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}