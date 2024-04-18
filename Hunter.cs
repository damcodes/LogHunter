using System.IO;
using System.Text.Json;

namespace LogHunter
{
    public class Hunter(IEnumerable<Arg> args, string logDirectory)
    {
        private readonly static JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly string _logDirectory = logDirectory;
        private IEnumerable<Arg> _query = args;
        public Dictionary<string, IEnumerable<Log>> CapturedLogs { get; private set; } = [];
        public int LogCount { get; private set; } = 0;

        public void HuntLogs()
        {
            DateTime startDate = _query.Where(arg => arg.Name == "Start").Single().Value;
            DateTime endDate = _query.Where(arg => arg.Name == "End").Single().Value;
            IEnumerable<App> apps = _query.Where(arg => arg.Name == "Apps").SingleOrDefault()?.Value ?? App.AllApps;
            var logFileNamesByApp = CollectFilesInTimeRangeByApp(startDate, endDate, apps);

            foreach (KeyValuePair<string, IEnumerable<string>> item in logFileNamesByApp)
            {
                string appName = item.Key;
                IEnumerable<string> logFilePaths = item.Value;
                IEnumerable<Log> logObjects = ReadAndMapToLogs(logFilePaths).Where(log => _query.All(param =>
                {
                    return param.Name switch
                    {
                        "LogLevel" => ((IEnumerable<string>)param.Value).Contains(log.Level),
                        "Callsite" => log.Callsite == param.Value,
                        "Start" => DateTime.Parse(log.Time) >= param.Value,
                        "End" => DateTime.Parse(log.Time) <= param.Value,
                        "TransactionId" => log.Scope is not null ? log.Scope.TransactionId == param.Value : false,
                        "UserId" => log.Scope is not null ? log.Scope.UserObjectId == param.Value : false,
                        "Message" => log.Message == param.Value,
                        "Apps" => true, //always true, as we're grouping not filtering by apps at this point
                        _ => false,
                    };
                }));
                int logCount = logObjects.Count();
                if (logCount > 0) 
                {
                    CapturedLogs.Add(appName, logObjects);
                    LogCount += logCount;
                }
            }
        }

        private Dictionary<string, IEnumerable<string>> CollectFilesInTimeRangeByApp(DateTime start, DateTime end, IEnumerable<App> apps)
        {
            IEnumerable<string> logFileNames = [];
            Dictionary<string, IEnumerable<string>> logFileNamesByApp = [];
            bool inTimeRange;
            foreach (App app in apps)
            {
                string appLogsPath = $@"{_logDirectory}\{app.Name}";
                IEnumerable<string> appLogFileNames = Directory.Exists(appLogsPath) ? Directory.EnumerateFiles(appLogsPath).Where(fileNameWithPath =>
                {
                    var fileNameNoExtension = Path.GetFileNameWithoutExtension(fileNameWithPath);
                    var fileNameSplit = fileNameNoExtension.Split('-');
                    inTimeRange = false;
                    if (int.TryParse(fileNameSplit[2], out int year) && int.TryParse(fileNameSplit[3], out int month) && int.TryParse(fileNameSplit[4], out int day))
                    {
                        var fileDate = new DateTime(year, month, day);
                        inTimeRange = fileDate >= start.Date && fileDate <= end.Date;
                    }
                    return inTimeRange;
                }) 
                : [];
                if (appLogFileNames.Any()) 
                    logFileNamesByApp.Add(app.Name, appLogFileNames);
            }
            return logFileNamesByApp;
        }

        private static IEnumerable<Log> ReadAndMapToLogs(IEnumerable<string> files)
        {
            ICollection<Log> logs = [];
            foreach (string filePath in files)
            {
                using var reader = new StreamReader(filePath);
                string jsonFileText = reader.ReadToEnd();
                IEnumerable<string> jsonStrings = jsonFileText.Split(Environment.NewLine).Where(str => str != string.Empty);
                foreach (string json in jsonStrings)
                {
                    var log = JsonSerializer.Deserialize<Log>(json, _jsonSerializerOptions);
                    if (log is not null) logs.Add(log);
                }
            }
            return logs;
        }
    }
}