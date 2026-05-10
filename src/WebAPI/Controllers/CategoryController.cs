using Core.Application.Interface;
using Core.Application.Interface.Repository.Sales;
using Core.Application.Model.Request;
using Infrastructure.Services.Sales.Reports;
using Infrastructure.Utils.DateUtils;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAPI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("category")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly ISalesRepository _salesRepository;

    public CategoryController(ICategoryService service, ISalesRepository salesRepository)
    {
        _service = service;
        _salesRepository = salesRepository;
    }
    [HttpPost("test", Name = "test")]
    public async Task<ActionResult<IEnumerable<bool>>> Tets()
    {
        DateTime dateStart = DateConverter.TodayHaitiToUTC(7);
        DateTime dateEnd = DateConverter.TodayHaitiToUTC(19);
        var startDate = new DateTime(2025, 6, 17, 0, 0, 0);
        var endDate = new DateTime(2025, 6, 17, 23, 59, 59);
        var result = await _salesRepository.GetSalesSummaryByDateRangeAsync(dateStart, dateEnd);
        QuestPDF.Settings.License = LicenseType.Community;
        var document = new DailySalesReportPDF(result);
        document.GeneratePdf("rapport-ventes-aurabe.pdf");
        return Ok(true);
    }

    [HttpPost("GetAll", Name ="GetAllCategory")]
    public async Task<ActionResult<IEnumerable<CreateCategoryRequest>>> GetProducts()
    {
        var category = await _service.GetCategoryAsync();
        return Ok(category);
    }

    [HttpPost("Create", Name = "CreateCategory")]
    public async Task<ActionResult<int>> CreateCategory(List<CreateCategoryRequest> categories)
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
        foreach (var category in categories) {
            var result = await _service.CreateCategoryAsync(category);
            
           
        }
        return Ok();
    }

}
