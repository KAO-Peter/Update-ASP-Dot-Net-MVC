using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Data;
using Autofac;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.Databases.SqlClient.Repositories;
using YoungCloud.Configurations;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public abstract class SignFlowRepositoryBase<TEntity> :  RepositoryBase<TEntity>, ISignFlowRepositoryBase<TEntity> where TEntity : class
    {
        public SignFlowRepositoryBase()
        {

            this.m_Context = _scope.Resolve<SignFlowEntities>();
        }        
    }
}