namespace KnolageTests.Models
{
    public class Article
    {
        public string Title { get; set; }
        public string Description { get; set; }    // краткий подзаголовок
        public string Image { get; set; }
        public string[] Tags { get; set; }
        public string Content { get; set; }        // полный текст статьи
    }
}