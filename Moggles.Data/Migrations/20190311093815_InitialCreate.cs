using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Moggles.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppName = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeployEnvironments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnvName = table.Column<string>(maxLength: 50, nullable: true),
                    DefaultToggleValue = table.Column<bool>(nullable: false),
                    ApplicationId = table.Column<int>(nullable: false),
                    SortOrder = table.Column<int>(nullable: false, defaultValue: 500)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeployEnvironments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeployEnvironments_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatureToggles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ToggleName = table.Column<string>(maxLength: 80, nullable: true),
                    UserAccepted = table.Column<bool>(nullable: false),
                    Notes = table.Column<string>(maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ApplicationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureToggles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureToggles_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatureToggleStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Enabled = table.Column<bool>(nullable: false),
                    IsDeployed = table.Column<bool>(nullable: false),
                    FirstTimeDeployDate = table.Column<DateTime>(nullable: true),
                    LastDeployStatusUpdate = table.Column<DateTime>(nullable: true),
                    EnvironmentId = table.Column<int>(nullable: false),
                    FeatureToggleId = table.Column<int>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureToggleStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureToggleStatuses_DeployEnvironments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "DeployEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeatureToggleStatuses_FeatureToggles_FeatureToggleId",
                        column: x => x.FeatureToggleId,
                        principalTable: "FeatureToggles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_AppName",
                table: "Applications",
                column: "AppName");

            migrationBuilder.CreateIndex(
                name: "IX_DeployEnvironments_ApplicationId",
                table: "DeployEnvironments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeployEnvironments_EnvName",
                table: "DeployEnvironments",
                column: "EnvName");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureToggles_ApplicationId",
                table: "FeatureToggles",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureToggles_ToggleName",
                table: "FeatureToggles",
                column: "ToggleName");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureToggleStatuses_EnvironmentId",
                table: "FeatureToggleStatuses",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureToggleStatuses_FeatureToggleId",
                table: "FeatureToggleStatuses",
                column: "FeatureToggleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureToggleStatuses");

            migrationBuilder.DropTable(
                name: "DeployEnvironments");

            migrationBuilder.DropTable(
                name: "FeatureToggles");

            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
