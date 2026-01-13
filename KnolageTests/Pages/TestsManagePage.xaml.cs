using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using KnolageTests.Models;
using KnolageTests.Services;
using KnolageTests.Helpers;

namespace KnolageTests.Pages
{
    public partial class TestsManagePage : ContentPage
    {
        readonly TestsService _testsService;
        readonly KnowledgeBaseService _kbService = new KnowledgeBaseService();

        // full list of display items
        List<TestDisplay> _allDisplays = new();

        public TestsManagePage()
        {
            InitializeComponent();
            _testsService = ServiceHelper.GetService<TestsService>();
            _ = LoadAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadAsync();
        }

        async Task LoadAsync()
        {
            try
            {
                var tests = await _testsService.GetAllAsync().ConfigureAwait(false);
                var articles = await _kbService.GetAllAsync().ConfigureAwait(false);
                var articleById = (articles ?? new List<KnowledgeArticle>()).ToDictionary(a => a.Id, StringComparer.OrdinalIgnoreCase);

                _allDisplays = (tests ?? new List<Test>()).Select(t => new TestDisplay
                {
                    Test = t,
                    ArticleTitles = (t.ArticleIds ?? new List<string>())
                                    .Select(id => articleById.TryGetValue(id, out var a) ? (a.Title ?? string.Empty) : id)
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .ToList()
                }).ToList();

                await MainThread.InvokeOnMainThreadAsync(() => ApplyFilter(SearchBar?.Text));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Ошибка", $"Не удалось загрузить тесты: {ex.Message}", "OK");
                });
            }
        }

        void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(e.NewTextValue);
        }

        void ApplyFilter(string? query)
        {
            if (_allDisplays == null) _allDisplays = new List<TestDisplay>();

            if (string.IsNullOrWhiteSpace(query))
            {
                ManageCollectionView.ItemsSource = _allDisplays;
                return;
            }

            var q = query.Trim();
            var filtered = _allDisplays.Where(d =>
                (d.Test.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (d.Test.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            ManageCollectionView.ItemsSource = filtered;
        }

        async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadAsync();
        }

        async void OnCreateClicked(object sender, EventArgs e)
        {
            var page = new TestEditorPage();
            if (Navigation != null)
                await Navigation.PushAsync(page);
            else
                Application.Current.MainPage = new NavigationPage(page);
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Test test)
            {
                var page = new TestEditorPage(test.Id);
                if (Navigation != null)
                    await Navigation.PushAsync(page);
                else
                    Application.Current.MainPage = new NavigationPage(page);
            }
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Test test)
            {
                var confirm = await DisplayAlert("Подтвердите удаление",
                    $"Уверены что хотите удалить '{test.Title}'?", "Удалить", "Отмена");

                if (!confirm) return;

                try
                {
                    await _testsService.DeleteAsync(test.Id).ConfigureAwait(false);
                    await LoadAsync();
                }
                catch (Exception ex)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlert("Ошибка", $"Ошибка удаления: {ex.Message}", "OK");
                    });
                }
            }
        }

        // small display helper
        class TestDisplay
        {
            public Test Test { get; set; } = new Test();
            public List<string> ArticleTitles { get; set; } = new List<string>();
        }
    }
}