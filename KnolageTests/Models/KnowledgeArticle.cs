using System;
using System.Collections.Generic;

namespace KnolageTests.Models
{
    public class KnowledgeArticle
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; } // replaces Description
        public string ThumbnailPath { get; set; } // replaces Image
        public string? TempThumbnailSource { get; set; }
        public List<string> Tags { get; set; }
        public List<ArticleBlock> Blocks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public KnowledgeArticle()
        {
            Id = Guid.NewGuid().ToString();
            Title = string.Empty;
            Subtitle = string.Empty;
            ThumbnailPath = string.Empty;
            Tags = new List<string>();
            Blocks = new List<ArticleBlock>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }
    }
}