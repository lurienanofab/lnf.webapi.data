using Microsoft.Owin;

[assembly: OwinStartup(typeof(LNF.WebApi.Data.Startup))]

namespace LNF.WebApi.Data
{
    /// <summary>
    /// This class must be local to the application or there is an issue with routing when IIS resets.
    /// </summary>
    public class Startup : ApiOwinStartup { }
}