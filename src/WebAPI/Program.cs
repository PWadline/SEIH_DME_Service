using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Application.Abstractions;
using Core.Application.Interface.Security;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.Transfer;
using DotNetEnv;
using Infrastructure;
using Infrastructure.Constants;
using Infrastructure.External;
using Infrastructure.Security.Permission;
using Infrastructure.Services.SEIH;
using Infrastructure.Services.SEIH.Background;
using Infrastructure.Services.SEIH.Hospital;
using Infrastructure.Services.SEIH.Transfer;
using Infrastructure.Services.Transfer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524288000; // 500 MB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000; // 500 MB
});
Console.WriteLine("===== CONFIG DEBUG =====");

foreach (var kv in builder.Configuration.AsEnumerable())
{
    if (kv.Key.Contains("ApiKey"))
        Console.WriteLine($"{kv.Key} = {kv.Value}");
}

Console.WriteLine("========================");

Console.WriteLine("====== SEIH CONFIG DEBUG ======");
Console.WriteLine("HOSPITAL ID = " + builder.Configuration["SeihSettings:HospitalId"]);
Console.WriteLine("API KEY = " + builder.Configuration["SEIH:ApiKey"]);
Console.WriteLine("PRIVATE KEY = " + builder.Configuration["SEIH:AuthKey:PrivateKeyPath"]);
Console.WriteLine("ENVIRONMENT = " + builder.Environment.EnvironmentName);
Console.WriteLine("================================");

Env.Load();
builder.Services.AddHttpClient<TransfertApiClient>(client =>
{
    var configuration = builder.Configuration;

    var baseUrl = configuration["TransferSettings:BaseUrl"];
    var apiKey = configuration["SEIH:ApiKey"];

    client.BaseAddress = new Uri(baseUrl!);

})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    var configuration = builder.Configuration;

    var certPath = configuration["SEIH:ClientCert:Path"];
    var certPassword = configuration["SEIH:ClientCert:Password"];

    var fullPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        certPath!
    );

    var certificate = new X509Certificate2(
        fullPath,
        certPassword,
        X509KeyStorageFlags.MachineKeySet |
        X509KeyStorageFlags.PersistKeySet
    );

    handler.ClientCertificates.Add(certificate);

    return handler;
});


builder.Services.AddHttpClient<ISeihTransferClient, HttpSeihTransferClient>(client =>
{
    var configuration = builder.Configuration;
    var baseUrl = configuration["TransferSettings:BaseUrl"];

    client.BaseAddress = new Uri(baseUrl!);
    client.Timeout = TimeSpan.FromMinutes(10);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    var configuration = builder.Configuration;

    var certPath = configuration["SEIH:ClientCert:Path"];
    var certPassword = configuration["SEIH:ClientCert:Password"];

    var fullPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        certPath!
    );

    var certificate = new X509Certificate2(
        fullPath,
        certPassword,
        X509KeyStorageFlags.MachineKeySet |
        X509KeyStorageFlags.PersistKeySet
    );
    Console.WriteLine("CERT PATH = " + fullPath);
    Console.WriteLine("CERT EXISTS = " + File.Exists(fullPath));

    Console.WriteLine("CLIENT CERT LOADED: " + certificate.Subject);

    handler.ClientCertificates.Add(certificate);

    handler.ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

    return handler;
});


builder.Services.AddScoped<IAuthKeyService, AuthKeyService>();
builder.Services.AddScoped<Infrastructure.Services.Transfer.TransferClientService>();
builder.Services.AddScoped<IHospitalSyncService, HospitalSyncService>();
builder.Services.AddScoped<ISeihPackageBuilder, SeihPackageBuilder>();
builder.Services.AddScoped<IInboundTransferService, InboundTransferService>();
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior =
        BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddHostedService<HospitalSyncBackgroundService>();
builder.Services.AddHostedService<TransferPollingService>();
builder.Services.AddHostedService<TransferRequestPullService>();
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        options.TokenLifespan = TimeSpan.FromHours(3));
builder.Logging.AddConsole();
//builder.WebHost.UseUrls("http://10.56.102.88:5254");
var app = builder.Build();
app.UseCors("GeneralPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseForwardedHeaders();
}

app.UseHsts();

app.Use((context, next) =>
{
    var host = Environment.GetEnvironmentVariable(EnvFileConstants.HOST);
    context.Request.Host = new HostString(host!);
    context.Request.Scheme = Environment.GetEnvironmentVariable(EnvFileConstants.SCHEME)!;
    return next();
});

app.UseCookiePolicy();

var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(); // pour wwwroot si tu l'utilises

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});


app.UseAuthentication();
app.UseForwardedHeaders();
app.UseAuthorization();

app.UseSession();
app.UseMiddleware<PermissionMiddleware>();
app.MapControllers();

app.Run();







// // DME----------------------------------------------------------
// {
//   "Logging": {
//     "LogLevel": {
//       "Default": "Information",
//       "Microsoft.AspNetCore": "Debug",
//       "Microsoft.AspNetCore.Authentication": "Trace"
//     }
//   },
//   "AllowedHosts": "*",
//   "TransferSettings": {
//     "BaseUrl": "https://localhost:5256/"
//   },
//   "SEIH": {
//     "HospitalId": "40d87410-6b19-11f0-b3bb-10653024cc5c",
//     "ApiKey": "nJUHdTMT99rVbqtSSHV1ZFMalzhKDJE9hGGJ6xxZgW8=",
//     "ClientCert": {
//     "Path": "certs/dme.pfx",
//     "Password": "Se1H#Cert_2026!X9"
//   },
//     "AuthKey": {
//       "PrivateKeyPath": "certs/dme-auth-private.pem",
//       "KeyVersion": "1"
//     },
//     "TransferKey": {
//       "PrivateKeyPath": "certs/transfert/private_key_40.pem"
//     }
//   }
// }

// // DMD----------------------------------------------------------
// {
//   "Logging": {
//     "LogLevel": {
//       "Default": "Information",
//       "Microsoft.AspNetCore": "Debug",
//       "Microsoft.AspNetCore.Authentication": "Trace"
//     }
//   },
//   "AllowedHosts": "*",
//   "TransferSettings": {
//     "BaseUrl": "https://localhost:5256/"
//   },
//   "SEIH": {
//     "HospitalId": "96afb9e8-20ea-11f1-b93d-938bfe5ac8d9",
//     "ApiKey": "kwnPRO1DxZBAqNu4rM7VX6Myh8hF3H4e7lvTy1/JWhA=",
//      "ClientCert": {
//     "Path": "certs/dmd.pfx",
//     "Password": "Se1H#Cert_2026!X9"
//   },
//     "AuthKey": {
//       "PrivateKeyPath": "certs/dmd-auth-private.pem",
//       "KeyVersion": "1"
//     },
//     "TransferKey": {
//       "PrivateKeyPath": "certs/transfert/private_key.pem"
//     }
//   }
// }

// // DMN----------------------------------------------------------
// {
//   "Logging": {
//     "LogLevel": {
//       "Default": "Information",
//       "Microsoft.AspNetCore": "Debug",
//       "Microsoft.AspNetCore.Authentication": "Trace"
//     }
//   },
//   "AllowedHosts": "*",
//   "TransferSettings": {
//     "BaseUrl": "https://localhost:5256/"
//   },
//   "SEIH": {
//     "HospitalId": "9d203870-20ea-11f1-b93d-938bfe5ac8d9",
//     "ApiKey": "Kg7Bir4bHWG4xUU34riC1cSr8T7y1a6quegK0vd3p0s=",
//      "ClientCert": {
//     "Path": "certs/dmn.pfx",
//     "Password": "Se1H#Cert_2026!X9"
//   },
//     "AuthKey": {
//       "PrivateKeyPath": "certs/dmn-auth-private.pem",
//       "KeyVersion": "1"
//     },
//     "TransferKey": {
//       "PrivateKeyPath": "certs/transfert/private_key_9d.pem"
//     }
//   }
// }



