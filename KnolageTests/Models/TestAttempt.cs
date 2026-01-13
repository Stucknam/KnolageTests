using SQLite;

namespace KnolageTests.Models
{
    public class TestAttempt
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? TestId { get; set; }
        public DateTime CompletedAt { get; set; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public bool IsPerfect => Score == MaxScore;
    }
}
