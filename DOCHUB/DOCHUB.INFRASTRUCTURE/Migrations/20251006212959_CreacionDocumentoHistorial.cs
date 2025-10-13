using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DOCHUB.Migrations
{
    /// <inheritdoc />
    public partial class CreacionDocumentoHistorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "idEstado",
                table: "Documentos");

            migrationBuilder.CreateTable(
                name: "DocumentoHistorials",
                columns: table => new
                {
                    idDocumento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuarioId = table.Column<int>(type: "int", nullable: false),
                    idEstado = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ruta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoHistorials", x => x.idDocumento);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentoHistorials");

            migrationBuilder.AddColumn<int>(
                name: "idEstado",
                table: "Documentos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
