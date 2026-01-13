using KnolageTests.Helpers;
using KnolageTests.Models;
using KnolageTests.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnolageTests.Pages
{
    public partial class TestRunPage : ContentPage
    {
        readonly TestsService _testsService = new TestsService();
        Test? _test;
        readonly TestAttemptDatabaseService _dbService;
        readonly INotificationService _notificationService;

        // maps questionId -> set of selected option Ids
        readonly Dictionary<string, HashSet<string>> _selections = new();

        public TestRunPage(Test test)
        {
            InitializeComponent();

            _dbService = ServiceHelper.GetService<TestAttemptDatabaseService>();
            _notificationService = ServiceHelper.GetService<INotificationService>();

            _ = InitializeWithTestAsync(test);
        }

        public TestRunPage(string testId)
        {
            InitializeComponent();

            _dbService = ServiceHelper.GetService<TestAttemptDatabaseService>();
            _notificationService = ServiceHelper.GetService<INotificationService>();

            _ = InitializeWithIdAsync(testId);
        }


        async Task InitializeWithIdAsync(string testId)
        {
            if (string.IsNullOrWhiteSpace(testId))
            {
                await DisplayAlert("Ошибка", "Неправильный Id теста.", "OK");
                await Navigation.PopAsync();
                return;
            }

            _test = await _testsService.GetByIdAsync(testId);
            if (_test == null)
            {
                await DisplayAlert("Не найдено", "Тест не найден.", "OK");
                await Navigation.PopAsync();
                return;
            }

            await InitializeWithTestAsync(_test);
        }

        async Task InitializeWithTestAsync(Test test)
        {
            _test = test ?? throw new ArgumentNullException(nameof(test));

            // set title and description
            TitleLabel.Text = _test.Title;
            DescriptionLabel.Text = _test.Description;
            Title = _test.Title ?? "Тест";

            // prepare empty selection sets
            _selections.Clear();
            foreach (var q in _test.Questions ?? Enumerable.Empty<TestQuestion>())
            {
                _selections[q.Id] = new HashSet<string>();
            }

            // set items source
            QuestionsCollection.ItemsSource = _test.Questions ?? new List<TestQuestion>();

            await Task.CompletedTask;
        }

        public void UpdateTest(string testId)
        {
            _ = InitializeWithIdAsync(testId);
        }

        void OnOptionCheckedChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (sender is not CheckBox cb) return;

            // the CheckBox.BindingContext will be the TestAnswerOption instance
            if (cb.BindingContext is not TestAnswerOption option) return;

            // find the question that contains this option
            var question = _test?.Questions?.FirstOrDefault(q => q.Options != null && q.Options.Any(o => string.Equals(o.Id, option.Id, StringComparison.OrdinalIgnoreCase)));
            if (question == null) return;

            if (!_selections.TryGetValue(question.Id, out var set))
            {
                set = new HashSet<string>();
                _selections[question.Id] = set;
            }

            if (e.Value)
                set.Add(option.Id);
            else
                set.Remove(option.Id);
        }

        private async void OnCheckResultClicked(object? sender, EventArgs e)
        {
            if (_test == null)
            {
                await DisplayAlert("Ошибка", "Тест не загружен.", "OK");
                return;
            }

            int totalQuestions = _test.Questions?.Count ?? 0;
            int correctCount = 0;

            var answers = new List<TestAttemptAnswer>();

            foreach (var question in _test.Questions ?? Enumerable.Empty<TestQuestion>())
            {
                // множество правильных вариантов
                var correctIds = new HashSet<string>(
                    question.Options?.Where(o => o.IsCorrect).Select(o => o.Id)
                    ?? Enumerable.Empty<string>()
                );

                // множество выбранных пользователем вариантов
                _selections.TryGetValue(question.Id, out var selectedIds);
                selectedIds ??= new HashSet<string>();

                bool isCorrect = correctIds.SetEquals(selectedIds);
                if (isCorrect) correctCount++;

                // если пользователь ничего не выбрал — всё равно сохраняем запись
                if (selectedIds.Count == 0)
                {
                    answers.Add(new TestAttemptAnswer
                    {
                        QuestionId = question.Id,
                        SelectedOptionId = null,
                        IsCorrect = false
                    });
                }
                else
                {
                    // сохраняем каждый выбранный вариант
                    foreach (var selectedId in selectedIds)
                    {
                        answers.Add(new TestAttemptAnswer
                        {
                            QuestionId = question.Id,
                            SelectedOptionId = selectedId,
                            IsCorrect = correctIds.Contains(selectedId)
                        });
                    }
                }

            }

            var attempt = new TestAttempt
            {
                TestId = _test.Id,
                CompletedAt = DateTime.Now,
                Score = correctCount,
                MaxScore = totalQuestions
            };

            // сохраняем попытку и ответы в БД
            await _dbService.AddAttemptAsync(attempt, answers);

            // уведомление, если результат не идеален
            if (!attempt.IsPerfect)
            {
                
                _notificationService.ShowNotification(
                    $"Тест: {_test.Title}",
                    "Ваш результат ниже 100%. Попробуйте пройти тест снова.", _test.Id
                );
            }

            // показываем пользователю результат
            await DisplayAlert("Результат", $"Правильно: {correctCount} из {totalQuestions}", "OK");
            await Navigation.PopAsync();

            MessagingCenter.Send(this, "TestCompleted", _test.Id);

        }
    }
}