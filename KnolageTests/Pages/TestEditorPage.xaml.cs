using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using KnolageTests.Models;
using KnolageTests.Services;

namespace KnolageTests.Pages
{
    public partial class TestEditorPage : ContentPage
    {
        readonly TestsService _testsService = new TestsService();
        readonly KnowledgeBaseService _kbService = new KnowledgeBaseService();

        Test _test = new Test();
        List<KnowledgeArticle> _articles = new();
        HashSet<string> _selectedArticleIds = new();

        // New test
        public TestEditorPage()
        {
            InitializeComponent();
            _test = new Test();
            _ = InitializeAsync();
        }

        // Edit by id
        public TestEditorPage(string testId)
        {
            InitializeComponent();
            _test = new Test(); // temporary
            _ = LoadTestAsync(testId);
            _ = InitializeAsync();
        }

        // Edit by instance
        public TestEditorPage(Test test)
        {
            InitializeComponent();
            _test = test ?? new Test();
            _ = InitializeAsync();
        }

        async Task InitializeAsync()
        {
            await LoadArticlesAsync().ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                BindBasicFields();
                RenderArticlesList();
                RenderQuestions();
            });
        }

        async Task LoadTestAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;

            var t = await _testsService.GetByIdAsync(id).ConfigureAwait(false);
            if (t != null)
            {
                _test = t;
                _selectedArticleIds = new HashSet<string>(_test.ArticleIds ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                BindBasicFields();
                RenderArticlesList();
                RenderQuestions();
            });
        }

        async Task LoadArticlesAsync()
        {
            try
            {
                _articles = await _kbService.GetAllAsync().ConfigureAwait(false);
            }
            catch
            {
                _articles = new List<KnowledgeArticle>();
            }
        }

        void BindBasicFields()
        {
            TitleEntry.Text = _test.Title;
            DescriptionEditor.Text = _test.Description;
            if (_test.ArticleIds != null)
                _selectedArticleIds = new HashSet<string>(_test.ArticleIds, StringComparer.OrdinalIgnoreCase);
            else
                _selectedArticleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        void RenderArticlesList()
        {
            ArticlesContainer.Children.Clear();
            if (_articles == null) _articles = new List<KnowledgeArticle>();

            foreach (var a in _articles)
            {
                var chk = new CheckBox
                {
                    IsChecked = _selectedArticleIds.Contains(a.Id),
                    VerticalOptions = LayoutOptions.Center,
                    BindingContext = a,
                    Color = (Color)Application.Current.Resources["PrimaryColor"]

                };
                chk.CheckedChanged += OnArticleCheckedChanged;

                var title = new Label
                {
                    Text = a.Title ?? string.Empty,
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center
                };

                var subtitle = new Label
                {
                    Text = a.Subtitle ?? string.Empty,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                };

                var info = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children = { title, subtitle },
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                var row = new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children = { chk, info }
                };

                ArticlesContainer.Children.Add(row);
            }
        }

        void OnArticleCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox cb && cb.BindingContext is KnowledgeArticle a)
            {
                if (e.Value)
                    _selectedArticleIds.Add(a.Id);
                else
                    _selectedArticleIds.Remove(a.Id);
            }
        }

        void RenderQuestions()
        {
            QuestionsContainer.Children.Clear();
            if (_test.Questions == null) _test.Questions = new List<TestQuestion>();

            for (int qIdx = 0; qIdx < _test.Questions.Count; qIdx++)
            {
                var question = _test.Questions[qIdx];

                var qContainer = new Frame
                {
                    Padding = 8,
                    CornerRadius = 6,
                    HasShadow = false,
                    Content = new VerticalStackLayout { Spacing = 8 },
                    BackgroundColor = (Color)Application.Current.Resources["SurfaceColor"]

                };

                var v = (VerticalStackLayout)qContainer.Content;

                var qEntry = new Entry
                {
                    Text = question.Text,
                    Placeholder = "Текст вопроса"
                };
                qEntry.TextChanged += (s, e) => question.Text = e.NewTextValue ?? string.Empty;
                v.Children.Add(qEntry);

                // Options container
                var optsContainer = new VerticalStackLayout { Spacing = 6 };
                if (question.Options == null) question.Options = new List<TestAnswerOption>();

                for (int oIdx = 0; oIdx < question.Options.Count; oIdx++)
                {
                    var option = question.Options[oIdx];

                    var optRow = new Grid
                    {
                        ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },   // Entry растягивается
                        new ColumnDefinition { Width = GridLength.Auto },   // CheckBox
                        new ColumnDefinition { Width = GridLength.Auto }    // Delete button
                    },
                        ColumnSpacing = 8
                    };

                    var optEntry = new Entry
                    {
                        Text = option.Text,
                        HorizontalOptions = LayoutOptions.Fill
                        
                    };
                    optEntry.TextChanged += (s, e) => option.Text = e.NewTextValue ?? string.Empty;

                    var isCorrect = new CheckBox
                    {
                        IsChecked = option.IsCorrect,
                        VerticalOptions = LayoutOptions.Center
                    };
                    isCorrect.CheckedChanged += (s, e) => option.IsCorrect = e.Value;

                    var delOpt = new Button
                    {
                        ImageSource = "ic_fluent_backspace_24_filled.png",
                        HeightRequest = 40,
                        WidthRequest = 40,
                        BackgroundColor = (Color)Application.Current.Resources["ErrorColor"],
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.End
                        
                    };
                    delOpt.Clicked += (_, __) =>
                    {
                        question.Options.Remove(option);
                        RenderQuestions();
                    };

                    optRow.Add(optEntry, 0, 0);
                    optRow.Add(isCorrect, 1, 0);
                    optRow.Add(delOpt, 2, 0);


                    optsContainer.Children.Add(optRow);
                }

                v.Children.Add(optsContainer);

                var optsActions = new HorizontalStackLayout { Spacing = 8 };
                var addOptBtn = new Button { ImageSource = "ic_fluent_add_circle_24_filled.png", Text = "Добавить ответ", TextColor=Colors.White, BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"], HeightRequest = 40};
                addOptBtn.Clicked += (_, __) =>
                {
                    question.Options.Add(new TestAnswerOption());
                    RenderQuestions();
                };
                var delQBtn = new Button { Text = "Удалить вопрос", BackgroundColor = (Color)Application.Current.Resources["ErrorColor"], TextColor = Colors.White, HeightRequest = 40 };
                delQBtn.Clicked += (_, __) =>
                {
                    _test.Questions.Remove(question);
                    RenderQuestions();
                };

                optsActions.Children.Add(addOptBtn);
                optsActions.Children.Add(delQBtn);

                v.Children.Add(optsActions);

                QuestionsContainer.Children.Add(qContainer);
            }
        }

        void OnAddQuestionClicked(object sender, EventArgs e)
        {
            _test.Questions ??= new List<TestQuestion>();
            var q = new TestQuestion();
            q.Options.Add(new TestAnswerOption());
            _test.Questions.Add(q);
            RenderQuestions();
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var title = TitleEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                await DisplayAlert("Ошибка", "Заголовок не может быть пустым.", "OK");
                return;
            }

            _test.Title = title;
            _test.Description = DescriptionEditor.Text?.Trim() ?? string.Empty;
            _test.ArticleIds = _selectedArticleIds.ToList();

            // Ensure ids for questions/options
            if (_test.Questions != null)
            {
                foreach (var q in _test.Questions)
                {
                    if (string.IsNullOrWhiteSpace(q.Id))
                        q.Id = Guid.NewGuid().ToString();

                    if (q.Options != null)
                    {
                        foreach (var o in q.Options)
                        {
                            if (string.IsNullOrWhiteSpace(o.Id))
                                o.Id = Guid.NewGuid().ToString();
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(_test.Id))
                _test.Id = Guid.NewGuid().ToString();

            try
            {
                await _testsService.SaveAsync(_test).ConfigureAwait(false);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Сохранено", "Тест успешно сохранён.", "OK");
                    await Navigation.PopAsync();
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Ошибка", $"Failed to save: {ex.Message}", "OK");
                });
            }
        }

        async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}