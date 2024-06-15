using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IUserService : IBaseService<User>
    {
        Task<Result<User>> AuthenticationByEmailAndPasswordAsync(User user);
        bool IsValidPassword(User user, string decryptedPass);
        new Task<Result<User>> UpdateAsync(User user);
        Task<Result<User>> ValidOldPasswordAndChangeUserPasswordAsync(User user, string newPassword);
        Task<Result<User>> ChangeUserPasswordAsync(User user, string newPassword);
        Task<Result> GenerateHashCodePasswordAndSendEmailToUserAsync(string email);
        Task<Result> ConfirmHashCodePasswordAsync(string hashCodePassword);
        IList<User> GetFacilitators(Guid userIdDonator);
        IList<User> GetAdmins();
        Task<IList<User>> GetBySolicitedBookCategoryAsync(Guid bookCategoryId);
        Task<UserStatsDTO> GetStatsAsync(Guid? userId);
        Task<Result<User>> InsertAsync(RegisterUserDTO userDto);
        Task ParentAprovalAsync(string parentHashCodeAproval);
    }
}
