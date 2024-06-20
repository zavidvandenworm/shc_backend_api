namespace Common.Models;

public class HealthCheck
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int UserId { get; set; }
    public bool IsSubmitted { get; set; }
    public int VersionId { get; set; }
    public Company Company { get; set; } = null!;
    public List<Question> Questions { get; set; } = null!;
}