using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConsentUpdatedAt",
                table: "Patients",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DataSharingConsent",
                table: "Patients",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentUpdatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DataSharingConsent",
                table: "Patients");
        }
    }
}
