using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service.Lgpd
{
    public class LgpdService: ILgpdService
    {
        private readonly IUserService _userService;
        private readonly IUserEmailService _userEmailService;
        private readonly IBookUserService _bookUserService;
        private readonly ApplicationDbContext _ctx;

        public LgpdService(
            IUserService userService,
            IUserEmailService userEmailService,
            IBookUserService bookUserService, 
            ApplicationDbContext context)
        {
            _userService = userService;
            _userEmailService = userEmailService;
            _bookUserService = bookUserService;
            _ctx = context;
        }

        public async Task AnonymizeAsync(UserAnonymizeDTO dto)
        {
            var user = await _ctx.Users
                .Where(u => u.Id == dto.UserId)
                .Include(u => u.Address)
                .Include(u => u.BooksDonated)
                .Include(u => u.BookUsers) // livros solicitados
                .FirstOrDefaultAsync();

            if (user == null)
                throw new ShareBookException(ShareBookException.Error.NotFound, "Nenhum usuário encontrado.");

            if (!_userService.IsValidPassword(user, dto.Password))
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Senha inválida.");

            if (string.IsNullOrEmpty(dto.Reason))
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Favor informar a justificativa.");

            if (!user.Active)
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Essa conta não está ativa.");

            // 1 - Exclui as solicitações em aberto.
            RemoveOpenRequests(user);

            // 2 - Cancela doações em aberto.
            await RemoveOpenDonationsAsync(user);

            // 3 - Anonimiza a conta
            user.Anonymize();

            // 4 - Exclui os logs.
            RemoveLogs(user);

            // 5 - Notifica os adms.
            _userEmailService.SendEmailAnonymizeNotifyAdms(dto);

            // 6 - Enfim salva
            await _ctx.SaveChangesAsync();

        }

        private void RemoveOpenRequests(User user)
        {
            foreach (var request in user.BookUsers)
            {
                if (request.Status == DonationStatus.WaitingAction)
                {
                    request.Reason = "Pedido excluído. Favor ignorar.";
                }
            }
        }

        private async Task RemoveOpenDonationsAsync(User user)
        {
            foreach (var book in user.BooksDonated)
            {
                if (book.Status == BookStatus.WaitingApproval || book.Status == BookStatus.Available || book.Status == BookStatus.AwaitingDonorDecision)
                {
                    await CancelDonationAsync(book);
                }
            }
        }

        private async Task CancelDonationAsync(Book book)
        {
            var cancelationDTO = new BookCancelationDTO
            {
                Book = book,
                CanceledBy = "USUÁRIO ANONIMIZADO",
                Reason = "Doação cancelada porque o usuário removeu a própria conta."
            };

            await _bookUserService.CancelAsync(cancelationDTO);
        }

        private void RemoveLogs(User user)
        {
            var logsUser = _ctx.LogEntries.Where(log => log.EntityName == "User" && log.EntityId == user.Id).ToArray();
            _ctx.LogEntries.RemoveRange(logsUser);

            var logsAddress = _ctx.LogEntries.Where(log => log.EntityName == "Address" && log.EntityId == user.Address.Id).ToArray();
            _ctx.LogEntries.RemoveRange(logsAddress);
        }

    }
}
