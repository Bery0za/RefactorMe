namespace RefactorMe.Dal.Models;

public class SurveyResult
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SurveyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Score { get; set; }
}