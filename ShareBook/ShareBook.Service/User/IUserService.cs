using System;
using ShareBook.VM.Common;
using ShareBook.VM.User.Model;

namespace ShareBook.Service
{
    public interface IUserService
    {
        UserVM GetById(Guid id);

        UserVM Login(UserVM userVM);

        ResultServiceVM Register(UserVM userVM);
    }
}