using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotesAi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LatestUpdate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "DbMetadataProperty",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbMetadataProperty", x => new { x.DocumentId, x.Key });
                    table.ForeignKey(
                        name: "FK_DbMetadataProperty_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DbParagraph",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    DocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbParagraph", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbParagraph_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_DbParagraph_DocumentId_Index",
                table: "DbParagraph",
                columns: new[] { "DocumentId", "Index" },
                unique: true
            );

            migrationBuilder.CreateIndex(name: "IX_Documents_Name", table: "Documents", column: "Name", unique: true);
            migrationBuilder.Sql(
                """
                CREATE VIRTUAL TABLE "DbParagraphVector" USING vectorlite(embedding float32[256], hnsw(max_elements=100), "./.notesai/vector.index");

                CREATE TRIGGER OnDeleteDbParagraphDeleteDbParagraphVector AFTER DELETE ON DbParagraph BEGIN
                    DELETE FROM DbParagraphVector WHERE rowid = old."RowId";
                END;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DbMetadataProperty");

            migrationBuilder.DropTable(name: "DbParagraph");

            migrationBuilder.DropTable(name: "Documents");

            migrationBuilder.DropTable(name: "DbParagraphVector");
        }
    }
}
