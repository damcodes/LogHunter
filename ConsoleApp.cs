namespace LogHunter
{
    public class ConsoleApp(bool isInteractiveMode)
    {
        private readonly bool _isInteractiveMode = isInteractiveMode;
        public bool Run(string[] args)
        {
            try
            {
                if (_isInteractiveMode)
                {
                    
                    
                    Console.WriteLine("Another hunt? (y/n)");
                    return Console.ReadLine()?.Trim().ToLower() == "y";
                }
                else
                {
                    var cliArgs = new CliArgs(args).Verify();
                    if (!cliArgs.Verified) 
                    {
                        string errors = string.Join('\n', cliArgs.Errors);
                        throw new Exception(errors);
                    }
                    cliArgs.Build();
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.Message}\n");
                return _isInteractiveMode;
            }
        }
    }
}