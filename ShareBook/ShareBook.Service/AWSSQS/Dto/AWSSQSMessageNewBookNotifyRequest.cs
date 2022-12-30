﻿using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.AWSSQS.Dto
{
    public class SendEmailRequest
    {
        public string Subject { get; set; }
        public string BodyHTML { get; set; }
        public IList<Destination> Destinations { get; set; }
    }

    public class Destination
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class GetInterestedUsersRequest{
        public Guid BookId { get; set; }
        public string BookTitle { get; set; }
        public Guid CategoryId { get; set; }
    }
}