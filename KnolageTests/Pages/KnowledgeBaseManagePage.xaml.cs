using KnolageTests.Models;
using KnolageTests.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KnolageTests.Pages
{
    public partial class KnowledgeBaseManagePage : ContentPage
    {
        readonly KnowledgeBaseService _service = new KnowledgeBaseService();
        List<KnowledgeArticle> _articles = new();
        public ObservableCollection<KnowledgeArticle> VisibleArticles { get; }
    = new ObservableCollection<KnowledgeArticle>();

        private bool _isLoaded;
        private bool _isLoading;

        public KnowledgeBaseManagePage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (!_isLoaded)
            {
                _isLoaded = true;
                await LoadAsync();
            }
        }

        async Task LoadAsync()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => SetLoading(true));
                //await Task.Delay(5000);
                var list = await _service.GetAllAsync().ConfigureAwait(false);
                // Apply current search (or show all if empty)
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _articles = list ?? new();
                    ApplyFilter(SearchBar?.Text);
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    DisplayAlert("Ошибка", $"Не удалось загрузить статьи: {ex.Message}", "OK"));
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() => SetLoading(false));
            }
        }

        void SetLoading(bool isLoading)
        {
            _isLoading = isLoading;
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;

            ManageCollectionView.EmptyView = isLoading ? "" : "Статьи не найдены.";

        }

        void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(e.NewTextValue);
        }

        void ApplyFilter(string? query)
        {
            if (_articles == null || _articles.Count == 0)
            {
                VisibleArticles.Clear();
                return;
            }

            IEnumerable<KnowledgeArticle> filtered;

            if (string.IsNullOrWhiteSpace(query))
            {
                filtered = _articles;
            }
            else
            {
                var q = query.Trim();

                filtered = _articles.Where(a =>
                    (a.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (a.Subtitle?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (a.Tags?.Any(tag => tag?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ?? false)
                    || (a.Blocks?.Any(b =>
                        (b.Type == BlockType.Header
                         || b.Type == BlockType.Paragraph
                         || b.Type == BlockType.Quote
                         || b.Type == BlockType.List)
                        && (b.Content?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)) ?? false)
                );
            }

            UpdateVisibleCollection(filtered);
        }

        void UpdateVisibleCollection(IEnumerable<KnowledgeArticle> items)
        {
            VisibleArticles.Clear();

            foreach (var item in items)
                VisibleArticles.Add(item);
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
                    VisibleArticles.Remove(article);
                    _articles.Remove(article);

                    _ = Task.Run(() => _service.DeleteAsync(article.Id).ConfigureAwait(false));
                    
                }
                catch (Exception ex)
                {
                        await DisplayAlert("Ошибка", $"Не удалось удалить: {ex.Message}", "OK");
                }
            }
        }
    }
}