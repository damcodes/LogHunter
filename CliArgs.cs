using System.Text.RegularExpressions;

namespace LogHunter
{
    public class CliArgs(string[] args)
    {
        private readonly string[] _acceptableArgs = ["--level", "--callsite", "--time-range", "--message", "--transaction-id", "--user-id", "--help"];
        private readonly string[] _acceptableArgsFlags = ["-l", "-cs", "-tr", "-m", "-tid", "-uid", "-h"];
        private readonly string[] _logLevels = ["info", "trace", "error", "warn"];
        private readonly string[] providedArgs = args;
        public Args Args { get; set; } = new();
        public bool Verified { get; set; } = false;
        public ICollection<string> Errors { get; } = [];

        public CliArgs Verify()
        {
            for (int i = 0; i < providedArgs.Length; i += 2)
            {
                if (!_acceptableArgs.Contains(providedArgs[i]) && !_acceptableArgsFlags.Contains(providedArgs[i]))
                    Errors.Add($"Invalid argument provided: '{providedArgs[i]}'");
                if (!VerifyValue(providedArgs[i], providedArgs[i + 1], out string errorMessage))
                    Errors.Add(errorMessage);
            }
            Verified = Errors.Count == 0;
            return this;
        }

        public CliArgs Build()
        {
            for (int i = 0; i < providedArgs.Length; i += 2)
            {
                string arg = providedArgs[i];
                string value = providedArgs[i + 1];
                Args.LogLevel = arg == "--level" || arg == "-l" ? value : Args.LogLevel;
                Args.Callsite = arg == "--callsite" || arg == "-cs" ? value : Args.Callsite;
                Args.TransactionId = arg == "--transaction-id" || arg == "-tid" ? value : Args.TransactionId;
                Args.UserId = arg == "--user-id" || arg == "-uid" ? value : Args.UserId;
                Args.Message = arg == "--message" || arg == "-m" ? value : Args.Message;
                if (arg == "--time-range" || arg == "-tr")
                {
                    string[] splitTimeRange = (value.Contains("to") ? value.Split("to") : value.Split('-')).Select(val => val.Trim()).ToArray();
                    DateTime? start = splitTimeRange[0].Contains('T') ? DateTime.Parse(string.Join(' ', splitTimeRange[0].Split('T'))) : null;
                    DateTime? end = splitTimeRange[1].Contains('T') ? DateTime.Parse(string.Join(' ', splitTimeRange[1].Split('T'))) : null;
                    Args.Start = start;
                    Args.End = end;
                }
            }
            return this;
        }

        private bool VerifyValue(string arg, string value, out string errorMessage)
        {
            errorMessage = "";
            if (arg == "--level" || arg == "-l")
            {
                if (!_logLevels.Contains(value.ToLower()))
                    errorMessage = $"'{value}' is not a valid log level. Must be info, trace, error, or warn.";
            }
            if (arg == "--time-range" || arg == "-tr")
            {
                string[] splitTimeRange = (value.Contains("to") ? value.Split("to") : value.Split('-')).Select(val => val.Trim()).ToArray();
                if (splitTimeRange.Length != 2)
                    errorMessage = $"\nTime range format is invalid.\nProvide time range enclosed in double quotes and separated by 'to' like this: \n\t\"m/dd/YYYYTHH:mm:SS to m/dd/YYYYTHH:mm:SS\"\nor separted by a dash like this: \n\tm/dd/YYYYTHH:mm:SS-m/dd/YYYYTHH:mm:SS";
                if (splitTimeRange.Length == 2)
                {
                    string startDateTime = splitTimeRange[0];
                    string endDateTime = splitTimeRange[1];
                    if (!IsValidDate(startDateTime))
                        errorMessage += $"\nStart time provided '{splitTimeRange[0]}' is not a valid date.";
                    if (!IsValidDate(endDateTime))
                        errorMessage += $"\nEnd time provided '{splitTimeRange[1]}' is not a valid date.";
                    if (DateTime.TryParse(startDateTime, out DateTime parsedStartDateTime) && DateTime.TryParse(endDateTime, out DateTime parsedEndDateTime))
                        if (parsedStartDateTime >= parsedEndDateTime)
                            errorMessage += $"\nInvalid time range. Start date '{startDateTime}' cannot be equal to or after end date '{endDateTime}'.";
                }
            }
            return errorMessage == "";
        }

        private static bool IsValidDate(string dateTime)
        {
            if (dateTime.Length <= 10)
            {
                string mmddYYYY = @"(\d{2})\/(\d{2})\/(\d{4})";
                string mmdYYYY = @"(\d{2})\/(\d{1})\/(\d{4})";
                string mddYYYY = @"(\d{1})\/(\d{2})\/(\d{4})";
                string mdYYYY = @"(\d{1})\/(\d{1})\/(\d{4})";
                return new string[] { mmddYYYY, mmdYYYY, mddYYYY, mdYYYY }.Any(pattern => Regex.IsMatch(dateTime, pattern));
            }
            if (dateTime.Length > 10)
            {
                string mmddYYYYTHHMMss = @"(\d{2})\/(\d{2})\/(\d{4})T(\d{2}):(\d{2}):(\d{2})";
                string mmdYYYYTHHMM = @"(\d{2})\/(\d{1})\/(\d{4})T(\d{2}):(\d{2})";
                string mddYYYYTHHMM = @"(\d{1})\/(\d{2})\/(\d{4})T(\d{2}):(\d{2})";
                string mdYYYYTHHMM = @"(\d{1})\/(\d{1})\/(\d{4})T(\d{2}):(\d{2})";
                return new string[] { mmddYYYYTHHMMss, mmdYYYYTHHMM, mddYYYYTHHMM, mdYYYYTHHMM }.Any(pattern => Regex.IsMatch(dateTime, pattern));
            }
            return false;
        }
    }
}
