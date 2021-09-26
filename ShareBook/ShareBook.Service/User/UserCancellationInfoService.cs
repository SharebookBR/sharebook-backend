using FluentValidation;

using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service {
    public class UserCancellationInfoService : BaseService<UserCancellationInfo>, IUserCancellantionInfoService {
        private readonly IUserCancellationInfoRepository _userCancellationInfoRepository;
        private readonly IUserService _userService;
        private readonly IBookUserService _bookUserService;
        private readonly IBookService _bookService;
        private readonly IUserEmailService _userEmailService;
        public UserCancellationInfoService(IUserCancellationInfoRepository repository, 
            IUnitOfWork unitOfWork,
            IValidator<UserCancellationInfo> validator, 
            IBookUserService bookUserService, 
            IUserService userService, 
            IBookService bookService,
            IUserEmailService userEmailService)
                : base(repository, unitOfWork, validator) {
            _userCancellationInfoRepository = repository;
            _bookUserService = bookUserService;
            _userService = userService;
            _bookService = bookService;
            _userEmailService = userEmailService;
        }

       public async Task<bool> ToProceed(UserCancellationInfo userCancelInfo) {
            var bookOrders = new List<Book>();
            var result = false;
            var dto = new UserCancellationDto();
            dto.SetDate(userCancelInfo.CancellationUserDate);
            dto.SetReason(userCancelInfo.Reason);
            dto.SetUserId(userCancelInfo.UserId);
            try {
                _unitOfWork.BeginTransaction();
                //1. Cancelar pedidos aos livros do doador que solicitou a anonimização
                var cancel1 = _bookUserService.CancelDonationRequestsByDonor(userCancelInfo.UserId);
                
                await foreach (var c in cancel1) {
                    dto.AddCancellationsBooksForDonations(c);
                }

                //2. Cancelar os pedidos feitos pelo usuário que solicitou a anonimização
                var cancel2 = _bookUserService.CancelDonationRequestsByRequester(userCancelInfo.UserId);

                await foreach (var c in cancel2) {
                    dto.AddCanceledDonationRequest(c);
                }

                //3. Cancelar os livros do doador, que solicitou anonimização
                var cancel3 = _bookService.CancelBooksByDonor(userCancelInfo.UserId);

                await foreach (var c in cancel3) {
                    dto.AddCanceledBooks(c);
                }

                //4. Anonimizar o usuário e seu endereço
                var user = _userService.Find(userCancelInfo.UserId);
                user.AnonymizeMe();
                await _userService.UpdateAsync(user);

                //5. Gravar dados do cancelamento
                await _userCancellationInfoRepository.InsertAsync(userCancelInfo);

                //6. Disparar e-mail para os usuários
                await _userEmailService.SendEmailUserCancellation(dto);

                result = true;
            } catch (Exception e) {
                result = false;
                _unitOfWork.Rollback();
                return result;

            } finally {
                _unitOfWork.Commit();
            }

            return result;
       }
    }
}