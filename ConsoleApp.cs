
namespace LogHunter
{
    public class ConsoleApp(bool isInteractiveMode)
    {
        private readonly bool _isInteractiveMode = isInteractiveMode;
        private IEnumerable<Arg> Args { get; } =
        [
            new Arg<IEnumerable<string>>("LogLevel"),
            new Arg<string?>("Callsite"),
            new Arg<DateTime>("Start"),
            new Arg<DateTime>("End"),
            new Arg<string?>("TransactionId"),
            new Arg<string?>("UserId"),
            new Arg<string?>("Message"),
        ];
        private ICollection<string> Errors { get; } = [];
        public bool Run(string[] args)
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
                    if (Errors.Count > 0) throw new Exception(string.Join(Environment.NewLine, Errors));
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
                        throw new Exception(string.Join(Environment.NewLine, Errors));
                    }
                }
                PrintInColor("Beginning hunt for logs with the following query filters:", color: ConsoleColor.Yellow);
                PrintQueryObj(Args);

                var hunter = new Hunter(Args.Where(arg => arg.Value is not null));
                hunter.HuntLogs();

                PrintInColor($"Captured {hunter.CapturedLogs.Count()} {(hunter.CapturedLogs.Count() == 1 ? "log" : "logs")}", color: ConsoleColor.Green);
                Thread.Sleep(5000);
                return _isInteractiveMode && ShouldContinue();
            }
            catch (Exception e)
            {
                Console.Clear();
                PrintInColor($"{Environment.NewLine}{e.Message}{Environment.NewLine}", color: ConsoleColor.Red);
                Thread.Sleep(5000);
                return _isInteractiveMode;
            }
        }

        private static bool ShouldContinue()
        {
            Console.WriteLine("Another hunt? (y/n)");
            return Console.ReadLine()?.Trim().ToLower() == "y";
        }

        private static void PrintQueryObj(IEnumerable<Arg> args)
        {
            string queryObjStr = "{" + Environment.NewLine;
            foreach (Arg arg in args)
                if (arg.Value is not null)
                    queryObjStr += $"\t{arg.Name}: {(arg.Name == "LogLevel" ? string.Join(", ", arg.Value!) : arg.Value)}{Environment.NewLine}";
            queryObjStr += "}";
            Console.WriteLine(queryObjStr);
        }

        private static void PrintInColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Prompt(string message) => Console.Write($"{Environment.NewLine}{message}{Environment.NewLine}{Environment.NewLine}---> ");
    }
}