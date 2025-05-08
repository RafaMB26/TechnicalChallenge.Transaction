using Common.Entities;
using Common.Result;

namespace Common.Interfaces;

public interface IRepository<T> where T : Entity
{
    Task<Result<T>> GetByIdAsync(int id);
    Task<Result<IEnumerable<T>>> GetAllAsync();
    Task<Result<T>> CreateAsync(T entity);
    Task<Result<T>> UpdateAsync(int id, T entity);
    Task<Result<T>> DeleteAsync(int id);
}
