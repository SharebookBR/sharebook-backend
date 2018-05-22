using System;
using System.Threading.Tasks;
using ShareBook.VM.Common;
using ShareBook.VM.User.Model;

namespace ShareBook.Service
{
    public interface IUserService
    {
        Task<ResultServiceVM> CreateUserAsync(UserVM userVM);

        Task<UserVM> GetUserByIdAsync(Guid id);

        Task<UserVM> GetUserByEmailAndPasswordAsync(UserVM userVM);
    }
}