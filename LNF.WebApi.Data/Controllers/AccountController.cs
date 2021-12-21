using LNF.Data;
using LNF.Impl.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    /// <summary>
    /// Resource for working with Account and AccountInfo items
    /// </summary>
    public class AccountController : ApiControllerBase
    {
        public AccountController(IProvider provider) : base(provider) { }

        /// <summary>
        /// Gets an unfiltered list of accounts
        /// </summary>
        /// <param name="limit">The number of items to return (max 100)</param>
        /// <param name="skip">The number of items to skip (optional)</param>
        /// <returns>A list of AccountInfo items</returns>
        [Route("account")]
        public IEnumerable<AccountInfo> Get(int limit, int skip = 0)
        {
            if (limit > 100)
                throw new ArgumentOutOfRangeException("The parameter 'limit' must not be greater than 100.");

            var query = DataSession.Query<AccountInfo>().Skip(skip).Take(limit).OrderBy(x => x.AccountName);

            return query.ToList();
        }

        /// <summary>
        /// Gets currently active accounts
        /// </summary>
        /// <param name="limit">The number of items to return (max 100)</param>
        /// <param name="skip">The number of items to skip (optional)</param>
        /// <returns>A list of AccountInfo items</returns>
        [Route("account/active")]
        public IEnumerable<AccountInfo> GetActive(int limit, int skip = 0)
        {
            if (limit > 100)
                throw new ArgumentOutOfRangeException("The parameter 'limit' must not be greater than 100.");

            var query = DataSession.Query<AccountInfo>().Where(x => x.AccountActive).Skip(skip).Take(limit).OrderBy(x => x.AccountName);

            return query;
        }


        /// <summary>
        /// Gets accounts active during a date range
        /// </summary>
        /// <param name="limit">The number of items to return (max 100)</param>
        /// <param name="sd">The range start date</param>
        /// <param name="ed">The range end date</param>
        /// <param name="clientId">The ClientID to filter by (optional) - when provided only accounts assigned to the client will be selected</param>
        /// <param name="skip">The number of items to skip (optional)</param>
        /// <returns>A list of AccountInfo items</returns>
        [Route("account/active/range")]
        public IEnumerable<AccountInfo> GetActiveByRange(int limit, DateTime sd, DateTime ed, int clientId = 0, int skip = 0)
        {
            if (limit > 100)
                throw new ArgumentOutOfRangeException("The parameter 'limit' must not be greater than 100.");

            if (clientId == 0)
            {
                var query = DataSession.Query<ActiveLogAccount>()
                    .Where(x => x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

                var join = query.Join(DataSession.Query<AccountInfo>(), o => o.AccountID, i => i.AccountID, (outer, inner) => inner);

                return join.OrderBy(x => x.AccountName).Skip(skip).Take(limit).ToList();
            }
            else
            {
                var query = DataSession.Query<ActiveLogClientAccount>()
                    .Where(x => x.ClientID == clientId && (x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd)));

                var join = query.Join(DataSession.Query<AccountInfo>(), o => o.AccountID, i => i.AccountID, (outer, inner) => inner);

                return join.OrderBy(x => x.AccountName).Skip(skip).Take(limit).ToList();
            }
        }

        /// <summary>
        /// Gets a single AccountInfo item using a unique id
        /// </summary>
        /// <param name="accountId">The id value</param>
        /// <returns>An AccountInfo item</returns>
        [Route("account/{accountId}")]
        public IAccount GetByAccountID(int accountId)
        {
            return CreateAccountItems(DataSession.Query<AccountInfo>().Where(x => x.AccountID == accountId)).FirstOrDefault();
        }

        /// <summary>
        /// Gets a single AccountInfo item using a ShortCode
        /// </summary>
        /// <param name="shortcode">The account ShortCode (a six digit account identifier)</param>
        /// <returns>An AccountInfo item</returns>
        [Route("account/shortcode/{shortcode}")]
        public AccountInfo GetByShortCode(string shortcode)
        {
            return DataSession.Query<AccountInfo>().FirstOrDefault(x => x.ShortCode.Trim() == shortcode.Trim());
        }

        private IEnumerable<IAccount> CreateAccountItems(IQueryable<AccountInfo> query)
        {
            return query.ToList();
            //return query.ToList().Select(x => new AccountItem
            //{
            //    AccountID = x.AccountID,
            //    OrgID = x.OrgID,
            //    OrgName = x.OrgName,
            //    AccountTypeID = x.AccountTypeID,
            //    AccountTypeName = x.AccountTypeName,
            //    AccountName = x.AccountName,
            //    AccountNumber = x.AccountNumber,
            //    ShortCode = x.ShortCode,
            //    FundingSourceID = x.FundingSourceID,
            //    FundingSourceName = x.FundingSourceName,
            //    TechnicalFieldID = x.TechnicalFieldID,
            //    TechnicalFieldName = x.TechnicalFieldName,
            //    SpecialTopicID = x.SpecialTopicID,
            //    SpecialTopicName = x.SpecialTopicName,
            //    BillAddressID = x.BillAddressID,
            //    ShipAddressID = x.ShipAddressID,
            //    InvoiceNumber = x.InvoiceNumber,
            //    InvoiceLine1 = x.InvoiceLine1,
            //    InvoiceLine2 = x.InvoiceLine2,
            //    PoEndDate = x.PoEndDate,
            //    PoInitialFunds = x.PoInitialFunds,
            //    PoRemainingFunds = x.PoRemainingFunds,
            //    AccountActive = x.AccountActive
            //}).ToList();
        }
    }
}
