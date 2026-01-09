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
    public partial class KnowledgeBaseManagePage : ContentPage
    {
        readonly KnowledgeBaseService _service = new KnowledgeBaseService();
        List<KnowledgeArticle> _articles = new();

        public KnowledgeBaseManagePage()
        {
            InitializeComponent();
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
                _articles = await _service.GetAllAsync().ConfigureAwait(false);
                // Apply current search (or show all if empty)
                await MainThread.InvokeOnMainThreadAsync(() => ApplyFilter(SearchBar?.Text));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Ошибка", $"Не удалось загрузить статьи: {ex.Message}", "OK");
                });
            }
        }

        void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(e.NewTextValue);
        }

        void ApplyFilter(string? query)
        {
            if (_articles == null) _articles = new List<KnowledgeArticle>();

            if (string.IsNullOrWhiteSpace(query))
            {
                ManageCollectionView.ItemsSource = _articles;
                return;
            }

            var q = query.Trim();
            var filtered = _articles.Where(a =>
                (a.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (a.Subtitle?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (a.Tags != null && a.Tags.Any(tag => tag?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false))
                || (a.Blocks != null && a.Blocks.Any(b =>
                    (b.Type == BlockType.Header
                     || b.Type == BlockType.Paragraph
                     || b.Type == BlockType.Quote
                     || b.Type == BlockType.List)
                    && (b.Content?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false))))
                .ToList();

            ManageCollectionView.ItemsSource = filtered;
        }

        async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadAsync();
        }

        async void OnCreateClicked(object sender, EventArgs e)
        {
            var page = new ArticleEditorPage();
            if (Navigation != null)
                await Navigation.PushAsync(page);
            else
                Application.Current.MainPage = new NavigationPage(page);
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is KnowledgeArticle article)
            {
                var page = new ArticleEditorPage(article.Id);
                if (Navigation != null)
                    await Navigation.PushAsync(page);
                else
                    Application.Current.MainPage = new NavigationPage(page);
            }
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is KnowledgeArticle article)
            {
                var confirm = await DisplayAlert("Подтвердить удаление",
                    $"Вы уверены что хотите удалить '{article.Title}'?", "Удалить", "Отмена");

                if (!confirm) return;

                try
                {
                    await _service.DeleteAsync(article.Id).ConfigureAwait(false);
                    await LoadAsync();
                }
                catch (Exception ex)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlert("Ошибка", $"Не удалось удалить: {ex.Message}", "OK");
                    });
                }
            }
        }
    }
}