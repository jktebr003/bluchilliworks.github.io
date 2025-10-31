namespace Web.Shared;

public class BuildInfo
{
    public string BuildId { get; set; }
    public string BuildNumber { get; set; }
    public string Branch { get; set; }
    public string Commit { get; set; }
    public string Pipeline { get; set; }
    public string Date { get; set; }
}
