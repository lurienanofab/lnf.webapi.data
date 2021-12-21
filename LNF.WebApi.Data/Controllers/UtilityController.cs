using LNF.Util.AutoEnd;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class UtilityController : ApiController
    {
        [Route("utility/billing-checks/auto-end-problems")]
        public IEnumerable<AutoEndProblem> GetAutoEndProblems(DateTime period)
        {
            return ServiceProvider.Current.Utility.AutoEnd.GetAutoEndProblems(period);
        }

        [HttpGet, Route("utility/billing-checks/auto-end-problems/fix-all")]
        public int FixAllAutoEndProblems(DateTime period)
        {
            return ServiceProvider.Current.Utility.AutoEnd.FixAllAutoEndProblems(period);
        }

        [HttpGet, Route("utility/billing-checks/auto-end-problems/fix")]
        public int FixAutoEndProblem(DateTime period, int reservationId)
        {
            return ServiceProvider.Current.Utility.AutoEnd.FixAutoEndProblem(period, reservationId);
        }
    }
}
