using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using KnolageTests.Models;
using KnolageTests.Services;
using KnolageTests.Helpers;


namespace KnolageTests.Pages
{
    public partial class ArticlePage : ContentPage
    {
        KnowledgeArticle? _article;
        readonly TestsService _testsService;
        private readonly TestAttemptDatabaseService _db;
        readonly KnowledgeBaseService _service = new KnowledgeBaseService();
        public ArticlePage(string articleId)
        {
            InitializeComponent();
            _db = ServiceHelper.GetService<TestAttemptDatabaseService>();
            _ = LoadByIdAsync(articleId);
            _testsService = ServiceHelper.GetService<TestsService>();
            MessagingCenter.Subscribe<TestRunPage, string>(this, "TestCompleted", async (sender, testId) =>
            {
                await LoadTestsAsync();
            });
        }

        public ArticlePage(KnowledgeArticle article)
        {
            InitializeComponent();
            if (article == null) throw new ArgumentNullException(nameof(article));
            _article = article;
            BindingContext = article;
            Title = article.Title;
            RenderArticle(article);
            _db = ServiceHelper.GetService<TestAttemptDatabaseService>();
            _testsService = ServiceHelper.GetService<TestsService>();
            _ = LoadTestsAsync();
            MessagingCenter.Subscribe<TestRunPage, string>(this, "TestCompleted", async (sender, testId) =>
            {
                await LoadTestsAsync();
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<TestRunPage, string>(this, "TestCompleted");
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
                var lastAttempt = await _db.GetLastAttemptForTestAsync(t.Id);
                var local = t;

                int? percent = null;

                if (lastAttempt != null && lastAttempt.MaxScore > 0)
                {
                    percent = (int)Math.Round((double)lastAttempt.Score / lastAttempt.MaxScore * 100);
                }



                // Карточка
                var card = new Frame
                {
                    CornerRadius = 12,
                    Padding = 12,
                    Margin = new Thickness(0, 8),
                    BackgroundColor = (Color)Application.Current.Resources["SurfaceColor"],
                    HasShadow = true
                };

                var cardLayout = new VerticalStackLayout
                {
                    Spacing = 8
                };

                // Кнопка запуска теста
                var testButton = new Button
                {
                    Text = string.IsNullOrWhiteSpace(local.Title) ? "Без названия" : local.Title,
                    HorizontalOptions = LayoutOptions.Fill,
                    BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"],
                    TextColor = (Color)Application.Current.Resources["TextPrimaryColor"],
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 8
                };
                testButton.Clicked += async (_, __) =>
                {
                    await Navigation.PushAsync(new TestRunPage(local.Id));
                };

                // Горизонтальный блок: результат + кнопка истории
                var bottomRow = new HorizontalStackLayout
                {
                    Spacing = 10,
                    VerticalOptions = LayoutOptions.Center
                };

                // Результат последней попытки

                var resultLabel = new Label
                {

                    Text = percent != null
                        ? $"Последний результат: {percent}%"
                        : "Нет попыток",
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                };

                // Кнопка истории (ImageButton)
                var historyButton = new ImageButton
                {
                    Source = "ic_fluent_history_24_filled.png", // добавь иконку в Resources/Images
                    BackgroundColor = Colors.Transparent,
                    WidthRequest = 32,
                    HeightRequest = 32,
                    
                };
                historyButton.Clicked += async (_, __) =>
                {
                   await Navigation.PushAsync(new AttemptsHistoryPage(local.Id));
                };

                bottomRow.Children.Add(resultLabel);
                bottomRow.Children.Add(historyButton);

                // Собираем карточку
                cardLayout.Children.Add(testButton);
                cardLayout.Children.Add(bottomRow);

                card.Content = cardLayout;

                TestsContainer.Children.Add(card);
            }
        }

        async void OnStartTestClicked(object sender, EventArgs e)
        {

            if (_article == null) return;
            var tests = await _testsService.GetByArticleIdAsync(_article.Id);
            var first = tests?.FirstOrDefault();
            if (first != null)
                await Navigation.PushAsync(new TestRunPage(first.Id));
        }
        void RenderArticle(KnowledgeArticle article)
        {
 
            if (!string.IsNullOrWhiteSpace(article.ThumbnailPath))
            {
                ThumbnailImage.Source = article.ThumbnailPath;
                ThumbnailImage.IsVisible = true;
            }
            else
            {
                ThumbnailImage.IsVisible = false;
            }

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