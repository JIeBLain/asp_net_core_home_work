namespace API.Todo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using View;
    using View.Todo;

    public class TodoRepository : ITodoRepository
    {
        private readonly IList<Todo> todos;

        public TodoRepository()
        {
            this.todos = new List<Todo>
            {
                new()
                {
                    Id = Guid.Parse("4D448355-E91B-4B6C-AC39-499C37F865AD"),
                    Deadline = DateTime.Now + TimeSpan.FromDays(10),
                    Description = "Выучить времена.",
                    Title = "Подготовиться к экзамену по английскому",
                    CreatedAt = DateTime.Now,
                    IsCompleted = false,
                    UserId = "516A1D60-E9D0-4913-A8EF-40D8922876DB",
                },
                new()
                {
                    Id = Guid.Parse("FEC9AA20-3907-4EA5-A64D-FD2C83629A65"),
                    Deadline = DateTime.Now + TimeSpan.FromDays(4),
                    Description = "Пройти блоки \"Функциональное программировани\" и \"LINQ\".",
                    Title = "Решить домашку на ulearn-е",
                    CreatedAt = DateTime.Now,
                    IsCompleted = false,
                    UserId = "516A1D60-E9D0-4913-A8EF-40D8922876DB",
                },
                new()
                {
                    Id = Guid.Parse("927B7A86-F948-4CF6-8AC1-C729C601322E"),
                    Deadline = DateTime.Now + TimeSpan.FromDays(1),
                    Description = "Купить мандарины и хлеб.",
                    Title = "Сходить в магазин",
                    CreatedAt = DateTime.Now,
                    IsCompleted = true,
                    UserId = "516A1D60-E9D0-4913-A8EF-40D8922876DB",
                },
            };
        }

        public Task<List<TodoInfo>> SearchAsync(TodoInfoSearchQuery query, CancellationToken cancellationToken)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<Todo> searchResult = this.todos;

            if (query.CreatedFrom != null)
            {
                searchResult = this.todos.Where(todoInfo => todoInfo.CreatedAt >= query.CreatedFrom.Value);
            }

            if (query.CreatedTo != null)
            {
                searchResult = searchResult.Where(todoInfo => todoInfo.CreatedAt <= query.CreatedTo.Value);
            }

            if (query.DeadLineFrom != null)
            {
                searchResult = searchResult.Where(todoInfo => todoInfo.Deadline >= query.DeadLineFrom.Value);
            }

            if (query.DeadLineTo != null)
            {
                searchResult = searchResult.Where(todoInfo => todoInfo.Deadline <= query.DeadLineTo.Value);
            }

            if (query.UserId != null)
            {
                searchResult = searchResult.Where(todoInfo => todoInfo.UserId == query.UserId);
            }

            if (query.IsCompleted != null)
            {
                searchResult = searchResult.Where(todoInfo => todoInfo.IsCompleted == query.IsCompleted.Value);
            }

            if (query.Offset != null)
            {
                searchResult = searchResult.Skip(query.Offset.Value);
            }

            if (query.Limit != null)
            {
                searchResult = searchResult.Take(query.Limit.Value);
            }

            var sort = query.Sort ?? SortType.Ascending;
            var sortBy = query.SortBy ?? TodoSortBy.Creation;

            DateTime Select(TodoInfo todo)
            {
                switch (sortBy)
                {
                    case TodoSortBy.Deadline:
                        return todo.Deadline;

                    case TodoSortBy.Creation:
                        return todo.CreatedAt;

                    default:
                        throw new ArgumentException($"Unknown todo sort by value \"{sortBy}\".", nameof(query));
                }
            }

            searchResult = sort == SortType.Ascending
                ? searchResult.OrderBy(Select)
                : searchResult.OrderByDescending(Select);

            var res = searchResult.ToList();
            var result = res.Cast<TodoInfo>().ToList();

            return Task.FromResult(result);
        }

        public Task<Todo> GetAsync(string id, CancellationToken token)
        {
            if (!Guid.TryParse(id, out var guidTodoId))
            {
                throw new TodoNotFoundException(id);
            }

            var result = this.todos.FirstOrDefault(it => it.Id == guidTodoId);

            if (result == null)
            {
                throw new TodoNotFoundException(id);
            }

            return Task.FromResult(result);
        }

        public Task<TodoInfo> CreateAsync(TodoBuildInfo buildInfo, CancellationToken cancellationToken)
        {
            if (buildInfo == null)
            {
                throw new ArgumentNullException(nameof(buildInfo));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var now = DateTime.Now;

            var todo = new Todo
            {
                Id = Guid.NewGuid(),
                UserId = buildInfo.UserId,
                CreatedAt = now,
                Deadline = buildInfo.Deadline,
                IsCompleted = false,
                Title = buildInfo.Title,
                Description = buildInfo.Description,
            };

            this.todos.Add(todo);

            return Task.FromResult<TodoInfo>(todo);
        }

        public Task<Todo> PatchAsync(string todoId, TodoPatchInfo patchInfo, CancellationToken cancellationToken)
        {
            if (patchInfo == null)
            {
                throw new ArgumentNullException(nameof(patchInfo));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var todo = todos.FirstOrDefault(t => Guid.Parse(todoId) == t.Id);

            if (todo == null)
            {
                throw new TodoNotFoundException(todoId);
            }

            if (patchInfo.IsCompleted != null)
            {
                todo.IsCompleted = patchInfo.IsCompleted.Value;
            }

            if (patchInfo.Title != null)
            {
                todo.Title = patchInfo.Title;
            }

            if (patchInfo.Description != null)
            {
                todo.Description = patchInfo.Description;
            }

            if (patchInfo.Deadline != null)
            {
                todo.Deadline = (DateTime) patchInfo.Deadline;
            }

            return Task.FromResult(todo);
        }

        public Task RemoveAsync(string id, CancellationToken token)
        {
            var todo = todos.FirstOrDefault(t => Guid.Parse(id) == t.Id);

            if (todo == null)
                throw new TodoNotFoundException(id);

            todos.Remove(todo);
            return Task.CompletedTask;
        }
    }
}