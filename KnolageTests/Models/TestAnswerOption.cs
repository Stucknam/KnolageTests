using System;

namespace KnolageTests.Models
{
    public class TestAnswerOption
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }

        public TestAnswerOption()
        {
            Id = Guid.NewGuid().ToString();
            Text = string.Empty;
            IsCorrect = false;
        }
    }
}