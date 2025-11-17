using Microsoft.EntityFrameworkCore;
using TodoListApp.DataAccess.Context;
using TodoListApp.DataAccess.Repositories.Interfaces;
using TodoListApp.Entities;

namespace TodoListApp.DataAccess.Repositories.Implementations;
public class TodoTaskRepository : ITodoTaskRepository
{
    private readonly TodoListDbContext _dbcontext;

    public TodoTaskRepository(TodoListDbContext dbcontext)
    {
        ArgumentNullException.ThrowIfNull(dbcontext);

        this._dbcontext = dbcontext;
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await this._dbcontext.Tasks
            .SingleOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TaskItem>> GetByListIdAsync(int listId)
    {
        return await this._dbcontext.Tasks
            .Where(t => t.TodoListId == listId)
            .OrderBy(t => t.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByListIdAsync(int listId, bool? isComleted)
    {
        return await this._dbcontext.Tasks
            .Where(t => t.TodoListId == listId && t.IsCompleted == isComleted)
            .OrderBy(t => t.CreatedDate)
            .ToListAsync();
    }

    public async Task<TaskItem> AddAsync(TaskItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var taskItem = await this._dbcontext.Tasks.AddAsync(item);
        await this._dbcontext.SaveChangesAsync();

        return taskItem.Entity;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var updatedItem = this._dbcontext.Tasks.Update(item);
        await this._dbcontext.SaveChangesAsync();
        return updatedItem.Entity;
    }

    public async Task DeleteAsync(int id)
    {
        var task = await this.GetByIdAsync(id); 

        if (task is null)
        {
            throw new InvalidOperationException($"Task with {id} not found");
        }

        this._dbcontext.Tasks.Remove(task);
        await this._dbcontext.SaveChangesAsync();

    }

    public async Task<int?> CountByListIdAsync(int listId)
    {
        return await this._dbcontext.Tasks
            .Where(t => t.TodoListId == listId)
            .CountAsync();
    }
}
