using LogHunter;

try
{
    bool run;

    do 
    {
        var app = new ConsoleApp(args.Length == 0);
        run = app.Run(args);
    }
    while (run);
}
catch (Exception e)
{
    Console.WriteLine($"\n{e.Message}\n");
}


