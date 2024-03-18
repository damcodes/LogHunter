using LogHunter;

try
{
    bool run;

    do 
    {
        var app = new ConsoleApp(args.Length == 0);
        run = await app.Run(args);
    }
    while (run);
}
catch (System.Exception e)
{
    Console.WriteLine($"\n{e.Message}\n");
}


