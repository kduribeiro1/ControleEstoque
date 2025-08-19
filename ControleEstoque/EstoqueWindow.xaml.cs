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
        private int? _fornecedorId;
        private bool _atualizandoTabs = false;

        public EstoqueWindow(int? fornecedorId)
        {
            InitializeComponent();
            _fornecedorId = fornecedorId;
            CarregarFornecedoresTabs();
            CarregarEstoque();
            CarregarProdutos();
        }

        private void TabFornecedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_atualizandoTabs)
                return; // Ignora evento durante atualização das abas

            var item = tabFornecedores.SelectedItem as TabItem;
            if (item != null)
            {
                _fornecedorId = item.Tag as int?;
            }
            else
            {
                _fornecedorId = null;
            }
            CarregarProdutos();
        }

        private void CarregarFornecedoresTabs()
        {
            _atualizandoTabs = true; // Inicia trava

            var fornecedores = EstoqueEntityManager.ObterFornecedores();

            tabFornecedores.Items.Clear();

            // Aba "Todos"
            var tabTodos = new TabItem { Header = "Todos", Tag = null };
            tabFornecedores.Items.Add(tabTodos);

            foreach (var fornecedor in fornecedores)
            {
                var tab = new TabItem { Header = fornecedor.Nome, Tag = fornecedor.Id };
                tabFornecedores.Items.Add(tab);
            }

            if (_fornecedorId == null)
            {
                // Seleciona a aba "Todos" se nenhum fornecedor estiver selecionado
                tabFornecedores.SelectedIndex = 0;
            }
            else
            {
                // Seleciona a aba do fornecedor específico
                var tabSelecionada = tabFornecedores.Items.Cast<TabItem>().FirstOrDefault(t => (int?)t.Tag == _fornecedorId);
                if (tabSelecionada != null)
                {
                    tabFornecedores.SelectedItem = tabSelecionada;
                }
                else
                {
                    // Se o fornecedor não estiver na lista, seleciona a aba "Todos"
                    tabFornecedores.SelectedIndex = 0;
                }
            }

            _atualizandoTabs = false; // Libera trava
        }

        private void CarregarProdutos()
        {
            try
            {
                List<IdNomeViewModel> produtos = [
                    new IdNomeViewModel(0, "Todos os produtos"),
                    .. EstoqueEntityManager.ObterProdutosPorFornecedor(_fornecedorId, true)
                        .Select(p => new IdNomeViewModel(p))
                        .OrderBy(p => p.Nome).ToList()
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
            var produto = cbProduto.SelectedItem as IdNomeViewModel;
            int? idproduto = produto?.Id;
            if (idproduto < 1)
            {
                idproduto = null;
            }
            EntradaSaidaWindow entradaSaidaWindow = new(_fornecedorId, idproduto);
            if (entradaSaidaWindow.ShowDialog() == true)
            {
                CarregarEstoque();
            }
        }
        
        private void CarregarEstoque()
        {
            try
            {
                var produto = cbProduto.SelectedItem as IdNomeViewModel;
                int? idProduto = produto?.Id;
                lstEstoque.ItemsSource = EstoqueEntityManager.ObterEstoquesPorFornecedorProduto(_fornecedorId, idProduto, null, null)
                    .Select(e => new EstoqueViewModel(e))
                    .ToList();
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
                    EntradaSaidaWindow entradaSaidaWindow = new(_fornecedorId, idEstoque, true);
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
                        var estoque = context.Estoques.Include(p => p.Produto).Where(p => p.Id == idEstoque).FirstOrDefault();
                        if (estoque != null)
                        {
                            Produto? produtoEdt = estoque.Produto;

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
                headerRow.CreateCell(2).SetCellValue("Código Produto");
                headerRow.CreateCell(3).SetCellValue("Modelo Produto");
                headerRow.CreateCell(4).SetCellValue("Fio Produto");
                headerRow.CreateCell(5).SetCellValue("Milímetro Produto");
                headerRow.CreateCell(6).SetCellValue("Tamanho Produto");
                headerRow.CreateCell(7).SetCellValue("Fornecedor");
                headerRow.CreateCell(8).SetCellValue("Unidade");
                headerRow.CreateCell(9).SetCellValue("Tipo Movimento");
                headerRow.CreateCell(10).SetCellValue("Data Entrada/Saída");
                headerRow.CreateCell(11).SetCellValue("Quantidade");
                headerRow.CreateCell(12).SetCellValue("Observação");

                int rowIndex = 1;
                foreach (var estoque in estoques)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(estoque.Id);
                    row.CreateCell(1).SetCellValue(estoque.ProdutoId);
                    row.CreateCell(2).SetCellValue(estoque.ProdutoCodigo);
                    row.CreateCell(3).SetCellValue(estoque.ProdutoModelo);
                    row.CreateCell(4).SetCellValue(estoque.ProdutoFio);
                    row.CreateCell(5).SetCellValue(estoque.ProdutoMilimetro);
                    row.CreateCell(6).SetCellValue(estoque.ProdutoTamanho);
                    row.CreateCell(7).SetCellValue(estoque.FornecedorNome);
                    row.CreateCell(8).SetCellValue(estoque.Unidade);
                    row.CreateCell(9).SetCellValue(estoque.TipoMovimento);
                    row.CreateCell(10).SetCellValue(estoque.DataEntradaSaida.ToString("dd/MM/yyyy HH:mm"));
                    row.CreateCell(11).SetCellValue(estoque.Quantidade);
                    row.CreateCell(12).SetCellValue(estoque.Observacao);
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
                var produto = cbProduto.SelectedItem as IdNomeViewModel;
                int? idProduto = produto?.Id;
                if (idProduto < 1)
                {
                    idProduto = null;
                }

                lstEstoque.ItemsSource = EstoqueEntityManager.ObterEstoquesPorFornecedorProduto(_fornecedorId, idProduto, null, null)
                    .Select(e => new EstoqueViewModel(e))
                    .ToList();
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
