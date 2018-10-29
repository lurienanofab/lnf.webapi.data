using LNF.Models.Data.Utility.BillingChecks;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class UtilityController : ApiController
    {
        [Route("utility/billing-checks/auto-end-problems")]
        public IEnumerable<AutoEndProblem> GetAutoEndProblems(DateTime period)
        {
            var dt = DA.Command()
                .Param("Period", period)
                .FillDataTable("dbo.BillingChecks_FindAutoEndProblems");

            var result = new List<AutoEndProblem>();

            foreach (DataRow dr in dt.Rows)
            {
                var item = new AutoEndProblem
                {
                    ReservationID = dr.Field<int>("ReservationID"),
                    AutoEndType = dr.Field<string>("AutoEndType"),
                    AutoEndReservation = dr.Field<bool>("AutoEndReservation"),
                    AutoEndResource = TimeSpan.FromMinutes(dr.Field<int>("AutoEndResource")),
                    Duration = TimeSpan.FromMinutes(dr.Field<double>("Duration")),
                    EndDateTime = dr.Field<DateTime>("EndDateTime"),
                    ActualEndDateTime = dr.Field<DateTime?>("ActualEndDateTime"),
                    ActualEndDateTimeExpected = dr.Field<DateTime>("ActualEndDateTimeExpected"),
                    Diff = TimeSpan.FromSeconds(dr.Field<int>("DiffSeconds")),
                    ActualEndDateTimeCorrected = dr.Field<DateTime>("ActualEndDateTimeCorrected"),
                    ChargeMultiplier = dr.Field<double>("ChargeMultiplier")
                };

                result.Add(item);
            }

            return result;
        }

        [HttpGet, Route("utility/billing-checks/auto-end-problems/fix-all")]
        public int FixAllAutoEndProblems(DateTime period)
        {
            var count = DA.Command()
                .Param("Period", period)
                .ExecuteScalar<int>("dbo.BillingChecks_FixAutoEndProblems");

            return count;
        }

        [HttpGet, Route("utility/billing-checks/auto-end-problems/fix")]
        public int FixAutoEndProblem(DateTime period, int reservationId)
        {
            var count = DA.Command()
                .Param("Period", period)
                .Param("ReservationID", reservationId)
                .ExecuteScalar<int>("dbo.BillingChecks_FixAutoEndProblems");

            return count;
        }
    }
}
