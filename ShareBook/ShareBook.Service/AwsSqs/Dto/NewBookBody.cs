using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.AwsSqs.Dto
{
    public class NewBookBody{

        public Guid BookId { get; set; }
        public string BookTitle { get; set; }
        public Guid CategoryId { get; set; }

        public string ReceiptHandle { get; set; }
    }
}