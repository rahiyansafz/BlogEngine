using System.Linq.Expressions;

using DataAccess.DataContext;
using DataAccess.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

using Models.ApiModels.ResponseDTO;

namespace DataAccess.Repositories.Implementations;
public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _appContext;
    internal DbSet<T> _dbSet;

    public Repository(AppDbContext appContext)
    {
        _appContext = appContext;
        _dbSet = _appContext.Set<T>();
    }

    public async Task<T> GetOneAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
    {
        IQueryable<T> query = tracked ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        query = query.Where(filter);
        if (includeProperties is not null)
            foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(item);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, string? includeProperties = null)
    {
        IQueryable<T> query = _dbSet;
        if (filter is not null)
            query.Where(filter);
        if (includeProperties is not null)
            foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(item);
        return await query.ToListAsync();
    }

    protected async Task<PagedList<T>> GetPageAsync(IQueryable<T> query, int pageNumber, int pageSize/*, string? includeProperties = null*/)
    => await PagedList<T>.CreateAsync(query, pageNumber, pageSize);

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
}