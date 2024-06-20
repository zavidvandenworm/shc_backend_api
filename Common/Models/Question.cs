namespace Common.Models;

public class Question
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public Answer? Answer { get; set; }
}