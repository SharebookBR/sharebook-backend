using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class EbookController : ControllerBase
    {
        private readonly IEbookComplaintService _ebookComplaintService;

        public EbookController(IEbookComplaintService ebookComplaintService)
        {
            _ebookComplaintService = ebookComplaintService;
        }

        [Authorize("Bearer")]
        [HttpPost("complaint/{bookId}")]
        public IActionResult ComplaintEbook(Guid bookId)
        {
            _ebookComplaintService.Insert(new EbookComplaint(bookId));
            return Ok(new Result("Denúncia registrada com sucesso!"));
        }
    }
}
