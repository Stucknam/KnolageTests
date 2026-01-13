using KnolageTests.Services;
using KnolageTests.Models;
using KnolageTests.Helpers;

namespace KnolageTests.Pages;

public partial class AttemptDetailsPage : ContentPage
{
    private readonly TestAttemptDatabaseService _db;
    private readonly TestsService _testsService;
    private readonly int _attemptId;

    public AttemptDetailsPage(int attemptId)
    {
        InitializeComponent();

        _db = ServiceHelper.GetService<TestAttemptDatabaseService>();
        _testsService = ServiceHelper.GetService<TestsService>();

        _attemptId = attemptId;

        LoadAttempt();
    }

    private async void LoadAttempt()
    {
        QuestionsContainer.Children.Clear();

        // 1. «агружаем попытку и еЄ ответы
        var (attempt, answers) = await _db.GetAttemptWithAnswerAsync(_attemptId);

        // 2. «агружаем тест
        var test = await _testsService.GetByIdAsync(attempt.TestId);
        if (test?.Questions == null || test.Questions.Count == 0)
        {
            QuestionsContainer.Children.Add(new Label
            {
                Text = "¬опросы теста не найдены.",
                TextColor = Colors.Gray
            });
            return;
        }

        // 3. ƒл€ каждого вопроса Ч ищем св€занные ответы (или их отсутствие)
        foreach (var question in test.Questions)
        {
            var questionAnswers = answers
                .Where(a => a.QuestionId == question.Id)
                .ToList();

            // если вообще нет записи Ч считаем вопрос неотвеченным,
            // но всЄ равно показываем его
            var selectedIds = new HashSet<string>(
                questionAnswers
                    .Where(a => !string.IsNullOrEmpty(a.SelectedOptionId))
                    .Select(a => a.SelectedOptionId!)
            );

            var card = new Frame
            {
                CornerRadius = 12,
                Padding = 14,
                BackgroundColor = (Color)Application.Current.Resources["SurfaceColor"],
                HasShadow = true
            };

            var layout = new VerticalStackLayout
            {
                Spacing = 8
            };

            // заголовок вопроса
            layout.Children.Add(new Label
            {
                Text = question.Text,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18
            });

            // необ€зательно, но удобно: пометка, отвечЄн вопрос или нет
            bool isAnswered = selectedIds.Count > 0;
            layout.Children.Add(new Label
            {
                Text = isAnswered ? "ќтвечено" : "Ѕез ответа",
                FontSize = 12,
                TextColor = isAnswered ? Colors.Gray : Colors.OrangeRed
            });

            // варианты ответа
            foreach (var option in question.Options ?? Enumerable.Empty<TestAnswerOption>())
            {
                Color color;

                if (option.IsCorrect)
                {
                    // правильный вариант Ч зелЄный
                    color = (Color)Application.Current.Resources["SuccessColor"];
                }
                else if (selectedIds.Contains(option.Id))
                {
                    // выбранный, но неправильный Ч красный
                    color = (Color)Application.Current.Resources["ErrorColor"];
                }
                else
                {
                    // остальные Ч обычный
                    color = (Color)Application.Current.Resources["TextSecondaryColor"];
                }

                layout.Children.Add(new Label
                {
                    Text = "Х " + option.Text,
                    TextColor = color,
                    FontSize = 16
                });
            }

            card.Content = layout;
            QuestionsContainer.Children.Add(card);
        }
    }
}