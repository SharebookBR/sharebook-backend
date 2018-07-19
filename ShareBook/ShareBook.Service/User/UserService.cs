using System;
using System.Collections.Generic;
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

        public Result<User> AuthenticationByEmailAndPassword(User user)
        {

            var result = Validate(user, x => x.Email, x => x.Password);

            string decryptedPass = user.Password;

            user = _repository.Get()
                .Where(e => e.Email.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();



            if (user == null || !IsValidPassword(user, decryptedPass))
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

        public override Result<User> Update(User user)
        {
            Result<User> result = Validate(user, x =>
                x.Email,
                x => x.Linkedin,
                x => x.Name,
                x => x.Phone,
                x => x.PostalCode);

            if (result.Success == false) return result;

            User userAux = _repository.Get(user.Id);

            if (userAux == null) result.Messages.Add("Usuário não existe.");

            if (result.Success)
            {
                userAux.ChangeEmail(user.Email);
                userAux.ChangeName(user.Name);
                userAux.ChangeLinkedin(user.Linkedin);
                userAux.ChangePostalCode(user.PostalCode);
                userAux.ChangePhone(user.Phone);
                result.Value = UserCleanup(_repository.Update(userAux));
            }
            return result;
        }

        public override User Get(params object[] keyValues) => UserCleanup(_repository.Get(keyValues));

        public IEnumerable<User> GetAllAdministrators()
        {
            return _repository.Get().Where(x => x.Profile == Domain.Enums.Profile.Administrator);
        }
        #endregion

        #region Private

        private Result<User> ChangeUserPassword(User user, string oldPassword)
        {
            Result<User> result = Validate(user, x =>  x.Password);

            if (result.Success == false)
            {
                return result;
            }

            var resultUserAuth = this.AuthenticationByEmailAndPassword(new User() { Email = user.Email, Password = oldPassword });

            if (resultUserAuth.Success == true)
            {
                var userAuth = resultUserAuth.Value;
                userAuth.ChangePassword(user.Password);
                userAuth = GetUserEncryptedPass(userAuth);
                resultUserAuth.Value = userAuth;               
            }

            return resultUserAuth;
        }

        private bool IsValidPassword(User user, string decryptedPass)
        {
            return user.Password == Hash.Create(decryptedPass, user.PasswordSalt);
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
