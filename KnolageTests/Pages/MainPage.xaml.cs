using System;
using Microsoft.Maui.Controls;
using KnolageTests.Pages;   

namespace KnolageTests.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void OnKnowledgeClicked(object sender, EventArgs e)
        {
            var page = new KnowledgeBaseListPage();

            if (Navigation != null)
                await Navigation.PushAsync(page);
            else
                Application.Current.MainPage = new NavigationPage(page);
        }

        async void OnManageKnowledgeClicked(object sender, EventArgs e)
        {
            var page = new KnowledgeBaseManagePage();

            if (Navigation != null)
                await Navigation.PushAsync(page);
            else
                Application.Current.MainPage = new NavigationPage(page);
        }

        async void OnTakeTestsClicked(object sender, EventArgs e)
        {
            var page = new ContentPage
            {
                Title = "Прохождение тестов",
                Content = new StackLayout
                {
                    Padding = 20,
                    Children =
                    {
                        new Label { Text = "Раздел: Прохождение тестов", HorizontalOptions = LayoutOptions.Center }
                    }
                }
            };

            if (Navigation != null)
                await Navigation.PushAsync(page);
            else
                Application.Current.MainPage = new NavigationPage(page);
        }

        async void OnTestsClicked(object sender, EventArgs e)
        {
            var page = new TestsManagePage();

            if (Navigation != null)
                await Navigation.PushAsync(page);
            else
                Application.Current.MainPage = new NavigationPage(page);
        }

        async void OnEditTestsClicked(object sender, EventArgs e)
        {
            var page = new ContentPage
            {
                Title = "Редактирование тестов",
                Content = new StackLayout
                {
                    Padding = 20,
                    Children =
                    {
                        new Label { Text = "Раздел: Редактирование тестов", HorizontalOptions = LayoutOptions.Center }
                    }
                }
            };

            if (Navigation != null)
                await Navigation.PushAsync(page);
            else
                Application.Current.MainPage = new NavigationPage(page);
        }
    }
}
