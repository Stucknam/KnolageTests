using System;
using System.Collections.Generic;

namespace KnolageTests.Models
{
    public class TestQuestion
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<TestAnswerOption> Options { get; set; }

        public TestQuestion()
        {
            Id = Guid.NewGuid().ToString();
            Text = string.Empty;
            Options = new List<TestAnswerOption>();
        }
    }
}