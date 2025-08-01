using Microsoft.EntityFrameworkCore;

namespace ControleEstoque
{
    public class TipoUnidade
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int IdTipoUnidade { get; set; }
        public virtual TipoUnidade TipoUnidade { get; set; } = null!;
        public int PrecoUnidade { get; set; }
        public int QuantidadeTotal { get; set; }
        public bool Ativo { get; set; }
        public DateTime Alteracao { get; set; }
    }

    public class Estoque
    {
        public int Id { get; set; }
        public int IdProduto { get; set; }
        public virtual Produto Produto { get; set; } = null!;
        public int Quantidade { get; set; }
        public int ValorTotal { get; set; }
        public DateTime DataEntradaSaida { get; set; }
        public bool Entrada { get; set; }
        public string Observacao { get; set; } = string.Empty;
    }

    public class Pedido
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public int QtdeItens { get; set; }
        public int ValorTotal { get; set; }
        public DateTime DataPedido { get; set; }
    }

    public class ItemPedido
    {
        public int Id { get; set; }
        public int IdPedido { get; set; }
        public virtual Pedido Pedido { get; set; } = null!;
        public int IdProduto { get; set; }
        public virtual Produto Produto { get; set; } = null!;
        public int Quantidade { get; set; }
        public int ValorUnitario { get; set; }
        public int ValorTotal { get; set; }
    }

    public class EstoqueContext : DbContext
    {
        public DbSet<TipoUnidade> TiposUnidades { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Estoque> Estoques { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedidos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=controle_estoque.db");
        }
    }
}