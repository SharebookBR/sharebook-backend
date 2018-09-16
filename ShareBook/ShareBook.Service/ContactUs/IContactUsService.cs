using ShareBook.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service
{
    public interface IContactUsService
    {
        void SendContactUs(ContactUs contactUs);
    }
}
