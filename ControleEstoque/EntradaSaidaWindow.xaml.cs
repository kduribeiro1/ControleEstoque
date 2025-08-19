using Microsoft.EntityFrameworkCore;
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
    /// Lógica interna para EntradaSaidaWindow.xaml
    /// </summary>
    
    public partial class EntradaSaidaWindow : Window
    {
        private int? _fornecedorId;
        private bool _editar = false;
        private int IdEstoque { get; set; } = 0;
        private Estoque Estoque { get; set; } = new Estoque();
        private Produto? Produto { get; set; }
        private Fornecedor? Fornecedor { get; set; }

        public EntradaSaidaWindow(int? idFornecedor, int? idProduto = null)
        {
            InitializeComponent();
            _fornecedorId = idFornecedor;
            Fornecedor = null;

            _editar = true;
            try
            {
                List<IdNomeViewModel> fornecedores =
                [
                    new IdNomeViewModel { Id = 0, Nome = "Todos os fornecedores" },
                        .. EstoqueEntityManager.ObterFornecedores()
                            .Select(p => new IdNomeViewModel(p))
                            .OrderBy(p => p.Nome).ToList(),
                    ];
                cbFornecedor.ItemsSource = fornecedores;
                cbFornecedor.DisplayMemberPath = "Nome";
                cbFornecedor.SelectedValuePath = "Id";
                cbFornecedor.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (_fornecedorId.HasValue && _fornecedorId.Value > 0)
            {
                Fornecedor = EstoqueEntityManager.ObterFornecedorPorId(_fornecedorId.Value);
                if (Fornecedor != null)
                {
                    cbFornecedor.SelectedValue = Fornecedor.Id;
                }
            }
            _editar = false;

            Estoque = new Estoque();
            IdEstoque = 0;
            try
            {
                List<IdNomeViewModel> produtos =
                [
                    new IdNomeViewModel { Id = 0, Nome = "Selecione um produto" },
                    .. EstoqueEntityManager.ObterProdutosPorFornecedor(_fornecedorId)
                        .Select(p => new IdNomeViewModel(p))
                        .OrderBy(p => p.Nome).ToList(),
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
            if (idProduto.HasValue)
            {
                cbProduto.SelectedValue = idProduto;
                IdNomeViewModel? prodSelecionado = cbProduto.SelectedItem as IdNomeViewModel;
                if (prodSelecionado == null || prodSelecionado.Id <= 0)
                {
                    Produto = null;
                }
                else
                {
                    Produto = EstoqueEntityManager.ObterProdutoPorId(prodSelecionado.Id);
                }
                if (Produto?.TipoUnidade?.Nome != null)
                {
                    txtTituloQtde.Text = $"Quantidade: ({Produto.TipoUnidade.Nome})";
                    txtTituloQtdeTotal.Text = $"Quantidade Total: ({Produto.TipoUnidade.Nome})";
                    txtQuantidadeTotal.Text = Produto.QuantidadeTotal.ToString();
                }
                else
                {
                    txtTituloQtde.Text = "Quantidade:";
                    txtTituloQtdeTotal.Text = "Quantidade Total:";
                    txtQuantidadeTotal.Text = "0";
                }
            }
            else
            {
                txtTituloQtde.Text = "Quantidade:";
                txtTituloQtdeTotal.Text = "Quantidade Total:";
                txtQuantidadeTotal.Text = "0";
            }
            dpDataEntradaSaida.SelectedDate = DateTime.UtcNow.AddHours(-3);
            txtHoraEntradaSaida.Text = DateTime.UtcNow.AddHours(-3).ToString("HH:mm");
            cbTipoMovimento.SelectedValue = "0"; // Saída por padrão
        }

        public EntradaSaidaWindow(int? idFornecedor, int id, bool editar)
        {
            InitializeComponent();
            _fornecedorId = idFornecedor;
            IdEstoque = id;

            _editar = true;
            try
            {
                List<IdNomeViewModel> fornecedores =
                [
                    new IdNomeViewModel { Id = 0, Nome = "Todos os fornecedores" },
                        .. EstoqueEntityManager.ObterFornecedores()
                            .Select(p => new IdNomeViewModel(p))
                            .OrderBy(p => p.Nome).ToList(),
                    ];
                cbFornecedor.ItemsSource = fornecedores;
                cbFornecedor.DisplayMemberPath = "Nome";
                cbFornecedor.SelectedValuePath = "Id";
                cbFornecedor.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (_fornecedorId.HasValue && _fornecedorId.Value > 0)
            {
                Fornecedor = EstoqueEntityManager.ObterFornecedorPorId(_fornecedorId.Value);
                if (Fornecedor != null)
                {
                    cbFornecedor.SelectedValue = Fornecedor.Id;
                }
            }
            _editar = false;

            try
            {
                List<IdNomeViewModel> produtos =
                [
                    new IdNomeViewModel { Id = 0, Nome = "Selecione um produto" },
                    .. EstoqueEntityManager.ObterProdutosPorFornecedor(_fornecedorId)
                        .Select(p => new IdNomeViewModel(p))
                        .OrderBy(p => p.Nome).ToList(),
                ];
                cbProduto.ItemsSource = produtos;
                cbProduto.DisplayMemberPath = "Nome";
                cbProduto.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Estoque? estoque = EstoqueEntityManager.ObterEstoquePorId(id);
            if (estoque == null)
            {
                MessageBox.Show("Registro não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            else
            {
                cbFornecedor.IsEnabled = false;
                cbProduto.IsEnabled = false;
                cbTipoMovimento.IsEnabled = false;

                Produto = estoque.Produto;
                Estoque = estoque;
                dpDataEntradaSaida.SelectedDate = Estoque.DataEntradaSaida;
                txtHoraEntradaSaida.Text = Estoque.DataEntradaSaida.ToString("HH:mm");
                cbProduto.SelectedValue = Estoque.Produto.Id;
                txtQuantidade.Text = Estoque.Quantidade.ToString();
                cbTipoMovimento.SelectedValue = Estoque.Entrada ? "1" : "0";
                txtObservacao.Text = Estoque.Observacao;
                
                // Atualiza o título da quantidade com a unidade do produto selecionado
                if (Produto?.TipoUnidade?.Nome != null)
                {
                    txtTituloQtde.Text = $"Quantidade: ({Produto.TipoUnidade.Nome})";
                    txtTituloQtdeTotal.Text = $"Quantidade Total: ({Produto.TipoUnidade.Nome})";
                    txtQuantidadeTotal.Text = Produto.QuantidadeTotal.ToString();
                }
                else
                {
                    txtTituloQtde.Text = "Quantidade:";
                    txtTituloQtdeTotal.Text = "Quantidade Total:";
                    txtQuantidadeTotal.Text = "0";
                }

                txtQuantidade.IsEnabled = editar;
                txtObservacao.IsEnabled = editar;
                dpDataEntradaSaida.IsEnabled = editar;
                txtHoraEntradaSaida.IsEnabled = editar;
            }
        }

        // Permite apenas dígitos e dois pontos, e limita a estrutura HH:mm
        private void TxtHoraEntradaSaida_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite apenas números e dois pontos
            Regex regex = new Regex(@"^[0-9:]$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        // Valida o formato ao perder o foco
        private void TxtHoraEntradaSaida_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(txtHoraEntradaSaida.Text, @"^(?:[01]\d|2[0-3]):[0-5]\d$"))
            {
                MessageBox.Show("Informe a hora no formato HH:mm (ex: 14:30)", "Hora inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtHoraEntradaSaida.Focus();
            }
        }

        private void CbProduto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbProduto.SelectedItem is IdNomeViewModel produtoSelecionado && produtoSelecionado.Id > 0)
            {
                Produto = EstoqueEntityManager.ObterProdutoPorId(produtoSelecionado.Id);
                if (Produto == null)
                {
                    Estoque.Produto = null!;
                    txtTituloQtde.Text = "Quantidade:";
                    txtTituloQtdeTotal.Text = "Quantidade Total:";
                    txtQuantidadeTotal.Text = "0";
                    MessageBox.Show("Produto não encontrado.");
                    return;
                }
                Estoque.Produto = Produto;
                txtTituloQtde.Text = Produto.TipoUnidade?.Nome != null
                    ? $"Quantidade: ({Produto.TipoUnidade.Nome})"
                    : "Quantidade:";
                txtTituloQtdeTotal.Text = Produto.TipoUnidade?.Nome != null
                    ? $"Quantidade Total: ({Produto.TipoUnidade.Nome})"
                    : "Quantidade Total:";
                txtQuantidadeTotal.Text = Produto.QuantidadeTotal.ToString();


                if (cbFornecedor.SelectedItem is IdNomeViewModel fornecedorSelecionado && fornecedorSelecionado.Id < 1)
                {
                    cbFornecedor.SelectedValue = Produto.FornecedorId;
                }
            }
            else
            {
                Produto = null;
                Estoque.Produto = null!;
                txtTituloQtde.Text = "Quantidade:";
                txtTituloQtdeTotal.Text = "Quantidade Total:";
                txtQuantidadeTotal.Text = "0";
            }
        }

        private void CbFornecedor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_editar)
                return;

            Produto? produtoAntes = Produto;

            if (cbFornecedor.SelectedItem is IdNomeViewModel fornecedorSelecionado && fornecedorSelecionado.Id > 0)
            {
                _fornecedorId = fornecedorSelecionado.Id;
                Fornecedor = EstoqueEntityManager.ObterFornecedorPorId(fornecedorSelecionado.Id);
                if (Fornecedor == null)
                {
                    _fornecedorId = null;
                    MessageBox.Show("Fornecedor não encontrado.");
                }
            }
            else
            {
                _fornecedorId = null;
            }
            try
            {
                List<IdNomeViewModel> produtos =
                [
                    new IdNomeViewModel { Id = 0, Nome = "Selecione um produto" },
                        .. EstoqueEntityManager.ObterProdutosPorFornecedor(_fornecedorId)
                            .Select(p => new IdNomeViewModel(p))
                            .OrderBy(p => p.Nome).ToList(),
                    ];
                cbProduto.ItemsSource = produtos;
                cbProduto.DisplayMemberPath = "Nome";
                cbProduto.SelectedValuePath = "Id";
                int idBuscar = produtoAntes?.Id ?? 0;
                cbProduto.SelectedValue = produtos.Where(p => p.Id == idBuscar).Any() ? idBuscar : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtQuantidade_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Aceita apenas números e vírgula/ponto para decimais
            Regex regex = new(@"^[0-9]*(?:[\.,][0-9]*)?$");
            string textoAtual = txtQuantidade.Text.Insert(txtQuantidade.SelectionStart, e.Text);
            e.Handled = !regex.IsMatch(textoAtual);
        }

        private void TxtQuantidade_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string texto = (string)e.DataObject.GetData(typeof(string));
                Regex regex = new(@"^[0-9]*(?:[\.,][0-9]*)?$");
                if (!regex.IsMatch(texto))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var produtoSelecionado = cbProduto.SelectedItem as IdNomeViewModel;
                if (produtoSelecionado == null)
                {
                    MessageBox.Show("Selecione um produto.");
                    return;
                }
                if (produtoSelecionado.Id < 1)
                {
                    MessageBox.Show("Selecione um produto.");
                    return;
                }
                if (!Regex.IsMatch(txtHoraEntradaSaida.Text, @"^(?:[01]\d|2[0-3]):[0-5]\d$"))
                {
                    MessageBox.Show("Informe a hora no formato HH:mm (ex: 14:30)", "Hora inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtHoraEntradaSaida.Focus();
                    return;
                }
                if (!int.TryParse(txtQuantidade.Text, out int quantidade) || quantidade < 0)
                {
                    MessageBox.Show("Informe uma quantidade válida.", "Quantidade inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (IdEstoque > 0)
                {
                    if (Estoque == null)
                    {
                        using var context1 = new EstoqueContext();
                        var estoque = context1.Estoques.FirstOrDefault(e => e.Id == IdEstoque);
                        if (estoque == null)
                        {
                            MessageBox.Show("Registro não encontrado.");
                            return;
                        }
                        Estoque = estoque;
                    }

                    DateTime data = dpDataEntradaSaida.SelectedDate ?? DateTime.UtcNow.AddHours(-3);
                    TimeSpan hora;
                    if (!TimeSpan.TryParse(txtHoraEntradaSaida.Text, out hora))
                        hora = TimeSpan.Zero; // ou trate como desejar

                    DateTime dataHoraEntradaSaida = data.Date + hora;

                    Estoque.ProdutoId = produtoSelecionado.Id;
                    Estoque.DataEntradaSaida = dataHoraEntradaSaida;
                    Estoque.Quantidade = int.Parse(txtQuantidade.Text);
                    Estoque.Entrada = cbTipoMovimento.SelectedValue?.ToString() == "1";
                    Estoque.Observacao = txtObservacao.Text;
                    if (EstoqueEntityManager.LancarEstoque(Estoque))
                    {
                        MessageBox.Show("Registro atualizado com sucesso!");
                        this.Close();
                    }
                }
                else
                {
                    DateTime data = dpDataEntradaSaida.SelectedDate ?? DateTime.UtcNow.AddHours(-3);
                    TimeSpan hora;
                    if (!TimeSpan.TryParse(txtHoraEntradaSaida.Text, out hora))
                        hora = TimeSpan.Zero; // ou trate como desejar

                    DateTime dataHoraEntradaSaida = data.Date + hora;

                    Estoque = new Estoque
                    {
                        ProdutoId = produtoSelecionado.Id,
                        Quantidade = int.Parse(txtQuantidade.Text),
                        DataEntradaSaida = dataHoraEntradaSaida,
                        Entrada = cbTipoMovimento.SelectedValue?.ToString() == "1",
                        Observacao = txtObservacao.Text
                    };
                    if (EstoqueEntityManager.LancarEstoque(Estoque))
                    {
                        MessageBox.Show("Registro salvo com sucesso!");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
