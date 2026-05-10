
using Core.Application.Interface;
using Core.Application.Interface.Token;
using Core.Application.Model.Request.Product;
using Core.Domain.Entity;
using Infrastructure.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApi.Controllers.Base;
using WebApi.Filters;

namespace WebAPI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("products")]
[ApiController]
public class ProductController : AuthorizeBaseController
{
    private readonly IProductService _service;
    private readonly IProductUpdateService _updateService;

    public ProductController(
            IProductService service, 
            IProductUpdateService updateService, 
            ITokenServices tokenServices) : base(tokenServices)
    {
        _service = service;
        _updateService = updateService; 
    }

    [HttpPost]
    [AuthorizeRoles]
    [Route("GetProductList", Name = "GetProductsAsync")]
    public async Task<ActionResult<IEnumerable<ProductEntity>>> GetProductsAsync()
    {

        var products = await _service.GetProductsAsync();
        return Ok(products);
    }

    [HttpPost]
    [Route("GetProductListPagination", Name = "GetProductsAsyncPagination")]
    public async Task<ActionResult<IEnumerable<ProductEntity>>> GetProductsPaginationAsync(int page = 1, int pageSize = 10)
    {
        var response = await _service.GetProductsWithPaginationAsync(page, pageSize);
        return Ok(response);
    }


    [HttpPost]
    [Route("GetProductByID", Name = "GetProductById")]
    public async Task<ActionResult<ProductEntity>> GetProductById([FromBody] GetProductRequest request)
    {
        var product = await _service.GetProductByIdAsync(request.value);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
    [HttpPost]
    [AuthorizeRoles]
    [Route("GetProductByBarCode", Name = "GetProductByBarCode")]
    public async Task<ActionResult<ProductEntity>> GetProductByBarCode([FromBody] GetProductRequest request)
    {
        var product = await _service.GetProductByBarCodeAsync(request.value);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
    [HttpPost]
    [AuthorizeRoles("Admin")]
    public async Task<ActionResult<HttpStatusCode>> CreateProduct(List<ProductRequest> products)
    {
        if (!ModelState.IsValid)
        {
            // Extract error messages from ModelState
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // Return BadRequest with error messages
            return BadRequest(new { Errors = errorMessages });
        }
        var result = await _service.CreateProductsAsync(products);
        if (result.IsOk)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [AuthorizeRoles("Admin")]
    [Route("updateProduct", Name = "UpdateProduct")]
    [HttpPost]
    public async Task<IActionResult> UpdateProduct(ProductRequest product)
    {
        if (!ModelState.IsValid)
        {
            // Extract error messages from ModelState
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // Return BadRequest with error messages
            return BadRequest(new { Errors = errorMessages });
        }

      var result =  await _updateService.UpdateProductsAsync(product);
         if (result.IsOk)
        {
             return Ok(result);
        }

        return BadRequest(result);
    }

    [AuthorizeRoles("Admin")]
    [HttpPost]
    [Route("updateProductList", Name = "UpdateProducts")]
    public async Task<IActionResult> UpdateProducts(List<ProductRequest> products)
    {
        if (!ModelState.IsValid)
        {
            // Extract error messages from ModelState
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // Return BadRequest with error messages
            return BadRequest(new { Errors = errorMessages });
        }

        var result = await _updateService.UpdateProductsAsync(products);
        if (result.IsOk)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [AuthorizeRoles("Admin")]
    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        await _service.DeleteProductAsync(productId);
        return NoContent();
    }

    [HttpPost("suggestions/get")]
    [AuthorizeRoles]
    public async Task<IActionResult> GetInventorySuggestionsAsync([FromQuery] string userInput)
    {
        try
        {
            var inventorySuggestions = await _service.GetProductSuggestions(userInput);
            if (inventorySuggestions == null || !inventorySuggestions.Any())
            {
                return NotFound(new { Errors = "Error C1000" });
            }

            return Ok(inventorySuggestions);
        }
        catch (Exception ex)
        {
            {
               // return NotFound();
               return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

    }
}

