using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLabResultsPrescriptionsMaternalHealth2FA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AntenatalVisits",
                table: "FieldVisitLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildAgeMonths",
                table: "FieldVisitLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ChildWeightKg",
                table: "FieldVisitLogs",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DangerSigns",
                table: "FieldVisitLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GestationalAgeWeeks",
                table: "FieldVisitLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ImmunizationsUpToDate",
                table: "FieldVisitLogs",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PregnancyStatus",
                table: "FieldVisitLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthenticatorKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabResults",
                columns: table => new
                {
                    LabResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceRange = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabResults", x => x.LabResultId);
                    table.ForeignKey(
                        name: "FK_LabResults_AspNetUsers_OrderedByUserId",
                        column: x => x.OrderedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabResults_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabResults_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "FacilityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabResults_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    Medication = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrescribedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PrescribedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.PrescriptionId);
                    table.ForeignKey(
                        name: "FK_Prescriptions_AspNetUsers_PrescribedByUserId",
                        column: x => x.PrescribedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "VisitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_FacilityId",
                table: "LabResults",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_OrderedByUserId",
                table: "LabResults",
                column: "OrderedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_PatientId",
                table: "LabResults",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_ReviewedByUserId",
                table: "LabResults",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PrescribedByUserId",
                table: "Prescriptions",
                column: "PrescribedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_VisitId",
                table: "Prescriptions",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabResults");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "AntenatalVisits",
                table: "FieldVisitLogs");

            migrationBuilder.DropColumn(
                name: "ChildAgeMonths",
                table: "FieldVisitLogs");

            migrationBuilder.DropColumn(
                name: "ChildWeightKg",
                table: "FieldVisitLogs");

            migrationBuilder.DropColumn(
                name: "DangerSigns",
                table: "FieldVisitLogs");

            migrationBuilder.DropColumn(
                name: "GestationalAgeWeeks",
                table: "FieldVisitLogs");

            migrationBuilder.DropColumn(
                name: "ImmunizationsUpToDate",
                table: "FieldVisitLogs");

            migrationBuilder.DropColumn(
                name: "PregnancyStatus",
                table: "FieldVisitLogs");

            migrationBuilder.DropColumn(
                name: "AuthenticatorKey",
                table: "AspNetUsers");
        }
    }
}
