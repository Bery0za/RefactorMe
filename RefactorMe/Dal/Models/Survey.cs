namespace RefactorMe.Dal.Models;

public class Survey
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public ICollection<SurveyQuestion> Questions { get; set; } 
}