using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using YoungCloud.Configurations;

namespace YoungCloud.Databases.SqlClient.Repositories
{
    /// <summary>
    /// 實作Entity Framework Generic Repository 的 Class
    /// </summary>
    /// <typeparam name="TEntity">EF Model 裡面的Type</typeparam>
    public abstract class RepositoryBase<TEntity> : ClassBase, IRepositoryBase<TEntity>
        where TEntity : class
    {
        protected DbContext m_Context { get; set; }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Create(TEntity entity)
        {
            m_Context.Set<TEntity>().Add(entity);
        }

        /// <summary>
        /// Finds the first TEntity that match the predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public virtual TEntity FindOne(Expression<Func<TEntity, bool>> predicate)
        {
            return m_Context.Set<TEntity>().Where(predicate).FirstOrDefault();
        }

        /// <summary>
        /// 取得Entity全部筆數的IQueryable。
        /// </summary>
        /// <returns>Entity全部筆數的IQueryable。</returns>
        public virtual IQueryable<TEntity> GetAll()
        {
            return m_Context.Set<TEntity>().AsQueryable();
        }

        /// <summary>
        /// 更新一筆Entity內容。
        /// </summary>
        /// <param name="entity">要更新的內容</param>
        public virtual void Update(TEntity entity)
        {
            m_Context.Entry<TEntity>(entity).State = System.Data.Entity.EntityState.Modified;
        }

        /// <summary>
        /// 更新一筆Entity的內容。只更新有指定的Property。
        /// </summary>
        /// <param name="entity">要更新的內容。</param>
        /// <param name="updateProperties">需要更新的欄位。</param>
        public virtual void Update(TEntity entity, Expression<Func<TEntity, object>>[] updateProperties)
        {
            m_Context.Configuration.ValidateOnSaveEnabled = false;

            m_Context.Entry<TEntity>(entity).State = System.Data.Entity.EntityState.Unchanged;

            if (updateProperties != null)
            {
                foreach (var property in updateProperties)
                {
                    m_Context.Entry<TEntity>(entity).Property(property).IsModified = true;
                }
            }
        }

        /// <summary>
        /// 刪除一筆資料內容。
        /// </summary>
        /// <param name="entity">要被刪除的Entity。</param>
        public void Delete(TEntity entity)
        {
            m_Context.Entry<TEntity>(entity).State = System.Data.Entity.EntityState.Deleted;
        }

        /// <summary>
        /// 儲存異動。
        /// </summary>
        public virtual void SaveChanges()
        {
            try
            {
                m_Context.SaveChanges();
                // 因為Update 單一model需要先關掉validation，因此重新打開
                if (m_Context.Configuration.ValidateOnSaveEnabled == false)
                {
                    m_Context.Configuration.ValidateOnSaveEnabled = true;
                }


            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var _errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => string.Format("{0}.{1}: {2}", x.GetType().Name, x.PropertyName, x.ErrorMessage));

                // Join the list to a single string.
                var _fullErrorMessage = string.Join("; ", _errorMessages);

                // Combine the original exception message with the new one.
                var _exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", _fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new Exceptions.DbEntityValidationException(_exceptionMessage, ex);
            }
        }

        /// <summary>
        /// Gets the by primary keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public virtual TEntity GetByPrimaryKeys(params object[] keys)
        {
            return m_Context.Set<TEntity>().Find(keys);
        }
    }
}