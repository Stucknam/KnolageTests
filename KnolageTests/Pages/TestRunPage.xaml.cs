using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using KnolageTests.Models;
using KnolageTests.Services;

namespace KnolageTests.Pages
{
    public partial class TestRunPage : ContentPage
    {
        readonly TestsService _testsService = new TestsService();
        Test? _test;

        // maps questionId -> set of selected option Ids
        readonly Dictionary<string, HashSet<string>> _selections = new();

        public TestRunPage(Test test)
        {
            InitializeComponent();
            _ = InitializeWithTestAsync(test);
        }

        public TestRunPage(string testId)
        {
            InitializeComponent();
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

        async void OnCheckResultClicked(object? sender, EventArgs e)
        {
            if (_test == null)
            {
                await DisplayAlert("Ошибка", "Тест не загружен.", "OK");
                return;
            }

            int total = 0;
            int correctCount = 0;

            foreach (var q in _test.Questions ?? Enumerable.Empty<TestQuestion>())
            {
                total++;

                // correct option ids for this question
                var correctIds = new HashSet<string>(q.Options?.Where(o => o.IsCorrect).Select(o => o.Id) ?? Enumerable.Empty<string>());

                // user's selected ids
                _selections.TryGetValue(q.Id, out var selectedIds);
                selectedIds ??= new HashSet<string>();

                // consider a question correct if selected set equals the correct set
                if (correctIds.SetEquals(selectedIds))
                    correctCount++;
            }

            await DisplayAlert("Результат", $"Правильно: {correctCount} из {total}", "OK");
        }
    }
}