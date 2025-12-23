using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MauiIcons.Fluent;
using KnolageTests.Models;
using KnolageTests.Services;

namespace KnolageTests.Pages
{
    public partial class ArticleEditorPage : ContentPage
    {
        readonly KnowledgeBaseService _service = new KnowledgeBaseService();
        KnowledgeArticle _article;

        // New article
        public ArticleEditorPage()
        {
            InitializeComponent();
            _article = new KnowledgeArticle();
            BindBasicFields();
            RenderBlocks();
        }

        // Edit by id
        public ArticleEditorPage(string articleId)
        {
            InitializeComponent();
            _article = new KnowledgeArticle(); // temporary
            _ = LoadArticleAsync(articleId);
        }

        // Edit by instance
        public ArticleEditorPage(KnowledgeArticle article)
        {
            InitializeComponent();
            _article = article ?? new KnowledgeArticle();
            BindBasicFields();
            RenderBlocks();
        }

        async Task LoadArticleAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                await DisplayAlert("Ошибка", "Invalid article id", "OK");
                await Navigation.PopAsync();
                return;
            }

            var art = await _service.GetByIdAsync(id).ConfigureAwait(false);
            if (art == null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Не найдено", "Статья не найдена", "OK");
                    await Navigation.PopAsync();
                });
                return;
            }

            _article = art;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                BindBasicFields();
                RenderBlocks();
            });
        }

        void BindBasicFields()
        {
            TitleEntry.Text = _article.Title;
            SubtitleEntry.Text = _article.Subtitle;
            ThumbnailEntry.Text = _article.ThumbnailPath;
            TagsEntry.Text = _article.Tags != null ? string.Join(", ", _article.Tags) : string.Empty;
            Title = string.IsNullOrWhiteSpace(_article.Title) ? "Новая статья" : _article.Title;
        }

        void RenderBlocks()
        {
            BlocksContainer.Children.Clear();
            if (_article.Blocks == null) _article.Blocks = new List<ArticleBlock>();

            for (int i = 0; i < _article.Blocks.Count; i++)
            {
                var idx = i;
                var block = _article.Blocks[i];

                var container = new Frame
                {
                    Padding = 8,
                    CornerRadius = 6,
                    HasShadow = false,
                    Content = new VerticalStackLayout { Spacing = 6 },
                    BackgroundColor = (Color)Application.Current.Resources["SurfaceColor"]
                };

                var v = (VerticalStackLayout)container.Content;

                v.Children.Add(new Label { Text = block.Type.ToString(), FontAttributes = FontAttributes.Bold });

                switch (block.Type)
                {
                    case BlockType.Header:
                    case BlockType.Paragraph:
                    case BlockType.Quote:
                    case BlockType.List:
                        var editor = new Editor
                        {
                            Text = block.Content,
                            AutoSize = EditorAutoSizeOption.TextChanges,
                            HeightRequest = 80
                        };
                        editor.TextChanged += (s, e) =>
                        {
                            block.Content = e.NewTextValue ?? string.Empty;
                        };
                        v.Children.Add(editor);
                        break;

                    case BlockType.Image:
                        var h = new HorizontalStackLayout { Spacing = 8 };

                        var pathEntry = new Entry
                        {
                            Text = block.Content,
                            HorizontalOptions = LayoutOptions.FillAndExpand
                        };

                        // При изменении текста вручную — обновляем блок
                        pathEntry.TextChanged += (s, e) =>
                            block.Content = e.NewTextValue ?? string.Empty;

                        var pickBtn = new Button { Text = "Выбрать" };

                        pickBtn.Clicked += async (_, __) =>
                        {
                            var fr = await FilePicker.PickAsync(new PickOptions
                            {
                                PickerTitle = "Выберите изображение"
                            });

                            if (fr == null)
                                return;

                            var appFolder = FileSystem.AppDataDirectory;
                            var fileName = Path.GetFileName(fr.FullPath ?? fr.FileName);
                            var destinationPath = Path.Combine(appFolder, fileName);

                            using (var sourceStream = await fr.OpenReadAsync())
                            using (var destinationStream = File.OpenWrite(destinationPath))
                            {
                                await sourceStream.CopyToAsync(destinationStream);
                            }

                            // ВАЖНО: сохраняем путь в блок, а не в статью
                            block.Type = BlockType.Image;
                            block.Content = destinationPath;

                            // Обновляем UI
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                pathEntry.Text = destinationPath;
                            });
                        };

                        h.Children.Add(pathEntry);
                        h.Children.Add(pickBtn);
                        v.Children.Add(h);
                        break;

                    case BlockType.Divider:
                        v.Children.Add(new Label { Text = "— Разделитель —", HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Gray });
                        break;
                }

                // action buttons
                var actions = new HorizontalStackLayout { Spacing = 8 };

                var up = new Button { ImageSource = "ic_fluent_arrow_sort_up_24_filled.png", BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"] ,  WidthRequest = 40, HeightRequest = 36 };
                up.Clicked += (_, __) => MoveBlockUp(idx);
                actions.Children.Add(up);

                var down = new Button { ImageSource = "ic_fluent_arrow_sort_down_24_filled.png", BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"],  WidthRequest = 40, HeightRequest = 36 };
                down.Clicked += (_, __) => MoveBlockDown(idx);
                actions.Children.Add(down);

                var del = new Button { ImageSource = "ic_fluent_delete_24_filled.png", HeightRequest = 36, BackgroundColor = Colors.IndianRed, TextColor = Colors.White };
                del.Clicked += (_, __) => DeleteBlock(idx);
                actions.Children.Add(del);

                v.Children.Add(actions);

                BlocksContainer.Children.Add(container);
            }
        }

        void MoveBlockUp(int index)
        {
            if (index <= 0 || index >= _article.Blocks.Count) return;
            var b = _article.Blocks[index];
            _article.Blocks.RemoveAt(index);
            _article.Blocks.Insert(index - 1, b);
            RenderBlocks();
        }

        void MoveBlockDown(int index)
        {
            if (index < 0 || index >= _article.Blocks.Count - 1) return;
            var b = _article.Blocks[index];
            _article.Blocks.RemoveAt(index);
            _article.Blocks.Insert(index + 1, b);
            RenderBlocks();
        }

        void DeleteBlock(int index)
        {
            if (index < 0 || index >= _article.Blocks.Count) return;
            _article.Blocks.RemoveAt(index);
            RenderBlocks();
        }

        void AddBlock(BlockType type)
        {
            if (_article.Blocks == null) _article.Blocks = new List<ArticleBlock>();
            var block = new ArticleBlock(type, type == BlockType.Divider ? string.Empty : string.Empty);
            _article.Blocks.Add(block);
            RenderBlocks();
        }

        async void OnAddBlockClicked(object sender, EventArgs e)
        {
            // Cross-platform action sheet to present block types
            var choice = await DisplayActionSheet("Добавить блок", "Отмена", null,
                "Заголовок", "Абзац", "Изображение", "Список", "Цитата", "Разделитель");

            if (choice == "Заголовок")
                OnAddHeaderClicked(sender, EventArgs.Empty);
            else if (choice == "Абзац")
                OnAddParagraphClicked(sender, EventArgs.Empty);
            else if (choice == "Изображение")
                OnAddImageClicked(sender, EventArgs.Empty);
            else if (choice == "Список")
                OnAddListClicked(sender, EventArgs.Empty);
            else if (choice == "Цитата")
                OnAddQuoteClicked(sender, EventArgs.Empty);
            else if (choice == "Разделитель")
                OnAddDividerClicked(sender, EventArgs.Empty);
            // Cancel or null -> do nothing
        }

        void OnAddHeaderClicked(object sender, EventArgs e) => AddBlock(BlockType.Header);
        void OnAddParagraphClicked(object sender, EventArgs e) => AddBlock(BlockType.Paragraph);
        void OnAddImageClicked(object sender, EventArgs e) => AddBlock(BlockType.Image);
        void OnAddListClicked(object sender, EventArgs e) => AddBlock(BlockType.List);
        void OnAddQuoteClicked(object sender, EventArgs e) => AddBlock(BlockType.Quote);
        void OnAddDividerClicked(object sender, EventArgs e) => AddBlock(BlockType.Divider);

        async void OnPickThumbnailClicked(object sender, EventArgs e)
        {
            try
            {
                var fr = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите изображение"
                });

                if (fr == null)
                    return;

                // Папка приложения
                var appFolder = FileSystem.AppDataDirectory;

                // Имя файла
                var fileName = Path.GetFileName(fr.FullPath ?? fr.FileName);

                // Путь назначения
                var destinationPath = Path.Combine(appFolder, fileName);

                // Копирование файла в локальную папку приложения
                using (var sourceStream = await fr.OpenReadAsync())
                using (var destinationStream = File.OpenWrite(destinationPath))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                // Сохраняем путь к локальной копии
                _article.ThumbnailPath = destinationPath;

                // Обновляем UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ThumbnailEntry.Text = destinationPath;
                });
            }
            catch
            {
                // ignore
            }
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var title = TitleEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                await DisplayAlert("Ошибка", "Заголовок не должен быть пустым.", "OK");
                return;
            }

            // Update article fields
            _article.Title = title;
            _article.Subtitle = SubtitleEntry.Text?.Trim() ?? string.Empty;
            _article.ThumbnailPath = ThumbnailEntry.Text?.Trim() ?? string.Empty;

            var tagsText = TagsEntry.Text ?? string.Empty;
            var tags = tagsText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(t => t.Trim())
                               .Where(t => !string.IsNullOrWhiteSpace(t))
                               .ToList();
            _article.Tags = tags;

            if (string.IsNullOrWhiteSpace(_article.Id))
                _article.Id = Guid.NewGuid().ToString();

            try
            {
                await _service.SaveAsync(_article).ConfigureAwait(false);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Сохрнено", "Статья успешно сохранена.", "OK");
                    await Navigation.PopAsync();
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
                });
            }
        }

        async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}