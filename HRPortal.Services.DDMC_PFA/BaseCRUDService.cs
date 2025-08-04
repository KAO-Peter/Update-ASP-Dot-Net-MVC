using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Core.Utilities;

namespace HRPortal.Services.DDMC_PFA
{
    public abstract class BaseCrudService<TEntity> : BaseGridService<TEntity>
        where TEntity : class
    {
        public BaseCrudService(HRPortal_Services services)
            : base(services)
        {

        }

        public int Create(IEnumerable<TEntity> data, bool isSave = true)
        {
            foreach (TEntity item in data.ToArray())
            {
                this.Create(item, false);
            }
            return this.SaveChanges(isSave);
        }

        public virtual int Create(TEntity model, bool isSave = true)
        {
            var validationResults = new HashSet<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model, null, null), validationResults, true);
            if (!isValid)
            {
                var resultsGroupedByMembers = validationResults
                    .SelectMany(_ => _.MemberNames.Select(
                         x => new
                         {
                             MemberName = x ?? "",
                             Error = _.ErrorMessage
                         }))
                    .GroupBy(_ => _.MemberName);
                throw new Exception(string.Join("", resultsGroupedByMembers.Select(x => x.Key + string.Join("、", x.Select(m => m.Error)))));
            }

            try
            {
                Db.Set<TEntity>().Add(model);
                return this.SaveChanges(isSave);
            }
            catch
            {
                Db.Set<TEntity>().Remove(model);
                return 0;
            }
        }

        public virtual int Update(TEntity data, bool isSave = true)
        {
            var entity = Db.Entry(data);
            try
            {
                entity.State = EntityState.Modified;
                return this.SaveChanges(isSave);
            }
            catch
            {
                entity.State = EntityState.Unchanged;
                return 0;
            }
        }

        public int Update(IEnumerable<TEntity> data, bool isSave = true)
        {
            foreach (TEntity item in data)
            {
                this.Update(item, false);
            }
            return this.SaveChanges(isSave);
        }

        public virtual int Update(TEntity oldData, TEntity newData, string[] includeProperties, bool isSave = true)
        {
            foreach (string propertie in includeProperties)
            {
                PropertyInfo propertyInfo = newData.GetType().GetProperty(propertie);
                propertyInfo.SetValue(oldData, propertyInfo.GetValue(newData));
            }
            return Update(oldData, isSave);
        }

        public virtual int UpdateRange(IEnumerable<TEntity> rows, IEnumerable<TEntity> models, string[] includeProperties, bool isSave = true)
        {
            if (models == null)
            {
                int retVal = this.Delete(rows, false);
            }
            else
            {
            }

            return this.SaveChanges(isSave);
        }

        public virtual int Delete(TEntity data, bool isSave = true)
        {
            if (data == null)
                return 0;
            Db.Set<TEntity>().Remove(data);
            return this.SaveChanges(isSave);
        }

        public int Delete(IEnumerable<TEntity> data, bool isSave = true)
        {
            if (data == null || data.Count() == 0)
                return 0;
            foreach (TEntity item in data.ToArray())
            {
                this.Delete(item, false);
            }
            return this.SaveChanges(isSave);
        }
    }
}