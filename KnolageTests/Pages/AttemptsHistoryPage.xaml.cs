using KnolageTests.Services;
using KnolageTests.Models;
using KnolageTests.Helpers;

namespace KnolageTests.Pages
{



    public partial class AttemptsHistoryPage : ContentPage
    {
        private readonly TestAttemptDatabaseService _db;
        private readonly string _testId;
        readonly TestsService _testsService;

        public AttemptsHistoryPage(string testId)
        {
            InitializeComponent();
            _db = ServiceHelper.GetService<TestAttemptDatabaseService>();
            _testsService = ServiceHelper.GetService<TestsService>();
            _testId = testId;

           // LoadAttempts();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing ();
            LoadAttempts();
        }

        private async void LoadAttempts()
        {
            try
            {
                var attempts = await _db.GetAttemptsByTestIdAsync(_testId);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AttemptsContainer.Children.Clear();

                    if (attempts.Count == 0)
                    {
                        AttemptsContainer.Children.Add(new Label
                        {
                            Text = "Попыток пока нет.",
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.Center
                        });
                        return;
                    }
                });

                var test = await ServiceHelper.GetService<TestsService>().GetByIdAsync(_testId);
                int totalQuestions = test.Questions.Count;

                foreach (var attempt in attempts)
                {
                    int percent = totalQuestions > 0
                        ? (int)Math.Round((double)attempt.Score / totalQuestions * 100)
                        : 0;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var card = new Frame
                        {
                            CornerRadius = 12,
                            Padding = 14,
                            BackgroundColor = (Color)Application.Current.Resources["SurfaceColor"],
                            HasShadow = true
                        };

                        var layout = new VerticalStackLayout { Spacing = 6 };

                        layout.Children.Add(new Label
                        {
                            Text = attempt.CompletedAt.ToString("dd.MM.yyyy HH:mm"),
                            FontSize = 14,
                            TextColor = Colors.Gray
                        });

                        layout.Children.Add(new Label
                        {
                            Text = $"{percent}%   ({attempt.Score} / {totalQuestions})",
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = percent == 100 ? Colors.Green : Colors.OrangeRed
                        });

                        var detailsBtn = new Button
                        {
                            Text = "Посмотреть попытку",
                            BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"],
                            TextColor = (Color)Application.Current.Resources["TextPrimaryColor"],
                            CornerRadius = 8,
                            Padding = new Thickness(10, 6)
                        };

                        detailsBtn.Clicked += async (_, __) =>
                        {
                            await Navigation.PushAsync(new AttemptDetailsPage(attempt.Id));
                        };

                        layout.Children.Add(detailsBtn);

                        card.Content = layout;
                        AttemptsContainer.Children.Add(card);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== ERROR IN LoadAttempts ===");
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            }
        }
}