using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace KnolageTests.Controls
{
    public partial class ArticleCard : ContentView
    {
        public ArticleCard()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ArticleCard), string.Empty);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty DescriptionProperty =
            BindableProperty.Create(nameof(Description), typeof(string), typeof(ArticleCard), string.Empty);

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(nameof(ImageSource), typeof(string), typeof(ArticleCard), default(string));

        public string ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty TagsProperty =
            BindableProperty.Create(nameof(Tags), typeof(IEnumerable<string>), typeof(ArticleCard), default(IEnumerable<string>));

        public IEnumerable<string> Tags
        {
            get => (IEnumerable<string>)GetValue(TagsProperty);
            set => SetValue(TagsProperty, value);
        }
    }
}