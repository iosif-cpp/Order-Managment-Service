using AutoMapper;
using CatalogService.Application.Categories;
using CatalogService.Application.Categories.Commands.CreateCategory;
using CatalogService.Application.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CategoryController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateCategoryCommand>(request);

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetAll), null, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

