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
    public partial class KnowledgeBaseListPage : ContentPage
    {
        readonly KnowledgeBaseService _service = new KnowledgeBaseService();
        List<KnowledgeArticle> _articles = new();

        public KnowledgeBaseListPage()
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
                ArticlesCollectionView.ItemsSource = _articles;
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

            ArticlesCollectionView.ItemsSource = filtered;
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is KnowledgeArticle selected)
            {
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                var page = new ArticlePage(selected);
                if (Navigation != null)
                    _ = Navigation.PushAsync(page);
                else
                    Application.Current.MainPage = new NavigationPage(page);
            }
        }

    }
}