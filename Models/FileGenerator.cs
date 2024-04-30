
namespace LogHunter
{
    public abstract class FileGenerator(Dictionary<string, IEnumerable<Log>> logsByApp, string fileType) : IFileGenerator
    {
        public string FileType { get; set; } = fileType;
        public Dictionary<string, IEnumerable<Log>> LogsByApp { get; } = logsByApp;
        public string? HuntPath { get; protected set; }
        public abstract Task Dump();
        public abstract void GroupAndFormat();
    }
}