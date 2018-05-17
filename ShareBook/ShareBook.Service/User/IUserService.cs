using ShareBook.VM.Common;
using ShareBook.VM.User.Model;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IUserService
    {
        Task<ResultServiceVM> CreateUser(UserVM userVM);

        Task<UserVM> GetUserById(Guid id);

        Task<ResultServiceVM> GetUserByEmailAndPasswordAsync(UserVM userVM);
    }
}
