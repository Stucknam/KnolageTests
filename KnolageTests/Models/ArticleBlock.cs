using System;

namespace KnolageTests.Models
{
    public class ArticleBlock
    {
        public BlockType Type { get; set; }
        public string Content { get; set; }
        public string? TempImageSource { get; set; } = null;

        public ArticleBlock()
        {
            Content = string.Empty;
            Type = BlockType.Paragraph;
        }

        public ArticleBlock(BlockType type, string content)
        {
            Type = type;
            Content = content ?? string.Empty;
        }
    }
}