using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class AjusteNosIdsForekey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdTipoUnidade",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "IdProduto",
                table: "Estoques");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoUnidade",
                table: "Produtos",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PrecoUnidade",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "IdTipoUnidade",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdProduto",
                table: "Estoques",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
