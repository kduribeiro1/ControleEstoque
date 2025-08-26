using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace ControleEstoque
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Milimetros { get; set; } = string.Empty;
        public string Tamanho { get; set; } = string.Empty;
        public int TipoUnidadeId { get; set; }
        public TipoUnidade TipoUnidade { get; set; } = null!;
        public int FornecedorId { get; set; }
        public virtual Fornecedor Fornecedor { get; set; } = null!;
        public int QuantidadeMinima { get; set; }
        public int QuantidadeTotal { get; set; }
        public bool Ativo { get; set; }
        public DateTime Alteracao { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }

    public class Estoque
    {
        [Key]
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;
        public int Quantidade { get; set; }
        public DateTime DataEntradaSaida { get; set; }
        public bool Entrada { get; set; }
        public string Observacao { get; set; } = string.Empty;


        public bool UpdateExistente(ref Estoque estoque)
        {
            try
            {
                estoque.ProdutoId = ProdutoId;
                estoque.Quantidade = Quantidade;
                estoque.DataEntradaSaida = DataEntradaSaida;
                estoque.Entrada = Entrada;
                estoque.Observacao = Observacao;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class TipoUnidade
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int QuantidadeMinima { get; set; }
    }

    public class Fornecedor
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }

    public class EstoqueContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Estoque> Estoques { get; set; }
        public DbSet<TipoUnidade> TiposUnidades { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=controle_estoque.db");
        }
    }

    public static class EstoqueEntityManager
    {
        #region Tipos de Unidade
        public static bool LancarTipoUnidade(TipoUnidade tipoUnidade)
        {
            using var db = new EstoqueContext();
            try
            {
                if (!db.Entry(tipoUnidade).IsKeySet || tipoUnidade.Id == 0)
                {
                    db.TiposUnidades.Add(tipoUnidade);
                }
                else
                {
                    db.TiposUnidades.Update(tipoUnidade);
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar tipo de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeletarTipoUnidade(TipoUnidade tipoUnidade)
        {
            using var db = new EstoqueContext();
            try
            {
                if (db.Produtos.Where(p => p.TipoUnidadeId == tipoUnidade.Id).Any())
                {
                    MessageBox.Show("Não é possível deletar este tipo de unidade porque existem produtos associados a ele.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                db.TiposUnidades.Remove(tipoUnidade);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar tipo de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static List<TipoUnidade> ObterTiposUnidades()
        {
            try
            {
                using var db = new EstoqueContext();
                return [.. db.TiposUnidades.OrderBy(t => t.Nome)];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter tipos de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static List<TipoUnidade> ObterTiposUnidadesPorNome(string? nome = null)
        {
            try
            {
                using var db = new EstoqueContext();
                return [.. db.TiposUnidades
                    .Where(t => string.IsNullOrWhiteSpace(nome) || t.Nome.ToLower().Contains(nome.ToLower()))
                    .OrderBy(t => t.Nome)];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter tipos de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static TipoUnidade? ObterTipoUnidadePorId(int id)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.TiposUnidades
                    .FirstOrDefault(t => t.Id == id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter tipo de unidade por ID: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static TipoUnidade? ObterTipoUnidadePorNome(string nome)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.TiposUnidades
                    .FirstOrDefault(t => t.Nome.ToLower() == nome.ToLower());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter tipo de unidade por nome: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static bool ExisteTipoUnidade(string nome, int? idatual = null)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.TiposUnidades.Any(t => t.Nome.ToLower() == nome.ToLower() && (!idatual.HasValue || t.Id != idatual.Value));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao verificar tipo de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
        }
        #endregion

        #region Fornecedores
        public static bool LancarFornecedor(Fornecedor fornecedor)
        {
            using var db = new EstoqueContext();
            try
            {
                if (!db.Entry(fornecedor).IsKeySet || fornecedor.Id == 0)
                {
                    db.Fornecedores.Add(fornecedor);
                }
                else
                {
                    db.Fornecedores.Update(fornecedor);
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar fornecedor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeletarFornecedor(Fornecedor fornecedor)
        {
            using var db = new EstoqueContext();
            try
            {
                if (db.Produtos.Where(p => p.FornecedorId == fornecedor.Id).Any())
                {
                    MessageBox.Show("Não é possível deletar este fornecedor porque existem produtos associados a ele.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                db.Fornecedores.Remove(fornecedor);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar fornecedor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static List<Fornecedor> ObterFornecedores(string? filtro = null, bool? ativo = true)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Fornecedores
                    .OrderBy(f => f.Nome)
                    .Where(f => 
                        (string.IsNullOrWhiteSpace(filtro) || f.Nome.ToLower().Contains(filtro.ToLower())) &&
                        (!ativo.HasValue || f.Ativo == ativo.Value)
                    ).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter fornecedores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static Fornecedor? ObterFornecedorPorId(int id)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Fornecedores
                    .FirstOrDefault(f => f.Id == id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter fornecedor por ID: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static Fornecedor? ObterFornecedorPorNome(string nome)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Fornecedores
                    .FirstOrDefault(f => f.Nome.ToLower() == nome.ToLower());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter fornecedor por nome: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static bool ExisteFornecedor(string nome, int? idatual = null)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Fornecedores.Any(f => f.Nome.ToLower() == nome.ToLower() && (!idatual.HasValue || f.Id != idatual.Value));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao verificar fornecedor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
        }
        #endregion

        #region Produtos
        public static bool LancarProduto(Produto produto)
        {
            try
            {
                using var db = new EstoqueContext();
                if (!db.Entry(produto).IsKeySet || produto.Id == 0)
                {
                    db.Produtos.Add(produto);
                }
                else
                {
                    db.Produtos.Update(produto);
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeletarProduto(Produto produto)
        {
            try
            {
                using var db = new EstoqueContext();
                if (db.Estoques.Any(e => e.ProdutoId == produto.Id))
                {
                    MessageBox.Show("Não é possível deletar este produto porque existem registros de estoque associados a ele.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                db.Produtos.Remove(produto);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static Produto? ObterProdutoPorId(int id)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Produtos
                    .Include(p => p.TipoUnidade)
                    .Include(p => p.Fornecedor)
                    .FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter produto por ID: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static Produto? ObterProdutoPorDadosFornecedorId(string codigo, string fio, string modelo, string milimetros, string tamanho, int fornecedorId)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Produtos
                    .Include(p => p.TipoUnidade)
                    .Include(p => p.Fornecedor)
                    .FirstOrDefault(p =>
                        p.Codigo.ToLower() == codigo.ToLower() &&
                        p.Fio.ToLower() == fio.ToLower() &&
                        p.Modelo.ToLower() == modelo.ToLower() &&
                        p.Milimetros.ToLower() == milimetros.ToLower() &&
                        p.Tamanho.ToLower() == tamanho.ToLower() &&
                        p.FornecedorId == fornecedorId
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static List<Produto> ObterProdutosPorDados(string codigo, string fio, string modelo, string milimetros, string tamanho, bool? ativo = true)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Produtos
                    .Include(p => p.TipoUnidade)
                    .Include(p => p.Fornecedor)
                    .OrderBy(p => p.Modelo)
                    .Where(p =>
                        p.Codigo.ToLower() == codigo.ToLower() &&
                        p.Fio.ToLower() == fio.ToLower() &&
                        p.Modelo.ToLower() == modelo.ToLower() &&
                        p.Milimetros.ToLower() == milimetros.ToLower() &&
                        p.Tamanho.ToLower() == tamanho.ToLower() &&
                        (!ativo.HasValue || p.Ativo == ativo.Value)
                    ).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static List<Produto> ObterProdutos(bool? ativo = true)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Produtos
                    .Include(p => p.TipoUnidade)
                    .Include(p => p.Fornecedor)
                    .OrderBy(p => p.Modelo)
                    .Where(p => !ativo.HasValue || p.Ativo == ativo.Value)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static List<Produto> ObterProdutosPorFornecedor(int? fornecedorId, bool? ativo = true)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Produtos
                    .Include(p => p.TipoUnidade)
                    .Include(p => p.Fornecedor)
                    .Where(p => (!fornecedorId.HasValue || fornecedorId.Value < 1 || p.FornecedorId == fornecedorId.Value) && (!ativo.HasValue || p.Ativo == ativo.Value))
                    .OrderBy(p => p.Modelo)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter produtos por fornecedor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static List<Produto> ObterProdutosPorFornecedorOrdemAcabando(int? fornecedorId, bool? ativo = true)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Produtos
                    .Include(p => p.TipoUnidade)
                    .Include(p => p.Fornecedor)
                    .Where(p => (!fornecedorId.HasValue || fornecedorId.Value < 1 || p.FornecedorId == fornecedorId.Value) && (!ativo.HasValue || p.Ativo == ativo.Value))
                    .OrderBy(p => p.QuantidadeTotal)
                    .ThenBy(p => p.Modelo)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter produtos por fornecedor ordenados por quantidade acabando: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static List<Produto> ObterProdutosPorFornecedorFiltroOrdemAcabando(int? fornecedorId, string? tipofiltro, string? filtro, bool? ativo = true)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Produtos
                    .Include(p => p.TipoUnidade)
                    .Include(p => p.Fornecedor)
                    .Where(p =>
                        (!fornecedorId.HasValue || fornecedorId.Value < 1 || p.FornecedorId == fornecedorId.Value) &&
                        (string.IsNullOrWhiteSpace(tipofiltro) || string.IsNullOrWhiteSpace(filtro) ||
                            (tipofiltro.ToLower() == "codigo" && p.Codigo.ToLower().Contains(filtro.ToLower())) ||
                            (tipofiltro.ToLower() == "modelo" && p.Modelo.ToLower().Contains(filtro.ToLower())) ||
                            (tipofiltro.ToLower() == "fio" && p.Fio.ToLower().Contains(filtro.ToLower())) ||
                            (tipofiltro.ToLower() == "milimetros" && p.Milimetros.ToLower().Contains(filtro.ToLower())) ||
                            (tipofiltro.ToLower() == "tamanho" && p.Tamanho.ToLower().Contains(filtro.ToLower())) ||
                            (tipofiltro.ToLower() == "tipounidade" && p.TipoUnidade.Nome.ToLower().Contains(filtro.ToLower()))
                        ) &&
                        (!ativo.HasValue || p.Ativo == ativo.Value))
                    .OrderBy(p => p.QuantidadeTotal)
                    .ThenBy(p => p.Modelo)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter produtos por fornecedor com filtro e ordenados por quantidade acabando: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static bool ExisteProduto(string codigo, string fio, string modelo, string milimetros, string tamanho, int? fornecedorId, int? idatual = null)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Produtos.Any(p => 
                    p.Codigo.ToLower() == codigo.ToLower() &&
                    p.Fio.ToLower() == fio.ToLower() &&
                    p.Modelo.ToLower() == modelo.ToLower() &&
                    p.Milimetros.ToLower() == milimetros.ToLower() &&
                    p.Tamanho.ToLower() == tamanho.ToLower() &&
                    (!fornecedorId.HasValue || p.FornecedorId == fornecedorId.Value) &&
                    (!idatual.HasValue || p.Id != idatual.Value)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao verificar se o produto já existe: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
        }

        public static bool ExisteProdutoEstoque(int produtoId)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Estoques.Any(e => e.ProdutoId == produtoId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao verificar estoque do produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        #endregion

        #region Estoque
        public static bool LancarEstoque(Estoque estoque)
        {
            try
            {
                using var db = new EstoqueContext();
                if (!db.Entry(estoque).IsKeySet || estoque.Id == 0)
                {
                    Produto? produto = db.Produtos.Find(estoque.ProdutoId);
                    if (produto == null)
                    {
                        MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if (estoque.Entrada)
                    {
                        produto.QuantidadeTotal += estoque.Quantidade;
                    }
                    else
                    {
                        produto.QuantidadeTotal -= estoque.Quantidade;
                    }
                    if (produto.QuantidadeTotal < 0)
                    {
                        MessageBox.Show("Quantidade total do produto não pode ser negativa.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    produto.Alteracao = DateTime.Now;
                    db.Produtos.Update(produto);
                    db.Estoques.Add(estoque);
                }
                else
                {
                    Estoque? estExist = db.Estoques.Find(estoque.Id);
                    if (estExist == null)
                    {
                        MessageBox.Show("Estoque não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    Estoque estoqueExistente = estExist;
                    Produto? produto = db.Produtos.Find(estoque.ProdutoId);
                    if (produto == null)
                    {
                        MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    if (estoqueExistente.Entrada)
                    {
                        produto.QuantidadeTotal -= estoqueExistente.Quantidade;
                    }
                    else
                    {
                        produto.QuantidadeTotal += estoqueExistente.Quantidade;
                    }
                    if (estoque.Entrada)
                    {
                        produto.QuantidadeTotal += estoque.Quantidade;
                    }
                    else
                    {
                        produto.QuantidadeTotal -= estoque.Quantidade;
                    }
                    if (produto.QuantidadeTotal < 0)
                    {
                        MessageBox.Show("Quantidade total do produto não pode ser negativa.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    produto.Alteracao = DateTime.Now;
                    if (estoque.UpdateExistente(ref estoqueExistente))
                    {
                        db.Produtos.Update(produto);
                        db.Estoques.Update(estoqueExistente);
                    }
                    else
                    {
                        MessageBox.Show("Erro ao atualizar os dados do estoque.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar estoque: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public static bool DeletarEstoque(int idEstoque)
        {
            try
            {
                using var db = new EstoqueContext();
                Estoque? estoque = db.Estoques.Find(idEstoque);
                if (estoque == null)
                {
                    MessageBox.Show("Estoque não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                Produto? produto = db.Produtos.Find(estoque.ProdutoId);
                if (produto == null)
                {
                    MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                if (estoque.Entrada)
                {
                    produto.QuantidadeTotal -= estoque.Quantidade;
                }
                else
                {
                    produto.QuantidadeTotal += estoque.Quantidade;
                }
                if (produto.QuantidadeTotal < 0)
                {
                    MessageBox.Show("Quantidade total do produto não pode ser negativa.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                produto.Alteracao = DateTime.Now;
                db.Produtos.Update(produto);
                db.Estoques.Remove(estoque);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar estoque: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeletarEstoquePorProduto(int produtoId)
        {
            try
            {
                using var db = new EstoqueContext();
                Produto? produto = db.Produtos.Find(produtoId);
                if (produto != null)
                {
                    var estoques = db.Estoques.Where(e => e.ProdutoId == produtoId).ToList();
                    if (!estoques.Any())
                    {
                        MessageBox.Show("Nenhum estoque encontrado para o produto especificado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    foreach (var estoque in estoques)
                    {
                        if (estoque.Entrada)
                        {
                            produto.QuantidadeTotal -= estoque.Quantidade;
                        }
                        else
                        {
                            produto.QuantidadeTotal += estoque.Quantidade;
                        }
                        db.Estoques.Remove(estoque);
                    }
                    if( produto.QuantidadeTotal < 0)
                    {
                        produto.QuantidadeTotal = 0; // Garantir que a quantidade total não fique negativa
                    }
                    produto.Alteracao = DateTime.Now;
                    db.Produtos.Update(produto);
                    db.SaveChanges();
                    return true;
                }
                else
                {
                    MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar estoque: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static List<Estoque> ObterEstoques(bool? entrada = null, bool? produtoativo = true)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Estoques
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.TipoUnidade)
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.Fornecedor)
                    .OrderByDescending(e => e.DataEntradaSaida)
                    .Where(e => (!entrada.HasValue || e.Entrada == entrada.Value) && (!produtoativo.HasValue || e.Produto.Ativo == produtoativo.Value))
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter estoques: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static List<Estoque> ObterEstoquesPorFornecedorProduto(int? fornecedorId, int? produtoId, bool? fornecedorativo = true, bool? produtoativo = true)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Estoques
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.TipoUnidade)
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.Fornecedor)
                    .Where(e =>
                        (!fornecedorId.HasValue || fornecedorId.Value < 1 || e.Produto.FornecedorId == fornecedorId.Value) &&
                        (!produtoId.HasValue || produtoId.Value < 1 || e.ProdutoId == produtoId.Value) &&
                        (!fornecedorativo.HasValue || e.Produto.Fornecedor.Ativo == fornecedorativo.Value) &&
                        (!produtoativo.HasValue || e.Produto.Ativo == produtoativo.Value)
                    )
                    .OrderByDescending(e => e.DataEntradaSaida)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter estoques por fornecedor e produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static Estoque? ObterEstoquePorId(int id)
        {
            try
            {
                using var db = new EstoqueContext();
                return db.Estoques
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.TipoUnidade)
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.Fornecedor)
                    .FirstOrDefault(e => e.Id == id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter estoque por ID: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        #endregion
    }
}