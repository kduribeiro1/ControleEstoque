using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ControleEstoque
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer;
        private int? _fornecedorId; 
        private bool _atualizandoTabs = false;

        public MainWindow()
        {
            InitializeComponent();
            _fornecedorId = null; // Inicializa o fornecedorId como nulo
            CarregarFornecedoresTabs();
            CarregarProdutosAcabando();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            txtKRDG.Text = "KRDG";
        }

        private static string PrimeiraMaiuscula(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;
            StringBuilder sb = new(texto.Length);
            bool isNextUpper = true;
            foreach (char c in texto)
            {
                if (char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                    isNextUpper = true;
                }
                else
                {
                    sb.Append(isNextUpper ? char.ToUpper(c) : c);
                    isNextUpper = false;
                }
            }
            return sb.ToString();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            DateTime agora = DateTime.UtcNow.AddHours(-3);
            txtData.Text = $"{agora:dd} de {PrimeiraMaiuscula(agora.ToString("MMMM"))} de {agora:yyyy}";
            txtHora.Text = DateTime.UtcNow.AddHours(-3).ToString("HH:mm:ss");
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
            CarregarProdutosAcabando();
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

        private void CarregarProdutosAcabando()
        {
            try
            {
                var produtosBaixoEstoque = EstoqueEntityManager.ObterProdutosPorFornecedorOrdemAcabando(_fornecedorId)
                    .Select(p => new ProdutoViewModel(p))
                    .OrderBy(p => p.QuantidadeTotal)
                    .ToList();

                listViewProdutosAcabando.ItemsSource = produtosBaixoEstoque;
                txtQtdeProdutos.Content = $"Quantidade Produtos: {produtosBaixoEstoque.Count}";

                string strPesoTotal = string.Empty;
                double pesoTotalGramas = produtosBaixoEstoque.Sum(p => (p.QuantidadeTotal * (p.PesoUnitarioGrama > 0 ? p.PesoUnitarioGrama : 1)));

                if (pesoTotalGramas < 1000)
                {
                    strPesoTotal = $"{pesoTotalGramas:N3} g";
                }
                else if (pesoTotalGramas < 1000000)
                {
                    double pesoKg = pesoTotalGramas / 1000;
                    strPesoTotal = $"{pesoKg:N3} kg";
                }
                else
                {
                    double pesoToneladas = pesoTotalGramas / 1000000;
                    strPesoTotal = $"{pesoToneladas:N3} t";
                }

                txtPesoProdutos.Content = $"Peso Total: {strPesoTotal}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEntSaidEstoque_Click(object sender, RoutedEventArgs e)
        {
            EntradaSaidaWindow entradaSaidaWindow = new(_fornecedorId, null);
            entradaSaidaWindow.ShowDialog();
            CarregarProdutosAcabando();
        }

        private void BtnAtualizarProdutos_Click(object sender, RoutedEventArgs e)
        {
            CarregarProdutosAcabando();
        }

        private void BtnExportarProdutos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CarregarProdutosAcabando();
                var produtos = listViewProdutosAcabando.ItemsSource as IEnumerable<ProdutoViewModel>;
                if (produtos == null || !produtos.Any())
                {
                    MessageBox.Show("Não há produtos para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Produtos Acabando");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Id");
                headerRow.CreateCell(1).SetCellValue("Modelo");
                headerRow.CreateCell(2).SetCellValue("Código");
                headerRow.CreateCell(3).SetCellValue("Fio");
                headerRow.CreateCell(4).SetCellValue("Milímetros");
                headerRow.CreateCell(5).SetCellValue("Tamanho");
                headerRow.CreateCell(6).SetCellValue("Fornecedor");
                headerRow.CreateCell(7).SetCellValue("Unidade");
                headerRow.CreateCell(8).SetCellValue("Quantidade Total");
                headerRow.CreateCell(9).SetCellValue("Quantidade Mínima");
                headerRow.CreateCell(10).SetCellValue("Peso Unidade (grama)");
                headerRow.CreateCell(11).SetCellValue("Ativo");
                headerRow.CreateCell(12).SetCellValue("Alteração");
                headerRow.CreateCell(13).SetCellValue("Descrição");


                int rowIndex = 1;
                foreach (var produto in produtos)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(produto.Id);
                    row.CreateCell(1).SetCellValue(produto.Modelo);
                    row.CreateCell(2).SetCellValue(produto.Codigo);
                    row.CreateCell(3).SetCellValue(produto.Fio);
                    row.CreateCell(4).SetCellValue(produto.Milimetros);
                    row.CreateCell(5).SetCellValue(produto.Tamanho);
                    row.CreateCell(6).SetCellValue(produto.FornecedorNome);
                    row.CreateCell(7).SetCellValue(produto.TipoUnidadeNome);
                    row.CreateCell(8).SetCellValue(produto.QuantidadeTotal);
                    row.CreateCell(9).SetCellValue(produto.QuantidadeMinima);
                    row.CreateCell(10).SetCellValue((produto.PesoUnitarioGrama > 0 ? produto.PesoUnitarioGrama : 1));
                    row.CreateCell(11).SetCellValue(produto.Ativo);
                    row.CreateCell(12).SetCellValue(produto.Alteracao.ToString("dd/MM/yyyy HH:mm"));
                    row.CreateCell(13).SetCellValue(produto.Descricao);
                }

                // Diálogo para salvar arquivo
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "ProdutosAcabando.xlsx"
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

        private void BtnFornecedores_Click(object sender, RoutedEventArgs e)
        {
            FornecedoresWindow fornecedoresWindow = new();
            fornecedoresWindow.ShowDialog();
            CarregarFornecedoresTabs();
            CarregarProdutosAcabando();
        }

        private void BtnProdutos_Click(object sender, RoutedEventArgs e)
        {
            ProdutosWindow produtosWindow = new(_fornecedorId);
            produtosWindow.ShowDialog();
            CarregarProdutosAcabando();
        }

        private void BtnTipos_Click(object sender, RoutedEventArgs e)
        {
            TiposUndsWindow tiposUndsWindow = new();
            tiposUndsWindow.ShowDialog();
            CarregarProdutosAcabando();
        }

        private void BtnEstoque_Click(object sender, RoutedEventArgs e)
        {
            EstoqueWindow estoqueWindow = new(_fornecedorId);
            estoqueWindow.ShowDialog();
            CarregarProdutosAcabando();
        }
    }
}