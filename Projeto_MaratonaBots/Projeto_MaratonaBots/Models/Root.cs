using Newtonsoft.Json;

namespace Projeto_MaratonaBots.Models
{
    public class RootJson
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("total_results")]
        public int TotalResults { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty("results")]
        public Results[] Results { get; set; }

    }
}