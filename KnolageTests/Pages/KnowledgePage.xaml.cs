using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using KnolageTests.Models;

namespace KnolageTests.Pages
{
    public partial class KnowledgePage : ContentPage
    {
        readonly ObservableCollection<Article> allArticles;
        CollectionView? _articlesCollection;

        public KnowledgePage()
        {
            InitializeComponent();

            _articlesCollection = this.FindByName<CollectionView>("ArticlesCollection");
            var searchBar = this.FindByName<SearchBar>("SearchBar");

            if (_articlesCollection == null)
            {
                throw new InvalidOperationException("CollectionView with x:Name=\"ArticlesCollection\" not found in XAML.");
            }

            //_articlesCollection.SelectionChanged += OnArticleSelected;
            //if (searchBar != null)
            //    searchBar.TextChanged += OnSearchTextChanged;

            allArticles = new ObservableCollection<Article>
            {
                new Article
                {
                    Title = "Введение в .NET",
                    Description = "Основы платформы .NET",
                    Image = "dotnet_bot.png",
                    Tags = new[] { "Основы", ".NET" },
                    Content = "Полный текст: это подробный вводный материал о .NET. Здесь будет несколько абзацев, примеры и пояснения."
                },
                new Article
                {
                    Title = "Создание UI в .NET MAUI",
                    Description = "Адаптивный интерфейс и XAML",
                    Image = "dotnet_bot.png",
                    Tags = new[] { "MAUI", "UI", "Test" },
                    Content = "Полный текст: руководство по созданию интерфейсов в .NET MAUI. Содержит примеры использования Layout, Controls и адаптивных приёмов."
                },
                new Article
                {
                    Title = "Тестирование приложений",
                    Description = "Инструменты и практики",
                    Image = "dotnet_bot.png",
                    Tags = new[] { "Тесты", "Практика" },
                    Content = "Полный текст: обзор подходов к тестированию, unit-тесты, интеграционные тесты и советы по покрытию."
                },
                new Article
                {
                    Title = "Советы по оптимизации",
                    Description = "Повышение производительности",
                    Image = "dotnet_bot.png",
                    Tags = new[] { "Производительность" },
                    Content = "Полный текст: рекомендации и примеры оптимизаций для приложений на .NET."
                }
            };

            _articlesCollection.ItemsSource = allArticles;
        }

        void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var q = e.NewTextValue?.Trim();
            if (_articlesCollection == null) return;

            if (string.IsNullOrWhiteSpace(q))
            {
                _articlesCollection.ItemsSource = allArticles;
                return;
            }

            var filtered = allArticles
                .Where(a => (a.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                         || (a.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                         || (a.Content?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                         || (a.Tags != null && a.Tags.Any(tag => tag?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)))
                .ToList();

            _articlesCollection.ItemsSource = filtered;
        }

        async void OnArticleSelected(object sender, SelectionChangedEventArgs e)
        {
            //if (e.CurrentSelection?.FirstOrDefault() is Article selected)
            //{
            //    // очистить выбор, чтобы можно было снова тапнуть на тот же элемент
            //    if (sender is CollectionView cv)
            //        cv.SelectedItem = null;

            //    var page = new ArticlePage(selected);
            //    if (Navigation != null)
            //        await Navigation.PushAsync(page);
            //    else
            //        Application.Current.MainPage = new NavigationPage(page);
            //}
        }
    }
}