using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISY.Domain.Abstract
{
    public interface IRepositoryBase<T>
          where T : class, IDbEntity
    {
        bool AddItem(T item);
        System.Linq.IQueryable<T> AllItems { get; }
        bool ChangeItem(T item);
        bool DeleteItem(int id);
        T GetItem(int id);
        bool SaveChanges();
    }
}
