using Common.Entities;
using Common.Interfaces;
using Common.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Transaction.Ports.Postgres.Repositories;

public class Repository<T> : IRepository<T> where T:Entity
{
    private readonly DbContext _dbContext;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger<Repository<T>> _logger;

    public Repository(TransactionDbContext dbContext, ILogger<Repository<T>> logger)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<T>();
        _logger = logger;
    }
    public async Task<Result<T>> CreateAsync(T entity)
    {
        try
        {
            var newEntity = _dbSet.Add(entity);
            await _dbContext.SaveChangesAsync();
            return new Result<T>(newEntity.Entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(CreateAsync), ex.Message);
            return new Result<T>(new Error("Error while trying to insert the new record", 503));
        }
    }

    public async Task<Result<T>> DeleteAsync(int id)
    {
        try
        {
            var entityToDelete = await _dbSet.Where(d => d.Id == id).FirstOrDefaultAsync();
            if (entityToDelete is null)
            {
                return new Result<T>(new Error("The record does not exist", 404));
            }
            _dbSet.Remove(entityToDelete);
            await _dbContext.SaveChangesAsync();
            return new Result<T>(entityToDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(DeleteAsync), ex.Message);
            return new Result<T>(new Error("Error while trying to delete the record", 503));
        }
    }

    public async Task<Result<IEnumerable<T>>> GetAllAsync()
    {
        try
        {
            var result = await _dbSet.ToListAsync();
            return new Result<IEnumerable<T>>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(GetAllAsync), ex.Message);
            return new Result<IEnumerable<T>>(new Error("Error while trying to get all the records", 503));
        }
    }

    public async Task<Result<T>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _dbSet.SingleOrDefaultAsync(d => d.Id == id);
            if (result is null)
            {
                return new Result<T>(new Error($"Record with id {id} not found", 404));
            }
            return new Result<T>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(GetByIdAsync), ex.Message);
            return new Result<T>(new Error($"Error while trying to get the record with id {id}", 503));
        }
    }

    public async Task<Result<T>> UpdateAsync(int id, T entity)
    {
        try
        {
            var entityToUpdate = await _dbSet.Where(d => d.Id == id).FirstOrDefaultAsync();
            if (entityToUpdate != null)
            {
                _dbContext.Entry(entityToUpdate).State = EntityState.Detached;
            }
            _dbSet.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return new Result<T>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(UpdateAsync), ex.Message);
            return new Result<T>(new Error($"Error while trying to update the record with id {id}", 503));
        }
    }
}
