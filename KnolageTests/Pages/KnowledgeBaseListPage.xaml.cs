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
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ArticlesCollectionView.ItemsSource = _articles;
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Error", $"Failed to load articles: {ex.Message}", "OK");
                });
            }
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is KnowledgeArticle selected)
            {
                // clear selection so user can tap again
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                // Navigate to ArticlePage using KnowledgeArticle
                var page = new ArticlePage(selected);
                if (Navigation != null)
                    _ = Navigation.PushAsync(page);
                else
                    Application.Current.MainPage = new NavigationPage(page);
            }
        }

        async void OnCreateArticleClicked(object sender, EventArgs e)
        {
            var page = new ArticleEditorPage();
            if (Navigation != null)
                await Navigation.PushAsync(page);
            else
                Application.Current.MainPage = new NavigationPage(page);
        }

        async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadAsync();
        }
    }
}