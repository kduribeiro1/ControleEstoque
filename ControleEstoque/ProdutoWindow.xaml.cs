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
    /// Lógica interna para ProdutoWindow.xaml
    /// </summary>
    public partial class ProdutoWindow : Window
    {
        private Produto? _produtoEditando;

        // Construtor para novo produto
        public ProdutoWindow()
        {
            InitializeComponent();
            CarregarUnidades();
            chkAtivo.IsChecked = true;
            this.Title = "Cadastro de Produto";
            lblTitulo.Content = "Cadastro de Produto";
        }

        // Construtor para edição
        public ProdutoWindow(Produto produto) : this()
        {
            _produtoEditando = produto;
            PreencherCampos(produto);
            this.Title = "Edição de Produto";
            lblTitulo.Content = "Edição de Produto";
        }

        private void CarregarUnidades()
        {
            try
            {
                using var db = new EstoqueContext();
                List<TipoUnidadeCbmViewModel> lista = [new TipoUnidadeCbmViewModel() { Id = null, Nome = "Selecione uma unidade" },
                    .. db.TiposUnidades.Select(u => new TipoUnidadeCbmViewModel() { Id = u.Id, Nome = u.Nome }).OrderBy(u => u.Nome).ToList()
                ];
                cbUnidade.ItemsSource = lista;
                cbUnidade.DisplayMemberPath = "Nome";
                cbUnidade.SelectedValuePath = "Id";
                cbUnidade.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar unidades: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreencherCampos(Produto produto)
        {
            txtNome.Text = produto.Nome;
            cbUnidade.SelectedValue = produto.IdTipoUnidade;
            txtPrecoUnidade.Text = produto.PrecoUnidade.ToString();
            lblQuantidadeTotal.Content = produto.QuantidadeTotal.ToString();
            chkAtivo.IsChecked = produto.Ativo;
            lblAlteracao.Content = produto.Alteracao.ToString("dd/MM/yyyy HH:mm");
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                cbUnidade.SelectedValue == null)
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtPrecoUnidade.Text, out int precoUnidade))
            {
                precoUnidade = 0;
            }

            try
            {
                using var db = new EstoqueContext();

                if (_produtoEditando == null)
                {
                    // Novo produto
                    var produto = new Produto
                    {
                        Nome = txtNome.Text.Trim(),
                        IdTipoUnidade = (int)cbUnidade.SelectedValue,
                        PrecoUnidade = precoUnidade,
                        QuantidadeTotal = 0,
                        Ativo = chkAtivo.IsChecked ?? false,
                        Alteracao = DateTime.UtcNow.AddHours(-3)
                    };
                    db.Produtos.Add(produto);
                }
                else
                {
                    // Editar produto existente
                    var produto = db.Produtos.Find(_produtoEditando.Id);
                    if (produto == null)
                    {
                        MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    produto.Nome = txtNome.Text.Trim();
                    produto.IdTipoUnidade = (int)cbUnidade.SelectedValue;
                    produto.PrecoUnidade = precoUnidade;
                    produto.Ativo = chkAtivo.IsChecked ?? false;
                    produto.Alteracao = DateTime.UtcNow.AddHours(-3);
                }

                db.SaveChanges();

                MessageBox.Show("Produto salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TxtPrecoUnidade_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
