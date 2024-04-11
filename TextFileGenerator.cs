namespace LogHunter
{
    public class TextFileGenerator(Dictionary<string, IEnumerable<Log>> logsByApp) : FileGenerator(logsByApp, "txt")
    {
        private static readonly string _filePathForDump = @"C:\TEMP\hunter";
        private Dictionary<string, Dictionary<DateTime, Dictionary<string, string>>> FormattedStringsByApp { get; set; } = [];

        public override void GroupAndFormat()
        {
            Dictionary<string, string> formattedStringsByLevel = [];
            Dictionary<DateTime, Dictionary<string, string>> formattedStringsByLevelAndDate = [];
            foreach (KeyValuePair<string, IEnumerable<Log>> item in LogsByApp)
            {
                string appName = item.Key;
                IEnumerable<Log> logs = item.Value;
                var groupedByLevelAndDate = logs.GroupBy(log => (log.Level, Date: DateTime.Parse(log.Time.Split(' ')[0]))).GroupBy(group => group.Key.Date);
                foreach (IGrouping<DateTime, IGrouping<(string Level, DateTime Date), Log>> groupByDate in groupedByLevelAndDate)
                {
                    foreach (IGrouping<(string Level, DateTime Time), Log> groupByLevel in groupByDate)
                    {
                        if (!formattedStringsByLevel.TryGetValue(groupByLevel.Key.Level, out string? formattedString))
                        {
                            foreach (Log log in groupByLevel)
                            {
                                var displayStrings = log.GetDisplayStrings().Select(FormatSpacing);
                                formattedString += formattedString?.Length > 0 ? Environment.NewLine + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, displayStrings) : string.Join(Environment.NewLine, displayStrings);
                            }
                            formattedStringsByLevel.Add(groupByLevel.Key.Level, formattedString ?? "");
                        }
                    }
                    if (!formattedStringsByLevelAndDate.TryGetValue(groupByDate.Key, out Dictionary<string, string>? formattedStringByLevelDictionary))
                        formattedStringsByLevelAndDate.Add(groupByDate.Key, formattedStringsByLevel);
                    formattedStringsByLevel = [];
                }
                FormattedStringsByApp.Add(appName, formattedStringsByLevelAndDate);                
                formattedStringsByLevelAndDate = [];
            }
        }

        public async override Task Dump()
        {
            string timestamp = DateTime.Now.ToString("MM-dd-yyyyTHH.mm.ss");
            HuntPath = Directory.CreateDirectory(_filePathForDump + '\\' + timestamp).FullName;
            foreach (KeyValuePair<string, Dictionary<DateTime, Dictionary<string, string>>> formattedStringsByDate in FormattedStringsByApp)
            {
                string appName = formattedStringsByDate.Key;
                string appNamePath = Directory.CreateDirectory(HuntPath + $@"\{appName}").FullName;
                foreach (KeyValuePair<DateTime, Dictionary<string, string>> groupByDate in formattedStringsByDate.Value)
                {
                    string datePath = Directory.CreateDirectory(appNamePath + '\\' + groupByDate.Key.ToString("MM-dd-yyyy")).FullName;
                    foreach (KeyValuePair<string, string> groupByLevel in groupByDate.Value)
                    {
                        using var outputFile = new StreamWriter(datePath + $@"\{groupByLevel.Key}.txt");
                        await outputFile.WriteAsync(groupByLevel.Value);
                    }
                }
            }
        }

        private static string FormatSpacing(string unformatted)
        {
            if (unformatted.Length > 120)
            {
                ICollection<string> strings = [];
                int start = 0;
                int end = 120;
                int idxOfNextSpace;
                while (start < end && end < unformatted.Length)
                {
                    idxOfNextSpace = unformatted[end..].IndexOf(' ');
                    strings.Add(unformatted[start..(end + idxOfNextSpace)]);
                    start = end + idxOfNextSpace + 1;
                    end = start + 120;
                }
                if (unformatted[start..].Length > 0) strings.Add(unformatted[start..]);
                return string.Join(Environment.NewLine + "\t", strings);
            }
            return unformatted;
        }
    }
}