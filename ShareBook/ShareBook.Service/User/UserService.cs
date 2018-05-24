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

        public Result<User> AutenticationByEmailAndPassword(User user)
        {
            var result = Validate(user);
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

            if (!IsValidPassword(user, decryptedPass))
            {
                result.Messages.Add("Email ou senha incorretos");
                return result;
            }
                
            result.Value = UserCleanup(user);
            return result;
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
                result.Value = UserCleanup(_repository.Insert(user));
            }
            return result;
        }
        #endregion

        #region Private
        private bool IsValidPassword(User user, string decryptedPass)
        {
            return  user == null ? false : user.Password == Hash.Create(decryptedPass, user.PasswordSalt);
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
