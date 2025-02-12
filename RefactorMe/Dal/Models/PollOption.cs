namespace RefactorMe.Dal.Models;

public class PollOption
{
	public int Id { get; set; }
	public string Text { get; set; }
	public int QuestionId { get; set; }
}