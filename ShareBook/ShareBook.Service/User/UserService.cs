using System;
using AutoMapper;
using ShareBook.Data;
using ShareBook.Data.Common;
using ShareBook.Data.Entities.User;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.VM.Common;
using ShareBook.VM.User.Model;

namespace ShareBook.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public UserVM GetById(Guid id)
        {
            var user = _userRepository.GetById(id);

            return Mapper.Map<UserVM>(user);
        }

        public UserVM Login(UserVM userVM)
        {
            var user = Mapper.Map<User>(userVM);

            new UserValidation().Validate(user);

            user = _userRepository.GetByEmailAndPassword(user);

            return Mapper.Map<UserVM>(user);
        }

        public ResultServiceVM Register(UserVM userVM)
        {
            var user = Mapper.Map<User>(userVM);

            var resultService = new ResultService(new UserValidation().Validate(user));

            if (_userRepository.GetByEmail(userVM.Email) != null)
            {
                resultService.Messages.Add("Usuário já possui email cadastrado.");
            }

            if (resultService.Success)
            {
                _userRepository.Insert(user);
                _unitOfWork.SaveChanges();
            }

            return Mapper.Map<ResultServiceVM>(resultService);
        }
    }
}