using RefactorMe.Dal.Models;

namespace RefactorMe.Dto;

public class SurveyDto
{
    public class SurveyQuestionDto
    {
        public int Id { get; set; }
        public SurveyQuestion.QuestionAnswerType Type { get; set; }
        public string Text { get; set; }
    }

    public int Id { get; set; }
    public SurveyQuestionDto[] Questions { get; set; }
}