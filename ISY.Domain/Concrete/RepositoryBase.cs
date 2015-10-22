using ISY.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ISY.Domain.Concrete
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class , IDbEntity
    {
        /// <summary>
        /// Reflection - get name of T.
        /// </summary>olll
        private string name
        {
            get { return typeof(T).Name; }
        }

        //DataBase context
        ISYContext _context;
        //Simple macros
        Type _contextType;


        public RepositoryBase()
        {
            _context = new ISYContext();
            _contextType = typeof(ISYContext);



        }


        /// <summary>
        /// Get all items in Set as IQueryable, making easy to operate with data.
        /// </summary>
        /// <returns>Items in Set</returns>
        public IQueryable<T> AllItems
        {
            get
            {
                return (IQueryable<T>)AllItemsAsObj;
            }
        }

        /// <summary>
        /// Get the items (ObjectSet) as Object. For internal use only.
        /// </summary>
        /// <returns>Object</returns>
        private object AllItemsAsObj
        {
            get
            {
                PropertyInfo mi = _contextType.GetProperty(name);
                object obj = mi.GetValue(_context, null);
                return obj;

            }
        }

        /// <summary>
        /// Add item to collection and save changes.
        /// </summary>
        /// <typeparam name="T">The type of item</typeparam>
        /// <param name="item">Added item</param>
        /// <returns>True if no errors.</returns>
        public bool AddItem(T item)
        {
            try
            {
                object obj = AllItemsAsObj;
                obj.GetType().GetMethod("Add").Invoke(obj, new object[] { item });
                _context.SaveChanges();
                return true;
            }
            catch
            { return false; }
        }

        /// <summary>
        /// Get the single T item by it's ID
        /// </summary>
        /// <param name="id">Guid ID</param>
        /// <returns>Null if nothing found.</returns>
        public T GetItem(int id)
        {
            foreach (var item in AllItems)
            {
                if ((int)(item.GetType().GetProperty("Id").GetValue(item, null)) == id)
                    return (T)item;
            }
            return null;
        }

        /// <summary>
        /// Delets an item by it's ID.
        /// </summary>
        /// <param name="id">ID of item</param>
        /// <returns>True if no errors.</returns>
        public bool DeleteItem(int id)
        {
            try
            {
                T item = GetItem(id);
                object set = AllItemsAsObj;
                set.GetType().GetMethod("Remove").Invoke(set, new object[] { item });
                SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ChangeItem(T item)
        {
            try
            {
                var guid = new Guid(item.GetType().GetProperty("Id").GetValue(item, null).ToString());
                T modyfying = AllItems.Single(x => x.GetType().GetProperty("Id").GetValue(null, null).ToString() == guid.ToString());
                modyfying = item;
                SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Force save changes to DB.
        /// </summary>
        /// <returns>True if no errors.</returns>
        public bool SaveChanges()
        {
            try
            {
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
