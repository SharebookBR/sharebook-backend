using System;
using System.Linq;
using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Helper.Crypto;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public class UserService : BaseService<User>, IUserService
    {
        #region Public
        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IValidator<User> validator) : base(userRepository, unitOfWork, validator) { }

        public User GetByEmailAndPassword(User user)
        {
            string decryptedPass = user.Password;

            user = _repository.Get()
                .Where(e => e.Email.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => new User
                {
                    Id = x.Id,
                    Email = x.Email,
                    Password = x.Password,
                    PasswordSalt = x.PasswordSalt
                }).FirstOrDefault();

            if (user != null)
                user = GetUserByPassword(user, decryptedPass);
             
            return user;
        }
         
        public override Result<User> Insert(User user)
        {
            var result = Validate(user);

            if (_repository.Any(x => x.Email == user.Email))
                result.Messages.Add("Usuário já possui email cadastrado.");

            user.Email = user.Email.ToLowerInvariant();
            if (result.Success)
            {
                user = GetUserEncryptedPass(user);
                result.Value = _repository.Insert(user);
                result.Value = UserCleanup(result.Value);
            }

            return result;
        }
        #endregion

        #region Private
        private User GetUserByPassword(User user, string decryptedPass)
        {
            if (user.Password == Hash.Create(decryptedPass, user.PasswordSalt))
                return UserCleanup(user);
            return null;
        }
        private User GetUserEncryptedPass(User user)
        {
            user.PasswordSalt = Salt.Create();
            user.Password = Hash.Create(user.Password, user.PasswordSalt);
            return user;
        }
        private User UserCleanup(User user)
        {
            user.Password = string.Empty;
            user.PasswordSalt = string.Empty;
            return user;
        }
        #endregion
    }
}
