using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.AwsSqs.Dto
{
    public class MailSenderbody
    {
        public string Subject { get; set; }
        public string BodyHTML { get; set; }
        public IList<Destination> Destinations { get; set; }
        public bool CopyAdmins { get; set; } = false;
    }

    public class Destination
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}