using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ControleEstoque
{
    /// <summary>
    /// Lógica interna para ProdutosWindow.xaml
    /// </summary>
    public partial class ProdutosWindow : Window
    {
        public ProdutosWindow()
        {
            InitializeComponent();
            CarregarProdutos();
        }

        private void TxtFiltroNome_TextChanged(object sender, TextChangedEventArgs e)
        {
            CarregarProdutos(txtFiltroNome.Text);
        }

        private void CarregarProdutos(string filtro = "")
        {
            try
            {
                using var db = new EstoqueContext();
                var produtos = db.Produtos.Include(p => p.TipoUnidade)
                    .Where(p => p.Ativo)
                    .Select(p => new ProdutoViewModel
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Unidade = p.TipoUnidade.Nome,
                        PrecoUnidade = p.PrecoUnidade.ToString("F2"),
                        QuantidadeTotal = p.QuantidadeTotal,
                        Ativo = p.Ativo ? "Sim" : "Não",
                        Alteracao = p.Alteracao,
                        Descricao = p.Descricao ?? string.Empty,
                        IdTipoUnidade = p.TipoUnidade.Id
                    })
                    .Where(p => string.IsNullOrWhiteSpace(filtro) || p.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.Nome)
                    .ToList();

                lstProdutos.ItemsSource = produtos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            ProdutoWindow produtoWindow = new ProdutoWindow();
            if (produtoWindow.ShowDialog() == true)
            {
                CarregarProdutos(txtFiltroNome.Text);
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var produtoSelecionado = lstProdutos.SelectedItem as ProdutoViewModel;
                if (produtoSelecionado == null)
                {
                    MessageBox.Show("Selecione um produto para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Produto? produto = null;
                using (var db = new EstoqueContext())
                {
                    produto = db.Produtos.Include(p => p.TipoUnidade).FirstOrDefault(p => p.Id == produtoSelecionado.Id);
                }
                if (produto == null)
                {
                    MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                ProdutoWindow produtoWindow = new ProdutoWindow(produto);
                if (produtoWindow.ShowDialog() == true)
                {
                    CarregarProdutos(txtFiltroNome.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar a janela de edição: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeletar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProdutoViewModel? itemSelecionado = lstProdutos.SelectedItem as ProdutoViewModel;
                if (itemSelecionado != null)
                {
                    int idProduto = itemSelecionado.Id;
                    Produto? produto = null;
                    using (var db = new EstoqueContext())
                    {
                        produto = db.Produtos.FirstOrDefault(p => p.Id == idProduto);
                    }
                    if (produto == null)
                    {
                        MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (MessageBox.Show("Tem certeza que deseja deletar este item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        using var context = new EstoqueContext();
                        var estoques = context.Estoques.Include(p => p.Produto).Where(p => p.Produto.Id == itemSelecionado.Id).ToList();
                        if (estoques.Any())
                        {
                            if (MessageBox.Show("Este produto possui estoques associados. Deseja deletar inclusive os estoques?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                context.Estoques.RemoveRange(estoques);
                                context.SaveChanges();
                            }
                            else
                            {
                                MessageBox.Show("Produto não pode ser deletado pois possui estoques associados.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                        context.Produtos.Remove(produto);
                        context.SaveChanges();
                        CarregarProdutos(txtFiltroNome.Text);
                    }
                }
                else
                {
                    MessageBox.Show("Selecione um item para deletar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var db = new EstoqueContext();
                var produtos = db.Produtos.Include(p => p.TipoUnidade)
                    .Where(p => p.Ativo)
                    .Select(p => new ProdutoViewModel
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Unidade = p.TipoUnidade.Nome,
                        PrecoUnidade = p.PrecoUnidade.ToString("F2"),
                        QuantidadeTotal = p.QuantidadeTotal,
                        Ativo = p.Ativo ? "Sim" : "Não",
                        Alteracao = p.Alteracao,
                        Descricao = p.Descricao ?? string.Empty,
                        IdTipoUnidade = p.TipoUnidade.Id
                    })
                    .Where(p => string.IsNullOrWhiteSpace(txtFiltroNome.Text) || p.Nome.Contains(txtFiltroNome.Text, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.Nome)
                    .ToList();

                lstProdutos.ItemsSource = produtos;
                MessageBox.Show("Produtos atualizados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnGerarModelo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("ModeloProdutos");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Nome");
                headerRow.CreateCell(1).SetCellValue("Unidade"); // Nome da unidade
                headerRow.CreateCell(2).SetCellValue("Preço Unidade");
                headerRow.CreateCell(3).SetCellValue("Quantidade Total");
                headerRow.CreateCell(4).SetCellValue("Ativo"); // Sim/Não
                headerRow.CreateCell(5).SetCellValue("Alteração"); // dd/MM/yyyy HH:mm
                headerRow.CreateCell(6).SetCellValue("Descrição");

                // Sugestão: Adicionar exemplos de valores na segunda linha
                var exampleRow = sheet.CreateRow(1);
                exampleRow.CreateCell(0).SetCellValue("Produto Exemplo");
                exampleRow.CreateCell(1).SetCellValue("Unidade Exemplo");
                exampleRow.CreateCell(2).SetCellValue("10.00");
                exampleRow.CreateCell(3).SetCellValue("100");
                exampleRow.CreateCell(4).SetCellValue("Sim");
                exampleRow.CreateCell(5).SetCellValue(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                exampleRow.CreateCell(6).SetCellValue("Descrição do produto");

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "ModeloProdutos.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fs);
                    }
                    MessageBox.Show("Modelo gerado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar modelo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx"
                };

                if (openFileDialog.ShowDialog() != true)
                    return;

                var workbook = new XSSFWorkbook(openFileDialog.OpenFile());
                var sheet = workbook.GetSheetAt(0);

                using var db = new EstoqueContext();
                int importados = 0;

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    string nome = row.GetCell(0)?.ToString() ?? "";
                    string unidadeNome = row.GetCell(1)?.ToString() ?? "";
                    string precoUnidadeStr = row.GetCell(2)?.ToString() ?? "";
                    string quantidadeTotalStr = row.GetCell(3)?.ToString() ?? "";
                    string ativoStr = row.GetCell(4)?.ToString() ?? "Sim";
                    string alteracaoStr = row.GetCell(5)?.ToString() ?? "";
                    string descricao = row.GetCell(6)?.ToString() ?? "";

                    if (precoUnidadeStr.IndexOf(',') >= 0 && precoUnidadeStr.IndexOf('.') >= 0)
                    {
                        if (precoUnidadeStr.IndexOf(',') < precoUnidadeStr.IndexOf('.'))
                        {
                            precoUnidadeStr = precoUnidadeStr.Replace(".","").Replace(',', '.'); // Converte vírgula para ponto
                        }
                    }
                    else if (precoUnidadeStr.IndexOf(',') >= 0)
                    {
                        precoUnidadeStr = precoUnidadeStr.Replace(',', '.'); // Converte vírgula para ponto
                    }

                    if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(unidadeNome))
                        continue;

                    if (db.Produtos.Where(db => db.Nome == nome).Any())
                    {
                        continue;
                    }

                    // Busca ou cadastra o tipo de unidade
                    var tipoUnidade = db.TiposUnidades.FirstOrDefault(u => u.Nome == unidadeNome);
                    if (tipoUnidade == null)
                    {
                        tipoUnidade = new TipoUnidade
                        {
                            Nome = unidadeNome,
                            QuantidadeAviso = 0 // ou outro valor padrão
                        };
                        db.TiposUnidades.Add(tipoUnidade);
                        db.SaveChanges();
                    }

                    if (!decimal.TryParse(precoUnidadeStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal precoUnidade)) precoUnidade = 0;
                    if (!int.TryParse(quantidadeTotalStr, out int quantidadeTotal)) quantidadeTotal = 0;
                    if (quantidadeTotal < 0) quantidadeTotal = 0; // Evita valores negativos
                    if (precoUnidade < 0) precoUnidade = 0; // Evita valores negativos
                    if (string.IsNullOrWhiteSpace(ativoStr)) ativoStr = "Sim"; // Valor padrão
                    if (string.IsNullOrWhiteSpace(alteracaoStr)) alteracaoStr = DateTime.Now.ToString("dd/MM/yyyy HH:mm"); // Valor padrão

                    bool ativo = ativoStr.Equals("Sim", StringComparison.OrdinalIgnoreCase);
                    DateTime alteracao = DateTime.TryParse(alteracaoStr, out var dt) ? dt : DateTime.Now;

                    var produto = new Produto
                    {
                        Nome = nome,
                        TipoUnidade = tipoUnidade,
                        PrecoUnidade = precoUnidade,
                        QuantidadeTotal = quantidadeTotal,
                        Ativo = ativo,
                        Alteracao = alteracao,
                        Descricao = descricao
                    };

                    db.Produtos.Add(produto);
                    db.SaveChanges(); // Salva para obter o Id do produto

                    // Se quantidade total > 0, cadastra entrada no estoque
                    if (quantidadeTotal > 0)
                    {
                        var estoque = new Estoque
                        {
                            Produto = produto,
                            Quantidade = quantidadeTotal,
                            DataEntradaSaida = alteracao,
                            Entrada = true,
                            Observacao = "Cadastro inicial via importação"
                        };
                        db.Estoques.Add(estoque);
                        db.SaveChanges();
                    }

                    importados++;
                }

                MessageBox.Show($"{importados} produtos importados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                CarregarProdutos(txtFiltroNome.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao importar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var produtos = lstProdutos.ItemsSource as IEnumerable<ProdutoViewModel>;
                if (produtos == null || !produtos.Any())
                {
                    MessageBox.Show("Não há produtos para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Produtos");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Id");
                headerRow.CreateCell(1).SetCellValue("Nome");
                headerRow.CreateCell(2).SetCellValue("IdTipoUnidade");
                headerRow.CreateCell(3).SetCellValue("Unidade");
                headerRow.CreateCell(4).SetCellValue("Preço Unidade");
                headerRow.CreateCell(5).SetCellValue("Quantidade Total");
                headerRow.CreateCell(6).SetCellValue("Ativo");
                headerRow.CreateCell(7).SetCellValue("Alteração");
                headerRow.CreateCell(8).SetCellValue("Descrição");

                int rowIndex = 1;
                foreach (var produto in produtos)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(produto.Id);
                    row.CreateCell(1).SetCellValue(produto.Nome);
                    row.CreateCell(2).SetCellValue(produto.IdTipoUnidade);
                    row.CreateCell(3).SetCellValue(produto.Unidade);
                    row.CreateCell(4).SetCellValue(produto.PrecoUnidade);
                    row.CreateCell(5).SetCellValue(produto.QuantidadeTotal);
                    row.CreateCell(6).SetCellValue(produto.Ativo);
                    row.CreateCell(7).SetCellValue(produto.Alteracao.ToString("dd/MM/yyyy HH:mm"));
                    row.CreateCell(8).SetCellValue(produto.Descricao);
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "Produtos.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fs);
                    }
                    MessageBox.Show("Exportação concluída com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
