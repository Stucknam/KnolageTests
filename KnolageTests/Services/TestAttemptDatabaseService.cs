using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KnolageTests.Models;
using SQLite;

namespace KnolageTests.Services
{
    public class TestAttemptDatabaseService
    {
        private readonly SQLiteAsyncConnection _db;

        public TestAttemptDatabaseService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<TestAttempt>().Wait();
            _db.CreateTableAsync<TestAttemptAnswer>().Wait();
        }

        public async Task InitializeAsync()
        {
            await _db.CreateTableAsync<TestAttempt>();
            await _db.CreateTableAsync<TestAttemptAnswer>();
        }

        public async Task AddAttemptAsync(TestAttempt attempt, List<TestAttemptAnswer> answers)
        {
            await _db.InsertAsync(attempt);

            foreach (var answer in answers)
            {
                answer.AttemptId = attempt.Id;
                await _db.InsertAsync(answer);
            }
        }

        public Task<List<TestAttempt>> GetAttemptsByTestIdAsync(string testId)
        {
            return _db.Table<TestAttempt>()
                .Where(attempt => attempt.TestId == testId)
                .OrderByDescending(attempt => attempt.CompletedAt)
                .ToListAsync();
        }

        public async Task<(TestAttempt attempt, List<TestAttemptAnswer>)> GetAttemptWithAnswerAsync(int attemptId)
        {
            var attempt = await _db.FindAsync<TestAttempt>(attemptId);
            var answers = await _db.Table<TestAttemptAnswer>()
                .Where(a => a.AttemptId == attemptId)
                .ToListAsync();
            return (attempt, answers);
        }

        public async Task<TestAttempt?> GetLastAttemptForTestAsync(string testId)
        {
            return await _db.Table<TestAttempt>()
                .Where(a => a.TestId == testId)
                .OrderByDescending(a => a.CompletedAt)
                .FirstOrDefaultAsync();
        }


    }
}
