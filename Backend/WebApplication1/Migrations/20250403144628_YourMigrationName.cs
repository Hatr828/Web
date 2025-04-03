using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class YourMigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersAccess_Users_UserId",
                schema: "site",
                table: "UsersAccess");

            migrationBuilder.DropIndex(
                name: "IX_Users_Slug",
                schema: "site",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersAccess",
                schema: "site",
                table: "UsersAccess");

            migrationBuilder.DeleteData(
                schema: "site",
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("1e7b62ed-1810-441b-a781-622f2bf86d66"));

            migrationBuilder.DeleteData(
                schema: "site",
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3cf44c28-9b0b-4314-a7bd-410864432f7a"));

            migrationBuilder.DeleteData(
                schema: "site",
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("706c9d0d-d766-48b2-8615-3dfe795b048e"));

            migrationBuilder.DeleteData(
                schema: "site",
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("cc51b8ca-ad48-456d-b83f-023f17a7cec8"));

            migrationBuilder.RenameTable(
                name: "UsersAccess",
                schema: "site",
                newName: "Accesses",
                newSchema: "site");

            migrationBuilder.RenameColumn(
                name: "Dk",
                schema: "site",
                table: "Accesses",
                newName: "DK");

            migrationBuilder.RenameIndex(
                name: "IX_UsersAccess_UserId",
                schema: "site",
                table: "Accesses",
                newName: "IX_Accesses_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UsersAccess_Login",
                schema: "site",
                table: "Accesses",
                newName: "IX_Accesses_Login");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "site",
                table: "Products",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                schema: "site",
                table: "Accesses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accesses",
                schema: "site",
                table: "Accesses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AuthTokens",
                schema: "site",
                columns: table => new
                {
                    Jti = table.Column<Guid>(type: "uuid", nullable: false),
                    Iss = table.Column<string>(type: "text", nullable: true),
                    Sub = table.Column<Guid>(type: "uuid", nullable: true),
                    Aud = table.Column<string>(type: "text", nullable: true),
                    Iat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Exp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Nbf = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthTokens", x => x.Jti);
                    table.ForeignKey(
                        name: "FK_AuthTokens_Accesses_Sub",
                        column: x => x.Sub,
                        principalSchema: "site",
                        principalTable: "Accesses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                schema: "site",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "site",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CanCreate = table.Column<bool>(type: "boolean", nullable: false),
                    CanRead = table.Column<bool>(type: "boolean", nullable: false),
                    CanUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmployee = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductPromotion",
                schema: "site",
                columns: table => new
                {
                    ProductsId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPromotion", x => new { x.ProductsId, x.PromotionsId });
                    table.ForeignKey(
                        name: "FK_ProductPromotion_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalSchema: "site",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPromotion_Promotions_PromotionsId",
                        column: x => x.PromotionsId,
                        principalSchema: "site",
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "site",
                table: "Categories",
                columns: new[] { "Id", "Description", "ImagesCsv", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("112f9ace-b5e9-4c38-8fa5-3d6ad440d090"), "Товари та вироби з деревини", "wood.jpg", "Дерево", "wood" },
                    { new Guid("283e039b-c71d-46a7-b69f-f8bb7aeb1a5f"), "Вироби з натурального та штучного камінняня", "stone.jpg", "Каміння", "stone" },
                    { new Guid("c4971b90-c145-411d-a35a-0ec565423db7"), "Товари та вироби зі скла", "glass.jpg", "Скло", "glass" },
                    { new Guid("f889f0b9-abb3-434b-93f4-1ba9331196bb"), "Офісні та настільні товари", "office.jpg", "Офіс", "office" }
                });

            migrationBuilder.InsertData(
                schema: "site",
                table: "UserRoles",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanRead", "CanUpdate", "Description", "IsEmployee", "Name" },
                values: new object[,]
                {
                    { new Guid("d1a3d3a4-3a3d-4d1a-3a3d-4d1a3d3a4d1a"), true, true, true, true, "Адміністратор", true, "Admin" },
                    { new Guid("d1a3d3a4-3a3d-4d1a-3a3d-4d1a3d3a4d2a"), false, false, false, false, "Користувач", false, "User" },
                    { new Guid("d1a3d3a4-3a3d-4d1a-3a3d-4d1a3d3a4d3a"), false, false, false, false, "Працівник", true, "Employee" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Slug",
                schema: "site",
                table: "Users",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_RoleId",
                schema: "site",
                table: "Accesses",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthTokens_Sub",
                schema: "site",
                table: "AuthTokens",
                column: "Sub");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPromotion_PromotionsId",
                schema: "site",
                table: "ProductPromotion",
                column: "PromotionsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accesses_UserRoles_RoleId",
                schema: "site",
                table: "Accesses",
                column: "RoleId",
                principalSchema: "site",
                principalTable: "UserRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accesses_Users_UserId",
                schema: "site",
                table: "Accesses",
                column: "UserId",
                principalSchema: "site",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accesses_UserRoles_RoleId",
                schema: "site",
                table: "Accesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Accesses_Users_UserId",
                schema: "site",
                table: "Accesses");

            migrationBuilder.DropTable(
                name: "AuthTokens",
                schema: "site");

            migrationBuilder.DropTable(
                name: "ProductPromotion",
                schema: "site");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "site");

            migrationBuilder.DropTable(
                name: "Promotions",
                schema: "site");

            migrationBuilder.DropIndex(
                name: "IX_Users_Slug",
                schema: "site",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accesses",
                schema: "site",
                table: "Accesses");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_RoleId",
                schema: "site",
                table: "Accesses");

            migrationBuilder.DeleteData(
                schema: "site",
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("112f9ace-b5e9-4c38-8fa5-3d6ad440d090"));

            migrationBuilder.DeleteData(
                schema: "site",
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("283e039b-c71d-46a7-b69f-f8bb7aeb1a5f"));

            migrationBuilder.DeleteData(
                schema: "site",
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c4971b90-c145-411d-a35a-0ec565423db7"));

            migrationBuilder.DeleteData(
                schema: "site",
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f889f0b9-abb3-434b-93f4-1ba9331196bb"));

            migrationBuilder.DropColumn(
                name: "RoleId",
                schema: "site",
                table: "Accesses");

            migrationBuilder.RenameTable(
                name: "Accesses",
                schema: "site",
                newName: "UsersAccess",
                newSchema: "site");

            migrationBuilder.RenameColumn(
                name: "DK",
                schema: "site",
                table: "UsersAccess",
                newName: "Dk");

            migrationBuilder.RenameIndex(
                name: "IX_Accesses_UserId",
                schema: "site",
                table: "UsersAccess",
                newName: "IX_UsersAccess_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Accesses_Login",
                schema: "site",
                table: "UsersAccess",
                newName: "IX_UsersAccess_Login");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "site",
                table: "Products",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersAccess",
                schema: "site",
                table: "UsersAccess",
                column: "Id");

            migrationBuilder.InsertData(
                schema: "site",
                table: "Categories",
                columns: new[] { "Id", "Description", "ImagesCsv", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("1e7b62ed-1810-441b-a781-622f2bf86d66"), "Товари та вироби з деревини", "wood.jpg", "Дерево", "wood" },
                    { new Guid("3cf44c28-9b0b-4314-a7bd-410864432f7a"), "Вироби з натурального та штучного каміння", "stone.jpg", "Каміння", "stone" },
                    { new Guid("706c9d0d-d766-48b2-8615-3dfe795b048e"), "Товари та вироби зі скла", "glass.jpg", "Скло", "glass" },
                    { new Guid("cc51b8ca-ad48-456d-b83f-023f17a7cec8"), "Офісні та настільні товари", "office.jpg", "Офіс", "office" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Slug",
                schema: "site",
                table: "Users",
                column: "Slug");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersAccess_Users_UserId",
                schema: "site",
                table: "UsersAccess",
                column: "UserId",
                principalSchema: "site",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
