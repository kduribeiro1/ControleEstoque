using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class addpesounitariograma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "QuantidadeMinima",
                table: "TiposUnidades",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<double>(
                name: "PesoUnitarioGrama",
                table: "TiposUnidades",
                type: "REAL",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "QuantidadeTotal",
                table: "Produtos",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "QuantidadeMinima",
                table: "Produtos",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<double>(
                name: "PesoUnitarioGrama",
                table: "Produtos",
                type: "REAL",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Quantidade",
                table: "Estoques",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PesoUnitarioGrama",
                table: "TiposUnidades");

            migrationBuilder.DropColumn(
                name: "PesoUnitarioGrama",
                table: "Produtos");

            migrationBuilder.AlterColumn<int>(
                name: "QuantidadeMinima",
                table: "TiposUnidades",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "QuantidadeTotal",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "QuantidadeMinima",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "Quantidade",
                table: "Estoques",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");
        }
    }
}
