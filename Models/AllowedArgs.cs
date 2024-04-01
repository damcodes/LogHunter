
namespace LogHunter
{
    public class AllowedArgs
    {
        private AllowedArgs(string fullArg, string flag, string name) 
        { 
            Full = fullArg;
            Flag = flag;
            Name = name;
        }
        public string Full { get; private set; }
        public string Flag { get; private set; }
        public string Name { get; private set; }
        public static AllowedArgs Apps { get => new("apps", "a", "Apps"); }
        public static AllowedArgs Level { get => new("level", "l", "LogLevel"); }
        public static AllowedArgs CallSite { get => new("callsite", "cs", "Callsite"); }
        public static AllowedArgs Start { get => new ("start", "s", "Start"); }
        public static AllowedArgs End { get => new ("end", "e", "End"); }
        public static AllowedArgs Message { get => new("message", "m", "Message"); }
        public static AllowedArgs TransactionId { get => new("transaction-id", "tid", "TransactionId"); }
        public static AllowedArgs UserId { get => new("user-id", "uid", "UserId"); }
        public static AllowedArgs Help { get => new("help", "h", "Help"); }
        public static readonly string[] FullArgs = [Apps.Full, Level.Full, CallSite.Full, Start.Full, End.Full, Message.Full, TransactionId.Full, UserId.Full, Help.Full];
        public static readonly string[] FlagArgs = [Apps.Flag, Level.Flag, CallSite.Flag, Start.Flag, End.Flag, Message.Flag, TransactionId.Flag, UserId.Flag, Help.Flag];
        public static readonly AllowedArgs[] AllArgs = [Apps, Level, CallSite, Start, End, Message, TransactionId, UserId, Help];
    }
}