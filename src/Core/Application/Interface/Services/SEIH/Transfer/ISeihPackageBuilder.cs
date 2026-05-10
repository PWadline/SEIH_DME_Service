using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using Core.Application.Model.Features.Hospital;
using Core.Domain.Entity;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Hospital;

public interface ISeihPackageBuilder
{
     SeihTransferPackage Build(
        HospitalBasicInfo source,
        HospitalBasicInfo target,
        RecordEntity record);
}
