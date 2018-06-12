using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Helper.Extensions;
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

        public List<ExpandoObject> GetAllFreightOptions()
        {
            return FillAndGetFreightOptionKeysAndValues();
        }

        public override Result<Book> Insert(Book entity)
        {
            entity.UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            var result = Validate(entity);
            if (result.Success)
            {
                entity.Image = ImageHelper.FormatImageName(entity.Image, entity.Id.ToString());

                _uploadService.UploadImage(entity.ImageBytes, entity.Image);
                result.Value = _repository.Insert(entity);
               
            }
                 
            return result;
        }

        #region Private 
        private List<string[]> FillAndGetFreightOptionValues()
        {
            List<string[]> enumValues = new List<string[]>();

            foreach (FreightOption freightOption in Enum.GetValues(typeof(FreightOption)))
            {
                enumValues.Add(new[] { freightOption.ToString(), freightOption.Description() });
            }

            return enumValues;
        }

        private List<ExpandoObject> FillAndGetFreightOptionKeysAndValues()
        {
            var enumValues = FillAndGetFreightOptionValues();
            var enumList = new List<ExpandoObject>();
            string[] keys = { "Value", "Text" };

            foreach (string[] enumValue in enumValues)
            {
                dynamic data = new ExpandoObject();
                for (int j = 0; j < keys.Count(); j++)
                {
                    ((IDictionary<String, Object>)data).Add(keys[j], enumValue[j]);
                }
                enumList.Add(data);
            }
            return enumList;
        }
        #endregion

    }
}
