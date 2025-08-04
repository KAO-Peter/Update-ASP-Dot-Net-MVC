using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace HRPortal.Mvc
{
    public class ApplicationUserSore<T, TKey> : IUserStore<T, Guid> where T : ApplicationUser, Microsoft.AspNet.Identity.IUser<TKey>
    {

        Task IUserStore<T, Guid>.CreateAsync(T user)
        {
            //Create /Create New User
            throw new NotImplementedException();
        }

        Task IUserStore<T, Guid>.DeleteAsync(T user)
        {
            //Delete User
            throw new NotImplementedException();
        }

        Task<T> IUserStore<T, Guid>.FindByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        Task<T> IUserStore<T, Guid>.FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        Task IUserStore<T, Guid>.UpdateAsync(T user)
        {
            //SaveList User Profile
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            // throw new NotImplementedException();

        }
    }
}