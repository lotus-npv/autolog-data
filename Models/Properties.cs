using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLog.Models
{
    public class Properties
    {
        public string? ActionId { get; set; }
        public string? ActionName { get; set; }
        public string? RequestId { get; set; }
        public string? RequestPath { get; set; }
        public string? ConnectionId { get; set; }
        public string? MachineName { get; set; }
        public int ProcessId { get; set; }
        public int ThreadId { get; set; }
        public string? EnvironmentName { get; set; }
        public string? CorrelationId { get; set; }
        public string? ClientIp { get; set; }
        public string? CacheControl { get; set; }
        public string? Application { get; set; }

    }
}
