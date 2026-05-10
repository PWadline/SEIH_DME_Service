using Core.Application.Model.Response;
using Core.Domain.Commons;
using Core.Domain.Entities;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using Core.Domain.Procedures;
using Core.Domain.Procedures.SEIH;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure;

public class AppDbContext : IdentityDbContext<UserEntity, UserRoleEntity, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ProductSalesEntity> ProductSales { get; set; }
    public DbSet<SalesMetadataEntity> SalesMetadata { get; set; }
    public DbSet<GetProductSuggestionsResponse> ProductSuggestionsResponses { get; set; }
    public DbSet<SalesMetadataAndProductResponse> SalesMetadataAndProductResponses { get; set; }
    public DbSet<GetAllSalesMetadata> GetAllSalesMetadata { get; set; }
    public DbSet<GetSellerSalesTotalPriceAndQuantityToday> GetSellerSalesTotalPriceAndQuantityTodays { get; set; }
    public DbSet<SellerDailyResumeEntity> SellerDailyResumes { get; set; }
    public DbSet<GetSalesSummaryDto> GetSalesSummary { get; set; }

    //NEW
    public DbSet<UsersRoleEntity> UsersRole { get; set; }
    public DbSet<RolePermissionEntity> RolesPermission { get; set; }
    public DbSet<PermissionEntity> Permissions { get; set; }
    public DbSet<RolesEntity> Rolesv2 { get; set; }
    public DbSet<UsersEntity> User { get; set; }
    public DbSet<HospitalEntity> Hospitals { get; set; }
    public DbSet<GetUserRolesResponse> GetUserRoles { get; set; }
    public DbSet<GetUserRolesWithPermissionResponse> GetUserRolesWithPermission { get; set; }
    public DbSet<GetUserListWithRolesResponse> GetUserListWithRoles { get; set; }
    public DbSet<TransferEntity> Transfers { get; set; }
    public DbSet<TransferRequestEntity> TransferRequests { get; set; }
    //NEW
    public DbSet<InstitutionKeyEntity> InstitutionKeys { get; set; }
    public DbSet<FormEntity> Forms { get; set; }
    public DbSet<FormFieldEntity> FormFields { get; set; }
    public DbSet<RecordEntity> Records { get; set; }
    public DbSet<RecordFieldValueEntity> RecordFieldValues { get; set; }
    public DbSet<FileStorageEntity> Files { get; set; }
    public DbSet<ConsentEntity> Consents { get; set; }
    public DbSet<ConsentHospitalEntity> ConsentHospitals { get; set; }
    public DbSet<AuditLogEntity> AuditLogs { get; set; }
    public DbSet<HospitalBranchEntity> HospitalBranches { get; set; }
    public DbSet<DmeHospitalEntity> DmeHospitals { get; set; }

    public DbSet<TransferInstitutionKeyEntity> TransferInstitutionKeys { get; set; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is AuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (AuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.Created = DateTime.UtcNow;
                entity.IsDeleted = false;
            }

            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = "SetLastModifiedBy";
        }

        // No manual transaction; let EF handle it
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // GUID MySQL fix
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(Guid))
                {
                    property.SetColumnType("char(36)");
                    property.SetCollation("ascii_general_ci");
                }
            }
        }

        // SEIH TABLES
        builder.Entity<HospitalEntity>().ToTable("seih_hospital");
        builder.Entity<UsersEntity>().ToTable("seih_users");
        builder.Entity<RolesEntity>().ToTable("seih_roles");
        builder.Entity<UsersRoleEntity>().ToTable("seih_userroles");
        builder.Entity<PermissionEntity>().ToTable("seih_permission");
        builder.Entity<RolePermissionEntity>().ToTable("seih_rolepermission");
        builder.Entity<TransferEntity>().ToTable("seih_transfer");


        builder.Entity<TransferRequestEntity>().ToTable("seih_transferRequest");

        builder.Entity<InstitutionKeyEntity>().ToTable("seih_institutionkey");
        builder.Entity<FormEntity>().ToTable("seih_forms");
        builder.Entity<FormFieldEntity>().ToTable("seih_formfields");
        builder.Entity<RecordEntity>().ToTable("seih_records");
        builder.Entity<RecordFieldValueEntity>().ToTable("seih_recordfieldvalues");
        builder.Entity<FileStorageEntity>().ToTable("seih_files");
        builder.Entity<ConsentEntity>().ToTable("seih_consent");
        builder.Entity<ConsentHospitalEntity>().ToTable("seih_consents_hospitals");
        builder.Entity<AuditLogEntity>().ToTable("seih_auditlogs");
        builder.Entity<HospitalBranchEntity>().ToTable("seih_hospitalbranch");
        builder.Entity<DmeHospitalEntity>().ToTable("dme_hospital");
        builder.Entity<TransferInstitutionKeyEntity>().ToTable("seih_transferinstitutionkey");

        // Old
        builder.Entity<UserEntity>(entity => { entity.ToTable("users"); });
        builder.Entity<UserRoleEntity>(entity => { entity.ToTable("roles"); });
        builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("userclaims"); });
        builder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("userlogins"); });
        builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("roleclaims"); });
        builder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("usertokens"); });
        builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("userroles"); });
        builder.Entity<ProductEntity>(entity => { entity.ToTable("products"); });
        builder.Entity<CategoryEntity>(entity => { entity.ToTable("categories"); });
        builder.Entity<SalesMetadataEntity>(entity => { entity.ToTable("salesmetadata"); });
        builder.Entity<ProductSalesEntity>(entity => { entity.ToTable("productsales"); });

        builder.Entity<GetProductSuggestionsResponse>().HasNoKey();
        builder.Entity<SalesMetadataAndProductResponse>().HasNoKey();
        builder.Entity<GetAllSalesMetadata>().HasNoKey();
        builder.Entity<SellerDailyResumeEntity>().HasNoKey();
        builder.Entity<GetSellerSalesTotalPriceAndQuantityToday>().HasNoKey();
        builder.Entity<GetSalesSummaryDto>().HasNoKey();

        //Procedure Mapping
        builder.Entity<GetUserRolesResponse>().HasNoKey();
        builder.Entity<GetUserRolesWithPermissionResponse>().HasNoKey();
        builder.Entity<GetUserListWithRolesResponse>().HasNoKey();

        builder.Entity<FileStorageEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OriginalFileName)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(x => x.StoredFileName)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(x => x.Path)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(x => x.ContentType)
                  .HasMaxLength(150);

            entity.Property(x => x.RelatedEntity)
                  .HasMaxLength(100);

            entity.Property(x => x.Size)
                  .IsRequired();

            entity.Property(x => x.IsDeleted)
                  .HasDefaultValue(false);

            entity.HasIndex(x => x.HospitalId);
        });


    }
}

