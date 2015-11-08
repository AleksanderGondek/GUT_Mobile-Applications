using System.Collections.Generic;
using Newtonsoft.Json;

namespace greengrocer_gut
{
    public class Groceries
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }
        [JsonProperty(PropertyName = "tags")]
        public string Tags { get; set; }
        [JsonProperty(PropertyName = "owneruserid")]
        public string OwnerUserId { get; set; }
    }
}
