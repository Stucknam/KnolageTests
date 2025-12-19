using System;
using Microsoft.Maui.Controls;
using KnolageTests.Models;

namespace KnolageTests.Pages
{
    public partial class ArticlePage : ContentPage
    {
        public ArticlePage(Article article)
        {
            InitializeComponent();
            BindingContext = article ?? throw new ArgumentNullException(nameof(article));
            Title = article.Title;
        }

        async void OnStartTestClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Тест", "Запуск теста — заглушка.", "OK");
        }
    }
}