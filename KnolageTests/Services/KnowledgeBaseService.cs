using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using KnolageTests.Models;
using KnolageTests.Helpers;

namespace KnolageTests.Services
{
    public class KnowledgeBaseService
    {
        const string FileName = "knowledge_articles.json";
        readonly string _filePath;
        readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        readonly ImageStorageService _imageStorageService;

        public KnowledgeBaseService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
            _imageStorageService = ServiceHelper.GetService<ImageStorageService>();
        }

        public async Task<List<KnowledgeArticle>> GetAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                    return new List<KnowledgeArticle>();

                var text = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(text))
                    return new List<KnowledgeArticle>();

                try
                {
                    var list = JsonSerializer.Deserialize<List<KnowledgeArticle>>(text, _jsonOptions);
                    return list ?? new List<KnowledgeArticle>();
                }
                catch (JsonException)
                {
                    // If file is corrupted or not JSON, start fresh
                    return new List<KnowledgeArticle>();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<KnowledgeArticle?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var all = await GetAllAsync().ConfigureAwait(false);
            return all.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SaveAsync(KnowledgeArticle article)
        {
            if (article == null) throw new ArgumentNullException(nameof(article));

            await _semaphore.WaitAsync();
            try
            {
                var list = new List<KnowledgeArticle>();
                if (File.Exists(_filePath))
                {
                    var text = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        try
                        {
                            var existing = JsonSerializer.Deserialize<List<KnowledgeArticle>>(text, _jsonOptions);
                            if (existing != null)
                                list = existing;
                        }
                        catch (JsonException)
                        {
                            // ignore and overwrite with fresh list
                        }
                    }
                }

                // Ensure id and timestamps
                if (string.IsNullOrWhiteSpace(article.Id))
                    article.Id = Guid.NewGuid().ToString();

                var found = list.FirstOrDefault(a => string.Equals(a.Id, article.Id, StringComparison.OrdinalIgnoreCase));
                if (found == null)
                {
                    article.CreatedAt = article.CreatedAt == default ? DateTime.UtcNow : article.CreatedAt;
                    article.UpdatedAt = DateTime.UtcNow;
                    list.Add(article);
                }
                else
                {
                    // update fields on existing entry (replace)
                    article.CreatedAt = found.CreatedAt == default ? DateTime.UtcNow : found.CreatedAt;
                    article.UpdatedAt = DateTime.UtcNow;

                    var index = list.IndexOf(found);
                    list[index] = article;
                }

                var json = JsonSerializer.Serialize(list, _jsonOptions);
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath) ?? FileSystem.AppDataDirectory);
                await File.WriteAllTextAsync(_filePath, json).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;

            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath)) return;

                var text = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(text)) return;

                List<KnowledgeArticle> list;
                try
                {
                    list = JsonSerializer.Deserialize<List<KnowledgeArticle>>(text, _jsonOptions) ?? new List<KnowledgeArticle>();
                }
                catch (JsonException)
                {
                    return;
                }
                // Выделяем удаяемую статью из списка
                var article = list.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
                if (article == null) return;
                // Выделяем изображения для удаления
                var tumbnailPath = article.ThumbnailPath;

                if (tumbnailPath != null)
                {
                    _imageStorageService.DeleteImageIfExists(tumbnailPath);
                }

                // Список всех изображений в статье
                var imagesPaths = article.Blocks
                    .Where(b => b.Type == BlockType.Image)
                    .Select(b => b.Content)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .ToList();

                foreach (var imagePath in imagesPaths)
                {
                    _imageStorageService.DeleteImageIfExists(imagePath);
                }

                list.Remove(article);

                //var remaining = list.Where(a => !string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase)).ToList();
                var json = JsonSerializer.Serialize(list, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}