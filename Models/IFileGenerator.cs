namespace LogHunter
{
    public interface IFileGenerator
    {
        string FileType { get; set; }
        Dictionary<string, IEnumerable<Log>> LogsByApp { get; }
        string? HuntPath { get; }
        void GroupAndFormat();
        Task Dump();
    }
}