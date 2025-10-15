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
using System.Globalization;

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
            DataObject.AddPastingHandler(txtPesoUnitarioGrama, TxtPesoUnitarioGrama_Pasting);
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
            txtPesoUnitarioGrama.Text = tipoUnidade.PesoUnitarioGrama.ToString();
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double qtdeAviso = 0;
            double pesoUnitarioGrama = 1;

            string qtdeAvisoText = txtQtdeAviso.Text.Trim();
            string pesoUnitarioText = txtPesoUnitarioGrama.Text.Trim();

            qtdeAvisoText = NormalizarDecimal(qtdeAvisoText);
            pesoUnitarioText = NormalizarDecimal(pesoUnitarioText);

            if (!string.IsNullOrWhiteSpace(qtdeAvisoText))
            {
                if (!double.TryParse(qtdeAvisoText, NumberStyles.Any, CultureInfo.InvariantCulture, out qtdeAviso))
                {
                    qtdeAviso = 0;
                }
                if (qtdeAviso < 0)
                {
                    qtdeAviso = 0;
                }
            }
            if (!string.IsNullOrWhiteSpace(pesoUnitarioText))
            {
                if (!double.TryParse(pesoUnitarioText, NumberStyles.Any, CultureInfo.InvariantCulture, out pesoUnitarioGrama))
                {
                    pesoUnitarioGrama = 1;
                }
                if (pesoUnitarioGrama <= 0)
                {
                    pesoUnitarioGrama = 1;
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
                tipoUnidade.PesoUnitarioGrama = pesoUnitarioGrama;
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
            // Permite apenas dígitos e um separador decimal (vírgula ou ponto)
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d,.]+$");
        }

        private void TxtQtdeAviso_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string texto = (string)e.DataObject.GetData(typeof(string));
                // Permite vazio ou só dígitos e separador decimal
                if (!Regex.IsMatch(texto, @"^[\d,.]*$"))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TxtPesoUnitarioGrama_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite apenas dígitos e um separador decimal (vírgula ou ponto)
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d,.]+$");
        }

        private void TxtPesoUnitarioGrama_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string texto = (string)e.DataObject.GetData(typeof(string));
                // Permite vazio ou só dígitos e separador decimal
                if (!Regex.IsMatch(texto, @"^[\d,.]*$"))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private string NormalizarDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            int lastComma = input.LastIndexOf(',');
            int lastDot = input.LastIndexOf('.');
            if (lastComma == -1 && lastDot == -1)
                return input; // só dígitos
            if (lastComma == -1)
                return input.Replace(",", "").Replace('.', '.'); // só ponto
            if (lastDot == -1)
                return input.Replace(".", "").Replace(',', '.'); // só vírgula
            // ambos presentes
            if (lastComma > lastDot)
            {
                // vírgula é o separador decimal
                string semPonto = input.Substring(0, lastComma).Replace(".", "");
                string decimalPart = input.Substring(lastComma + 1);
                return semPonto + "." + decimalPart;
            }
            else
            {
                // ponto é o separador decimal
                string semVirgula = input.Substring(0, lastDot).Replace(",", "");
                string decimalPart = input.Substring(lastDot + 1);
                return semVirgula + "." + decimalPart;
            }
        }
    }
}
