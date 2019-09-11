using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Moggles.Domain
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task DeleteAsync(T entity);
        Task UpdateAsync(T entity);
        Task<T> FindByIdAsync(Guid id);
    }
}
