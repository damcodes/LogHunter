
using Microsoft.Extensions.Configuration;

namespace LogHunter
{
    public class ConsoleApp(bool isInteractiveMode, IConfiguration configuration)
    {
        private readonly bool _isInteractiveMode = isInteractiveMode;
        private readonly IConfiguration _config = configuration;
        private IEnumerable<Arg> Args { get; } =
        [
            new Arg<IEnumerable<string>>("Apps"),
            new Arg<IEnumerable<string>>("LogLevel"),
            new Arg<string?>("Callsite"),
            new Arg<DateTime>("Start"),
            new Arg<DateTime>("End"),
            new Arg<string?>("TransactionId"),
            new Arg<string?>("UserId"),
            new Arg<string?>("Message"),
        ];
        private ICollection<string> Errors { get; } = [];
        public async Task<bool> Run(string[] args)
        {
            try
            {
                if (_isInteractiveMode)
                {
                    while (!Args.All(arg => arg.Valid))
                    {
                        Arg currentArg = Args.Where(arg => !arg.Valid).First();
                        Console.Clear();
                        if (currentArg.Error is not null) PrintInColor(currentArg.Error, color: ConsoleColor.Red);
                        currentArg.ClearErrors();
                        Prompt(currentArg.Prompt);
                        string input = Console.ReadLine() ?? "";
                        currentArg.Validate(input, isInteractiveMode: true);
                        if (currentArg.Name == "End" && (DateTime)Args.Where(arg => arg.Name == "Start").Single().Value > (DateTime)currentArg.Value)
                        {
                            Arg<DateTime> startDate = (Arg<DateTime>)Args.Where(arg => arg.Name == "Start").Single();
                            startDate.SetError($"Time range is invalid. Start date '{startDate.Value}' must be before End date '{(DateTime)currentArg.Value}'.");
                            currentArg.SetError($"Time range is invalid. End date '{(DateTime)currentArg.Value}' must be after Start date '{startDate.Value}'");
                        }
                        Console.Clear();
                    }
                    PrintInColor("Beginning hunt for logs with the following query filters:", color: ConsoleColor.Yellow);
                    PrintQueryObj(Args);
                }
                else
                {
                    for (int i = 0; i < args.Length; i += 2)
                    {
                        string flagArg = args[i][1..]; //ex: -cs === cs
                        string fullArg = args[i][2..]; //ex: --callsite === callsite
                        var allowedArg = AllowedArgs.AllArgs.Where(arg => arg.Flag == flagArg || arg.Full == fullArg).SingleOrDefault();
                        if (allowedArg is null)
                            Errors.Add($"Invalid argument provided: '{args[i]}'");
                        else
                        {
                            Arg arg = Args.Where(arg => arg.Name == allowedArg.Name).Single();
                            arg.Validate(args[i + 1], isInteractiveMode: false);
                        }
                    }
                    if (Errors.Count > 0) throw new System.Exception(string.Join(Environment.NewLine, Errors));
                    Arg start = Args.Where(arg => arg.Name == "Start").Single();
                    Arg end = Args.Where(arg => arg.Name == "End").Single();
                    if (start.Value > end.Value)
                    {
                        start.SetError($"Time range is invalid. Start date '{start.Value}' must be before End date '{end.Value}'.");
                        end.SetError($"Time range is invalid. End date '{end.Value}' must be after Start date '{start.Value}'.");
                    }
                    if (start.Value is null)
                    {
                        start.Value ??= DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0));
                        start.Valid = true;
                    }
                    if (end.Value is null)
                    {
                        end.Value ??= DateTime.Now;
                        end.Valid = true;
                    }

                    if (Args.Any(arg => arg.Error is not null) && !Args.All(arg => arg.Valid))
                    {
                        foreach (Arg arg in Args)
                            if (arg.Error is not null)
                                Errors.Add(arg.Error);
                        throw new System.Exception(string.Join(Environment.NewLine, Errors));
                    }
                }
                string logDirectory = _config["LogDirectory"] ?? throw new KeyNotFoundException("File path to log directory not found");
                var hunter = new Hunter(Args.Where(arg => arg.Value is not null), logDirectory);
                hunter.HuntLogs();
                if (hunter.CapturedLogs.Any())
                {
                    PrintInColor($"{hunter.CapturedLogs.Count()} {(hunter.CapturedLogs.Count() == 1 ? "log" : "logs")} captured!", color: ConsoleColor.Green);
                    PrintInColor("Prepping logs...", ConsoleColor.Yellow);
                    var fileGenerator = new TextFileGenerator(hunter.CapturedLogs);
                    fileGenerator.GroupAndFormat();
                    await fileGenerator.Dump();
                    PrintInColor("The hunt was a", ConsoleColor.White, newLine: false);
                    PrintInColor("success!", ConsoleColor.Green, newLine: false);
                    PrintInColor("Go to", ConsoleColor.White, newLine: false);
                    PrintInColor(fileGenerator.HuntPath!, ConsoleColor.Blue, newLine: false);
                    PrintInColor("to see the spoils.", ConsoleColor.White, newLine: false);
                }
                else Console.WriteLine("No logs were found that satisfy the parameters.");

                if (_isInteractiveMode) Thread.Sleep(5000);
                return _isInteractiveMode && ShouldContinue();
            }
            catch (System.Exception e)
            {
                Console.Clear();
                PrintInColor($"{Environment.NewLine}{e.Message}{Environment.NewLine}", color: ConsoleColor.Red);
                if (_isInteractiveMode) Thread.Sleep(5000);
                return _isInteractiveMode;
            }
        }

        private static bool ShouldContinue()
        {
            Console.WriteLine(Environment.NewLine);
            PrintInColor("Another hunt? (y/n)", ConsoleColor.Yellow);
            return Console.ReadLine()?.Trim().ToLower() == "y";
        }

        private static void PrintQueryObj(IEnumerable<Arg> args)
        {
            string queryObjStr = "{" + Environment.NewLine;
            foreach (Arg arg in args)
                if (arg.Value is not null)
                    queryObjStr += $"\t{arg.Name}: {(arg.Name == "LogLevel" || arg.Name == "Apps" ? string.Join(", ", arg.Value!) : arg.Value)}{Environment.NewLine}";
            queryObjStr += "}";
            Console.WriteLine(queryObjStr);
        }

        private static void PrintInColor(string message, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message + ' ');
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Prompt(string message) => Console.Write($"{Environment.NewLine}{message}{Environment.NewLine}{Environment.NewLine}---> ");
    }
}