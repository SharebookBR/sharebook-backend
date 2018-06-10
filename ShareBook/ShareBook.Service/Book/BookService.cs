using System;
using System.Threading;
using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Helper.Image;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Authorization;
using ShareBook.Service.CustomExceptions;
using ShareBook.Service.Generic;
using ShareBook.Service.Upload;

namespace ShareBook.Service
{
    public class BookService : BaseService<Book>, IBookService
    {
        private readonly IUploadService _uploadService;

        public BookService(IBookRepository bookRepository, IUnitOfWork unitOfWork, IValidator<Book> validator, IUploadService uploadService) 
            : base(bookRepository, unitOfWork, validator)
        {
            _uploadService = uploadService;
        }

        [AuthorizationInterceptor(Permissions.Permission.AprovarLivro)]
        public Result<Book> Approve(Guid bookId)
        {
            var book = _repository.Get(bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            book.Approved = true;
            _repository.Update(book);

            return new Result<Book>(book);
        }

        public override Result<Book> Insert(Book entity)
        {
            entity.UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            var result = Validate(entity);
            if (result.Success)
            {
                var imageExtension = ImageHelper.GetExtension(entity.Image);

                var imageName = ImageHelper.FormatImageName(entity.Id.ToString(), imageExtension);

                _uploadService.UploadImage(entity.ImageBytes, imageName);
                result.Value = _repository.Insert(entity);
               
            }
                  
            return result;
        }


    }
}
