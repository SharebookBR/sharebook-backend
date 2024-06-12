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
        Result<User> AuthenticationByEmailAndPassword(User user);
        bool IsValidPassword(User user, string decryptedPass);
        new Task<Result<User>> UpdateAsync(User user);
        Result<User> ValidOldPasswordAndChangeUserPassword(User user, string newPassword);
        Result<User> ChangeUserPassword(User user, string newPassword);
        Task<Result> GenerateHashCodePasswordAndSendEmailToUserAsync(string email);
        Result ConfirmHashCodePassword(string hashCodePassword);
        IList<User> GetFacilitators(Guid userIdDonator);
        IList<User> GetAdmins();
        Task<IList<User>> GetBySolicitedBookCategoryAsync(Guid bookCategoryId);
        Task<UserStatsDTO> GetStatsAsync(Guid? userId);
        Task<Result<User>> InsertAsync(RegisterUserDTO userDto);
        Task ParentAprovalAsync(string parentHashCodeAproval);
    }
}
