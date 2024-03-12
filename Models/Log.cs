
namespace LogHunter
{
    public class Log
    {
        public required string Level { get; set; }
        public required string Time { get; set; }
        public required string Callsite { get; set; }
        public required string Message { get; set; }
        public Scope? Scope { get; set; }
        public IEnumerable<string> GetDisplayStrings() => [$"LogLevel: {Level}", $"Time: {Time}", $"Callsite: {Callsite}", $"TransactionId: {Scope?.TransactionId ?? "NULL"}", $"UserId: {Scope?.UserObjectId ?? "NULL"}", $"Message: {Message}"];
    }
}