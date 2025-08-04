using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;

using HRPortal.WebAPI.Controllers;
using HRPortal.DBEntities;
using HRPortal.Services;

namespace HRPortal.WebAPI
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            #region Register Controller
            container.RegisterType<DepartmentController>();
            #endregion

            #region Register Service
            container.RegisterType<DepartmentService>(new HierarchicalLifetimeManager());
            #endregion

            #region Model
            container.RegisterType<Department>();

            #endregion

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}