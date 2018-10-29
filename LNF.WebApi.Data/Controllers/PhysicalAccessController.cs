using LNF.Models.PhysicalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class PhysicalAccessController : ApiController
    {
        [Route("physical-access/area/{area?}")]
        public IEnumerable<Badge> GetArea(string area = null)
        {
            var inlab = ServiceProvider.Current.PhysicalAccess.CurrentlyInArea();

            IList<Badge> result;

            if (string.IsNullOrEmpty(area))
                result = inlab.ToList();
            else
                result = inlab.Where(x => x.CurrentAreaName == GetAreaName(area)).ToList();

            return result.OrderBy(x => x.CurrentAreaName).ThenBy(x => x.CurrentAccessTime).ToList();
        }

        private string GetAreaName(string alias)
        {
            switch (alias)
            {
                case "cleanroom":
                    return "Clean Room";
                case "wetchem":
                case "robin":
                    return "Wet Chemistry";
                default:
                    throw new Exception($"Unknown alias: {alias}");
            }
        }
    }
}
