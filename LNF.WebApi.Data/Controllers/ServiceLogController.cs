using LNF.Models.Data;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    /// <summary>
    /// Endpoint provider for creating log entries
    /// </summary>
    public class ServiceLogController : ApiController
    {
        /// <summary>
        /// Gets a list of ServiceLog items
        /// </summary>
        /// <param name="limit">The number of items to return (max 100)</param>
        /// <param name="skip">The number of items to skip (optional)</param>
        /// <param name="id">The unique MessageID of the log entry (optional)</param>
        /// <param name="service">The service that created the log entry (optional)</param>
        /// <param name="subject">The subject of the log entry (optional)</param>
        /// <returns></returns>
        [Route("servicelog/{id?}")]
        public IEnumerable<IServiceLog> Get(int limit, int skip = 0, Guid? id = null, string service = null, string subject = null)
        {
            return ServiceProvider.Current.Data.ServiceLog.GetServiceLogs(limit, 0, id, service, subject);
        }

        /// <summary>
        /// Inserts a new ServiceLog item
        /// </summary>
        /// <param name="model">The item on which this action is performed</param>
        /// <returns>The inserted ServiceLog item with ServiceLogID and MessageID set</returns>
        [Route("servicelog")]
        public IServiceLog Post([FromBody] ServiceLogItem model)
        {
            ServiceProvider.Current.Data.ServiceLog.InsertServiceLog(model);
            return model;
        }

        /// <summary>
        /// Updates a service log entry by adding an additional data node
        /// </summary>
        /// <param name="id">The unique MessageID</param>
        /// <param name="data">The data to be added to the log entry. This should be encoded in the request body as '=[data]' (a key value pair with a blank key)</param>
        /// <returns>True if the entry was found and updated, otherwise false</returns>
        [HttpPut, Route("servicelog/{id}")]
        public bool AppendData([FromUri] Guid id, [FromBody] string data)
        {
            return ServiceProvider.Current.Data.ServiceLog.UpdateServiceLog(id, data);
        }
    }
}
