using Common.Models;

namespace Common.Dto;

public class AnswerDto
{
    public ColorEnum? AnswerColor { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsFlagged { get; set; }
}