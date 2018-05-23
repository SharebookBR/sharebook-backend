using System;
using System.Linq;
using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public class UserService : BaseService<User>, IUserService
    {
        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IValidator<User> validator) : base(userRepository, unitOfWork, validator) { }

        public User GetByEmailAndPassword(User user)
        {
            user = _repository.Get()
                .Where(e => e.Email.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase) && e.Password.Equals(user.Password))
                .Select(x => new User
                {
                    Id = x.Id,
                    Email = x.Email,
                }).FirstOrDefault();

            return user;
        }

        public override Result<User> Insert(User user)
        {
            var result = Validate(user);

            if ( _repository.Any(x => x.Email == user.Email))
                result.Messages.Add("Usuário já possui email cadastrado.");

            user.Email = user.Email.ToLowerInvariant();
            if (result.Success)
                result.Value = _repository.Insert(user);

            return result;
        }
    }
}
