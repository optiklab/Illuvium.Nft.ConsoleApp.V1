using Newtonsoft.Json;

namespace Illuvium.Nft.App.Models
{
    public class Transaction
    {
        [JsonProperty("Type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("TokenId", Required = Required.Always)]
        public string TokenId { get; set; }

        [JsonProperty("Address", Required = Required.Default)]
        public string Address { get; set; }

        [JsonProperty("From", Required = Required.Default)]
        public string From { get; set; }

        [JsonProperty("To", Required = Required.Default)]
        public string To { get; set; }
    }
}