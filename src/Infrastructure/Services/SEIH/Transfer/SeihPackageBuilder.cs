// using Core.Application.Commons.ServiceResult;
// using Core.Application.Interface.Repository.SEIH;
// using Core.Application.Interface.Repository.SEIH.Hospital;
// using Core.Application.Interface.Services.SEIH.Hospital;
// using Core.Application.Model.Features;
// using Core.Application.Model.Features.Hospital;
// using Core.Domain.Entity;
// using Core.Domain.Entity.SEIH;
// using System.Net;
// using System.Security.Claims;
// using System.Security.Cryptography;
// using System.Text;

// namespace Infrastructure.Services.SEIH.Transfer;

// public class SeihPackageBuilder : ISeihPackageBuilder
// {
//     public SeihTransferPackage Build(
//         HospitalBasicInfo source,
//         HospitalBasicInfo target,
//         RecordEntity record)
//     {
//         var package = new SeihTransferPackage
//         {
//             TransferId = Guid.NewGuid().ToString(),
//             Timestamp = DateTime.UtcNow.ToString("O"),

//             HospitalSource = new HospitalInfo
//             {
//                 Name = source.Name,
//                 Code = source.Code ?? string.Empty
//             },

//             HospitalTarget = new HospitalInfo
//             {
//                 Name = target.Name,
//                 Code = target.Code ?? string.Empty
//             },

//             PatientReference = record.PatientReference ?? record.Id.ToString(),

//             // TODO: remplacer par le vrai hash du consentement
//             ConsentHash = "TODO_CONSENT_HASH"
//         };

//         var fields = record.FieldValues ?? new List<RecordFieldValueEntity>();

//         package.Sections = fields
//             .GroupBy(f => f.Section ?? "Général")
//             .Select(group => new SeihSection
//             {
//                 Label = group.Key,
//                 Fields = group
//                     .OrderBy(f => f.Position)
//                     .Select(field => new SeihField
//                     {
//                         Label = field.FieldLabel ?? string.Empty,
//                         Type = field.FieldType ?? "text",
//                         Value = field.Value
//                     })
//                     .ToList()
//             })
//             .ToList();

//         return package;
//     }
// }





//Changement x

using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Repository.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Model.Features;
using Core.Application.Model.Features.Hospital;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.SEIH.Transfer;

public class SeihPackageBuilder : ISeihPackageBuilder
{
    public SeihTransferPackage Build(
        HospitalBasicInfo source,
        HospitalBasicInfo target,
        RecordEntity record)
    {
        var package = new SeihTransferPackage
        {
            TransferId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow.ToString("O"),

            HospitalSource = new HospitalInfo
            {
                Name = source.Name,
                Code = source.Code ?? string.Empty
            },

            HospitalTarget = new HospitalInfo
            {
                Name = target.Name,
                Code = target.Code ?? string.Empty
            },

            PatientReference = record.PatientReference ?? record.Id.ToString(),

            // TODO: remplacer par le vrai hash du consentement
            ConsentHash = "TODO_CONSENT_HASH"
        };

        var fields = record.FieldValues ?? new List<RecordFieldValueEntity>();

        package.Sections = fields

            .GroupBy(f => f.Section ?? "Général")
            .Select(group => new SeihSection
            {
                Label = group.Key,
                Fields = group
                    .OrderBy(f => f.Position)
                  .Select(field => new SeihField
                  {
                      Label = field.FieldLabel ?? string.Empty,
                      Type = field.FieldType ?? "text",
                      Value = field.Value,

                      FileName = field.FileId != null
                        ? $"{field.FieldLabel}_{field.FileId}"
                        : null,

                      FilePath = field.FileId != null
                        ? $"files/{field.FileId}"
                        : null
                  })
                    .ToList()
            })
                    .ToList();

       package.Files = fields
    .Where(f => f.FileId != null)
    .Select(f => new SeihFileMeta
    {
        StoredName = $"{f.FileId}",

        OriginalName = f.FieldLabel ?? "file",

        MimeType = f.FieldType switch
        {
            "image" => "image/*",
            "video" => "video/*",
            "audio" => "audio/*",
            "document" => "application/octet-stream",
            _ => "application/octet-stream"
        },

        Size = 0,
        Hash = ""
    })
    .ToList();

        return package;
    }
}