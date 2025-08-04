using HRPortal.Services.Models;
using LinqKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace HRPortal.Services
{
    public abstract class BaseGridService<TEntity> : BaseService
        where TEntity : class
    {
        public BaseGridService(HRPortal_Services services)
            : base(services)
        {
        }

        public bool Any(params Expression<Func<TEntity, bool>>[] filters)
        {
            return this.Any(true, filters);
        }

        public bool Any(bool include_disable, params Expression<Func<TEntity, bool>>[] filters)
        {
            return this.Where(include_disable, filters).Any();
        }

        //public TEntity Get(Guid id)
        //{
        //    return this.FirstOrDefault(x => x.ID == id);
        //}

        public TEntity First(params Expression<Func<TEntity, bool>>[] filters)
        {
            return this.Where(filters).First();
        }

        public TEntity FirstOrDefault(params Expression<Func<TEntity, bool>>[] filters)
        {
            return this.Where(true, filters).FirstOrDefault();
        }

        //public IQueryable<TEntity> Find(Guid[] ids)
        //{
        //    if (ids.Length == 0)
        //        return null;
        //    return this.Where(true, x => ids.Contains(x.ID));
        //}

        protected virtual IQueryable<TEntity> Where(bool include_disable, params Expression<Func<TEntity, bool>>[] filters)
        {
            //var context = ((IObjectContextAdapter)Db).ObjectContext;
            //context.Refresh(RefreshMode.ClientWins, Db.Set<TEntity>().Local);
            IQueryable<TEntity> query = this.Db.Set<TEntity>().AsQueryable();

            if (filters != null)
            {
                filters.ForEach(f => query = query.Where(f));
            }
            //TODO 20150910
            //if (typeof(TEntity).GetInterfaces().Contains(typeof(ISoftDelete)))
            //{
            //    query = query.Where("DeletedTime == null");
            //}
            //if (include_disable == false && typeof(TEntity).GetInterfaces().Contains(typeof(IDisable)))
            //{
            //    query = query.Where("DisableDate == null");
            //}
            //End

            //if (this.GetSysId() != null && typeof(TEntity).GetInterfaces().Contains(typeof(ISys)))
            //{
            //    query = query.Where("sys_id == @0", this.GetSysId().Value);
            //}

            return query;
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return this.Where(false);
        }

        public IQueryable<TEntity> Where(params Expression<Func<TEntity, bool>>[] filters)
        {
            return this.Where(false, filters);
        }

        public int Count(params Expression<Func<TEntity, bool>>[] filters)
        {
            return this.Where(filters).Count();
        }

        public int Count(bool include_disable, params Expression<Func<TEntity, bool>>[] filters)
        {
            return this.Where(include_disable, filters).Count();
        }

        public JQGridResponce PagedGetAll(JQGridRequest request, params Expression<Func<TEntity, bool>>[] filters)
        {
            return this.PagedGetAll(request, false, filters);
        }

        public virtual JQGridResponce PagedGetAll(JQGridRequest request, bool include_disable, params Expression<Func<TEntity, bool>>[] filters)
        {
            IQueryable<TEntity> query = this.Where(include_disable, filters);

            return this.PagedGetAll(request, query);
        }

        protected JQGridResponce PagedGetAll(JQGridRequest request, IQueryable<TEntity> query)
        {
            query = query.AsNoTracking();
            if (string.IsNullOrWhiteSpace(request.keyword) == false)
            {
                var predicate = PredicateBuilder.False<TEntity>();
                foreach (string col in request.colSearch)
                {
                    predicate = predicate.OrLike(col, request.keyword);
                }
                query = query.AsExpandable<TEntity>().Where(predicate);
            }

            if (request.filters != null)
            {
                query = query.AsExpandable<TEntity>().Where(Filter(request.filters));
            }

            JQGridResponce responce = new JQGridResponce();
            int totalItemCount = query.Count();

            if (totalItemCount > 0)
                responce.total = Convert.ToInt16(Math.Ceiling((double)totalItemCount / (double)request.rows));
            if (request.page > responce.total)
                responce.page = responce.total;
            else
                responce.page = request.page;
            if (responce.page <= 0)
                responce.page = 1;
            responce.records = totalItemCount;
            if (totalItemCount > 0)
            {
                int take = request.rows * responce.page;
                int skip = request.rows * (responce.page - 1);
                if (string.IsNullOrWhiteSpace(request.sidx) == false)
                {
                    if (request.sord.ToLower() == "desc")
                    {
                        string[] sortnames = request.sidx.Split(',');
                        query = query.OrderBy(string.Join(" Descending,", sortnames) + " Descending");
                    }
                    else
                    {
                        query = query.OrderBy(request.sidx);
                    }
                }
                if (request.queryMode == "ObjectMode")
                {
                    List<string> includes = new List<string>();
                    foreach (string col in request.colModel)
                    {
                        if (col.LastIndexOf(".") > -1)
                        {
                            string path = col.Substring(0, col.LastIndexOf("."));
                            if (includes.IndexOf(path) == -1)
                                includes.Add(path);
                        }
                    }
                    foreach (string include in includes)
                    {
                        query = query.Include(include);
                    }
                    responce.rows = GetRows(query.Take(take).Skip(skip).ToList(), request.colModel);
                }
                else
                {
                    responce.rows = query.Take(take).Skip(skip).Select("new(" + String.Join(",", request.colModel) + ")").AsEnumerable().ToList();
                }
            }
            return responce;
        }

        protected List<dynamic> GetRows(List<TEntity> query, string[] colModel)
        {
            List<dynamic> data = new List<dynamic>();
            foreach (TEntity item in query)
            {
                data.Add(GetRow(item, colModel));
            }
            return data;
        }

        protected dynamic GetRow(TEntity item, string[] colModel)
        {
            var row = new ExpandoObject() as IDictionary<string, Object>;
            foreach (string col in colModel)
            {
                string[] propertyNames = col.Split('.').ToArray();
                row.Add(col, GetRowFields(item, propertyNames));
            }
            return row;
        }

        protected virtual dynamic GetRowFields(TEntity item, string[] propertyNames)
        {
            object value = GetRowField(item, propertyNames[0]);

            for (int i = 1; i < propertyNames.Count(); i++)
            {
                value = GetRowField(value, propertyNames[i]);
            }

            return value;
        }

        protected dynamic GetRowField(object item, string propertyName)
        {
            if (item == null)
                return null;
            if (propertyName.Contains("()"))
            {
                MethodInfo methodInfo = item.GetType().GetMethod(propertyName.Replace("()", ""));
                return methodInfo.Invoke(item, new object[] { });
            }
            else
            {
                PropertyInfo barProperty = item.GetType().GetProperty(propertyName);
                if (barProperty == null)
                    return null;
                return barProperty.GetValue(item);
            }
        }

        private Expression<Func<TEntity, bool>> GetExpression(Expression<Func<TEntity, bool>> predicate, JqGridRule rule)
        {
            switch (rule.op)
            {
                case JqGridRule.RULE_OP_CN:
                    if (string.IsNullOrWhiteSpace(rule.data))
                        return predicate;
                    return predicate.GetExpression(LinqExtensions.Op.Contains, rule.field, rule.data);
                case JqGridRule.RULE_OP_EQ:
                    return predicate.GetExpression(LinqExtensions.Op.Equals, rule.field, rule.data);
                case JqGridRule.RULE_OP_EN:
                    return predicate.GetExpression(LinqExtensions.Op.NotEqual, rule.field, rule.data);
            }
            return predicate;
        }

        private Expression<Func<TEntity, bool>> Filter(JqGridFilter f)
        {
            Expression<Func<TEntity, bool>> predicate;
            if (f.groupOp == JqGridFilter.FILTER_GROUP_OP_AND)
                predicate = PredicateBuilder.True<TEntity>();
            else
                predicate = PredicateBuilder.False<TEntity>();
            if (f.rules != null)
            {
                foreach (JqGridRule rule in f.rules)
                {
                    switch (f.groupOp)
                    {
                        case JqGridFilter.FILTER_GROUP_OP_AND:
                            predicate = predicate.And(GetExpression(predicate, rule));
                            break;
                        case JqGridFilter.FILTER_GROUP_OP_OR:
                            predicate = predicate.Or(GetExpression(predicate, rule));
                            break;
                    }
                }
            }
            if (f.groups != null)
            {
                foreach (JqGridFilter i in f.groups)
                {
                    switch (f.groupOp)
                    {
                        case JqGridFilter.FILTER_GROUP_OP_AND:
                            predicate = predicate.And(Filter(i));
                            break;
                        case JqGridFilter.FILTER_GROUP_OP_OR:
                            predicate = predicate.Or(Filter(i));
                            break;
                    }
                }
            }
            return predicate;
        }

        public virtual List<dynamic> Suggest(string keyword, string searchPropertie, string[] selectors, int limit = 10)
        {
            IQueryable<TEntity> query = this.GetAll();

            if (string.IsNullOrWhiteSpace(keyword) == false)
            {
                var predicate = PredicateBuilder.False<TEntity>();
                predicate = predicate.OrLike(searchPropertie, keyword);
                query = query.AsExpandable<TEntity>().Where(predicate);
            }
            List<dynamic> data = query.Take(limit).Select(string.Format("new({0})", string.Join(",", selectors))).AsEnumerable().Distinct().ToList();
            return data;
        }
    }
}