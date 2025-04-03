using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class s2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "site",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ImagesCsv = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "site",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    ImagesCsv = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "site",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Categories_Slug",
                schema: "site",
                table: "Categories",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "site",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                schema: "site",
                table: "Products",
                column: "Slug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products",
                schema: "site");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "site");
        }
    }
}
