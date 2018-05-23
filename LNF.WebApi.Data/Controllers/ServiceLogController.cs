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
        public IEnumerable<ServiceLogItem> Get(int limit, int skip = 0, Guid? id = null, string service = null, string subject = null)
        {
            if (limit > 100)
                throw new ArgumentOutOfRangeException("The parameter 'limit' must not be greater than 100.");

            var query = DA.Current.Query<ServiceLog>()
                .Where(x =>
                    (service != null ? x.ServiceName == service : true)
                    && (subject != null ? x.LogSubject == subject : true)
                    && (id.HasValue ? x.MessageID == id.Value : true)
                );

            return query.Skip(skip).Take(limit).Model<ServiceLogItem>();
        }

        /// <summary>
        /// Inserts a new ServiceLog item
        /// </summary>
        /// <param name="model">The item on which this action is performed</param>
        /// <returns>The inserted ServiceLog item with ServiceLogID and MessageID set</returns>
        [Route("servicelog")]
        public ServiceLogItem Post([FromBody] ServiceLogItem model)
        {
            ServiceLog item = new ServiceLog()
            {
                ServiceName = model.ServiceName,
                LogDateTime = model.LogDateTime,
                LogSubject = model.LogSubject,
                LogLevel = model.LogLevel,
                LogMessage = model.LogMessage,
                MessageID = model.MessageID,
                Data = model.Data
            };

            DA.Current.Insert(item);

            return item.Model<ServiceLogItem>();
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
            ServiceLog serviceLog = DA.Current.Query<ServiceLog>().FirstOrDefault(x => x.MessageID == id);

            if (serviceLog != null)
            {
                serviceLog.AppendData(data);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
