using Transaction.Domain.Entities;
using Transaction.Domain.Result;

namespace Transaction.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : Entity
{
    Task<Result<T>> GetByIdAsync(int id);
    Task<Result<IEnumerable<T>>> GetAllAsync();
    Task<Result<T>> CreateAsync(T entity);
    Task<Result<T>> UpdateAsync(int id, T entity);
    Task<Result<T>> DeleteAsync(int id);
}
