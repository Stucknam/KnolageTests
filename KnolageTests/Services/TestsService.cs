using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using KnolageTests.Models;

namespace KnolageTests.Services
{
    public class TestsService
    {
        const string FileName = "tests.json";
        readonly string _filePath;
        readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public TestsService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
        }

        public async Task<List<Test>> GetAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                    return new List<Test>();

                var text = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(text))
                    return new List<Test>();

                try
                {
                    var list = JsonSerializer.Deserialize<List<Test>>(text, _jsonOptions);
                    return list ?? new List<Test>();
                }
                catch (JsonException)
                {
                    // corrupted file -> start fresh
                    return new List<Test>();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Test?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var all = await GetAllAsync().ConfigureAwait(false);
            return all.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SaveAsync(Test test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));

            await _semaphore.WaitAsync();
            try
            {
                var list = new List<Test>();
                if (File.Exists(_filePath))
                {
                    var text = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        try
                        {
                            var existing = JsonSerializer.Deserialize<List<Test>>(text, _jsonOptions);
                            if (existing != null)
                                list = existing;
                        }
                        catch (JsonException)
                        {
                            // ignore and overwrite with fresh list
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(test.Id))
                    test.Id = Guid.NewGuid().ToString();

                var found = list.FirstOrDefault(t => string.Equals(t.Id, test.Id, StringComparison.OrdinalIgnoreCase));
                if (found == null)
                {
                    list.Add(test);
                }
                else
                {
                    var index = list.IndexOf(found);
                    list[index] = test;
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

                List<Test> list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Test>>(text, _jsonOptions) ?? new List<Test>();
                }
                catch (JsonException)
                {
                    return;
                }

                var remaining = list.Where(t => !string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase)).ToList();
                var json = JsonSerializer.Serialize(remaining, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Test>> GetByArticleIdAsync(string articleId)
        {
            if (string.IsNullOrWhiteSpace(articleId)) return new List<Test>();

            var all = await GetAllAsync().ConfigureAwait(false);
            var matched = all.Where(t => t.ArticleIds != null && t.ArticleIds.Any(a => string.Equals(a, articleId, StringComparison.OrdinalIgnoreCase)))
                             .ToList();
            return matched;
        }
    }
}