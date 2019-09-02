using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Repository
{
    public interface IRepository<T> where T : IEntity
    {

        Task<IEnumerable<T>> GetAll(); 
        void Add(T entity);
        void Delete(T entity);
        void Update(T entity);
        Task<T> FindById(Guid Id);

    }
}
