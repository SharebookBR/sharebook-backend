using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class ComplaintController : ControllerBase
    {
        private readonly IEbookComplaintService _ebookComplaintService;
        private readonly IMapper _mapper;

        public ComplaintController(IEbookComplaintService ebookComplaintService, IMapper mapper)
        {
            _ebookComplaintService = ebookComplaintService;
            _mapper = mapper;
        }

        [Authorize("Bearer")]
        [HttpPost("ebook/{bookId}")]
        public IActionResult EbookComplaint(Guid bookId, [FromQuery]string reasonMessage)
        {
            var vm = new EbookComplaintVM()
            {
                BookId = bookId,
                ReasonMessage = reasonMessage
            };
            _ebookComplaintService.Insert(_mapper.Map<EbookComplaint>(vm));
            return Ok(new Result("Denúncia registrada com sucesso!"));
        }
    }
}
