using LNF.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class DryBoxController : ApiController
    {
        [Route("drybox")]
        public IEnumerable<IDryBox> Get()
        {
            return ServiceProvider.Current.Data.DryBox.GetDryBoxes();
        }

        [Route("drybox")]
        public bool Put(IDryBox model)
        {
            return ServiceProvider.Current.Data.DryBox.UpdateDryBox(model);
        }

        [Route("drybox/assignment/active")]
        public IEnumerable<IDryBoxAssignment> GetActiveAssignments(DateTime sd, DateTime ed)
        {
            return ServiceProvider.Current.Data.DryBox.GetActiveAssignments(sd, ed);
        }

        [HttpGet, Route("drybox/approve")]
        public bool Approve(int dryBoxAssignmentId, int modifiedByClientId)
        {
            ServiceProvider.Current.Data.DryBox.Approve(dryBoxAssignmentId, modifiedByClientId);
            return true;
        }

        [HttpGet, Route("webapi/data/drybox/clientaccount/{clientAccountId}/exists")]
        public bool ClientAccountHasDryBox([FromUri] int clientAccountId)
        {
            return ServiceProvider.Current.Data.DryBox.ClientAccountHasDryBox(clientAccountId);
        }

        [HttpGet, Route("webapi/data/drybox/clientorg/{clientOrgId}/exists")]
        public bool ClientOrgHasDryBox([FromUri] int clientOrgId)
        {
            return ServiceProvider.Current.Data.DryBox.ClientOrgHasDryBox(clientOrgId);
        }

        [Route("webapi/data/drybox/{dryBoxId}/assignment")]
        public IDryBoxAssignment GetCurrentAssignment([FromUri] int dryBoxId)
        {
            return ServiceProvider.Current.Data.DryBox.GetCurrentAssignment(dryBoxId);
        }

        [Route("webapi/data/drybox/clientorg/{dryBoxId}/clientaccount")]
        public IClientAccount GetDryBoxClientAccount([FromUri] int clientOrgId)
        {
            return ServiceProvider.Current.Data.DryBox.GetDryBoxClientAccount(clientOrgId);
        }

        [HttpGet, Route("webapi/data/drybox/{dryBoxId}/account/active")]
        public bool? IsAccountActive([FromUri] int dryBoxId)
        {
            return ServiceProvider.Current.Data.DryBox.IsAccountActive(dryBoxId);
        }
    }
}
