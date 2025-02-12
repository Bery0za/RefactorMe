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
            .ThenInclude(q => q.PollOptions)
            .Where(s => s.IsActive && !userCompletedSurveys.Contains(s.Id))
            .Select(s => new SurveyDto()
            {
                Id = s.Id,
                Questions = s.Questions
                    .Select(q => new SurveyDto.SurveyQuestionDto()
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Type = q.AnswerType,
                        PollOptions = q.PollOptions
                            .Select(po => new SurveyDto.SurveyQuestionDto.PollOptionDto()
                        {
                            Id = po.Id,
                            Text = po.Text
                        }).ToArray()
                    }).ToArray()
            }).ToArrayAsync();
    }
    
    /// <summary>
    /// Получение статистики пользователя
    /// </summary>
    public async Task<UserStatsDto> GetUserStats(int userId)
    {
        await using var db = new AppDbContext();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            throw new ArgumentException($"User {userId} not found.");
        }

        var fromDate = DateTime.Now.AddDays(-30);

        var last30DaysStats = await db.SurveyResults
            .Where(u => u.UserId == userId && u.CreatedAt > fromDate)
            .GroupBy(u => u.Id)
            .Select(g => new { Count = g.Count(), Score = g.Sum(sr => sr.Score) })
            .FirstOrDefaultAsync();
        
        var totalStats = await db.SurveyResults
            .Where(u => u.UserId == userId)
            .GroupBy(u => u.Id)
            .Select(g => new { Count = g.Count(), Score = g.Sum(sr => sr.Score) })
            .FirstOrDefaultAsync();

        return new UserStatsDto()
        {
            Name = user.Name,
            Last30DaysSurveysCount = last30DaysStats?.Count ?? 0,
            Last30DaysScore = last30DaysStats?.Score ?? 0,
            TotalSurveysCount = totalStats?.Count ?? 0,
            TotalScore = totalStats?.Score ?? 0
        };
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

            if ((question.AnswerType == SurveyQuestion.QuestionAnswerType.Boolean && answer.Value is true)
                || (question.AnswerType == SurveyQuestion.QuestionAnswerType.Number && answer.Value is int intValue && intValue > question.NumberMin)
                || (question.AnswerType == SurveyQuestion.QuestionAnswerType.Poll && answer.Value is int pollOptionId && pollOptionId == question.ValidPollOptionId))
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