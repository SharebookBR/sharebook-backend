using ShareBook.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUsersEmailService
    {
        Task SendEmailBookRequested(BookUser bookUser);
    }
}
