using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akcounts.Domain
{
    public interface IRepository<T>
    {
        T GetById(int id);
        ICollection<T> GetAll();

        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}
