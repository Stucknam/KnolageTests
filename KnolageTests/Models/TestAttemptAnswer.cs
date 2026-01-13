using SQLite;

namespace KnolageTests.Models
{
    public class TestAttemptAnswer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int AttemptId { get; set; }
        public string? QuestionId { get; set; }
        public string? SelectedOptionId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
