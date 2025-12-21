using System;
using System.Collections.Generic;

namespace KnolageTests.Models
{
    public class Test
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> ArticleIds { get; set; }
        public List<TestQuestion> Questions { get; set; }

        public Test()
        {
            Id = Guid.NewGuid().ToString();
            Title = string.Empty;
            Description = string.Empty;
            ArticleIds = new List<string>();
            Questions = new List<TestQuestion>();
        }
    }
}