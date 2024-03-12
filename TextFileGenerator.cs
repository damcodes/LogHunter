namespace LogHunter
{
    public class TextFileGenerator(IEnumerable<Log> logs) : FileGenerator(logs, "txt")
    {
        private static readonly string _filePathForDump = @"C:\TEMP\hunter\plutus";
        public Dictionary<DateTime, Dictionary<string, string>> FormattedStringsByLevelAndDate { get; set; } = [];

        public override void GroupAndFormat()
        {
            Dictionary<string, string> formattedStringsByLevel = [];
            var logsByLevelAndDate = Logs.GroupBy(log => (log.Level, Date: DateTime.Parse(log.Time.Split(' ')[0]))).GroupBy(group => group.Key.Date);
            foreach (IGrouping<DateTime, IGrouping<(string Level, DateTime Date), Log>> groupByDate in logsByLevelAndDate)
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
                if (!FormattedStringsByLevelAndDate.TryGetValue(groupByDate.Key, out Dictionary<string, string>? formattedStringByLevelDictionary))
                    FormattedStringsByLevelAndDate.Add(groupByDate.Key, formattedStringsByLevel);
                
                formattedStringsByLevel = [];
            }
        }

        public async override Task Dump()
        {
            string timestamp = DateTime.Now.ToString("MM-dd-yyyyTHH.mm.ss");
            HuntPath = Directory.CreateDirectory(_filePathForDump + '\\' + timestamp).FullName;
            foreach (KeyValuePair<DateTime, Dictionary<string, string>> groupByDate in FormattedStringsByLevelAndDate)
            {
                string path = HuntPath;
                if (!Directory.Exists(path + '\\' + groupByDate.Key.ToString("MM-dd-yyyy")))
                    path = Directory.CreateDirectory(path + '\\' + groupByDate.Key.ToString("MM-dd-yyyy")).FullName;
                foreach (KeyValuePair<string, string> groupByLevel in groupByDate.Value)
                {
                    using var outputFile = new StreamWriter(path + $@"\{groupByLevel.Key}.txt"); 
                    await outputFile.WriteAsync(groupByLevel.Value);
                }
                path = HuntPath;
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