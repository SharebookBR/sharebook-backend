using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.AwsSqs.Dto
{
    public class SharebookMessage<T>{
        public string ReceiptHandle { get; set; }
        public T Body { get; set; }
    }
}