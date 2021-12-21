using LNF.Mongo;
using LNF.WebApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class AlertController : ApiController
    {
        private Collection<Alert> _col;

        [HttpGet, Route("alert")]
        public async Task<IEnumerable<Alert>> GetByDate(DateTime? date = null)
        {
            var result = await GetCollection().FindAsync(x => x.StartDate <= date.GetValueOrDefault(DateTime.Now) && x.EndDate > date.GetValueOrDefault(DateTime.Now));
            return result;
        }

        [HttpGet, Route("alert/all")]
        public async Task<IEnumerable<Alert>> GetActive()
        {
            var result = await GetCollection().AllAsync();
            return result;
        }

        [HttpGet, Route("alert/{id}")]
        public async Task<Alert> GetById([FromUri] string id)
        {
            var list = await GetCollection().FindAsync(x => x.Id == id);
            var result = list.FirstOrDefault();
            return result;
        }

        [HttpPost, Route("alert")]
        public async Task<Alert> Post([FromBody] Alert alert)
        {
            await GetCollection().InsertOneAsync(alert);
            return alert;
        }

        [HttpDelete, Route("alert/{id}")]
        public async Task<long> Delete(string id)
        {
            var result = await GetCollection().DeleteOneAsync(x => x.Id == id);
            return result;
        }

        [HttpPut, Route("alert/{id}")]
        public async Task<Alert> Put([FromBody] Alert alert, string id)
        {
            var result = await GetCollection().FindOneAndReplaceAsync(x => x.Id == id, alert, true, false);
            return result;
        }

        private Collection<Alert> GetCollection()
        {
            if (_col == null)
            {
                var client = new Mongo.Repository(ConfigurationManager.AppSettings["MongoConnectionString"]);
                var db = client.Database("alerts");
                _col = db.Collection<Alert>("alerts");
            }

            return _col;
        }
    }
}
