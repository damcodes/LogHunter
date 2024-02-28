
namespace LogHunter
{
    public class LogLevels
    {
        private LogLevels(string val) { Value = val; }
        public string Value { get; private set; }
        public static LogLevels Info { get { return new LogLevels("INFO"); } }
        public static LogLevels Trace { get { return new LogLevels("TRACE"); } }
        public static LogLevels Error { get { return new LogLevels("ERROR"); } }
        public static LogLevels Critical { get { return new LogLevels("CRITICAL"); } }
        public static LogLevels Warn { get { return new LogLevels("WARN"); } }
        public static readonly string[] Levels = [Info.Value, Trace.Value, Error.Value, Critical.Value, Warn.Value];
        public static new string ToString() => string.Join("\n", Levels.Select((level, i) => $"{i + 1}) {level}"));
    }
}