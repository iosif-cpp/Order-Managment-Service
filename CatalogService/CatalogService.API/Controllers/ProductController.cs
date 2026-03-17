using AutoMapper;
using CatalogService.Application.Products;
using CatalogService.Application.Products.Commands.CreateProduct;
using CatalogService.Application.Products.Commands.UpdateProduct;
using CatalogService.Application.Products.Queries.GetProductById;
using CatalogService.Application.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ProductController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateProductCommand>(request);

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<UpdateProductCommand>(request) with
        {
            Id = id
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

