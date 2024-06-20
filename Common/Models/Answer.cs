namespace Common.Models;

public class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public ColorEnum? AnswerColor { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsFlagged { get; set; }
}