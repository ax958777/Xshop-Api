using Newtonsoft.Json;

namespace Api.Dto
{
    public class PaymentIntentCreateRequestDto
    {
        [JsonProperty("items")]
        public Item[] Items { get; set; }
    }
    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("Amount")]
        public long Amount { get; set; }
    }
}
