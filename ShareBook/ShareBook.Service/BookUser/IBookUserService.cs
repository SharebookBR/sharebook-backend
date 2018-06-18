using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;


namespace ShareBook.Service
{
    public interface IBookUserService 
    {
        void Insert(Guid idBook);
    }
}
