using System.Text.RegularExpressions;

namespace LogHunter
{
    public class Arg<T>(string name) : Arg(name)
    {
        public override dynamic Value { get; set; } = null!;
        public override void Validate(string value, bool isInteractiveMode = false)
        {
            value = value.Trim();
            string numeralPattern = @"\d";
            string guidPatten = @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
            switch (Name)
            {
                case "Apps":
                    IEnumerable<string> appSelectionStrings = value.Split(',').Select(str => str.Trim()).Where(str => str.Length > 0);
                    List<string> apps = [];
                    foreach (string input in appSelectionStrings)
                    {
                        if (isInteractiveMode)
                        {
                            bool isDigit = Regex.IsMatch(input, numeralPattern);
                            int index = isDigit ? int.Parse(input) - 1 : -1;
                            bool inRange = index >= 0 && index < Apps.AllApps.Length;
                            if (!isDigit)
                                Error = $"Selection '{input}' is invalid. Must be a numeric value.";
                            else if (!inRange)
                                Error = Error is null ? $"Selection '{input}' is invalid. Select only from the menu provided." : Error + Environment.NewLine + $"Selection '{input}' is invalid. Select only from the menu provided.";
                            else apps.Add(Apps.AllApps[index]);
                        }
                        else 
                        {
                            if (!Apps.AllApps.Contains(input.ToUpper()))
                                Error = Error is null ? $"Selection '{input}' is not a valid app." : Error + Environment.NewLine + $"Selection '{input}' is not a valid app.";
                            else apps.Add(input); 
                        }
                    }
                    Valid = Error is null;
                    if (Valid) Value = apps.Count > 0 ? apps : Apps.AllApps;
                    break;
                case "LogLevel":
                    IEnumerable<string> logLevelSelectionStrings = value.Split(',').Select(str => str.Trim()).Where(str => str.Length > 0);
                    List<string> levels = [];
                    foreach (string input in logLevelSelectionStrings)
                    {
                        if (isInteractiveMode)
                        {
                            bool isDigit = Regex.IsMatch(input, numeralPattern);
                            int index = isDigit ? int.Parse(input) - 1 : -1;
                            bool inRange = index >= 0 && index < LogLevels.Levels.Length;
                            if (!isDigit)
                                Error = Error is null ? $"Selection '{input}' is invalid. Must be a numeric value." : Error + Environment.NewLine + $"Selection '{input}' is invalid. Must be a numeric value.";
                            else if (!inRange)
                                Error = Error is null ? $"Selection '{input}' is invalid. Select only from the menu provided." : Error + Environment.NewLine + $"Selection '{input}' is invalid. Select only from the menu provided.";
                            else levels.Add(LogLevels.Levels[index]);
                        }
                        else
                        {
                            if (!LogLevels.Levels.Contains(input.ToUpper()))
                                Error = Error is null ? $"'{input}' is not a valid log level. Must be CRITICAL, ERROR, INFO, TRACE, or WARN." : Error + Environment.NewLine + $"'{input}' is not a valid log level. Must be CRITICAL, ERROR, INFO, TRACE, or WARN.";
                            else levels.Add(input.ToUpper());
                        }
                    }
                    Valid = Error is null;
                    if (Valid) Value = levels.Count > 0 ? levels : LogLevels.Levels;
                    break;
                case "Callsite":
                    Valid = value.Length == 0 || value.Length > 3;
                    if (!Valid) Error = $"Callsite '{value}' is invalid. {(isInteractiveMode ? "Hit enter to omit Callsite filter, or provide Callsite with at least 3 characters." : "Callsite must be at least 3 characters.")}";
                    Value = value.Length == 0 ? GetValue(null)! : GetValue(value)!;
                    break;
                case "Start":
                    Valid = IsValidDate(value) || value.Length == 0;
                    if (!Valid) Error = $"Start date '{value}' is not a valid date.";
                    if (Valid && value.Length > 0) Value = GetValue(value)!;
                    if (Valid && value.Length == 0) Value = GetValue(DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString())!;
                    break;
                case "End":
                    Valid = IsValidDate(value) || value.Length == 0;
                    if (!Valid) Error = $"End date '{value}' is not a valid date.";
                    if (Valid && value.Length > 0) Value = GetValue(value)!;
                    if (Valid && value.Length == 0) Value = GetValue(DateTime.Now.ToString())!;
                    break;
                case "TransactionId":
                    Valid = value.Length == 0 || Regex.IsMatch(value, guidPatten);
                    if (!Valid) Error = $"'{value}' is not a valid GUID value for TransactionId.";
                    else Value = value.Length == 0 ? GetValue(null)! : GetValue(value)!;
                    break;
                case "UserId":
                    Valid = true;
                    if (!Valid) Error = $"'{value}' is not a valid GUID value for UserId.";
                    else Value = value.Length == 0 ? GetValue(null)! : GetValue(value)!;
                    break;
                case "Message":
                    Valid = value.Length == 0 || value.Length > 3;
                    if (!Valid) Error = $"Message '{value}' is invalid. {(isInteractiveMode ? "Hit enter to omit Message filter, or provide Message with at least 3 characters." : "Message must be at least 3 characters.")}";
                    else Value = value.Length == 0 ? GetValue(null)! : GetValue(value)!;
                    break;
                default: break;
            }
            if (Valid) ClearErrors();
        }

        private static T GetValue(string? value) => (T)Convert.ChangeType(typeof(T) == typeof(DateTime) ? DateTime.Parse(value!) : value!, typeof(T));
        private static bool IsValidDate(string dateTime)
        {
            if (dateTime.Length <= 10)
            {
                string mmddYYYY = @"(\d{2})\/(\d{2})\/(\d{4})";
                string mmddYYYYDashes = @"(\d{2})-(\d{2})-(\d{4})";
                string mmdYYYY = @"(\d{2})\/(\d{1})\/(\d{4})";
                string mmdYYYYDashes = @"(\d{2})-(\d{1})-(\d{4})";
                string mddYYYY = @"(\d{1})\/(\d{2})\/(\d{4})";
                string mddYYYYDashes = @"(\d{1})-(\d{2})-(\d{4})";
                string mdYYYY = @"(\d{1})\/(\d{1})\/(\d{4})";
                string mdYYYYDashes = @"(\d{1})-(\d{1})-(\d{4})";
                return new string[] { mmddYYYY, mmdYYYY, mddYYYY, mdYYYY, mmddYYYYDashes, mmdYYYYDashes, mddYYYYDashes, mdYYYYDashes }.Any(pattern => Regex.IsMatch(dateTime, pattern));
            }
            if (dateTime.Length > 10)
            {
                string mmddYYYYTHHMMss = @"(\d{2})\/(\d{2})\/(\d{4})T(\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmddYYYYTHHMMssDashes = @"(\d{2})-(\d{2})-(\d{4})T(\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmddYYYYHHMMss = @"(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmddYYYYHHMMssDashes = @"(\d{2})-(\d{2})-(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYHHMMss = @"(\d{2})\/(\d{1})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYHHMMssDashes = @"(\d{2})-(\d{1})-(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYHHMMss = @"(\d{1})\/(\d{1})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYHHMMssDashes = @"(\d{1})-(\d{1})-(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYHHMMss = @"(\d{1})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYHHMMssDashes = @"(\d{1})-(\d{2})-(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYHHMM = @"(\d{2})\/(\d{1})\/(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYHHMMDashes = @"(\d{2})-(\d{1})-(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYHHMM = @"(\d{1})\/(\d{1})\/(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYHHMMDashes = @"(\d{1})-(\d{1})-(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYHHMM = @"(\d{1})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYHHMMDashes = @"(\d{1})-(\d{2})-(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYTHHMM = @"(\d{2})\/(\d{1})\/(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYTHHMMDashes = @"(\d{2})-(\d{1})-(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYTHHMM = @"(\d{1})\/(\d{2})\/(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYTHHMMDashes = @"(\d{1})-(\d{2})-(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYTHHMM = @"(\d{1})\/(\d{1})\/(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYTHHMMDashes = @"(\d{1})-(\d{1})-(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                return new string[] 
                    { mmddYYYYTHHMMss, mmddYYYYHHMMss, mmdYYYYHHMMss, mdYYYYHHMMss, mddYYYYHHMMss, mmdYYYYHHMM, mdYYYYHHMM, mddYYYYHHMM, 
                        mmdYYYYTHHMM, mddYYYYTHHMM, mdYYYYTHHMM,mmddYYYYTHHMMssDashes, mmddYYYYHHMMssDashes, mmdYYYYHHMMssDashes, mdYYYYHHMMssDashes, 
                        mddYYYYHHMMssDashes, mmdYYYYHHMMDashes, mdYYYYHHMMDashes, mddYYYYHHMMDashes, mmdYYYYTHHMMDashes, mddYYYYTHHMMDashes, mdYYYYTHHMMDashes }.Any(pattern => Regex.IsMatch(dateTime, pattern));
            }
            return false;
        }
    }
    public abstract class Arg(string name)
    {
        public string Name { get; private set; } = name;
        public virtual dynamic Value { get; set; } = null!;
        public string Prompt
        {
            get
            {
                return Name switch
                {
                    "Apps" => $"Select Apps:{Environment.NewLine}-----------------{Environment.NewLine}{Apps.ToString()}{Environment.NewLine}{Environment.NewLine}Provide comma separated values or hit enter to omit Apps filtering",
                    "LogLevel" => $"Select Log Levels:{Environment.NewLine}-----------------{Environment.NewLine}{LogLevels.ToString()}{Environment.NewLine}{Environment.NewLine}Provide comma separated values or hit enter to omit LogLevel filtering.",
                    "Callsite" => $"Callsite?{Environment.NewLine}Hit enter to omit Callsite filtering.",
                    "Start" => $"Start date? (m-d-YYYY [HH:mm:SS [AM/PM]]){Environment.NewLine}Hit enter to default to 3 days ago.",
                    "End" => $"End date? (m-d-YYYY [HH:mm:SS [AM/PM]]){Environment.NewLine}Hit enter to default to right now.",
                    "TransactionId" => $"TransactionId?{Environment.NewLine}Hit enter to omit TransactionId filtering.",
                    "UserId" => $"UserId?{Environment.NewLine}Hit enter to omit UserId filtering.",
                    "Message" => $"Message?{Environment.NewLine}Hit enter to omit Message filtering.",
                    _ => ""
                };
            }
        }
        public bool Valid { get; set; } = false;
        public string? Error { get; set; }
        public abstract void Validate(string value, bool isInteractiveMode);
        public void SetError(string error)
        {
            Error = error;
            Valid = false;
        }
        public void ClearErrors()
        {
            Error = null;
            Valid = true;
        }
    }
}