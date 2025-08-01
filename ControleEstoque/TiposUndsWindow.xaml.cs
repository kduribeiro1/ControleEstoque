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
    /// Lógica interna para TiposUndsWindow.xaml
    /// </summary>
    public partial class TiposUndsWindow : Window
    {
        public TiposUndsWindow()
        {
            InitializeComponent();
            txtFiltroNome.TextChanged += (s, e) => CarregarTiposUnidades(txtFiltroNome.Text);
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            TipoUnidadeWindow tipoUnidadeWindow = new TipoUnidadeWindow();
            if (tipoUnidadeWindow.ShowDialog() == true)
            {
                CarregarTiposUnidades(txtFiltroNome.Text);
            }
        }
        private void CarregarTiposUnidades(string filtro = "")
        {
            try
            {
                using var db = new EstoqueContext();
                var tipos = db.TiposUnidades
                    .Where(t => string.IsNullOrWhiteSpace(filtro) || t.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.Nome)
                    .ToList();

                lstTiposUnidades.ItemsSource = tipos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar tipos de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tipoUnidadeSelecionada = lstTiposUnidades.SelectedItem as TipoUnidade;
                if (tipoUnidadeSelecionada == null)
                {
                    MessageBox.Show("Selecione um tipo de unidade para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TipoUnidadeWindow tipoUnidadeWindow = new TipoUnidadeWindow(tipoUnidadeSelecionada);
                if (tipoUnidadeWindow.ShowDialog() == true)
                {
                    CarregarTiposUnidades(txtFiltroNome.Text);
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
                TipoUnidade? itemSelecionado = lstTiposUnidades.SelectedItem as TipoUnidade;
                if (itemSelecionado != null)
                {
                    int idEstoque = itemSelecionado.Id;
                    if (MessageBox.Show("Tem certeza que deseja deletar este item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        using var context = new EstoqueContext();
                        var produtos = context.Produtos.Where(p => p.IdTipoUnidade == itemSelecionado.Id).ToList();
                        if (produtos.Any())
                        {
                            MessageBox.Show("Não é possível deletar este tipo de unidade porque existem produtos associados a ele.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        context.TiposUnidades.Remove(itemSelecionado);
                        context.SaveChanges();
                        CarregarTiposUnidades(txtFiltroNome.Text);
                    }
                }
                else
                {
                    MessageBox.Show("Selecione um item para deletar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar tipo de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filtro = txtFiltroNome.Text ?? string.Empty;
                using var db = new EstoqueContext();
                var tipos = db.TiposUnidades
                    .Where(t => string.IsNullOrWhiteSpace(filtro) || t.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.Nome)
                    .ToList();

                lstTiposUnidades.ItemsSource = tipos;
                MessageBox.Show("Lista de tipos de unidade atualizada com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar tipos de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void BtnImportar_Click(object sender, RoutedEventArgs e)
        //{

        //}

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var tipos = lstTiposUnidades.ItemsSource as IEnumerable<TipoUnidade>;
                if (tipos == null || !tipos.Any())
                {
                    MessageBox.Show("Não há tipos de unidade para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Tipos de Unidades");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Id");
                headerRow.CreateCell(1).SetCellValue("Nome");
                headerRow.CreateCell(2).SetCellValue("Quantidade Aviso");

                int rowIndex = 1;
                foreach (var tipo in tipos)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(tipo.Id);
                    row.CreateCell(1).SetCellValue(tipo.Nome);
                    row.CreateCell(2).SetCellValue(tipo.QuantidadeAviso);
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "TiposUnidades.xlsx"
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
                MessageBox.Show($"Erro ao exportar tipos de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
