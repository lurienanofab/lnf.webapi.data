using MongoDB.Bson;
using Newtonsoft.Json;
using System;

namespace LNF.WebApi.Data.Models
{
    public class Alert
    {
        [JsonProperty("id")]
        public ObjectId Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}