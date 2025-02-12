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
    public async Task<SurveyDto[]> GetSurveys(int userId)
    {
        await using var db = new AppDbContext();

        var userCompletedSurveys = db.SurveyResults
            .Where(sr => sr.UserId == userId)
            .Select(sr => sr.SurveyId);

        return await db.Surveys
            .Include(x => x.Questions)
            .Where(x => x.IsActive && !userCompletedSurveys.Contains(x.Id))
            .Select(x => new SurveyDto()
            {
                Id = x.Id,
                Questions = x.Questions
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

        var s = 0;
        foreach (var v in value.Answers)
        {
            if (!questions.TryGetValue(v.QuestionId, out var q))
            {
                throw new ArgumentException($"Question {v.QuestionId} not found.");
            }

            if (q.AnswerType == SurveyQuestion.QuestionAnswerType.Boolean && (bool)v.Value == true)
            {
                s++;
            }
            else if (q.AnswerType == SurveyQuestion.QuestionAnswerType.Number && (int)v.Value > q.NumberMin)
            {
                s++;
            }
        }

        await db.SurveyResults.AddAsync(new SurveyResult()
        {
            UserId = value.UserId,
            SurveyId = value.SurveyId,
            Score = s,
            CreatedAt = DateTime.Now
        });

        await db.SaveChangesAsync();
    }
}