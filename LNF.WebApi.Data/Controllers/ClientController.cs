using LNF.Data;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    /// <summary>
    /// Resource for working with Client and ClientInfo items
    /// </summary>
    public class ClientController : ApiController
    {
        protected IActiveDataItemManager ActiveDataItemManager => DA.Use<IActiveDataItemManager>();
        protected IClientRemoteManager ClientRemoteManager => DA.Use<IClientRemoteManager>();

        /// <summary>
        /// Gets an unfiltered list of clients
        /// </summary>
        /// <param name="limit">The number of items to return (max 100)</param>
        /// <param name="skip">The number of items to skip (optional)</param>
        /// <returns>A list of ClientInfo items</returns>
        [Route("client")]
        public IEnumerable<ClientInfo> Get(int limit, int skip = 0)
        {
            if (limit > 100)
                throw new ArgumentOutOfRangeException("The parameter 'limit' must not be greater than 100.");

            var query = DA.Current.Query<ClientInfo>().Skip(skip).Take(limit).OrderBy(x => x.DisplayName);

            return query;
        }

        /// <summary>
        /// Gets currently active clients
        /// </summary>
        /// <param name="privs">The ClientPrivilege to filter by (optional)</param>
        /// <returns>A list of ClientInfo items</returns>
        [Route("client/active")]
        public IEnumerable<ClientInfo> GetActive(int privs = 0)
        {
            var query = DA.Current.Query<ClientInfo>().Where(x => x.ClientActive).OrderBy(x => x.DisplayName);

            if (privs == 0)
                return query;
            else
                return query.Where(x => (x.Privs & (ClientPrivilege)privs) > 0);
        }

        /// <summary>
        /// Gets all clients active during a date range
        /// </summary>
        /// <param name="sd">The range start date</param>
        /// <param name="ed">The range end date</param>
        /// <param name="privs">The ClientPrivilege to filter by (optional)</param>
        /// <returns>A list of ClientInfo items</returns>
        [Route("client/active/range")]
        public IEnumerable<ClientInfo> GetActiveByRange(DateTime sd, DateTime ed, int privs = 0)
        {
            var query = DA.Current.Query<ActiveLogClient>()
                .Where(x => x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DA.Current.Query<ClientInfo>(), o => o.ClientID, i => i.ClientID, (outer, inner) => inner);

            if (privs == 0)
                return join.OrderBy(x => x.DisplayName);
            else
                return join.Where(x => (x.Privs & (ClientPrivilege)privs) > 0).OrderBy(x => x.DisplayName);
        }

        /// <summary>
        /// Gets a single ClientInfo item using a unique id
        /// </summary>
        /// <param name="clientId">The id value</param>
        /// <returns>A ClientInfo item</returns>
        [Route("client/{clientId}")]
        public ClientInfo GetByClientID(int clientId)
        {
            return DA.Current.Single<ClientInfo>(clientId);
        }

        /// <summary>
        /// Gets a single ClientInfo item using a unique username
        /// </summary>
        /// <param name="username">The username value</param>
        /// <returns>A ClientModel item</returns>
        [Route("client/username/{username}")]
        public ClientItem GetByUserName(string username)
        {
            return ClientInfo.Find(username).GetClientItem();
        }

        /// <summary>
        /// Gets the currently logged in user using FormsAuthentication
        /// </summary>
        /// <returns>A ClientModel item</returns>
        [Route("client/current")]
        public ClientItem GetCurrent()
        {
            if (RequestContext.Principal.Identity.IsAuthenticated)
                return GetByUserName(RequestContext.Principal.Identity.Name);
            else
                return new ClientItem(); //return an empty object when not logged in
        }

        /// <summary>
        /// Inserts a new Client item
        /// </summary>
        /// <param name="model">The item on which this action is performed</param>
        /// <returns>The inserted Client item with ClientID set</returns>
        [Route("client")]
        public ClientItem Post([FromBody] ClientItem model)
        {
            //create a new client
            var client = new Client()
            {
                UserName = model.UserName,
                FName = model.FName,
                MName = model.MName,
                LName = model.LName,
                Privs = model.Privs,
                Communities = model.Communities,
                DemCitizenID = model.DemCitizenID,
                DemDisabilityID = model.DemCitizenID,
                DemEthnicID = model.DemEthnicID,
                DemGenderID = model.DemGenderID,
                DemRaceID = model.DemRaceID,
                TechnicalFieldID = model.TechnicalInterestID,
                IsChecked = model.IsChecked,
                IsSafetyTest = model.IsSafetyTest,
                Active = true
            };

            DA.Current.Insert(client);

            ActiveDataItemManager.Enable(client);
            client.ResetPassword();

            return client.Model<ClientItem>();
        }

        /// <summary>
        /// Updates an existing Client object
        /// </summary>
        /// <param name="model">The object on which this action is performed</param>
        /// <returns>True if the Client was modified, otherwise false</returns>
        [Route("client")]
        public bool Put([FromBody] ClientItem model)
        {
            if (model.ClientID == 0)
                return false;

            var client = DA.Current.Single<Client>(model.ClientID);

            if (model.ClientActive)
                ActiveDataItemManager.Enable(client);
            else
                ActiveDataItemManager.Disable(client);

            client.FName = model.FName;
            client.MName = model.MName;
            client.LName = model.LName;
            client.Privs = model.Privs;
            client.Communities = model.Communities;
            client.DemCitizenID = model.DemCitizenID;
            client.DemDisabilityID = model.DemCitizenID;
            client.DemEthnicID = model.DemEthnicID;
            client.DemGenderID = model.DemGenderID;
            client.DemRaceID = model.DemRaceID;
            client.TechnicalFieldID = model.TechnicalInterestID;
            client.IsChecked = model.IsChecked;
            client.IsSafetyTest = model.IsSafetyTest;

            return true;
        }

        /// <summary>
        /// Gets a list of ClientAccountModel items assigned to the specified client
        /// </summary>
        /// <param name="clientId">The unique ClientID</param>
        /// <returns>A list of ClientAccountInfo items</returns>
        [Route("client/{clientId}/accounts")]
        public IEnumerable<ClientAccountItem> GetClientAccounts(int clientId)
        {
            var query = DA.Current.Query<ClientAccountInfo>()
                .Where(x => x.ClientID == clientId)
                .OrderBy(x => x.EmailRank);

            return query.Model<ClientAccountItem>();
        }

        /// <summary>
        /// Gets currently active accounts assigned to the specified client
        /// </summary>
        /// <param name="clientId">The unique ClientID</param>
        /// <returns>A list of ClientAccountInfo items</returns>
        [Route("client/{clientId}/accounts/active")]
        public IEnumerable<ClientAccountItem> GetActiveClientAccounts(int clientId)
        {
            var query = DA.Current.Query<ClientAccountInfo>()
                .Where(x => x.ClientID == clientId && x.ClientActive && x.ClientOrgActive && x.ClientAccountActive)
                .OrderBy(x => x.EmailRank);

            return query.Model<ClientAccountItem>();
        }

        /// <summary>
        /// Gets all accounts active during a date range and assigned to the specified client
        /// </summary>
        /// <param name="clientId">The unique ClientID</param>
        /// <param name="sd">The range start date</param>
        /// <param name="ed">The range end date</param>
        /// <returns>A list of ClientAccountInfo items</returns>
        [Route("client/{clientId}/accounts/active/range")]
        public IEnumerable<ClientAccountItem> GetActiveByRangeClientAccounts(int clientId, DateTime sd, DateTime ed)
        {
            var query = DA.Current.Query<ActiveLogClientAccount>()
                .Where(x => x.ClientID == clientId && x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DA.Current.Query<ClientAccountInfo>(), o => o.ClientAccountID, i => i.ClientAccountID, (outer, inner) => inner);

            return join.OrderBy(x => x.EmailRank).Model<ClientAccountItem>();
        }

        /// <summary>
        /// Gets a list of ClientOrgInfo items assigned to the specified client
        /// </summary>
        /// <param name="clientId">The unique ClientID</param>
        /// <returns>A list of ClientOrgInfo items</returns>
        [Route("client/{clientId}/orgs")]
        public IEnumerable<ClientItem> GetClientOrgs(int clientId)
        {
            var query = DA.Current.Query<ClientOrgInfo>()
                .Where(x => x.ClientID == clientId)
                .OrderBy(x => x.EmailRank);

            return query.Model<ClientItem>();
        }

        /// <summary>
        /// Gets currently active orgs assigned to the specified client
        /// </summary>
        /// <param name="clientId">The unique ClientID</param>
        /// <returns>A list of ClientOrgInfo items</returns>
        [Route("client/{clientId}/orgs/active")]
        public IEnumerable<ClientItem> GetActiveClientOrgs(int clientId)
        {
            var query = DA.Current.Query<ClientOrgInfo>()
                .Where(x => x.ClientID == clientId && x.ClientActive && x.ClientOrgActive)
                .OrderBy(x => x.EmailRank);

            return query.Model<ClientItem>();
        }

        /// <summary>
        /// Gets orgs active during a date range and assigned to the specified client
        /// </summary>
        /// <param name="clientId">The unique ClientID</param>
        /// <param name="sd">The range start date</param>
        /// <param name="ed">The range end date</param>
        /// <returns>A list of ClientOrgInfo items</returns>
        [Route("client/{clientId}/org/active/range")]
        public IEnumerable<ClientItem> GetActiveByRangeClientOrgs(int clientId, DateTime sd, DateTime ed)
        {
            var query = DA.Current.Query<ActiveLogClientOrg>()
                .Where(x => x.ClientID == clientId && x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DA.Current.Query<ClientOrgInfo>(), o => o.ClientOrgID, i => i.ClientOrgID, (outer, inner) => inner);

            return join.OrderBy(x => x.EmailRank).Model<ClientItem>();
        }

        /// <summary>
        /// Gets a list of ClientRemote items active during a date range
        /// </summary>
        /// <param name="sd">The range start date</param>
        /// <param name="ed">The range end date</param>
        /// <returns>A list of ClientRemoteInfo items</returns>
        [Route("client/remote/active/range")]
        public IEnumerable<ClientRemoteItem> GetActiveClientRemotes(DateTime sd, DateTime ed)
        {
            var query = DA.Current.Query<ActiveLogClientRemote>()
                .Where(x => x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DA.Current.Query<ClientRemoteInfo>(), o => o.ClientRemoteID, i => i.ClientRemoteID, (outer, inner) => inner);

            return join.OrderBy(x => x.DisplayName).ThenBy(x => x.AccountName).Model<ClientRemoteItem>();
        }

        /// <summary>
        /// Inserts a new ClientRemote item
        /// </summary>
        /// <param name="model">The item on which this action is performed</param>
        /// <param name="period">The period during which the ClientRemote is active</param>
        /// <returns>The inserted ClientRemote item with ClientRemoteID set</returns>
        [Route("client/remote")]
        public ClientRemoteItem PostClientRemote([FromBody] ClientRemoteItem model, [FromUri] DateTime period)
        {
            var exists = DA.Current.Query<ActiveLogClientRemote>().Any(x =>
                x.ClientID == model.ClientID
                && x.RemoteClientID == model.RemoteClientID
                && x.AccountID == model.AccountID
                && (x.EnableDate < period.AddMonths(1) && (x.DisableDate == null || x.DisableDate > period)));

            if (!exists)
            {
                var client = DA.Current.Single<Client>(model.ClientID);
                var remoteClient = DA.Current.Single<Client>(model.RemoteClientID);
                var account = DA.Current.Single<Account>(model.AccountID);

                if (client == null)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("Cannot find a Client with ClientID = {0}", model.ClientID)));

                if (remoteClient == null)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("Cannot find a Client with ClientID = {0}", model.RemoteClientID)));

                if (account == null)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("Cannot find an Account with AccountID = {0}", model.AccountID)));

                //create a new ClientRemote
                var clientRemote = new ClientRemote()
                {
                    Client = client,
                    RemoteClient = remoteClient,
                    Account = account,
                    Active = true
                };

                DA.Current.Insert(clientRemote);
                ActiveDataItemManager.Enable(client);

                ClientRemoteManager.Enable(clientRemote, period);

                return clientRemote.Model<ClientRemoteItem>();
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "A duplicate record already exists."));
            }
        }

        /// <summary>
        /// Deletes a ClientRemote item
        /// </summary>
        /// <param name="clientRemoteId">The unique ClientRemoteID</param>
        /// <returns>True if the object was deleted, otherwise false</returns>
        [Route("client/remote/{clientRemoteId}")]
        public bool DeleteClientRemote(int clientRemoteId)
        {
            var cr = DA.Current.Single<ClientRemote>(clientRemoteId);

            if (cr == null)
                return false;

            // All the ActiveLog records for this ClientRemote
            var alogs = DA.Current.Query<ActiveLog>().Where(x =>
                x.TableName == "ClientRemote"
                && x.Record == clientRemoteId).ToList();

            // Delete any ActiveLogs
            DA.Current.Delete(alogs);

            // Delete the ClientRemote
            DA.Current.Delete(cr);

            return true;
        }
    }
}
