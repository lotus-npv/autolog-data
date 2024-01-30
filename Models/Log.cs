using Newtonsoft.Json;

namespace AutoLog.Models
{
    public class Log

    {
        [JsonProperty("@t")]
        public string? Timestamp { get; set; }

        [JsonProperty("@l")]
        public string? Level { get; set; }

        [JsonProperty("@mt")]
        public string? MessageTemplate { get; set; }

        [JsonProperty("@x")]
        public string? Exception { get; set; }

        [JsonProperty("ActionId")]
        public string? ActionId { get; set; }

        [JsonProperty("ActionName")]
        public string? ActionName { get; set; }

        [JsonProperty("RequestId")]
        public string? RequestId { get; set; }

        [JsonProperty("RequestPath")]
        public string? RequestPath { get; set; }

        [JsonProperty("ConnectionId")]
        public string? ConnectionId { get; set; }

        [JsonProperty("MachineName")]
        public string? MachineName { get; set; }

        [JsonProperty("ProcessId")]
        public int ProcessId { get; set; }

        [JsonProperty("ThreadId")]
        public int ThreadId { get; set; }

        [JsonProperty("EnvironmentName")]
        public string? EnvironmentName { get; set; }

        [JsonProperty("CorrelationId")]
        public string? CorrelationId { get; set; }

        [JsonProperty("ClientIp")]
        public string? ClientIp { get; set; }

        [JsonProperty("CacheControl")]
        public string? CacheControl { get; set; }

        [JsonProperty("Application")]
        public string? Application { get; set; }

        [JsonProperty("SourceContext")]
        public string? SourceContext { get; set; }

    }
}
