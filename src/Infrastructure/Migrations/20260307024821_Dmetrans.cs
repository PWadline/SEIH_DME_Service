using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Dmetrans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // ===============================
    // seih_transfer
    // ===============================

    migrationBuilder.CreateTable(
        name: "seih_transfer",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "char(36)", nullable: false),

            IdHospitalFrom = table.Column<Guid>(type: "char(36)", nullable: false),
            IdHospitalTo = table.Column<Guid>(type: "char(36)", nullable: false),

            EncryptedPayload = table.Column<byte[]>(type: "longblob", nullable: false),
            EncryptedSessionKey = table.Column<byte[]>(type: "longblob", nullable: false),
            IV = table.Column<byte[]>(type: "longblob", nullable: false),

            Signature = table.Column<string>(type: "longtext", nullable: false),
            PayloadHash = table.Column<string>(type: "longtext", nullable: false),
            PayloadSize = table.Column<long>(type: "bigint", nullable: false),

            PayloadType = table.Column<string>(type: "longtext", nullable: false),
            SchemaVersion = table.Column<string>(type: "longtext", nullable: false),

            IdConsent = table.Column<Guid>(type: "char(36)", nullable: false),
            ConsentHash = table.Column<string>(type: "longtext", nullable: false),
            ConsentExpiration = table.Column<DateTime>(type: "datetime", nullable: true),

            PatientReference = table.Column<string>(type: "longtext", nullable: false),
            Status = table.Column<string>(type: "varchar(50)", nullable: false),

            TransferRequestId = table.Column<Guid>(type: "char(36)", nullable: true),

            // AuditableEntity
            Created = table.Column<DateTime>(type: "datetime", nullable: false),
            CreatedBy = table.Column<string>(type: "longtext", nullable: true),
            LastModified = table.Column<DateTime>(type: "datetime", nullable: true),
            LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
            IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: true),
            LastDeleted = table.Column<DateTime>(type: "datetime", nullable: true),
            LastDeletedBy = table.Column<string>(type: "longtext", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_seih_transfer", x => x.Id);
        });


    // ===============================
    // seih_transferRequest
    // ===============================

    migrationBuilder.CreateTable(
        name: "seih_transferRequest",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "char(36)", nullable: false),

            InfoPatient = table.Column<string>(type: "longtext", nullable: true),

            IdHospitalFrom = table.Column<Guid>(type: "char(36)", nullable: false),
            IdHospitalTo = table.Column<Guid>(type: "char(36)", nullable: false),
            IdConsent = table.Column<Guid>(type: "char(36)", nullable: false),

            Status = table.Column<int>(type: "int", nullable: false),

            TransferId = table.Column<Guid>(type: "char(36)", nullable: true),
            ParentRequestId = table.Column<Guid>(type: "char(36)", nullable: true),

            // AuditableEntity
            Created = table.Column<DateTime>(type: "datetime", nullable: false),
            CreatedBy = table.Column<string>(type: "longtext", nullable: true),
            LastModified = table.Column<DateTime>(type: "datetime", nullable: true),
            LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
            IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: true),
            LastDeleted = table.Column<DateTime>(type: "datetime", nullable: true),
            LastDeletedBy = table.Column<string>(type: "longtext", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_seih_transferRequest", x => x.Id);
        });
}
        /// <inheritdoc />
       protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "seih_transfer");
    migrationBuilder.DropTable(name: "seih_transferRequest");
}
    
    }
}
