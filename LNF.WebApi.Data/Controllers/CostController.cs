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
        public IEnumerable<CostItem> Get(int limit, int skip = 0)
        {
            if (limit > 100)
                throw new ArgumentOutOfRangeException("The parameter 'limit' must not be greater than 100.");

            var query = DA.Current.Query<Cost>().Skip(skip).Take(limit).OrderBy(x => x.TableNameOrDescription).ThenBy(x => x.RecordID).ThenBy(x => x.ChargeTypeID);

            var result = CreateCostItems(query);

            return result;
        }

        [Route("cost/{costId}")]
        public CostItem Get(int costId)
        {
            var query = DA.Current.Query<Cost>().Where(x => x.CostID == costId);
            var result = CreateCostItems(query);
            return result.FirstOrDefault();
        }

        [Route("cost/resource/{resourceId}")]
        public IEnumerable<CostItem> GetResourceCosts(int resourceId, DateTime? cutoff = null, int chargeTypeId = 0)
        {
            string[] tables = new[] { "ToolCost", "ToolOvertimeCost" };
            var result = GetEffectiveCosts(tables, cutoff, resourceId, chargeTypeId).ToList();
            return result;
        }

        private IList<CostItem> GetEffectiveCosts(string[] tables, DateTime? cutoff, int recordId, int chargeTypeId)
        {
            var query = DA.Current.Query<Cost>().Where(x =>
                tables.Contains(x.TableNameOrDescription)
                && (cutoff == null || x.EffDate < cutoff)
                && (chargeTypeId == 0 || x.ChargeTypeID == chargeTypeId)
                && (recordId == 0 || x.RecordID == recordId || x.RecordID == null || x.RecordID == 0));

            var items = CreateCostItems(query);

            var agg = items
                .GroupBy(x => new { x.ChargeTypeID, x.TableNameOrDescription, x.RecordID })
                .Select(g => new
                {
                    g.Key.ChargeTypeID,
                    g.Key.TableNameOrDescription,
                    g.Key.RecordID,
                    EffDate = g.Max(m => m.EffDate)
                });

            var result = items.Join(agg,
                    o => new { o.ChargeTypeID, o.TableNameOrDescription, o.RecordID, o.EffDate },
                    i => new { i.ChargeTypeID, i.TableNameOrDescription, i.RecordID, i.EffDate },
                    (o, i) => o)
                .OrderBy(x => x.TableNameOrDescription)
                .ThenBy(x => x.RecordID)
                .ThenBy(x => x.ChargeTypeID)
                .ThenBy(x => x.EffDate);

            return result.ToList();
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
