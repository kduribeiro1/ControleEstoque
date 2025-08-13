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
    
    public class ProdutoBaixoEstoque
    {
        public string Nome { get; set; } = string.Empty;
        public string TipoUnidade { get; set; } = string.Empty;
        public int QuantidadeTotal { get; set; }
        public int QuantidadeAviso { get; set; }
    }

    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
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

        private void CarregarProdutosAcabando()
        {
            try
            {
                using var db = new EstoqueContext();
                // Busca produtos cujo QuantidadeTotal <= QuantidadeAviso do TipoUnidade
                var produtosBaixoEstoque = db.Produtos
                    .Include(p => p.TipoUnidade)
                    .Where(p => p.QuantidadeTotal <= p.TipoUnidade.QuantidadeAviso)
                    .Select(p => new ProdutoBaixoEstoque
                    {
                        Nome = p.Nome,
                        TipoUnidade = p.TipoUnidade.Nome,
                        QuantidadeTotal = p.QuantidadeTotal,
                        QuantidadeAviso = p.TipoUnidade.QuantidadeAviso
                    })
                    .ToList();

                listViewProdutosAcabando.ItemsSource = produtosBaixoEstoque;
                txtQtdeProdutos.Content = $"Quantidade de Produtos com estoque baixo: {produtosBaixoEstoque.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEntSaidEstoque_Click(object sender, RoutedEventArgs e)
        {
            EntradaSaidaWindow entradaSaidaWindow = new();
            if (entradaSaidaWindow.ShowDialog() == true)
            {
                CarregarProdutosAcabando();
            }
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
                var produtos = listViewProdutosAcabando.ItemsSource as IEnumerable<ProdutoBaixoEstoque>;
                if (produtos == null || !produtos.Any())
                {
                    MessageBox.Show("Não há produtos para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Produtos Acabando");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Nome");
                headerRow.CreateCell(1).SetCellValue("Unidade");
                headerRow.CreateCell(2).SetCellValue("Quantidade Total");
                headerRow.CreateCell(3).SetCellValue("Quantidade Aviso");

                int rowIndex = 1;
                foreach (var produto in produtos)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(produto.Nome);
                    row.CreateCell(1).SetCellValue(produto.TipoUnidade);
                    row.CreateCell(2).SetCellValue(produto.QuantidadeTotal);
                    row.CreateCell(3).SetCellValue(produto.QuantidadeAviso);
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

        private void BtnProdutos_Click(object sender, RoutedEventArgs e)
        {
            ProdutosWindow produtosWindow = new();
            if (produtosWindow.ShowDialog() == true)
            {
                CarregarProdutosAcabando();
            }
        }

        private void BtnTipos_Click(object sender, RoutedEventArgs e)
        {
            TiposUndsWindow tiposUndsWindow = new();
            if (tiposUndsWindow.ShowDialog() == true)
            {
                CarregarProdutosAcabando();
            }
        }

        private void BtnEstoque_Click(object sender, RoutedEventArgs e)
        {
            EstoqueWindow estoqueWindow = new();
            if (estoqueWindow.ShowDialog() == true)
            {
                CarregarProdutosAcabando();
            }
        }
    }
}