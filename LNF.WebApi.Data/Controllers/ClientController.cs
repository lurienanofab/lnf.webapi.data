using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    /// <summary>
    /// Resource for working with Client and ClientInfo items
    /// </summary>
    public class ClientController : ApiController
    {
        protected IProvider Provider => ServiceProvider.Current;

        //protected IActiveDataItemManager ActiveDataItemManager => ServiceProvider.Current.ActiveDataItemManager;
        //protected IClientRemoteManager ClientRemoteManager => new ClientRemoteManager(ServiceProvider.Current); //ServiceProvider.Current.Data.ClientRemote

        /// <summary>
        /// Gets an unfiltered list of clients
        /// </summary>
        /// <param name="limit">The number of items to return (max 100)</param>
        /// <param name="skip">The number of items to skip (optional)</param>
        /// <returns>A list of ClientInfo items</returns>
        [Route("client")]
        public IEnumerable<IClient> Get(int limit, int skip = 0)
        {
            return ServiceProvider.Current.Data.Client.GetClients(limit, skip);
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

            Provider.Data.ActiveLog.Enable("Client", client.ClientID);
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
                Provider.Data.ActiveLog.Enable("Client", client.ClientID);
            else
                Provider.Data.ActiveLog.Disable("Client", client.ClientID);

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
            return ServiceProvider.Current.Data.Client.GetClientOrgs(clientId);
        }

        /// <summary>
        /// Gets currently active orgs assigned to the specified client.
        /// </summary>
        [Route("client/{clientId}/orgs/active")]
        public IEnumerable<IClient> GetActiveClientOrgs(int clientId)
        {
            return ServiceProvider.Current.Data.Client.GetActiveClientOrgs(clientId);
        }

        /// <summary>
        /// Gets all orgs assigned to the specified client and active during the specified date range.
        /// </summary>
        [Route("client/{clientId}/org/active/range")]
        public IEnumerable<IClient> GetActiveClientOrgsInRange(int clientId, DateTime sd, DateTime ed)
        {
            return ServiceProvider.Current.Data.Client.GetActiveClientOrgs(clientId, sd, ed);
        }

        /// <summary>
        /// Inserts a new client remote item.
        /// </summary>
        [HttpPost, Route("client/remote")]
        public IClientRemote InsertClientRemote([FromBody] ClientRemoteItem model, DateTime period)
        {
            ServiceProvider.Current.Data.Client.InsertClientRemote(model, period);
            return model;
        }

        /// <summary>
        /// Deletes a client remote item.
        /// </summary>
        [Route("client/remote/{clientRemoteId}")]
        public bool DeleteClientRemote(int clientRemoteId, DateTime period)
        {
            ServiceProvider.Current.Data.Client.DeleteClientRemote(clientRemoteId, period);
            return true;
        }

        /// <summary>
        /// Gets a list of client remote items active during a date range.
        /// </summary>
        [Route("client/remote/active/range")]
        public IEnumerable<IClientRemote> GetActiveClientRemotes(DateTime sd, DateTime ed)
        {
            return ServiceProvider.Current.Data.Client.GetActiveClientRemotes(sd, ed);
        }

        [Route("client/priv")]
        public IEnumerable<IPriv> GetPrivs()
        {
            return ServiceProvider.Current.Data.Client.GetPrivs();
        }

        [Route("client/community")]
        public IEnumerable<ICommunity> GetCommunities()
        {
            return ServiceProvider.Current.Data.Client.GetCommunities();
        }
    }
}
