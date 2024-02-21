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
                case "LogLevel":
                    if (isInteractiveMode)
                    {
                        bool isDigit = Regex.IsMatch(value, numeralPattern);
                        int index = isDigit ? int.Parse(value) - 1 : -1;
                        bool inRange = index >= 0 && index < LogLevels.Levels.Length;
                        if (!isDigit)
                            Error = $"Selection '{value}' is invalid. Must be a numeric value.";
                        if (!inRange)
                            Error = $"Selection '{value}' is invalid. Select only from the menu provided.";
                        Valid = inRange && Error is null;
                        if (Valid) Value = GetValue(LogLevels.Levels[index])!;
                    }
                    else
                    {
                        if (!LogLevels.Levels.Contains(value.ToUpper()))
                            Error = $"'{value}' is not a valid log level. Must be CRITICAL, ERROR, INFO, TRACE, or WARN.";
                        Valid = Error is null;
                        Value = Valid ? GetValue(LogLevels.Levels.Where(level => level.Equals(value, StringComparison.CurrentCultureIgnoreCase)).Single())! : GetValue(null)!;
                    }
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
                    Valid = value.Length == 0 || Regex.IsMatch(value, guidPatten);
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

        private static T GetValue(string? value)
        {
            if (typeof(T) == typeof(DateTime?))
            {
                Type t = typeof(T);
                Type NullableDateTime = Nullable.GetUnderlyingType(t)!;
                return (T)Convert.ChangeType(DateTime.Parse(value!), NullableDateTime);
            }
            return (T)Convert.ChangeType(value!, typeof(T));
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
                string mmddYYYYTHHMMss = @"(\d{2})\/(\d{2})\/(\d{4})T(\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmddYYYYHHMMss = @"(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYHHMMss = @"(\d{2})\/(\d{1})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYHHMMss = @"(\d{1})\/(\d{1})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYHHMMss = @"(\d{1})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYHHMM = @"(\d{2})\/(\d{1})\/(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYHHMM = @"(\d{1})\/(\d{1})\/(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYHHMM = @"(\d{1})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2})( [AaPp][Mm])?";
                string mmdYYYYTHHMM = @"(\d{2})\/(\d{1})\/(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mddYYYYTHHMM = @"(\d{1})\/(\d{2})\/(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                string mdYYYYTHHMM = @"(\d{1})\/(\d{1})\/(\d{4})T(\d{2}):(\d{2})( [AaPp][Mm])?";
                return new string[] { mmddYYYYTHHMMss, mmddYYYYHHMMss, mmdYYYYHHMMss, mdYYYYHHMMss, mddYYYYHHMMss, mmdYYYYHHMM, mdYYYYHHMM, mddYYYYHHMM, mmdYYYYTHHMM, mddYYYYTHHMM, mdYYYYTHHMM }.Any(pattern => Regex.IsMatch(dateTime, pattern));
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
                switch (Name)
                {
                    case "LogLevel":
                        return $"Select Log Level:\n-----------------\n{LogLevels.ToString()}";
                    case "Callsite":
                        return "Callsite?\nHit enter to omit Callsite filtering.";
                    case "Start":
                        return "Start date? (m-d-YYYY [HH:mm:SS [AM/PM]])\nHit enter to default to 3 days ago.";
                    case "End":
                        return "End date? (m-d-YYYY [HH:mm:SS [AM/PM]])\nHit enter to default to right now.";
                    case "TransactionId":
                        return "TransactionId?\nHit enter to omit TransactionId filtering.";
                    case "UserId":
                        return "UserId?\nHit enter to omit UserId filtering.";
                    case "Message":
                        return "Message?\nHit enter to omit Message filtering.";
                    default: break;
                }
                return "";
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