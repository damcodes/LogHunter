namespace LogHunter
{
    public interface IFileGenerator
    {
        string FileType { get; set; }
        IEnumerable<Log> Logs { get; }
        string? HuntPath { get; }
        void GroupAndFormat();
        Task Dump();
    }
}