using LNF.Data;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class CostController : ApiController
    {
        [HttpGet, Route("cost")]
        public IEnumerable<ICost> Get(int limit, int skip = 0)
        {
            return ServiceProvider.Current.Data.Cost.GetCosts(limit, skip);
        }

        [HttpGet, Route("cost/{costId}")]
        public ICost GetById(int costId)
        {
            return ServiceProvider.Current.Data.Cost.GetCost(costId);
        }

        [HttpGet, Route("cost/resource/{resourceId}")]
        public IEnumerable<ICost> GetResourceCosts(int resourceId, DateTime? cutoff = null, int chargeTypeId = 0)
        {
            return ServiceProvider.Current.Data.Cost.FindToolCosts(resourceId, cutoff, chargeTypeId);
        }
    }
}
