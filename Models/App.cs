
namespace LogHunter
{
    public class App
    {
        private App(string name) { Name = name;; }
        public string Name { get; private set; }
        public static App Plutus { get => new("Plutus"); }
        public static App ChromeRiverService { get => new("ChromeriverSyncService"); }
        public static App CobblestoneService { get => new("CobblestoneSyncService"); }
        public static readonly App[] AllApps = [Plutus, ChromeRiverService, CobblestoneService];
        public static new string ToString() => string.Join(Environment.NewLine, AllApps.Select((app, i) => $"{i + 1}) {app.Name}"));
    }
}