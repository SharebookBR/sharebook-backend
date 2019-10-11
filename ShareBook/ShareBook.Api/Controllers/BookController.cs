using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShareBook.Helper;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BookController : Controller
    {
        private readonly IBookUserService _bookUserService;
        private readonly IBookService _service;
        private readonly IUserService _userService;
        private Expression<Func<Book, object>> _defaultOrder = x => x.Id;
        public BookController(IBookService bookService, IBookUserService bookUserService, IUserService userService)
        {
            _service = bookService;
            _bookUserService = bookUserService;
            _userService = userService;
        }

        protected void SetDefault(Expression<Func<Book, object>> defaultOrder)
        {
            _defaultOrder = defaultOrder;
        }

        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            var result = new
            {
                ServerNow = DateTime.Now,
                SaoPauloNow = DateTimeHelper.ConvertDateTimeSaoPaulo(DateTime.Now),
                ServerToday = DateTime.Today,
                SaoPauloToday = DateTimeHelper.GetTodaySaoPaulo(),
                Message = "Pong!"
            };
            return Ok(result);
        }

        [HttpGet()]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public PagedList<BooksVM> GetAll() => Paged(1, 15);

        [HttpGet("{page}/{items}")]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public PagedList<BooksVM> Paged(int page, int items)
        {
            // TODO: parar de usar esse get complicado e fazer uma query linq/ef tradicional
            // usando ThenInclude(). fonte: https://stackoverflow.com/questions/10822656/entity-framework-include-multiple-levels-of-properties
            var books = _service.Get(x => x.Title, page, items, new IncludeList<Book>(x => x.User, x => x.BookUsers, x => x.UserFacilitator));
            var responseVM = Mapper.Map<List<BooksVM>>(books.Items);
            return new PagedList<BooksVM>()
            {
                Page = page,
                TotalItems = books.TotalItems,
                ItemsPerPage = items,
                Items = responseVM
            };
        }

        [HttpGet("{id}")]
        public Book GetById(string id) => _service.Find(new Guid(id));

        [Authorize("Bearer")]
        [HttpPost("Approve/{id}")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public Result<Book> Approve(string id, [FromBody] ApproveBookVM model) => _service.Approve(new Guid(id), model?.ChooseDate);

        [Authorize("Bearer")]
        [HttpPost("Cancel/{id}")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public Result<Book> CancelAdmin(string id) => _bookUserService.Cancel(new Guid(id), true);

        [HttpGet("FreightOptions")]
        public IList<dynamic> FreightOptions()
        {
            var freightOptions = _service.FreightOptions();
            return freightOptions;
        }

        // TODO: método deprecado. Remover depois que todos usarem o novo 'RequestersList'.
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
            var requestersVM = Mapper.Map<List<RequestersListVM>>(requesters);

            return Ok(requestersVM);
        }

        [HttpGet("Slug/{slug}")]
        public IActionResult Get(string slug)
        {
            var book = _service.BySlug(slug);
            return book != null ? (IActionResult)Ok(book) : NotFound();
        }

        [HttpGet("Top15NewBooks")]
        public IList<Book> Top15NewBooks() => _service.Top15NewBooks();

        [HttpGet("Random15Books")]
        public IList<Book> Random15Books() => _service.Random15Books();

        [Authorize("Bearer")]
        [HttpGet("Title/{title}/{page}/{items}")]
        public PagedList<Book> ByTitle(string title, int page, int items) => _service.ByTitle(title, page, items);

        [Authorize("Bearer")]
        [HttpGet("Author/{author}/{page}/{items}")]
        public PagedList<Book> ByAuthor(string author, int page, int items) => _service.ByAuthor(author, page, items);

        [HttpGet("FullSearch/{criteria}/{page}/{items}")]
        public PagedList<Book> FullSearch(string criteria, int page, int items) => _service.FullSearch(criteria, page, items);

        [Authorize("Bearer")]
        [HttpGet("FullSearchAdmin/{criteria}")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public PagedList<Book> FullSearchAdmin(string criteria, int page, int items)
        {
            var isAdmin = true;
            return _service.FullSearch(criteria, page, items, isAdmin);
        }

        [HttpGet("Category/{categoryId}/{page}/{items}")]
        public PagedList<Book> ByCategoryId(Guid categoryId, int page, int items) => _service.ByCategoryId(categoryId, page, items);

        [Authorize("Bearer")]
        [ProducesResponseType(typeof(Result), 200)]
        [HttpPost("Request")]
        public IActionResult RequestBook([FromBody] RequestBookVM requestBookVM)
        {
            _bookUserService.Insert(requestBookVM.BookId, requestBookVM.Reason);
            return Ok(new Result { SuccessMessage = "Pedido realizado com sucesso!" });
        }

        [Authorize("Bearer")]
        [HttpPost]
        public IActionResult Create([FromBody] CreateBookVM createBookVM)
        {
            var book = Mapper.Map<Book>(createBookVM);
            _service.Insert(book);
            return Ok(new Result { SuccessMessage = "Livro cadastrado com sucesso! Aguarde aprovação." });
        }

        [Authorize("Bearer")]
        [HttpPut("{id}")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public IActionResult Update(Guid id, [FromBody] UpdateBookVM updateBookVM)
        {
            updateBookVM.Id = id;
            var book = Mapper.Map<Book>(updateBookVM);

            _service.Update(book);
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
            var myBooksRequestsVM = Mapper.Map<List<MyBookRequestVM>>(donation.Items);

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
        public IList<BooksVM> MyDonations()
        {
            Guid userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var donations = _service.GetUserDonations(userId);
            return Mapper.Map<List<BooksVM>>(donations);
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
        public IActionResult MainUsers(Guid bookId)
        {
            if (!_IsBookOwner(bookId)) return Unauthorized();

            var book = _service.GetBookWithAllUsers(bookId);

            var donor = Mapper.Map<UserVM>(book.User);
            var facilitator = Mapper.Map<UserVM>(book.UserFacilitator);
            var winner = Mapper.Map<UserVM>(book.WinnerUser());

            var result = new MainUsersVM
            {
                Donor = donor,
                Facilitator = facilitator,
                Winner = winner
            };

            return Ok(result);
        }

        [Authorize("Bearer")]
        [HttpPut("RenewChooseDate/{bookId}")]
        public IActionResult RenewChooseDate(Guid bookId)
        {
            if (!_IsBookOwner(bookId)) return Unauthorized();

            _service.RenewChooseDate(bookId);
            return Ok();
        }

        private bool _IsBookOwner(Guid bookId)
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var user = _userService.Find(userId);
            if (user == null) return false;
            if (user.Profile == Domain.Enums.Profile.Administrator) return true;

            var book = _service.Find(bookId);
            return book.UserId == userId;
        }
    }
}
