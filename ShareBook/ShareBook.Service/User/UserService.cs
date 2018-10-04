using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentValidation;
using ShareBook.Domain.Entities;
using ShareBook.Domain.Common;
using ShareBook.Helper.Crypto;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private const string PASSWORD_IS_WEAK = "A senha não atende os requisitos. Mínimo oito caracteres, um caractere especial, um caractere numérico e uma letra em maiúsculo.";
        #region Public
        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IValidator<User> validator) : base(userRepository, unitOfWork, validator)
        {
            _userRepository = userRepository;
        }

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

            if(!user.PasswordIsStrong())
                result.Messages.Add(PASSWORD_IS_WEAK);

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
            user.Id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            Result<User> result = Validate(user, x =>
                x.Email,
                x => x.Linkedin,
                x => x.Name,
                x => x.Phone,
                x => x.PostalCode,
                x => x.Id);

            if (result.Success == false) return result;

            User userAux = _repository.Get(user.Id);

            if (userAux == null) result.Messages.Add("Usuário não existe.");

            if (_repository.Any(u => u.Email == user.Email && u.Id != user.Id))
                result.Messages.Add("Email já existe.");

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

        public Result<User> ChangeUserPassword(User user, string newPassword)
        {
            user.Id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);       
            var resultUserAuth = this.AuthenticationByIdAndPassword(user);

            if (resultUserAuth.Success)
            {
                var userAuth = resultUserAuth.Value;
                userAuth.ChangePassword(newPassword);
                
                if(!userAuth.PasswordIsStrong())
                {
                    resultUserAuth.Messages.Add(PASSWORD_IS_WEAK);
                    resultUserAuth.Value = null;
                    return resultUserAuth;
                }
                    
                userAuth = GetUserEncryptedPass(userAuth);
                userAuth = _userRepository.UpdatePassword(userAuth).Result;
                resultUserAuth.Value = UserCleanup(userAuth);
            }
            return resultUserAuth;
        }
        #endregion


        #region Private

        private Result<User> AuthenticationByIdAndPassword(User user)
        {
            var result = Validate(user, x => x.Id, x => x.Password);

            string decryptedPass = user.Password;

            user = _repository.Get()
                .Where(e => e.Id == user.Id)
                .FirstOrDefault();

            if (user == null || !IsValidPassword(user, decryptedPass))
            {
                result.Messages.Add("Senha incorreta");
                return result;
            }

            result.Value = UserCleanup(user);
            return result;
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
