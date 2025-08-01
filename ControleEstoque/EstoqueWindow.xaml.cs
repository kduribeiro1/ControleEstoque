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
    /// Lógica interna para EstoqueWindow.xaml
    /// </summary>
    
    public partial class EstoqueWindow : Window
    {
        public EstoqueWindow()
        {
            InitializeComponent();
            CarregarEstoque();
            try
            {
                using var context = new EstoqueContext();
                List<ProdutoCmbViewModel> produtos =
                [
                    new ProdutoCmbViewModel { Id = null, Nome = "Todos os produtos", Unidade = string.Empty },
                        .. context.Produtos
                            .Where(p => p.Ativo)
                            .Select(p => new ProdutoCmbViewModel { Id = p.Id, Nome = p.Nome, Unidade = p.TipoUnidade.Nome })
                            .OrderBy(p => p.Nome).ToList(),
                    ];
                cbProduto.ItemsSource = produtos;
                cbProduto.DisplayMemberPath = "Nome";
                cbProduto.SelectedValuePath = "Id";
                cbProduto.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            int? idproduto = (int?)cbProduto.SelectedValue;
            if (idproduto == 0)
            {
                idproduto = null;
            }
            EntradaSaidaWindow entradaSaidaWindow = new(idproduto);
            if (entradaSaidaWindow.ShowDialog() == true)
            {
                CarregarEstoque();
            }
        }
        
        private void CarregarEstoque()
        {
            try
            {
                int? idProduto = (int?)cbProduto.SelectedValue;
                using var db = new EstoqueContext();
                var lista = db.Estoques
                    .Include(e => e.Produto)
                    .Include(e => e.Produto.TipoUnidade)
                    .Where(e => idProduto == null || idProduto < 1 || e.IdProduto == (int)idProduto)
                    .Select(e => new EstoqueViewModel
                    {
                        Id = e.Id,
                        IdProduto = e.IdProduto,
                        NomeProduto = e.Produto.Nome,
                        Quantidade = e.Quantidade,
                        DataEntradaSaida = e.DataEntradaSaida,
                        TipoMovimento = e.Entrada ? "Entrada" : "Saída",
                        Observacao = e.Observacao,
                        Unidade = e.Produto.TipoUnidade.Nome
                    })
                    .OrderByDescending(e => e.DataEntradaSaida)
                    .ToList();

                lstEstoque.ItemsSource = lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar o estoque: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var itemSelecionado = lstEstoque.SelectedItem as EstoqueViewModel;
                if (itemSelecionado != null)
                {
                    int idEstoque = itemSelecionado.Id;
                    EntradaSaidaWindow entradaSaidaWindow = new(idEstoque, true);
                    if (entradaSaidaWindow.ShowDialog() == true)
                    {
                        CarregarEstoque();
                    }
                }
                else
                {
                    MessageBox.Show("Selecione um item para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao editar o item: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeletar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EstoqueViewModel? itemSelecionado = lstEstoque.SelectedItem as EstoqueViewModel;
                if (itemSelecionado != null)
                {
                    int idEstoque = itemSelecionado.Id;
                    if (MessageBox.Show("Tem certeza que deseja deletar este item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        using var context = new EstoqueContext();
                        var estoque = context.Estoques.Find(idEstoque);
                        if (estoque != null)
                        {
                            Produto? produtoEdt = null;
                            using (var context2 = new EstoqueContext())
                            {
                                produtoEdt = context2.Produtos.FirstOrDefault(p => p.Id == estoque.IdProduto) ?? null;
                            }

                            if (produtoEdt != null)
                            {
                                if (estoque.Entrada)
                                {
                                    produtoEdt.QuantidadeTotal -= estoque.Quantidade;
                                }
                                else
                                {
                                    produtoEdt.QuantidadeTotal += estoque.Quantidade;
                                }
                                produtoEdt.Alteracao = DateTime.Now;
                                context.Produtos.Update(produtoEdt);
                            }
                            context.Estoques.Remove(estoque);
                            context.SaveChanges();
                            CarregarEstoque();
                        }
                        else
                        {
                            MessageBox.Show("Item não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Selecione um item para deletar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar o item: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void BtnImportar_Click(object sender, RoutedEventArgs e)
        //{

        //}

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var estoques = lstEstoque.ItemsSource as IEnumerable<EstoqueViewModel>;
                if (estoques == null || !estoques.Any())
                {
                    MessageBox.Show("Não há estoque para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Estoque");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Id");
                headerRow.CreateCell(1).SetCellValue("Id Produto");
                headerRow.CreateCell(2).SetCellValue("Nome Produto");
                headerRow.CreateCell(3).SetCellValue("Unidade");
                headerRow.CreateCell(4).SetCellValue("Tipo Movimento");
                headerRow.CreateCell(5).SetCellValue("Data Entrada/Saída");
                headerRow.CreateCell(6).SetCellValue("Quantidade");
                headerRow.CreateCell(7).SetCellValue("Observação");

                int rowIndex = 1;
                foreach (var estoque in estoques)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(estoque.Id);
                    row.CreateCell(1).SetCellValue(estoque.IdProduto);
                    row.CreateCell(2).SetCellValue(estoque.NomeProduto);
                    row.CreateCell(3).SetCellValue(estoque.Unidade);
                    row.CreateCell(4).SetCellValue(estoque.TipoMovimento);
                    row.CreateCell(5).SetCellValue(estoque.DataEntradaSaida);
                    row.CreateCell(6).SetCellValue(estoque.Quantidade);
                    row.CreateCell(7).SetCellValue(estoque.Observacao);
                }

                // Diálogo para salvar arquivo
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "Estoque.xlsx"
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
                MessageBox.Show($"Erro ao exportar o estoque: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? idProduto = (int?)cbProduto.SelectedValue;
                using var db = new EstoqueContext();
                var lista = db.Estoques
                    .Include(e => e.Produto)
                    .Where(e => idProduto == null || idProduto < 1 || e.IdProduto == (int)idProduto)
                    .Select(e => new EstoqueViewModel
                    {
                        Id = e.Id,
                        IdProduto = e.IdProduto,
                        NomeProduto = e.Produto.Nome,
                        Quantidade = e.Quantidade,
                        DataEntradaSaida = e.DataEntradaSaida,
                        TipoMovimento = e.Entrada ? "Entrada" : "Saída",
                        Observacao = e.Observacao
                    })
                    .OrderByDescending(e => e.DataEntradaSaida)
                    .ToList();

                lstEstoque.ItemsSource = lista;
                MessageBox.Show("Lista de estoque atualizada com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar o estoque: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CbProduto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CarregarEstoque();
        }
    }
}
