using Microsoft.EntityFrameworkCore;
using RefactorMe.Dal.Models;

namespace RefactorMe.Dal;

public class AppDbContext: DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Survey> Surveys { get; set; }
    public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public DbSet<SurveyResult> SurveyResults { get; set; }
}