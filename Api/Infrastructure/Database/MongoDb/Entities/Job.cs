namespace Api.Infrastructure.Database.MongoDb.Entities;

public class Job
{
    public string? Company { get; set; }
    public string? Position { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Responsibilities { get; set; }
}