namespace Common.Dto;

public class SendAnswerDto
{
    public int QuestionId { get; set; }
    public int UserId { get; set; }
    public AnswerDto Answer { get; set; } = null!;
}