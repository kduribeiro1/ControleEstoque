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
            txtQtdeAviso.Text = tipoUnidade.QuantidadeAviso.ToString();
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQtdeAviso.Text, out int qtdeAviso))
            {
                qtdeAviso = 0;
            }

            try
            {
                using var db = new EstoqueContext();

                if (_tipoUnidadeEditando == null)
                {
                    // Novo tipo de unidade
                    var tipoUnidade = new TipoUnidade
                    {
                        Nome = txtNome.Text.Trim(),
                        QuantidadeAviso = qtdeAviso
                    };
                    db.TiposUnidades.Add(tipoUnidade);
                }
                else
                {
                    // Editar tipo unidade existente
                    var tipoUnidade = db.TiposUnidades.Find(_tipoUnidadeEditando.Id);
                    if (tipoUnidade == null)
                    {
                        MessageBox.Show("Tipo Unidade não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    tipoUnidade.Nome = txtNome.Text.Trim();
                    tipoUnidade.QuantidadeAviso = qtdeAviso;
                }

                db.SaveChanges();

                MessageBox.Show("Tipo Unidade salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar tipo unidade: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TxtQtdeAviso_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
