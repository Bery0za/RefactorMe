namespace RefactorMe.Dal.Models;

public class SurveyQuestion
{
    public enum QuestionAnswerType
    {
        Boolean,
        Number
    }

    public int Id { get; set; }
    public int SurveyId { get; set; }
    public string Text { get; set; }
    public QuestionAnswerType AnswerType { get; set; }
    public int NumberMin { get; set; }
}
