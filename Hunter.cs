using System.IO;
using System.Text.Json;

namespace LogHunter
{
    public class Hunter(IEnumerable<Arg> args)
    {
        private static readonly string _logDirectory = @"C:\TEMP\plutus-logs";
        private readonly static JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly IEnumerable<Arg> _query = args;
        public IEnumerable<Log> CapturedLogs { get; set; } = [];

        public void HuntLogs()
        {
            DateTime startDate = _query.Where(arg => arg.Name == "Start").Single().Value;
            DateTime endDate = _query.Where(arg => arg.Name == "End").Single().Value; 
            var logFilesFilteredByDate = CollectFilesInTimeRange(startDate, endDate);

            IEnumerable<Log> logs = ReadAndMapToLogs(files: logFilesFilteredByDate);
            CapturedLogs = logs.Where(log => _query.All(param =>
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
                    _ => false,
                };
            }));
        }

        private static IEnumerable<string> CollectFilesInTimeRange(DateTime start, DateTime end)
        {
            var logFiles = Directory.EnumerateFiles(_logDirectory);
            return logFiles.Where(fileNameWithPath =>
            {
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(fileNameWithPath);
                var fileNameSplit = fileNameNoExtension.Split('-');
                bool inTimeRange = false;
                if (int.TryParse(fileNameSplit[2], out int year) && int.TryParse(fileNameSplit[3], out int month) && int.TryParse(fileNameSplit[4], out int day))
                {
                    var fileDate = new DateTime(year, month, day);
                    inTimeRange = fileDate >= start.Date && fileDate <= end.Date;
                }
                return inTimeRange;
            });
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