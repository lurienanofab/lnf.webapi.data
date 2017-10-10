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
            using (var adap = DA.Current.GetAdapter())
            {
                var dt = adap.AddParameter("@Period", period)
                    .CommandTypeStoredProcedure()
                    .FillDataTable("dbo.BillingChecks_FindAutoEndProblems");


                var result = new List<AutoEndProblem>();

                foreach(DataRow dr in dt.Rows)
                {
                    var item = new AutoEndProblem();
                    item.ReservationID = dr.Field<int>("ReservationID");
                    item.AutoEndType = dr.Field<string>("AutoEndType");
                    item.AutoEndReservation = dr.Field<bool>("AutoEndReservation");
                    item.AutoEndResource = TimeSpan.FromMinutes(dr.Field<int>("AutoEndResource"));
                    item.Duration = TimeSpan.FromMinutes(dr.Field<double>("Duration"));
                    item.EndDateTime = dr.Field<DateTime>("EndDateTime");
                    item.ActualEndDateTime = dr.Field<DateTime?>("ActualEndDateTime");
                    item.ActualEndDateTimeExpected = dr.Field<DateTime>("ActualEndDateTimeExpected");
                    item.Diff = TimeSpan.FromSeconds(dr.Field<int>("DiffSeconds"));
                    item.ActualEndDateTimeCorrected = dr.Field<DateTime>("ActualEndDateTimeCorrected");
                    item.ChargeMultiplier = dr.Field<double>("ChargeMultiplier");
                    result.Add(item);
                }

                return result;
            }
        }

        [HttpGet, Route("utility/billing-checks/auto-end-problems/fix-all")]
        public int FixAllAutoEndProblems(DateTime period)
        {
            using (var adap = DA.Current.GetAdapter())
            {
                var count = adap.CommandTypeStoredProcedure()
                    .AddParameter("@Period", period)
                    .ExecuteScalar<int>("dbo.BillingChecks_FixAutoEndProblems");

                return count;
            }
        }

        [HttpGet, Route("utility/billing-checks/auto-end-problems/fix")]
        public int FixAutoEndProblem(DateTime period, int reservationId)
        {
            using (var adap = DA.Current.GetAdapter())
            {
                var count = adap.CommandTypeStoredProcedure()
                    .AddParameter("@Period", period)
                    .AddParameter("@ReservationID", reservationId)
                    .ExecuteScalar<int>("dbo.BillingChecks_FixAutoEndProblems");

                return count;
            }
        }
    }
}
