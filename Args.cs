
using System.Collections;

namespace LogHunter
{
    public class Args
    {
        public string? LogLevel { get; set; }
        public string? Callsite { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? TransactionId { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
    }
}