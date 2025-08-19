using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Lógica interna para FornecedorWindow.xaml
    /// </summary>
    public partial class FornecedorWindow : Window
    {
        private Fornecedor? _fornecedorEditando;

        // Construtor para novo produto
        public FornecedorWindow()
        {
            InitializeComponent();
            this.Title = "Cadastro de Fornecedor";
            lblTitulo.Content = "Cadastro de Fornecedor";
        }

        // Construtor para edição
        public FornecedorWindow(Fornecedor fornecedor) : this()
        {
            _fornecedorEditando = fornecedor;
            PreencherCampos(fornecedor);
            this.Title = "Edição de Fornecedor";
            lblTitulo.Content = "Edição de Fornecedor";
        }

        private void PreencherCampos(Fornecedor fornecedor)
        {
            txtNome.Text = fornecedor.Nome;
            chkAtivo.IsChecked = fornecedor.Ativo;
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Fornecedor? fornecedor = new();
                if (_fornecedorEditando != null)
                {
                    fornecedor = EstoqueEntityManager.ObterFornecedorPorId(_fornecedorEditando.Id);
                }
                if (fornecedor == null)
                {
                    MessageBox.Show("Fornecedor não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                fornecedor.Nome = txtNome.Text.Trim();
                fornecedor.Ativo = chkAtivo.IsChecked.HasValue ? chkAtivo.IsChecked.Value : false;
                if (!EstoqueEntityManager.LancarFornecedor(fornecedor))
                {
                    return;
                }
                MessageBox.Show("Fornecedor salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar o fornecedor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
