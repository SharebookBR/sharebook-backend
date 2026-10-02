using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers;

[Route("api/[controller]")]
public class TagController(ITagService tagService, IMapper mapper) : ControllerBase
{
    private readonly ITagService _tagService = tagService;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    [ProducesResponseType(typeof(IList<TagSummaryVM>), 200)]
    public async Task<IList<TagSummaryVM>> GetPublicTagsAsync()
    {
        var tags = await _tagService.GetPublicTagsAsync();
        var countsByTag = await _tagService.GetPublicBookCountsByTagAsync(tags.Select(tag => tag.Id));
        var viewModels = _mapper.Map<IList<TagSummaryVM>>(tags);

        foreach (var viewModel in viewModels)
        {
            viewModel.TotalBooks = countsByTag.TryGetValue(viewModel.Id, out var totalBooks)
                ? totalBooks
                : 0;
        }

        return viewModels;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TagVM), 200)]
    public async Task<IActionResult> GetPublicTagAsync(string id)
    {
        var tag = await _tagService.FindPublicAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<TagVM>(tag);
        var countsByTag = await _tagService.GetPublicBookCountsByTagAsync(new[] { tag.Id });
        viewModel.TotalBooks = countsByTag.TryGetValue(tag.Id, out var totalBooks)
            ? totalBooks
            : 0;
        return Ok(viewModel);
    }

    [HttpGet("{id}/Books/{page:int}/{items:int}")]
    [ProducesResponseType(typeof(PagedList<BookVM>), 200)]
    public async Task<PagedList<BookVM>> GetPublicBooksAsync(string id, int page, int items)
    {
        var books = await _tagService.GetPublicBooksAsync(id, page, items);
        return new PagedList<BookVM>
        {
            Page = books.Page,
            ItemsPerPage = books.ItemsPerPage,
            TotalItems = books.TotalItems,
            Items = _mapper.Map<List<BookVM>>(books.Items)
        };
    }

    [HttpGet("Admin")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    [ProducesResponseType(typeof(IList<TagVM>), 200)]
    public async Task<IList<TagVM>> GetAdminTagsAsync()
        => _mapper.Map<IList<TagVM>>(await _tagService.GetAdminTagsAsync());

    [HttpPost]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    [ProducesResponseType(typeof(TagVM), 200)]
    public async Task<TagVM> CreateAsync([FromBody] UpsertTagVM vm)
    {
        var tag = await _tagService.CreateAsync(_mapper.Map<Tag>(vm));
        return _mapper.Map<TagVM>(tag);
    }

    [HttpPut("{id}")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    [ProducesResponseType(typeof(TagVM), 200)]
    public async Task<TagVM> UpdateAsync(string id, [FromBody] UpsertTagVM vm)
    {
        var tag = await _tagService.UpdateAsync(id, _mapper.Map<Tag>(vm));
        return _mapper.Map<TagVM>(tag);
    }

    [HttpDelete("{id}")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    public async Task<IActionResult> DeprecateAsync(string id)
    {
        await _tagService.DeprecateAsync(id);
        return Ok();
    }

    [HttpGet("Book/{bookId:guid}")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    [ProducesResponseType(typeof(IList<TagSummaryVM>), 200)]
    public async Task<IList<TagSummaryVM>> GetBookTagsAsync(Guid bookId)
        => _mapper.Map<IList<TagSummaryVM>>((await _tagService.GetBookTagsAsync(bookId)).Select(bookTag => bookTag.Tag));

    [HttpPut("Book/{bookId:guid}")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    [ProducesResponseType(typeof(IList<TagSummaryVM>), 200)]
    public async Task<IList<TagSummaryVM>> SetBookTagsAsync(Guid bookId, [FromBody] UpdateBookTagsVM vm)
        => _mapper.Map<IList<TagSummaryVM>>((await _tagService.SetBookTagsAsync(bookId, vm.TagIds)).Select(bookTag => bookTag.Tag));
}
