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
        protected IActiveDataItemManager ActiveDataItemManager => ServiceProvider.Current.ActiveDataItemManager;
        protected IClientRemoteManager ClientRemoteManager => new ClientRemoteManager(ServiceProvider.Current);

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
        /// Gets currently active clients, and optionaly with the specified privilege.
        /// </summary>
        [Route("client/active")]
        public IEnumerable<IClient> GetActive(int privs = 0)
        {
            var query = DA.Current.Query<ClientInfo>().Where(x => x.ClientActive).OrderBy(x => x.DisplayName);

            if (privs == 0)
                return query.CreateModels<IClient>();
            else
                return query.Where(x => (x.Privs & (ClientPrivilege)privs) > 0).CreateModels<IClient>();
        }

        /// <summary>
        /// Gets the currently logged in user based on FormsAuthentication.
        /// </summary>
        [Route("client/current")]
        public IClient GetCurrent()
        {
            // this endpoint is used by the scheduler when displaying helpdesk tickets - do not delete!

            IClient result = null;

            if (RequestContext.Principal.Identity.IsAuthenticated)
                result = DA.Current.Query<ClientInfo>().Where(x => x.UserName == RequestContext.Principal.Identity.Name).CreateModels<IClient>().FirstOrDefault();

            if (result == null)
                result = new ClientItem(); //return an empty object when not logged in

            return result;
        }

        /// <summary>
        /// Gets all clients active during a date range, and optionaly with the specified privilege.
        /// </summary>
        [Route("client/active/range")]
        public IEnumerable<IClient> GetActiveInRange(DateTime sd, DateTime ed, int privs = 0)
        {
            var query = DA.Current.Query<ActiveLogClient>()
                .Where(x => x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DA.Current.Query<ClientInfo>(), o => o.ClientID, i => i.ClientID, (outer, inner) => inner);

            if (privs == 0)
                return join.CreateModels<IClient>().OrderBy(x => x.DisplayName);
            else
                return join.Where(x => (x.Privs & (ClientPrivilege)privs) > 0).CreateModels<IClient>().OrderBy(x => x.DisplayName);
        }

        /// <summary>
        /// Gets a single client item using a unique id.
        /// </summary>
        [Route("client")]
        public IClient GetClient(int clientId)
        {
            var client = DA.Current.Query<ClientInfo>().FirstOrDefault(x => x.ClientID == clientId);
            return client.CreateModel<IClient>();
        }

        /// <summary>
        /// Gets a single client item using a unique username.
        /// </summary>
        [Route("client")]
        public IClient GetClient(string username)
        {
            var client = DA.Current.Query<ClientInfo>().FirstOrDefault(x => x.UserName == username);
            return client.CreateModel<IClient>();
        }

        /// <summary>
        /// Inserts a new Client.
        /// </summary>
        [Route("client")]
        public IClient Post([FromBody] ClientItem model)
        {
            //create a new client
            var client = new Client()
            {
                UserName = model.UserName,
                FName = model.FName,
                MName = model.MName,
                LName = model.LName,
                DemCitizenID = model.DemCitizenID,
                DemGenderID = model.DemGenderID,
                DemRaceID = model.DemRaceID,
                DemEthnicID = model.DemEthnicID,
                DemDisabilityID = model.DemDisabilityID,
                Privs = model.Privs,
                Communities = model.Communities,
                TechnicalFieldID = model.TechnicalInterestID,
                IsChecked = model.IsChecked,
                IsSafetyTest = model.IsSafetyTest,
                Active = true
            };

            DA.Current.Insert(client);

            ActiveDataItemManager.Enable(client);
            client.ResetPassword();

            return client.CreateModel<IClient>();
        }

        [Route("client/{clientId}/demographics")]
        public LNF.Models.Data.ClientDemographics GetClientDemographics(int clientId)
        {
            var c = DA.Current.Single<ClientInfo>(clientId);

            if (c == null) return null;

            return new LNF.Models.Data.ClientDemographics
            {
                ClientID = c.ClientID,
                UserName = c.UserName,
                LName = c.LName,
                FName = c.FName,
                DemCitizenID = c.DemCitizenID,
                DemCitizenName = c.DemCitizenName,
                DemGenderID = c.DemGenderID,
                DemGenderName = c.DemGenderName,
                DemRaceID = c.DemRaceID,
                DemRaceName = c.DemRaceName,
                DemEthnicID = c.DemEthnicID,
                DemEthnicName = c.DemEthnicName,
                DemDisabilityID = c.DemDisabilityID,
                DemDisabilityName = c.DemDisabilityName
            };
        }

        /// <summary>
        /// Updates an existing Client.
        /// </summary>
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
        /// Gets a list of accounts assigned to the specified client.
        /// </summary>
        [Route("client/{clientId}/accounts")]
        public IEnumerable<IClientAccount> GetClientAccounts(int clientId)
        {
            var query = DA.Current.Query<ClientAccountInfo>()
                .Where(x => x.ClientID == clientId)
                .OrderBy(x => x.EmailRank);

            return query.CreateModels<IClientAccount>();
        }

        /// <summary>
        /// Gets currently active accounts assigned to the specified client.
        /// </summary>
        [Route("client/{clientId}/accounts/active")]
        public IEnumerable<IClientAccount> GetActiveClientAccounts(int clientId)
        {
            var query = DA.Current.Query<ClientAccountInfo>()
                .Where(x => x.ClientID == clientId && x.ClientActive && x.ClientOrgActive && x.ClientAccountActive)
                .OrderBy(x => x.EmailRank);

            return query.CreateModels<IClientAccount>();
        }

        /// <summary>
        /// Gets all accounts assigned to the specified client and active during the specified date range.
        /// </summary>
        [Route("client/{clientId}/accounts/active/range")]
        public IEnumerable<IClientAccount> GetActiveClientAccountsInRange(int clientId, DateTime sd, DateTime ed)
        {
            var query = DA.Current.Query<ActiveLogClientAccount>()
                .Where(x => x.ClientID == clientId && x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DA.Current.Query<ClientAccountInfo>(), o => o.ClientAccountID, i => i.ClientAccountID, (outer, inner) => inner);

            return join.OrderBy(x => x.EmailRank).CreateModels<IClientAccount>();
        }

        /// <summary>
        /// Gets a list of client orgs assigned to the specified client.
        /// </summary>
        [Route("client/{clientId}/orgs")]
        public IEnumerable<IClient> GetClientOrgs(int clientId)
        {
            var query = DA.Current.Query<ClientOrgInfo>()
                .Where(x => x.ClientID == clientId)
                .OrderBy(x => x.EmailRank);

            return query.CreateModels<IClient>();
        }

        /// <summary>
        /// Gets currently active orgs assigned to the specified client.
        /// </summary>
        [Route("client/{clientId}/orgs/active")]
        public IEnumerable<IClient> GetActiveClientOrgs(int clientId)
        {
            var query = DA.Current.Query<ClientOrgInfo>()
                .Where(x => x.ClientID == clientId && x.ClientActive && x.ClientOrgActive)
                .OrderBy(x => x.EmailRank);

            return query.CreateModels<IClient>();
        }

        /// <summary>
        /// Gets all orgs assigned to the specified client and active during the specified date range.
        /// </summary>
        [Route("client/{clientId}/org/active/range")]
        public IEnumerable<IClient> GetActiveClientOrgsInRange(int clientId, DateTime sd, DateTime ed)
        {
            var query = DA.Current.Query<ActiveLogClientOrg>()
                .Where(x => x.ClientID == clientId && x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DA.Current.Query<ClientOrgInfo>(), o => o.ClientOrgID, i => i.ClientOrgID, (outer, inner) => inner);

            return join.OrderBy(x => x.EmailRank).CreateModels<IClient>();
        }

        /// <summary>
        /// Gets a list of client remote items active during a date range.
        /// </summary>
        [Route("client/remote/active/range")]
        public IEnumerable<IClientRemote> GetActiveClientRemotesInRange(DateTime sd, DateTime ed)
        {
            var query = DA.Current.Query<ActiveLogClientRemote>()
                .Where(x => x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DA.Current.Query<ClientRemoteInfo>(), o => o.ClientRemoteID, i => i.ClientRemoteID, (outer, inner) => inner);

            return join.CreateModels<IClientRemote>().OrderBy(x => x.DisplayName).ThenBy(x => x.AccountName);
        }

        /// <summary>
        /// Inserts a new client remote item.
        /// </summary>
        [Route("client/remote")]
        public IClientRemote PostClientRemote([FromBody] ClientRemoteItem model, [FromUri] DateTime period)
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

                return clientRemote.CreateModel<IClientRemote>();
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "A duplicate record already exists."));
            }
        }

        /// <summary>
        /// Deletes a client remote item.
        /// </summary>
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
