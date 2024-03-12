
namespace LogHunter
{
    public abstract class FileGenerator(IEnumerable<Log> logs, string fileType) : IFileGenerator
    {
        public string FileType { get; set; } = fileType;
        public IEnumerable<Log> Logs { get; } = logs;
        public string? HuntPath { get; protected set; }
        public abstract Task Dump();
        public abstract void GroupAndFormat();
    }
}