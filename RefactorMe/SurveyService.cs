using Microsoft.EntityFrameworkCore;
using RefactorMe.Dal;
using RefactorMe.Dal.Models;
using RefactorMe.Dto;

namespace RefactorMe;

public class SurveyService
{
    /// <summary>
    /// Получение опросов для пользователя
    /// </summary>
    public async Task<SurveyDto[]> GetIncompleteSurveys(int userId)
    {
        await using var db = new AppDbContext();

        var userCompletedSurveys = db.SurveyResults
            .Where(sr => sr.UserId == userId)
            .Select(sr => sr.SurveyId);

        return await db.Surveys
            .Include(s => s.Questions)
            .Where(s => s.IsActive && !userCompletedSurveys.Contains(s.Id))
            .Select(s => new SurveyDto()
            {
                Id = s.Id,
                Questions = s.Questions
                    .Select(q => new SurveyDto.SurveyQuestionDto()
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Type = q.AnswerType
                    }).ToArray()
            }).ToArrayAsync();
    }

    /// <summary>
    /// Сохранение результатов опроса
    /// </summary>
    public async Task SaveAnswers(SurveyAnswersDto value)
    {
        await using var db = new AppDbContext();

        var questionIds = value.Answers
            .Select(a => a.QuestionId)
            .ToArray();
        
        var questions = await db.SurveyQuestions
            .Where(sq => questionIds.Contains(sq.Id))
            .ToDictionaryAsync(sq => sq.Id);

        var score = 0;
        foreach (var answer in value.Answers)
        {
            if (!questions.TryGetValue(answer.QuestionId, out var question))
            {
                throw new ArgumentException($"Question {answer.QuestionId} not found.");
            }

            if (question.AnswerType == SurveyQuestion.QuestionAnswerType.Boolean && answer.Value is true)
            {
                score++;
            }
            else if (question.AnswerType == SurveyQuestion.QuestionAnswerType.Number && answer.Value is int intValue && intValue > question.NumberMin)
            {
                score++;
            }
        }

        await db.SurveyResults.AddAsync(new SurveyResult()
        {
            UserId = value.UserId,
            SurveyId = value.SurveyId,
            Score = score,
            CreatedAt = DateTime.Now
        });

        await db.SaveChangesAsync();
    }
}