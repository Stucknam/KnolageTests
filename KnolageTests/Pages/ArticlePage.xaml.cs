using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using KnolageTests.Models;
using KnolageTests.Services;

namespace KnolageTests.Pages
{
    public partial class ArticlePage : ContentPage
    {
        KnowledgeArticle? _article;
        readonly TestsService _testsService = new TestsService();

        readonly KnowledgeBaseService _service = new KnowledgeBaseService();

        // Constructor: load by KnowledgeArticle Id
        public ArticlePage(string articleId)
        {
            InitializeComponent();
            _ = LoadByIdAsync(articleId);
        }

        // Constructor: directly provide KnowledgeArticle
        public ArticlePage(KnowledgeArticle article)
        {
            InitializeComponent();
            if (article == null) throw new ArgumentNullException(nameof(article));
            _article = article;
            BindingContext = article;
            Title = article.Title;
            RenderArticle(article);
            _ = LoadTestsAsync();
        }

        
        async Task LoadByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                await MainThread.InvokeOnMainThreadAsync(() => DisplayAlert("Ошибка", "Неверный идентификатор статьи.", "OK"));
                await MainThread.InvokeOnMainThreadAsync(() => Navigation?.PopAsync());
                return;
            }

            var article = await _service.GetByIdAsync(id).ConfigureAwait(false);
            if (article == null)
            {
                await MainThread.InvokeOnMainThreadAsync(() => DisplayAlert("Не найдено", "Статья не найдена.", "OK"));
                await MainThread.InvokeOnMainThreadAsync(() => Navigation?.PopAsync());
                return;
            }

            // ensure field is set so tests can be loaded
            _article = article;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                BindingContext = article;
                Title = article.Title;
                RenderArticle(article);
                _ = LoadTestsAsync();
            });
        }

        async Task LoadTestsAsync()
        {
            TestsContainer.Children.Clear();

            if (_article == null)
            {
                TestsContainer.Children.Add(new Label { Text = "Для этой статьи пока нет тестов.", TextColor = Colors.Gray });
                return;
            }

            List<Test> tests;
            try
            {
                tests = await _testsService.GetByArticleIdAsync(_article.Id);
            }
            catch
            {
                // on error, show friendly message
                TestsContainer.Children.Add(new Label { Text = "Не удалось загрузить тесты.", TextColor = Colors.Gray });
                return;
            }

            if (tests == null || tests.Count == 0)
            {
                TestsContainer.Children.Add(new Label { Text = "Для этой статьи пока нет тестов.", TextColor = Colors.Gray });
                return;
            }

            foreach (var t in tests)
            {
                var local = t;
                var btn = new Button
                {
                    Text = string.IsNullOrWhiteSpace(local.Title) ? "Без названия" : local.Title,
                    HorizontalOptions = LayoutOptions.Fill,
                    BackgroundColor = Colors.Transparent,
                    BorderColor = Colors.Gray
                };
                btn.Clicked += async (_, __) =>
                {
                    // navigate to TestRunPage by id
                    await Navigation.PushAsync(new TestRunPage(local.Id));
                };
                TestsContainer.Children.Add(btn);
            }
        }

        async void OnStartTestClicked(object sender, EventArgs e)
        {
            // kept for compatibility if referenced elsewhere; no-op or open first test
            if (_article == null) return;
            var tests = await _testsService.GetByArticleIdAsync(_article.Id);
            var first = tests?.FirstOrDefault();
            if (first != null)
                await Navigation.PushAsync(new TestRunPage(first.Id));
        }
        void RenderArticle(KnowledgeArticle article)
        {
            // Thumbnail
            if (!string.IsNullOrWhiteSpace(article.ThumbnailPath))
            {
                ThumbnailImage.Source = article.ThumbnailPath;
                ThumbnailImage.IsVisible = true;
            }
            else
            {
                ThumbnailImage.IsVisible = false;
            }

            // Tags
            TagsContainer.Children.Clear();
            if (article.Tags != null)
            {
                foreach (var tag in article.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    var chip = new Frame
                    {
                        Padding = new Thickness(8, 4),
                        CornerRadius = 12,
                        HasShadow = false,
                        BackgroundColor = Colors.LightGray,
                        Content = new Label
                        {
                            Text = tag,
                            FontSize = 12,
                            TextColor = Colors.Black
                        },
                        Margin = new Thickness(0, 0, 8, 0)
                    };
                    TagsContainer.Children.Add(chip);
                }
            }

            // Blocks
            BlocksContainer.Children.Clear();
            if (article.Blocks == null || article.Blocks.Count == 0)
                return;

            foreach (var block in article.Blocks)
            {
                switch (block.Type)
                {
                    case BlockType.Header:
                        BlocksContainer.Children.Add(new Label
                        {
                            Text = block.Content,
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold,
                            LineBreakMode = LineBreakMode.WordWrap
                        });
                        break;

                    case BlockType.Paragraph:
                        BlocksContainer.Children.Add(new Label
                        {
                            Text = block.Content,
                            FontSize = 16,
                            LineBreakMode = LineBreakMode.WordWrap
                        });
                        break;

                    case BlockType.Image:
                        BlocksContainer.Children.Add(new Image
                        {
                            Source = block.Content,
                            Aspect = Aspect.AspectFit,
                            HeightRequest = 200,
                            HorizontalOptions = LayoutOptions.Center
                        });
                        break;

                    case BlockType.List:
                        var listStack = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(6, 0) };
                        var lines = (block.Content ?? string.Empty)
                                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var ln in lines)
                        {
                            listStack.Children.Add(new Label
                            {
                                Text = "• " + ln.Trim(),
                                FontSize = 16,
                                LineBreakMode = LineBreakMode.WordWrap
                            });
                        }
                        BlocksContainer.Children.Add(listStack);
                        break;

                    case BlockType.Quote:
                        BlocksContainer.Children.Add(new Frame
                        {
                            Padding = 12,
                            Margin = new Thickness(0, 6),
                            HasShadow = false,
                            BackgroundColor = Colors.Transparent,
                            BorderColor = Colors.Gray,
                            Content = new Label
                            {
                                Text = block.Content,
                                FontAttributes = FontAttributes.Italic,
                                LineBreakMode = LineBreakMode.WordWrap
                            }
                        });
                        break;

                    case BlockType.Divider:
                        BlocksContainer.Children.Add(new BoxView
                        {
                            HeightRequest = 1,
                            BackgroundColor = Colors.Gray,
                            Margin = new Thickness(0, 8)
                        });
                        break;
                }
            }
        }

    }
}