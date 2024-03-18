
namespace LogHunter
{
    public class Log
    {
        public required string Level { get; set; }
        public required string Time { get; set; }
        public required string Callsite { get; set; }
        public required string Message { get; set; }
        public Scope? Scope { get; set; }
        public Exception? Exception { get; set; }
        public IEnumerable<string> GetDisplayStrings() => 
            [
                $"LogLevel: {Level}", 
                $"Time: {Time}", 
                $"Callsite: {Callsite}", 
                $"TransactionId: {Scope?.TransactionId ?? "NULL"}", 
                $"UserId: {Scope?.UserObjectId ?? "NULL"}", 
                $"ExceptionType: {Exception?.Type ?? "NULL"}",
                $"ExceptionMessage: {Exception?.Message ?? "NULL"}",
                $"ExceptionStackTrace: {Exception?.Stacktrace ?? "NULL"}",
                $"Message: {Message}"];
    }
}