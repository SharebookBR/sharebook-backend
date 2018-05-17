using System;
using System.Threading.Tasks;
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
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;


        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResultServiceVM> CreateUser(UserVM userVM)
        {
            User user = Mapper.Map<User>(userVM);

            ResultService resultService = new ResultService(new UserValidation().Validate(user));

            _unitOfWork.BeginTransaction();

            if (resultService.Success)
            {
                await _userRepository.InsertAsync(user);
                _unitOfWork.Commit();
            }

            return Mapper.Map<ResultServiceVM>(resultService);
        }

        public async Task<ResultServiceVM> GetUserByEmailAndPasswordAsync(UserVM userVM)
        {
            User user = Mapper.Map<User>(userVM);

            ResultService resultService = new ResultService(new UserValidation().Validate(user));

            user = await _userRepository.GetByEmailAndPasswordAsync(user);

            return Mapper.Map<ResultServiceVM>(resultService);
        }

        public Task<UserVM> GetUserById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
