using AutoMapper;
using Flurl.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository.Repository;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using ShareBook.Service.AwsSqs.Dto;
using ShareBook.Service.EBook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BookController : ControllerBase
    {
        private readonly IBookUserService _bookUserService;
        private readonly IBookService _service;
        private readonly IUserService _userService;
        private readonly IAccessHistoryService _accessHistoryService;
        private readonly IEBookService _ebookService;
        private Expression<Func<Book, object>> _defaultOrder = x => x.Id;
        private readonly IMapper _mapper;

        public BookController(IBookService bookService,
                              IBookUserService bookUserService,
                              IUserService userService,
                              IMapper mapper,
                              IAccessHistoryService accessHistoryService,
                              IEBookService ebookService)
        {
            _service = bookService;
            _bookUserService = bookUserService;
            _userService = userService;
            _mapper = mapper;
            _accessHistoryService = accessHistoryService;
            _ebookService = ebookService;
        }

        protected void SetDefault(Expression<Func<Book, object>> defaultOrder)
        {
            _defaultOrder = defaultOrder;
        }

        [HttpGet()]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public async Task<PagedList<BookVMAdm>> GetAllAsync() => await PagedAsync(1, 15);

        [HttpGet("{page}/{items}")]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public async Task<PagedList<BookVMAdm>> PagedAsync(int page, int items)
        {
            // TODO: parar de usar esse get complicado e fazer uma query linq/ef tradicional usando
            // ThenInclude(). fonte: https://stackoverflow.com/questions/10822656/entity-framework-include-multiple-levels-of-properties
            var books = await _service.GetAsync(x => x.Title, page, items, new IncludeList<Book>(x => x.User, x => x.BookUsers, x => x.UserFacilitator));
            var responseVM = _mapper.Map<List<BookVMAdm>>(books.Items);

            return new PagedList<BookVMAdm>()
            {
                Page = page,
                TotalItems = books.TotalItems,
                ItemsPerPage = items,
                Items = responseVM
            };
        }

        [Authorize("Bearer")]
        [HttpPost("Approve/{id}")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public async Task<Result> ApproveAsync(string id, [FromBody] ApproveBookVM model)
        {
            await _service.ApproveAsync(new Guid(id), model?.ChooseDate);
            return new Result("Livro aprovado com sucesso.");
        }

        [Authorize("Bearer")]
        [HttpPost("promote")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public async Task<Result> Promote([FromBody] NewBookBody newBook)
        {
            await _service.Promote(newBook);
            return new Result("Livro promovido com sucesso.");
        }

        [Authorize("Bearer")]
        [HttpPost("Received/{bookId}")]
        public async Task<Result> ReceivedAsync(string bookId)
        {
            Guid winnerUserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            await _service.ReceivedAsync(new Guid(bookId), winnerUserId);
            return new Result("Livro Recebido com sucesso.");
        }

        [Authorize("Bearer")]
        [HttpPost("Cancel/{id}")]
        [ProducesResponseType(typeof(Result<CancelBookDonationVM>), 200)]
        public async Task<IActionResult> CancelAsync(string id, [FromQuery] string reason = "")
        {
            if (!await _IsBookOwnerAsync(new Guid(id))) return Unauthorized();

            var cancelationDTO = new BookCancelationDTO
            {
                Book = await _service.FindAsync(new Guid(id)),
                CanceledBy = (await GetSessionUserAsync()).Name,
                Reason = reason
            };

            var returnBook = await _bookUserService.CancelAsync(cancelationDTO);
            var returnBookVm = _mapper.Map<CancelBookDonationVM>(returnBook.Value);
            var result = new Result<CancelBookDonationVM>(returnBookVm);
            return Ok(result);
        }

        [HttpGet("FreightOptions")]
        public IList<dynamic> FreightOptions()
        {
            var freightOptions = _service.FreightOptions();
            return freightOptions;
        }

        // TODO: método deprecado. Remover depois que todos usarem o novo 'RequestersList'.
        [Obsolete]
        [Authorize("Bearer")]
        [HttpGet("GranteeUsersByBookId/{bookId}")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public async Task<IList<User>> GetGranteeUsersByBookIdAsync(string bookId) => await _bookUserService.GetGranteeUsersByBookIdAsync(new Guid(bookId));

        [Authorize("Bearer")]
        [HttpGet("RequestersList/{bookId}")]
        public async Task<IActionResult> GetRequestersListAsync(Guid bookId)
        {
            if (!await _IsBookOwnerAsync(bookId)) return Unauthorized();

            var requesters = await _bookUserService.GetRequestersListAsync(bookId);
            var requestersVM = _mapper.Map<List<RequestersListVM>>(requesters);

            return Ok(requestersVM);
        }

        [HttpGet("Slug/{slug}")]
        [ProducesResponseType(typeof(BookVM), 200)]
        public async Task<IActionResult> GetAsync(string slug)
        {
            var book = await _service.BySlugAsync(slug);
            if (book == null) return NotFound();

            var bookVM = _mapper.Map<BookVM>(book);
            return Ok(bookVM);
        }

        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)] // apenas adms
        [ProducesResponseType(typeof(BookVM), 200)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var book = await _service.FindAsync(new Guid(id));
            var bookVM = _mapper.Map<BookVMAdm>(book);
            return bookVM != null ? (IActionResult)Ok(bookVM) : NotFound();
        }

        [HttpGet("AvailableBooks")]
        public async Task<IList<BookVM>> AvailableBooksAsync()
        {
            var books = await _service.AvailableBooksAsync();
            return _mapper.Map<List<BookVM>>(books);
        }

        [HttpGet("Random15Books")]
        public async Task<IList<BookVM>> Random15BooksAsync()
        {
            var books = await _service.Random15BooksAsync();
            return _mapper.Map<List<BookVM>>(books);
        }

        [HttpGet("Random15EBooks")]
        public async Task<IList<BookVM>> Random15EBooksAsync()
        {
            var books = await _service.Random15EBooksAsync();
            return _mapper.Map<List<BookVM>>(books);
        }

        [HttpGet("FullSearch/{criteria}/{page}/{items}")]
        public async Task<PagedList<BookVM>> FullSearchAsync(string criteria, int page, int items)
        {
            var books = await _service.FullSearchAsync(criteria, page, items);
            var booksVM = _mapper.Map<List<BookVM>>(books.Items);
            return new PagedList<BookVM>()
            {
                Page = page,
                TotalItems = books.TotalItems,
                ItemsPerPage = items,
                Items = booksVM
            };
        }

        [Authorize("Bearer")]
        [HttpGet("FullSearchAdmin/{criteria}")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public async Task<PagedList<Book>> FullSearchAdminAsync(string criteria, int page, int items)
        {
            var isAdmin = true;
            return await _service.FullSearchAsync(criteria, page, items, isAdmin);
        }

        [HttpGet("Category/{categoryId}/{page}/{items}")]
        public async Task<PagedList<BookVM>> ByCategoryIdAsync(Guid categoryId, int page, int items)
        {
            var booksPaged = await _service.ByCategoryIdAsync(categoryId, page, items);
            var books = booksPaged.Items;
            var booksVM = _mapper.Map<List<BookVM>>(books);

            return new PagedList<BookVM>()
            {
                Page = page,
                ItemsPerPage = items,
                TotalItems = booksPaged.TotalItems,
                Items = booksVM
            };
        }

        [Authorize("Bearer")]
        [HttpPost("Request")]
        [ProducesResponseType(typeof(Result), 200)]
        public async Task<IActionResult> RequestBookAsync([FromBody] RequestBookVM requestBookVM)
        {
            User user = await GetUserAsync();
            if (await _IsDonatorAsync(requestBookVM.BookId, user) && !_IsAdmin(user)) //Permitido solicitar o próprio livro somente para Admin
                throw new ShareBookException("Não é possivel solicitar esse livro pois você é o doador.");

            await _bookUserService.InsertAsync(requestBookVM.BookId, requestBookVM.Reason);
            return Ok(new Result { SuccessMessage = "Pedido realizado com sucesso!" });
        }

        [HttpPost("CancelRequest/{requestId}")]
        [Authorize("Bearer")]
        public async Task<IActionResult> CancelRequestAsync(Guid requestId)
        {
            var request = await _bookUserService.GetRequestAsync(requestId);

            if (request == null)
                return NotFound();

            var user = await GetUserAsync();

            if (request.UserId != user.Id)
                return Forbid();

            bool success = await _bookUserService.CancelRequestAsync(request);
            if (!success)
                return BadRequest();

            return Ok(new Result("Solicitação cancelada."));
        }

        [HttpPost]
        [Authorize("Bearer")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateBookVM createBookVM)
        {
            var book = _mapper.Map<Book>(createBookVM);
            var result = await _service.InsertAsync(book);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(new Result { SuccessMessage = "Livro cadastrado com sucesso! Aguarde aprovação." });
        }

        [HttpPut("{id}")]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public async Task<IActionResult> UpdateAsync(Guid Id, [FromBody] UpdateBookVM updateBookVM)
        {
            updateBookVM.Id = Id;
            var book = _mapper.Map<Book>(updateBookVM);
            var result = await _service.UpdateAsync(book);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(new Result { SuccessMessage = "Livro alterado com sucesso!" });
        }

        [Authorize("Bearer")]
        [HttpPut("Donate/{bookId}")]
        [ProducesResponseType(typeof(Result), 200)]
        public async Task<IActionResult> DonateBookAsync(Guid bookId, [FromBody] DonateBookUserVM donateBookUserVM)
        {
            if (!await _IsBookOwnerAsync(bookId)) return Unauthorized();

            await _bookUserService.DonateBookAsync(bookId, donateBookUserVM.UserId, donateBookUserVM.Note);

            var result = new Result
            {
                SuccessMessage = "Livro doado com sucesso!",
            };

            return Ok(result);
        }

        [Authorize("Bearer")]
        [HttpDelete("{id}")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public async Task<Result> DeleteAsync(Guid id) => await _service.DeleteAsync(id);

        [Authorize("Bearer")]
        [HttpGet("Requested/{bookId}")]
        public async Task<Result> RequestedAsync(Guid bookId)
        {
            var result = new Result
            {
                Value = new { bookRequested = await _service.UserRequestedBookAsync(bookId) },
            };

            return result;
        }

        [Authorize("Bearer")]
        [HttpGet("MyRequests/{page}/{items}")]
        public async Task<PagedList<MyBookRequestVM>> MyRequestsAsync(int page, int items)
        {
            var donation = await _bookUserService.GetRequestsByUserAsync(page, items);
            var myBooksRequestsVM = _mapper.Map<List<MyBookRequestVM>>(donation.Items);

            return new PagedList<MyBookRequestVM>()
            {
                Page = page,
                TotalItems = donation.TotalItems,
                ItemsPerPage = items,
                Items = myBooksRequestsVM
            };
        }

        [Authorize("Bearer")]
        [HttpGet("MyDonations")]
        public async Task<IList<BookVMAdm>> MyDonationsAsync()
        {
            Guid userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var donations = await _service.GetUserDonationsAsync(userId);
            return _mapper.Map<List<BookVMAdm>>(donations);
        }

        [Authorize("Bearer")]
        [ProducesResponseType(typeof(Result), 200)]
        [HttpPost("InformTrackingNumber/{bookId}")]
        public async Task<IActionResult> InformTrackingNumberAsync(Guid bookId, [FromBody] TrackinNumberBookVM trackingNumberBookVM)
        {
            await _bookUserService.InformTrackingNumberAsync(bookId, trackingNumberBookVM.TrackingNumber);
            return Ok();
        }

        [Authorize("Bearer")]
        [ProducesResponseType(typeof(Result), 200)]
        [HttpPost("AddFacilitatorNotes")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public async Task<IActionResult> AddFacilitatorNotesAsync([FromBody] AddFacilitatorNotesVM vm)
        {
            await _service.AddFacilitatorNotesAsync(vm.BookId, vm.FacilitatorNotes);
            return Ok();
        }

        [Authorize("Bearer")]
        [ProducesResponseType(typeof(MainUsersVM), 200)]
        [HttpGet("MainUsers/{bookId}")]
        public async Task<IActionResult> MainUsers(Guid bookId)
        {
            if (!await _IsBookMainUserAsync(bookId)) return Unauthorized();

            var book = await _service.GetBookWithAllUsersAsync(bookId);

            var donor = _mapper.Map<UserVM>(book.User);
            var facilitator = _mapper.Map<UserVM>(book.UserFacilitator);
            var winner = _mapper.Map<UserVM>(book.WinnerUser());

            var result = new MainUsersVM
            {
                Donor = donor,
                Facilitator = facilitator,
                Winner = winner
            };

            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var visitor = await _userService.FindAsync(userId);
            var visitorProfile = GetVisitorProfile(result);

            await _accessHistoryService.InsertVisitorAsync(book.User, visitor, visitorProfile);

            return Ok(result);

            VisitorProfile GetVisitorProfile(MainUsersVM mainUsers)
            {
                if (mainUsers is null) return VisitorProfile.Undefined;

                var facilitatorId = Guid.Empty;
                if (mainUsers.Facilitator is not null)
                {
                    facilitatorId = mainUsers.Facilitator.Id;
                }

                var winnerId = Guid.Empty;
                if (mainUsers.Winner is not null)
                {
                    winnerId = mainUsers.Winner.Id;
                }

                var donorId = Guid.Empty;
                if (mainUsers.Donor is not null)
                {
                    donorId = mainUsers.Donor.Id;
                }

                //O id do usuário logado é comparado com o doador, facilitador e ganhador 
                if (visitor.Id.Equals(facilitatorId)) return VisitorProfile.Facilitator;
                else if (visitor.Id.Equals(winnerId)) return VisitorProfile.Winner;
                else if (visitor.Id.Equals(donorId)) return VisitorProfile.Donor;
                else return VisitorProfile.Undefined;
            }
        }

        [Authorize("Bearer")]
        [HttpPut("RenewChooseDate/{bookId}")]
        public async Task<IActionResult> RenewChooseDateAsync(Guid bookId)
        {
            if (!await _IsBookOwnerAsync(bookId))
                return Unauthorized();

            await _service.RenewChooseDateAsync(bookId);
            return Ok();
        }

        // apenas doador e adm
        private async Task<bool> _IsBookOwnerAsync(Guid bookId)
        {
            User user = await GetUserAsync();
            if (user == null)
                return false;

            // Adm
            if (_IsAdmin(user)) return true;

            // Doador
            return await _IsDonatorAsync(bookId, user);
        }

        private async Task<bool> _IsDonatorAsync(Guid bookId, User user)
        {
            if (user == null || user.Id == Guid.Empty) return false;
            Book book = await _service.GetBookWithAllUsersAsync(bookId);
            if (book == null || book.Id == Guid.Empty) return false;

            return book.UserId == user.Id;
        }

        private async Task<User> GetUserAsync()
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return await _userService.FindAsync(userId);
        }

        private bool _IsAdmin(User user)
        {
            if (user == null || user?.Profile == null) return false;
            return user.Profile.Equals(Domain.Enums.Profile.Administrator);
        }

        // doador, adm e ganhador
        private async Task<bool> _IsBookMainUserAsync(Guid bookId)
        {
            if (await _IsBookOwnerAsync(bookId))
                return true;

            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var book = await _service.GetBookWithAllUsersAsync(bookId);

            // Ganhador
            var winner = book.WinnerUser();
            if (winner.Id == userId)
                return true;

            return false;
        }

        private async Task<User> GetSessionUserAsync()
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return await _userService.FindAsync(userId);
        }

        [HttpGet("DownloadEBook/{slug}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DownloadEBookAsync(string slug)
        {
            var book = await _service.BySlugAsync(slug);

            if (book == null)
                return NotFound(new { message = "Livro não encontrado." });

            if (!book.IsEbook())
                return BadRequest(new { message = "Este livro não é um e-book." });

            if (string.IsNullOrEmpty(book.EBookPdfPath))
                return NotFound(new { message = "PDF do e-book não disponível." });

            var pdfPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "wwwroot",
                "EbookPdfs",
                book.EBookPdfPath
            );

            if (!System.IO.File.Exists(pdfPath))
                return NotFound(new { message = "Arquivo PDF não encontrado." });

            var pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
            var fileName = book.GetPdfFileName();

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}