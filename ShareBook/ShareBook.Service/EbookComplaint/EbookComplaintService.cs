using FluentValidation;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class EbookComplaintService  : BaseService<EbookComplaint>, IEbookComplaintService
    {
        private readonly IEbookComplaintRepository _ebookComplaintRepository;
        private readonly IBookService _bookService;

        public EbookComplaintService(IEbookComplaintRepository ebookComplaintRepository, IBookService bookService, IUnitOfWork unitOfWork, IValidator<EbookComplaint> validator) : base(ebookComplaintRepository, unitOfWork, validator)
        {
            _ebookComplaintRepository = ebookComplaintRepository;
            _bookService = bookService;
        }

        public override Result<EbookComplaint> Insert(EbookComplaint entity)
        {
            entity.UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            entity.Book = _bookService.Find(b => b.Id == entity.BookId);

            var result = Validate(entity);
            if (result.Success)
            {
                _ebookComplaintRepository.Insert(entity);
                _bookService.RevokeBookToWaitingApproval(entity.BookId);
            }
            return result;
        }
    }
}