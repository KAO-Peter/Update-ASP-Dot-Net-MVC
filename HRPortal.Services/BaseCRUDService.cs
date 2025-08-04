using HRPortal.Core.Utilities;
//using HRPortal.Models;
using HRPortal.DBEntities;
//using HRPortal.Models.Interfaces;
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

namespace HRPortal.Services
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
            //model.ID = Utility.GenerateGuid();
            //if (this.GetSysId() != null && model is ISys)
            //{
            //    (model as ISys).sys_id = this.GetSysId().Value;
            //}

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
                //int retVal = this.Delete(rows.Where(o => models.FirstOrDefault(n => n.ID == o.ID) == null).ToList(), false);
                //foreach (TEntity item in models.ToArray())
                //{
                //    TEntity destination = rows.FirstOrDefault(x => x.ID == item.ID);
                //    if (destination == null)
                //        this.Create(item, false);
                //    else
                //        this.Update(destination, item, includeProperties, false);
                //}
            }

            return this.SaveChanges(isSave);
        }

        public virtual int Delete(TEntity data, bool isSave = true)
        {
            if (data == null)
                return 0;
            //if (data is ISoftDelete)
            //{
            //    (data as ISoftDelete).deleted_time = DateTime.Now;
            //    return this.Update(data, isSave);
            //}
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

        //public int Delete(Guid[] ids, bool isSave = true)
        //{
        //    return this.Delete(this.Find(ids).ToList(), isSave);
        //}

        //public int Delete(bool isSave = true, params Expression<Func<TEntity, bool>>[] filters)
        //{
        //    return this.Delete(this.Where(filters).ToList(), isSave);
        //}

        //public int Delete(params Expression<Func<TEntity, bool>>[] filters)
        //{
        //    return this.Delete(true, filters);
        //}
    }
}