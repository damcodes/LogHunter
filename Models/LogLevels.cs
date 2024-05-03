
namespace LogHunter
{
    public class LogLevels
    {
        private LogLevels(string val) { Value = val; }
        public string Value { get; private set; }
        public static LogLevels Info { get => new("INFO"); }
        public static LogLevels Trace { get => new("TRACE"); }
        public static LogLevels Error { get => new("ERROR"); }
        public static LogLevels Fatal { get => new("FATAL"); }
        public static LogLevels Warn { get => new("WARN"); }
        public static readonly string[] Levels = [Info.Value, Trace.Value, Error.Value, Fatal.Value, Warn.Value];
        public static new string ToString() => string.Join("\n", Levels.Select((level, i) => $"{i + 1}) {level}"));
    }
}