using System;
using System.Collections.Generic;
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
using System.Text.RegularExpressions;

namespace ControleEstoque
{
    /// <summary>
    /// Lógica interna para TipoUnidadeWindow.xaml
    /// </summary>
    public partial class TipoUnidadeWindow : Window
    {
        private TipoUnidade? _tipoUnidadeEditando;

        // Construtor para novo produto
        public TipoUnidadeWindow()
        {
            InitializeComponent();
            this.Title = "Cadastro de Tipo de Unidade";
            lblTitulo.Content = "Cadastro de Tipo de Unidade";
            DataObject.AddPastingHandler(txtQtdeAviso, TxtQtdeAviso_Pasting);
        }

        // Construtor para edição
        public TipoUnidadeWindow(TipoUnidade tipoUnidade) : this()
        {
            _tipoUnidadeEditando = tipoUnidade;
            PreencherCampos(tipoUnidade);
            this.Title = "Edição de Tipo de Unidade";
            lblTitulo.Content = "Edição de Tipo de Unidade";
        }

        private void PreencherCampos(TipoUnidade tipoUnidade)
        {
            txtNome.Text = tipoUnidade.Nome;
            txtQtdeAviso.Text = tipoUnidade.QuantidadeMinima.ToString();
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int qtdeAviso = 0;
            if (!string.IsNullOrWhiteSpace(txtQtdeAviso.Text))
            {
                if (!int.TryParse(txtQtdeAviso.Text, out qtdeAviso) || qtdeAviso < 0)
                {
                    MessageBox.Show("Informe um valor inteiro maior ou igual a zero para quantidade de aviso.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                TipoUnidade? tipoUnidade = new();
                if (_tipoUnidadeEditando != null)
                {
                    tipoUnidade = EstoqueEntityManager.ObterTipoUnidadePorId(_tipoUnidadeEditando.Id);
                }
                if (tipoUnidade == null)
                {
                    MessageBox.Show("Tipo de Unidade não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                tipoUnidade.Nome = txtNome.Text.Trim();
                tipoUnidade.QuantidadeMinima = qtdeAviso;
                if (!EstoqueEntityManager.LancarTipoUnidade(tipoUnidade))
                {
                    return;
                }
                MessageBox.Show("Tipo de Unidade salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar o tipo de unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TxtQtdeAviso_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite apenas dígitos (0-9)
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void TxtQtdeAviso_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string texto = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(texto, @"^\d*$")) // Permite vazio ou só dígitos
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
