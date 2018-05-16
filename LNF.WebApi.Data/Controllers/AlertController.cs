using LNF.WebApi.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class AlertController : ApiController
    {
        private IMongoCollection<Alert> _col;

        [Route("alert")]
        public async Task<IEnumerable<Alert>> Get(DateTime? date = null)
        {
            var cursor = await GetCollection().FindAsync(x => x.StartDate <= date.GetValueOrDefault(DateTime.Now) && x.EndDate > date.GetValueOrDefault(DateTime.Now));
            var result = await cursor.ToListAsync();
            return result;
        }

        [Route("alert/all")]
        public async Task<IEnumerable<Alert>> GetActive()
        {
            var cursor = await GetCollection().FindAsync(FilterDefinition<Alert>.Empty);
            var result = await cursor.ToListAsync();
            return result;
        }

        [Route("alert/{id}")]
        public async Task<Alert> Get(string id)
        {
            var cursor = await GetCollection().FindAsync(x => x.Id == ObjectId.Parse(id));
            var result = await cursor.FirstOrDefaultAsync();
            return result;
        }

        [Route("alert")]
        public async Task<Alert> Post([FromBody] Alert alert)
        {
            await GetCollection().InsertOneAsync(alert);
            return alert;
        }

        [Route("alert/{id}")]
        public async Task<long> Delete(string id)
        {
            var deleteResult = await GetCollection().DeleteOneAsync(x => x.Id == ObjectId.Parse(id));
            return deleteResult.DeletedCount;
        }

        [Route("alert/{id}")]
        public async Task<Alert> Put([FromBody] Alert alert, string id)
        {
            var opts = new FindOneAndReplaceOptions<Alert>() { ReturnDocument = ReturnDocument.After };
            alert.Id = ObjectId.Parse(id);
            var result = await GetCollection().FindOneAndReplaceAsync<Alert>(x => x.Id == alert.Id, alert, opts);
            return result;
        }

        private IMongoCollection<Alert> GetCollection()
        {
            if (_col == null)
            {
                var client = new MongoClient(ConfigurationManager.AppSettings["MongoConnectionString"]);
                var db = client.GetDatabase("alerts");
                _col = db.GetCollection<Alert>("alerts");
            }

            return _col;
        }
    }
}
