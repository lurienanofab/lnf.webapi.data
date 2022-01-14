using LNF.Data;
using LNF.Impl;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    /// <summary>
    /// Resource for working with Client and ClientInfo items
    /// </summary>
    public class ClientController : ApiControllerBase
    {
        public ClientController(IProvider provider) : base(provider) { }

        [HttpGet, Route("client/list")]
        public IEnumerable<ClientListItem> GetClients()
        {
            return Provider.Data.Client.GetClients();
        }

        /// <summary>
        /// Gets an unfiltered list of clients
        /// </summary>
        /// <param name="limit">The number of items to return (max 100)</param>
        /// <param name="skip">The number of items to skip (optional)</param>
        /// <returns>A list of ClientInfo items</returns>
        [HttpGet, Route("client")]
        public IEnumerable<ClientItem> Get(int limit, int skip = 0)
        {
            return Provider.Data.Client.GetClients(limit, skip).Select(CreateClientItem).ToList();
        }

        /// <summary>
        /// Gets currently active clients, and optionaly with the specified privilege.
        /// </summary>
        [HttpGet, Route("client/active")]
        public IEnumerable<ClientItem> GetActive(int privs = 0)
        {
            var query = DataSession.Query<ClientInfo>().Where(x => x.ClientActive).OrderBy(x => x.DisplayName);

            if (privs == 0)
                return query.Select(CreateClientItem).ToList();
            else
                return query.Where(x => (x.Privs & (ClientPrivilege)privs) > 0).Select(CreateClientItem).ToList();
        }

        /// <summary>
        /// Gets the currently logged in user based on FormsAuthentication.
        /// </summary>
        [HttpGet, Route("client/current")]
        [AllowAnonymous]
        public ClientItem GetCurrent()
        {
            // this endpoint is used by the scheduler when displaying helpdesk tickets - do not delete!

            // this is needed because AllowAnonymous means the ApiAuthenticationAttribute (configured in Startup) won't be used
            var handler = new FormsAuthHandler();
            var authenticated = handler.Authenticate(ActionContext);

            ClientItem result = null;

            if (authenticated && RequestContext.Principal.Identity.IsAuthenticated)
                result = CreateClientItem(DataSession.Query<ClientInfo>().FirstOrDefault(x => x.UserName == RequestContext.Principal.Identity.Name));
            else
                result = new ClientItem(); //return an empty object when not logged in

            return result;
        }

        /// <summary>
        /// Gets all clients active during a date range, and optionaly with the specified privilege.
        /// </summary>
        [HttpGet, Route("client/active/range")]
        public IEnumerable<ClientItem> GetActiveInRange(DateTime sd, DateTime ed, int privs = 0)
        {
            var query = DataSession.Query<ActiveLogClient>()
                .Where(x => x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DataSession.Query<ClientInfo>(), o => o.ClientID, i => i.ClientID, (outer, inner) => inner);

            if (privs == 0)
                return join.OrderBy(x => x.DisplayName).Select(CreateClientItem).ToList();
            else
                return join.Where(x => (x.Privs & (ClientPrivilege)privs) > 0).OrderBy(x => x.DisplayName).Select(CreateClientItem).ToList();
        }

        /// <summary>
        /// Gets a single client item using a unique id.
        /// </summary>
        [HttpGet, Route("client/id/{clientId}")]
        public ClientItem GetClientById(int clientId)
        {
            var client = CreateClientItem(DataSession.Query<ClientInfo>().FirstOrDefault(x => x.ClientID == clientId));
            return client;
        }

        /// <summary>
        /// Gets a single client item using a unique username.
        /// </summary>
        [HttpGet, Route("client/username/{username}")]
        public ClientItem GetClientByUsername(string username)
        {
            var client = CreateClientItem(DataSession.Query<ClientInfo>().FirstOrDefault(x => x.UserName == username));
            return client;
        }

        /// <summary>
        /// Inserts a new Client.
        /// </summary>
        [HttpPost, Route("client")]
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
                TechnicalInterestID = model.TechnicalInterestID,
                IsChecked = model.IsChecked,
                IsSafetyTest = model.IsSafetyTest,
                Active = true
            };

            DataSession.Insert(client);

            Provider.Data.ActiveLog.Enable(client);

            var c = client.CreateModel<IClient>();
            var result = CreateClientItem(c);

            return result;
        }

        [HttpGet, Route("client/{clientId}/demographics")]
        public ClientDemographics GetClientDemographics(int clientId)
        {
            return Provider.Data.Client.GetClientDemographics(clientId);
        }

        /// <summary>
        /// Updates an existing Client.
        /// </summary>
        [HttpPut, Route("client")]
        public bool Put([FromBody] ClientItem model)
        {
            if (model.ClientID == 0)
                return false;

            var client = DataSession.Single<Client>(model.ClientID);

            if (model.ClientActive)
                Provider.Data.ActiveLog.Enable(client);
            else
                Provider.Data.ActiveLog.Disable(client);

            client.FName = model.FName;
            client.MName = model.MName;
            client.LName = model.LName;
            client.Privs = model.Privs;
            client.Communities = model.Communities;
            client.TechnicalInterestID = model.TechnicalInterestID;
            client.IsChecked = model.IsChecked;
            client.IsSafetyTest = model.IsSafetyTest;

            return true;
        }

        /// <summary>
        /// Gets a list of accounts assigned to the specified client.
        /// </summary>
        [HttpGet, Route("client/{clientId}/accounts")]
        public IEnumerable<IClientAccount> GetClientAccounts(int clientId)
        {
            var query = DataSession.Query<ClientAccountInfo>()
                .Where(x => x.ClientID == clientId)
                .OrderBy(x => x.EmailRank);

            return query.ToList();
        }

        /// <summary>
        /// Gets currently active accounts assigned to the specified client.
        /// </summary>
        [HttpGet, Route("client/{clientId}/accounts/active")]
        public IEnumerable<IClientAccount> GetActiveClientAccounts(int clientId)
        {
            var query = DataSession.Query<ClientAccountInfo>()
                .Where(x => x.ClientID == clientId && x.ClientActive && x.ClientOrgActive && x.ClientAccountActive)
                .OrderBy(x => x.EmailRank);

            return query.ToList();
        }

        /// <summary>
        /// Gets all accounts assigned to the specified client and active during the specified date range.
        /// </summary>
        [HttpGet, Route("client/{clientId}/accounts/active/range")]
        public IEnumerable<IClientAccount> GetActiveClientAccountsInRange(int clientId, DateTime sd, DateTime ed)
        {
            var query = DataSession.Query<ActiveLogClientAccount>()
                .Where(x => x.ClientID == clientId && x.EnableDate < ed && (x.DisableDate == null || x.DisableDate > sd));

            var join = query.Join(DataSession.Query<ClientAccountInfo>(), o => o.ClientAccountID, i => i.ClientAccountID, (outer, inner) => inner);

            return join.OrderBy(x => x.EmailRank).ToList();
        }

        /// <summary>
        /// Gets a list of client orgs assigned to the specified client.
        /// </summary>
        [HttpGet, Route("client/{clientId}/orgs")]
        public IEnumerable<ClientItem> GetClientOrgs(int clientId)
        {
            return Provider.Data.Client.GetClientOrgs(clientId).Select(CreateClientItem).ToList();
        }

        /// <summary>
        /// Gets currently active orgs assigned to the specified client.
        /// </summary>
        [HttpGet, Route("client/{clientId}/orgs/active")]
        public IEnumerable<ClientItem> GetActiveClientOrgs(int clientId)
        {
            return Provider.Data.Client.GetActiveClientOrgs(clientId).Select(CreateClientItem).ToList();
        }

        /// <summary>
        /// Gets all orgs assigned to the specified client and active during the specified date range.
        /// </summary>
        [HttpGet, Route("client/{clientId}/org/active/range")]
        public IEnumerable<ClientItem> GetActiveClientOrgsInRange(int clientId, DateTime sd, DateTime ed)
        {
            return Provider.Data.Client.GetActiveClientOrgs(clientId, sd, ed).Select(CreateClientItem).ToList();
        }

        /// <summary>
        /// Inserts a new client remote item.
        /// </summary>
        [HttpPost, Route("client/remote")]
        public IClientRemote InsertClientRemote([FromBody] ClientRemoteItem model, DateTime period)
        {
            Provider.Data.Client.InsertClientRemote(model, period);
            return model;
        }

        /// <summary>
        /// Deletes a client remote item.
        /// </summary>
        [HttpDelete, Route("client/remote/{clientRemoteId}")]
        public bool DeleteClientRemote(int clientRemoteId, DateTime period)
        {
            Provider.Data.Client.DeleteClientRemote(clientRemoteId, period);
            return true;
        }

        /// <summary>
        /// Gets a list of client remote items active during a date range.
        /// </summary>
        [HttpGet, Route("client/remote/active/range")]
        public IEnumerable<IClientRemote> GetActiveClientRemotes(DateTime sd, DateTime ed)
        {
            return Provider.Data.Client.GetActiveClientRemotes(sd, ed);
        }

        [HttpGet, Route("client/priv")]
        public IEnumerable<IPriv> GetPrivs()
        {
            return Provider.Data.Client.GetPrivs();
        }

        [HttpGet, Route("client/community")]
        public IEnumerable<ICommunity> GetCommunities()
        {
            return Provider.Data.Client.GetCommunities();
        }

        [HttpGet, Route("client/manager/active/list")]
        public IEnumerable<GenericListItem> AllActiveManagers()
        {
            return Provider.Data.Client.AllActiveManagers();
        }

        private ClientItem CreateClientItem(IClient x)
        {
            return new ClientItem
            {
                ClientID = x.ClientID,
                UserName = x.UserName,
                FName = x.FName,
                MName = x.MName,
                LName = x.LName,
                Privs = x.Privs,
                Communities = x.Communities,
                IsChecked = x.IsChecked,
                IsSafetyTest = x.IsSafetyTest,
                RequirePasswordReset = x.RequirePasswordReset,
                ClientActive = x.ClientActive,
                TechnicalInterestID = x.TechnicalInterestID,
                TechnicalInterestName = x.TechnicalInterestName,
                ClientOrgID = x.ClientOrgID,
                Phone = x.Phone,
                Email = x.Email,
                IsManager = x.IsManager,
                IsFinManager = x.IsFinManager,
                SubsidyStartDate = x.SubsidyStartDate,
                NewFacultyStartDate = x.NewFacultyStartDate,
                ClientAddressID = x.ClientAddressID,
                ClientOrgActive = x.ClientOrgActive,
                DepartmentID = x.DepartmentID,
                DepartmentName = x.DepartmentName,
                RoleID = x.RoleID,
                RoleName = x.RoleName,
                MaxChargeTypeID = x.MaxChargeTypeID,
                MaxChargeTypeName = x.MaxChargeTypeName,
                EmailRank = x.EmailRank,
                ChargeTypeAccountID = x.ChargeTypeAccountID,
                ChargeTypeID = x.ChargeTypeID,
                ChargeTypeName = x.ChargeTypeName,
                DefBillAddressID = x.DefBillAddressID,
                DefClientAddressID = x.DefClientAddressID,
                DefShipAddressID = x.DefShipAddressID,
                NNINOrg = x.NNINOrg,
                OrgActive = x.OrgActive,
                OrgID = x.OrgID,
                OrgName = x.OrgName,
                OrgTypeID = x.OrgTypeID,
                OrgTypeName = x.OrgTypeName,
                PrimaryOrg = x.PrimaryOrg
            };
        }

    }
}
