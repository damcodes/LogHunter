
namespace LogHunter
{
    public class Apps
    {
        private Apps(string name) { Name = name; }
        public string Name { get; private set; }
        public static Apps Plutus { get => new("Plutus"); }
        public static Apps ChromeRiverService { get => new("ChromeRiverService"); }
        public static Apps CobblestoneService { get => new("CobblestoneService"); }
        public static readonly string[] AllApps = [Plutus.Name, ChromeRiverService.Name, CobblestoneService.Name];
        public static new string ToString() => string.Join(Environment.NewLine, AllApps.Select((appName, i) => $"{i + 1} {appName}"));
    }
}