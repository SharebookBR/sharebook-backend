using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Repository.Repository;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Util;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;

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
        private Expression<Func<Book, object>> _defaultOrder = x => x.Id;
        private readonly IMapper _mapper;

        public BookController(IBookService bookService,
                              IBookUserService bookUserService,
                              IUserService userService,
                              IMapper mapper,
                              IAccessHistoryService accessHistoryService)
        {
            _service = bookService;
            _bookUserService = bookUserService;
            _userService = userService;
            _mapper = mapper;
            _accessHistoryService = accessHistoryService;
        }

        protected void SetDefault(Expression<Func<Book, object>> defaultOrder)
        {
            _defaultOrder = defaultOrder;
        }

        [HttpGet()]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public PagedList<BookVMAdm> GetAll() => Paged(1, 15);

        [HttpGet("{page}/{items}")]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public PagedList<BookVMAdm> Paged(int page, int items)
        {
            // TODO: parar de usar esse get complicado e fazer uma query linq/ef tradicional usando
            // ThenInclude(). fonte: https://stackoverflow.com/questions/10822656/entity-framework-include-multiple-levels-of-properties
            var books = _service.Get(x => x.Title, page, items, new IncludeList<Book>(x => x.User, x => x.BookUsers, x => x.UserFacilitator));
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
        public Result Approve(string id, [FromBody] ApproveBookVM model)
        {
            _service.Approve(new Guid(id), model?.ChooseDate);
            return new Result("Livro aprovado com sucesso.");
        }

        [Authorize("Bearer")]
        [HttpPost("Received/{bookId}")]
        public Result Received(string bookId)
        {
            Guid winnerUserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            _service.Received(new Guid(bookId), winnerUserId);
            return new Result("Livro Recebido com sucesso.");
        }

        [Authorize("Bearer")]
        [HttpPost("Cancel/{id}")]
        [ProducesResponseType(typeof(Result<CancelBookDonationVM>), 200)]
        public IActionResult Cancel(string id, [FromQuery] string reason = "")
        {
            if (!_IsBookOwner(new Guid(id))) return Unauthorized();

            var cancelationDTO = new BookCancelationDTO
            {
                Book = _service.Find(new Guid(id)),
                CanceledBy = GetSessionUser().Name,
                Reason = reason
            };

            var returnBook = _bookUserService.Cancel(cancelationDTO).Value;
            var returnBookVm = _mapper.Map<CancelBookDonationVM>(returnBook);
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
        public IList<User> GetGranteeUsersByBookId(string bookId) => _bookUserService.GetGranteeUsersByBookId(new Guid(bookId));

        [Authorize("Bearer")]
        [HttpGet("RequestersList/{bookId}")]
        public IActionResult GetRequestersList(Guid bookId)
        {
            if (!_IsBookOwner(bookId)) return Unauthorized();

            var requesters = _bookUserService.GetRequestersList(bookId);
            var requestersVM = _mapper.Map<List<RequestersListVM>>(requesters);

            return Ok(requestersVM);
        }

        [HttpGet("Slug/{slug}")]
        [ProducesResponseType(typeof(BookVM), 200)]
        public IActionResult Get(string slug)
        {
            var book = _service.BySlug(slug);
            var bookVM = _mapper.Map<BookVM>(book);
            return book != null ? (IActionResult)Ok(bookVM) : NotFound();
        }

        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)] // apenas adms
        [ProducesResponseType(typeof(BookVM), 200)]
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var book = _service.Find(new Guid(id));
            var bookVM = _mapper.Map<BookVMAdm>(book);
            return bookVM != null ? (IActionResult)Ok(bookVM) : NotFound();
        }

        [HttpGet("AvailableBooks")]
        public IList<BookVM> AvailableBooks()
        {
            var books = _service.AvailableBooks();
            return _mapper.Map<List<BookVM>>(books);
        }

        [HttpGet("Random15Books")]
        public IList<BookVM> Random15Books()
        {
            var books = _service.Random15Books();
            return _mapper.Map<List<BookVM>>(books);
        }

        [HttpGet("Random15EBooks")]
        public IList<BookVM> Random15EBooks()
        {
            var books = _service.Random15EBooks();
            return _mapper.Map<List<BookVM>>(books);
        }

        [HttpGet("FullSearch/{criteria}/{page}/{items}")]
        public PagedList<BookVM> FullSearch(string criteria, int page, int items)
        {
            var books = _service.FullSearch(criteria, page, items);
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
        public PagedList<Book> FullSearchAdmin(string criteria, int page, int items)
        {
            var isAdmin = true;
            return _service.FullSearch(criteria, page, items, isAdmin);
        }

        [HttpGet("Category/{categoryId}/{page}/{items}")]
        public PagedList<BookVM> ByCategoryId(Guid categoryId, int page, int items)
        {
            var booksPaged = _service.ByCategoryId(categoryId, page, items);
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
        public IActionResult RequestBook([FromBody] RequestBookVM requestBookVM)
        {
            User user = GetUser();
            if (_IsDonator(requestBookVM.BookId, user) && !_IsAdmin(user)) //Permitido solicitar o próprio livro somente para Admin
                throw new ShareBookException("Não é possivel solicitar esse livro pois você é o doador.");

            _bookUserService.Insert(requestBookVM.BookId, requestBookVM.Reason);
            return Ok(new Result { SuccessMessage = "Pedido realizado com sucesso!" });
        }

        [HttpPost("CancelRequest/{requestId}")]
        [Authorize("Bearer")]
        public IActionResult CancelRequest(Guid requestId)
        {
            var request = _bookUserService.GetRequest(requestId);

            if (request == null)
                return NotFound();

            var user = GetUser();

            if (request.UserId != user.Id)
                return Forbid();

            bool success = _bookUserService.CancelRequest(request);
            if (!success)
                return BadRequest();

            return Ok(new Result("Solicitação cancelada."));
        }

        [HttpPost]
        [Authorize("Bearer")]
        public IActionResult Create([FromBody] CreateBookVM createBookVM)
        {
            var book = _mapper.Map<Book>(createBookVM);
            var result = _service.Insert(book);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(new Result { SuccessMessage = "Livro cadastrado com sucesso! Aguarde aprovação." });
        }

        [HttpPut("{id}")]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public IActionResult Update(Guid Id, [FromBody] UpdateBookVM updateBookVM)
        {
            updateBookVM.Id = Id;
            var book = _mapper.Map<Book>(updateBookVM);
            var result = _service.Update(book);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(new Result { SuccessMessage = "Livro alterado com sucesso!" });
        }

        [Authorize("Bearer")]
        [HttpPut("Donate/{bookId}")]
        [ProducesResponseType(typeof(Result), 200)]
        public IActionResult DonateBook(Guid bookId, [FromBody] DonateBookUserVM donateBookUserVM)
        {
            if (!_IsBookOwner(bookId)) return Unauthorized();

            _bookUserService.DonateBook(bookId, donateBookUserVM.UserId, donateBookUserVM.Note);

            var result = new Result
            {
                SuccessMessage = "Livro doado com sucesso!",
            };

            return Ok(result);
        }

        [Authorize("Bearer")]
        [HttpDelete("{id}")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public Result Delete(Guid id) => _service.Delete(id);

        [Authorize("Bearer")]
        [HttpGet("Requested/{bookId}")]
        public Result Requested(Guid bookId)
        {
            var result = new Result
            {
                Value = new { bookRequested = _service.UserRequestedBook(bookId) },
            };

            return result;
        }

        [Authorize("Bearer")]
        [HttpGet("MyRequests/{page}/{items}")]
        public PagedList<MyBookRequestVM> MyRequests(int page, int items)
        {
            var donation = _bookUserService.GetRequestsByUser(page, items);
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
        public IList<BookVMAdm> MyDonations()
        {
            Guid userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var donations = _service.GetUserDonations(userId);
            return _mapper.Map<List<BookVMAdm>>(donations);
        }

        [Authorize("Bearer")]
        [ProducesResponseType(typeof(Result), 200)]
        [HttpPost("InformTrackingNumber/{bookId}")]
        public IActionResult InformTrackingNumber(Guid bookId, [FromBody] TrackinNumberBookVM trackingNumberBookVM)
        {
            _bookUserService.InformTrackingNumber(bookId, trackingNumberBookVM.TrackingNumber);
            return Ok();
        }

        [Authorize("Bearer")]
        [ProducesResponseType(typeof(Result), 200)]
        [HttpPost("AddFacilitatorNotes")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public IActionResult AddFacilitatorNotes([FromBody] AddFacilitatorNotesVM vm)
        {
            _service.AddFacilitatorNotes(vm.BookId, vm.FacilitatorNotes);
            return Ok();
        }

        [Authorize("Bearer")]
        [ProducesResponseType(typeof(MainUsersVM), 200)]
        [HttpGet("MainUsers/{bookId}")]
        public async Task<IActionResult> MainUsers(Guid bookId)
        {
            if (!_IsBookMainUser(bookId)) return Unauthorized();

            var book = _service.GetBookWithAllUsers(bookId);

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
            var visitor = _userService.Find(userId);
            var visitorProfile = GetVisitorProfile(result);

            await _accessHistoryService.InsertVisitor(book.User, visitor, visitorProfile);

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
        public IActionResult RenewChooseDate(Guid bookId)
        {
            if (!_IsBookOwner(bookId))
                return Unauthorized();

            _service.RenewChooseDate(bookId);
            return Ok();
        }

        // apenas doador e adm
        private bool _IsBookOwner(Guid bookId)
        {
            User user = GetUser();
            if (user == null)
                return false;

            // Adm
            if (_IsAdmin(user)) return true;

            // Doador
            return _IsDonator(bookId, user);
        }

        private bool _IsDonator(Guid bookId, User user)
        {
            if (user == null || user.Id == Guid.Empty) return false;
            Book book = _service.GetBookWithAllUsers(bookId);
            if (book == null || book.Id == Guid.Empty) return false;

            return book.UserId == user.Id;
        }

        private User GetUser()
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return _userService.Find(userId);
        }

        private bool _IsAdmin(User user)
        {
            if (user == null || user?.Profile == null) return false;
            return user.Profile.Equals(Domain.Enums.Profile.Administrator);
        }

        // doador, adm e ganhador
        private bool _IsBookMainUser(Guid bookId)
        {
            if (_IsBookOwner(bookId))
                return true;

            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var book = _service.GetBookWithAllUsers(bookId);

            // Ganhador
            var winner = book.WinnerUser();
            if (winner.Id == userId)
                return true;

            return false;
        }

        private User GetSessionUser()
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return _userService.Find(userId);
        }
    }
}