using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class CostController : ApiController
    {
        [Route("cost")]
        public IEnumerable<ICost> Get(int limit, int skip = 0)
        {
            return ServiceProvider.Current.Data.Cost.GetCosts(limit, skip);
        }

        [Route("cost/{costId}")]
        public ICost Get(int costId)
        {
            return ServiceProvider.Current.Data.Cost.GetCost(costId);
        }

        [Route("cost/resource/{resourceId}")]
        public IEnumerable<ICost> GetResourceCosts(int resourceId, DateTime? cutoff = null, int chargeTypeId = 0)
        {
            return ServiceProvider.Current.Data.Cost.FindToolCosts(resourceId, cutoff, chargeTypeId);
        }

        private IEnumerable<CostItem> CreateCostItems(IQueryable<Cost> query)
        {
            var join = query.Join(DA.Current.Query<ChargeType>(), o => o.ChargeTypeID, i => i.ChargeTypeID, (o, i) => new { Cost = o, ChargeType = i });

            return join.Select(x => new CostItem
            {
                CostID = x.Cost.CostID,
                ChargeTypeID = x.ChargeType.ChargeTypeID,
                ChargeTypeName = x.ChargeType.ChargeTypeName,
                TableNameOrDescription = x.Cost.TableNameOrDescription,
                RecordID = x.Cost.RecordID.GetValueOrDefault(),
                AcctPer = x.Cost.AcctPer,
                AddVal = x.Cost.AddVal,
                MulVal = x.Cost.MulVal,
                EffDate = x.Cost.EffDate,
                CreatedDate = x.Cost.CreatedDate
            }).ToList();
        }
    }
}
