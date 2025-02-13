namespace RefactorMe.Dto;

public class UserStatsDto
{
    public string Name { get; set; }
    public int Last30DaysSurveysCount { get; set; }
    public int Last30DaysScore { get; set; }
    public int TotalSurveysCount { get; set; }
    public int TotalScore { get; set; }
}