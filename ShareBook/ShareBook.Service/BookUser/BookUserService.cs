using System;
using System.Linq.Expressions;
using System.Threading;
using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public class BookUserService :  IBookUserService
    {

        private readonly IBookUserRepository _bookUserRepository;
        public BookUserService(IBookUserRepository bookUserRepository, IUnitOfWork unitOfWork)            
        {
            _bookUserRepository = bookUserRepository;
        }

        public void Insert(BookUser entity)
        {
            entity.UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            _bookUserRepository.Insert(entity);
        }
    }
}
