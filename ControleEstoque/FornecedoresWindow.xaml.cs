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
    /// Lógica interna para FornecedoresWindow.xaml
    /// </summary>
    public partial class FornecedoresWindow : Window
    {
        public FornecedoresWindow()
        {
            InitializeComponent();
            CarregarFornecedores();
            txtFiltroNome.TextChanged += (s, e) => CarregarFornecedores(txtFiltroNome.Text);
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            FornecedorWindow fornecedorWindow = new();
            if (fornecedorWindow.ShowDialog() == true)
            {
                CarregarFornecedores(txtFiltroNome.Text);
            }
        }
        private void CarregarFornecedores(string filtro = "")
        {
            try
            {
                lstFornecedores.ItemsSource = EstoqueEntityManager.ObterFornecedores(filtro, null).Select(f => new FornecedorViewModel(f)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fornecedorSelecionada = lstFornecedores.SelectedItem as FornecedorViewModel;
                if (fornecedorSelecionada == null)
                {
                    MessageBox.Show("Selecione um fornecedor para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                FornecedorWindow fornecedorWindow = new(fornecedorSelecionada.ToFornecedor());
                if (fornecedorWindow.ShowDialog() == true)
                {
                    CarregarFornecedores(txtFiltroNome.Text);
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
                FornecedorViewModel? itemSelecionado = lstFornecedores.SelectedItem as FornecedorViewModel;
                if (itemSelecionado != null)
                {
                    if (MessageBox.Show("Tem certeza que deseja deletar este item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (EstoqueEntityManager.DeletarFornecedor(itemSelecionado.ToFornecedor()))
                        {
                            MessageBox.Show("Fornecedor deletado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            CarregarFornecedores(txtFiltroNome.Text);
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
                MessageBox.Show($"Erro ao deletar fornecedor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CarregarFornecedores(txtFiltroNome.Text);
                MessageBox.Show("Lista de fornecedores atualizada com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void BtnImportar_Click(object sender, RoutedEventArgs e)
        //{

        //}

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var tipos = lstFornecedores.ItemsSource as IEnumerable<FornecedorViewModel>;
                if (tipos == null || !tipos.Any())
                {
                    MessageBox.Show("Não há fornecedores para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Tipos de Unidades");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Id");
                headerRow.CreateCell(1).SetCellValue("Nome");
                headerRow.CreateCell(2).SetCellValue("Ativo");

                int rowIndex = 1;
                foreach (var tipo in tipos)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(tipo.Id);
                    row.CreateCell(1).SetCellValue(tipo.Nome);
                    row.CreateCell(2).SetCellValue(tipo.Ativo);
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "Fornecedores.xlsx"
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
                MessageBox.Show($"Erro ao exportar fornecedores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
