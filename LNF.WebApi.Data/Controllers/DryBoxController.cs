using LNF.Data;
using LNF.WebApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class DryBoxController : ApiControllerBase
    {
        public DryBoxController(IProvider provider) : base(provider) { }

        [HttpGet, Route("drybox")]
        public List<DryBox> Get()
        {
            return Provider.Data.DryBox.GetDryBoxes().ToList();
        }

        [HttpPut, Route("drybox")]
        public bool Put([FromBody] DryBox model)
        {
            return Provider.Data.DryBox.UpdateDryBox(model);
        }

        [HttpGet, Route("drybox/{dryBoxId}/assignment")]
        public DryBoxAssignmentInfo GetCurrentAssignment([FromUri] int dryBoxId)
        {
            return Provider.Data.DryBox.GetCurrentDryBoxAssignment(dryBoxId);
        }

        [HttpPost, Route("drybox/{dryBoxId}/request")]
        public DryBoxAssignmentInfo RequestDryBox([FromBody] DryBoxRequest request)
        {
            return Provider.Data.DryBox.Request(request);
        }

        [HttpGet, Route("drybox/assignment/{dryBoxAssignmentId}/reject")]
        public DryBoxAssignmentInfo RejectDryBoxAssignment([FromUri] int dryBoxAssignmentId)
        {
            return Provider.Data.DryBox.Reject(dryBoxAssignmentId);
        }

        [HttpPut, Route("drybox/assignment/{dryBoxAssignmentId}/approve")]
        public DryBoxAssignmentInfo ApproveDryBoxAssignment([FromUri] int dryBoxAssignmentId, [FromBody] DryBoxAssignmentUpdate update)
        {
            return Provider.Data.DryBox.Approve(dryBoxAssignmentId, update);
        }

        [HttpPut, Route("drybox/assignment/{dryBoxAssignmentId}")]
        public DryBoxAssignmentInfo PutDryBoxAssignment([FromUri] int dryBoxAssignmentId, [FromBody] DryBoxAssignmentUpdate update)
        {
            return Provider.Data.DryBox.UpdateDryBoxAssignment(dryBoxAssignmentId, update);
        }

        [HttpGet, Route("drybox/{dryBoxId}/account/active")]
        public bool? IsAccountActive([FromUri] int dryBoxId)
        {
            return Provider.Data.DryBox.IsAccountActive(dryBoxId);
        }

        [HttpGet, Route("drybox/assignment/{dryBoxAssignmentId}/remove")]
        public bool RemoveDryBoxAssignment([FromUri] int dryBoxAssignmentId, int modifiedByClientId)
        {
            return Provider.Data.DryBox.Remove(dryBoxAssignmentId, modifiedByClientId);
        }

        [HttpGet, Route("drybox/assignment")]
        public DryBoxAssignment GetDryBoxAssignment(int dryBoxAssignmentId)
        {
            return Provider.Data.DryBox.GetDryBoxAssignment(dryBoxAssignmentId);
        }

        [HttpGet, Route("drybox/assignment/active")]
        public List<DryBoxAssignment> GetActiveAssignments(DateTime sd, DateTime ed)
        {
            return Provider.Data.DryBox.GetActiveDryBoxAssignments(sd, ed).ToList();
        }

        [HttpGet, Route("drybox/clientaccount/{clientAccountId}/exists")]
        public bool ClientAccountHasDryBox([FromUri] int clientAccountId)
        {
            return Provider.Data.DryBox.ClientAccountHasDryBox(clientAccountId);
        }

        [HttpGet, Route("drybox/clientorg/{clientOrgId}/exists")]
        public bool ClientOrgHasDryBox([FromUri] int clientOrgId)
        {
            return Provider.Data.DryBox.ClientOrgHasDryBox(clientOrgId);
        }

        [HttpGet, Route("drybox/clientorg/{clientOrgId}/clientaccount")]
        public ClientAccount GetDryBoxClientAccount([FromUri] int clientOrgId)
        {
            return CreateClientAccount(Provider.Data.DryBox.GetDryBoxClientAccount(clientOrgId));
        }

        [HttpGet, Route("drybox/assignment/current")]
        public List<DryBoxAssignmentInfo> GetCurrentDryBoxAssignments()
        {
            var result = Provider.Data.DryBox.GetCurrentDryBoxAssignments().ToList();

            return result;
        }

        [HttpGet, Route("drybox/assignment/{dryBoxAssignmentId}/cancel-request")]
        public DryBoxAssignmentInfo CancelRequest(int dryBoxAssignmentId)
        {
            return Provider.Data.DryBox.CancelRequest(dryBoxAssignmentId);
        }

        [HttpGet, Route("drybox/assignment/{dryBoxAssignmentId}/request-remove")]
        public DryBoxAssignmentInfo RequestRemoveDryBox([FromUri] int dryBoxAssignmentId)
        {
            return Provider.Data.DryBox.RequestRemove(dryBoxAssignmentId);
        }

        [HttpGet, Route("drybox/history/{dryBoxName}")]
        public IEnumerable<DryBoxHistory> GetDryBoxHistoryByName(string dryBoxName)
        {
            return Provider.Data.DryBox.GetDryBoxHistory(dryBoxName);
        }

        [HttpGet, Route("drybox/history/client/{clientId}")]
        public IEnumerable<DryBoxHistory> GetDryBoxHistoryByClient(int clientId)
        {
            return Provider.Data.DryBox.GetDryBoxHistory(clientId);
        }

        private ClientAccount CreateClientAccount(IClientAccount x)
        {
            return new ClientAccount
            {
                ClientAccountID = x.ClientAccountID,
                IsDefault = x.IsDefault,
                Manager = x.Manager,
                ClientAccountActive = x.ClientAccountActive,
                Communities = x.Communities,
                IsChecked = x.IsChecked,
                IsSafetyTest = x.IsSafetyTest,
                RequirePasswordReset = x.RequirePasswordReset,
                ClientActive = x.ClientActive,
                TechnicalInterestID = x.TechnicalInterestID,
                TechnicalInterestName = x.TechnicalInterestName,
                DepartmentID = x.DepartmentID,
                DepartmentName = x.DepartmentName,
                RoleID = x.RoleID,
                RoleName = x.RoleName,
                MaxChargeTypeID = x.MaxChargeTypeID,
                MaxChargeTypeName = x.MaxChargeTypeName,
                EmailRank = x.EmailRank,
                ClientID = x.ClientID,
                UserName = x.UserName,
                LName = x.LName,
                MName = x.MName,
                FName = x.FName,
                Privs = x.Privs,
                ClientOrgID = x.ClientOrgID,
                ClientAddressID = x.ClientAddressID,
                Phone = x.Phone,
                Email = x.Email,
                IsManager = x.IsManager,
                IsFinManager = x.IsFinManager,
                SubsidyStartDate = x.SubsidyStartDate,
                NewFacultyStartDate = x.NewFacultyStartDate,
                ClientOrgActive = x.ClientOrgActive,
                AccountID = x.AccountID,
                AccountName = x.AccountName,
                AccountNumber = x.AccountNumber,
                ShortCode = x.ShortCode,
                BillAddressID = x.BillAddressID,
                ShipAddressID = x.ShipAddressID,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceLine1 = x.InvoiceLine1,
                InvoiceLine2 = x.InvoiceLine2,
                PoEndDate = x.PoEndDate,
                PoInitialFunds = x.PoInitialFunds,
                PoRemainingFunds = x.PoRemainingFunds,
                AccountActive = x.AccountActive,
                FundingSourceID = x.FundingSourceID,
                FundingSourceName = x.FundingSourceName,
                TechnicalFieldID = x.TechnicalFieldID,
                TechnicalFieldName = x.TechnicalFieldName,
                SpecialTopicID = x.SpecialTopicID,
                SpecialTopicName = x.SpecialTopicName,
                AccountTypeID = x.AccountTypeID,
                AccountTypeName = x.AccountTypeName,
                OrgID = x.OrgID,
                OrgName = x.OrgName,
                DefClientAddressID = x.DefClientAddressID,
                DefBillAddressID = x.DefBillAddressID,
                DefShipAddressID = x.DefShipAddressID,
                NNINOrg = x.NNINOrg,
                PrimaryOrg = x.PrimaryOrg,
                OrgActive = x.OrgActive,
                OrgTypeID = x.OrgTypeID,
                OrgTypeName = x.OrgTypeName,
                ChargeTypeID = x.ChargeTypeID,
                ChargeTypeName = x.ChargeTypeName,
                ChargeTypeAccountID = x.ChargeTypeAccountID
            };
        }
    }
}
