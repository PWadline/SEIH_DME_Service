using AutoMapper;
using Core.Application.Commons.ServiceResult;
using Core.Application.Interface;
using Core.Application.Interface.Services.Sales;
using Core.Application.Model.Request.Sales;
using Core.Application.Model.Response.Sales;
using Core.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Sales
{
    public class CreateProductsSalesService : ICreateProductsSalesService
    {
        private readonly IRepository<ProductSalesEntity> _repository;
        private IMapper _mapper;

        public CreateProductsSalesService(IRepository<ProductSalesEntity> repository,
            IMapper mapper)
        {

            _repository = repository;
            _mapper = mapper;
        }
        public Task<ServiceResult<IEnumerable<ProductSalesResponse>>> CreateProductsSalesAsync(List<CreateProductSalesRequest> requests)
        {
            throw new NotImplementedException();
        }
    }
}
