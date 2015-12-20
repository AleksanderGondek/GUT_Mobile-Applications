using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

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
        [JsonProperty(PropertyName = "before")]
        public int Before { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public string Tags { get; set; }
        public string OwnerUserId { get; set; }
        [Version] // This is really important
        public string Version { get; set; }
    }
}
