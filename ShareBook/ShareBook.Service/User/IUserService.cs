using ShareBook.VM.Common;
using ShareBook.VM.User.In;
using ShareBook.VM.User.Out;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IUserService
    {
        Task<ResultServiceVM> CreateUser(UserInVM userInVM);

        Task<UserOutByIdVM> GetUserById(Guid id);

        Task<UserOutByIdVM> GetUserByEmailAndPasswordAsync(UserInVM userInVm);
    }
}
