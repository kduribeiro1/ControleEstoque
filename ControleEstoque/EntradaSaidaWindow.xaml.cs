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
        private int IdEstoque { get; set; } = 0;
        private Estoque Estoque { get; set; } = new Estoque();
        private Produto? Produto { get; set; }

        public EntradaSaidaWindow(int? idProduto = null)
        {
            InitializeComponent();
            Estoque = new Estoque();
            IdEstoque = 0;
            try
            {
                using var context = new EstoqueContext();
                List<Produto> produtos =
                [
                    new Produto { Id = 0, Nome = "Selecione um produto", TipoUnidade = null! },
                    .. context.Produtos.Include(p => p.TipoUnidade)
                        .Where(p => p.Ativo)
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
                Produto = cbProduto.SelectedItem as Produto;
                if (Produto?.TipoUnidade?.Nome != null)
                {
                    txtTituloQtde.Text = $"Quantidade: ({Produto.TipoUnidade.Nome})";
                }
                else
                {
                    txtTituloQtde.Text = "Quantidade:";
                }
            }
            else
            {
                txtTituloQtde.Text = "Quantidade:";
            }
            dpDataEntradaSaida.SelectedDate = DateTime.UtcNow.AddHours(-3);
            txtHoraEntradaSaida.Text = DateTime.UtcNow.AddHours(-3).ToString("HH:mm");
        }

        public EntradaSaidaWindow(int id, bool editar)
        {
            InitializeComponent();
            IdEstoque = id;
            try
            {
                using var context = new EstoqueContext();
                List<Produto> produtos =
                [
                    new Produto { Id = 0, Nome = "Selecione um produto", TipoUnidade = null! },
                    .. context.Produtos.Include(p => p.TipoUnidade)
                        .Where(p => p.Ativo)
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
            using (var context = new EstoqueContext())
            {
                var estoque = context.Estoques.Include(p => p.Produto).Include(c => c.Produto.TipoUnidade).FirstOrDefault(e => e.Id == id);
                if (estoque != null)
                {
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
                    }
                    else
                    {
                        txtTituloQtde.Text = "Quantidade:";
                    }
                }
            }
            dpDataEntradaSaida.IsEnabled = editar;
            cbProduto.IsEnabled = editar;
            txtQuantidade.IsEnabled = editar;
            cbTipoMovimento.IsEnabled = editar;
            txtObservacao.IsEnabled = editar;
            txtHoraEntradaSaida.IsEnabled = editar;
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
            if (cbProduto.SelectedItem is Produto produtoSelecionado && produtoSelecionado.Id > 0)
            {
                Estoque.Produto = produtoSelecionado;
                Produto = produtoSelecionado;
                txtTituloQtde.Text = produtoSelecionado.TipoUnidade?.Nome != null
                    ? $"Quantidade: ({produtoSelecionado.TipoUnidade.Nome})"
                    : "Quantidade:";
            }
            else
            {
                Estoque.Produto = null!;
                txtTituloQtde.Text = "Quantidade:";
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
                var produtoSelecionado = cbProduto.SelectedItem as Produto;
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

                    Estoque.Produto = produtoSelecionado;
                    Estoque.DataEntradaSaida = dataHoraEntradaSaida;
                    Estoque.Quantidade = int.Parse(txtQuantidade.Text);
                    Estoque.Entrada = cbTipoMovimento.SelectedValue?.ToString() == "1";
                    Estoque.Observacao = txtObservacao.Text;
                    try
                    {
                        using (var _context = new EstoqueContext())
                        {
                            if (Estoque.Entrada)
                            {
                                produtoSelecionado.QuantidadeTotal += Estoque.Quantidade;
                            }
                            else
                            {
                                produtoSelecionado.QuantidadeTotal -= Estoque.Quantidade;
                            }
                            produtoSelecionado.Alteracao = DateTime.Now;
                            _context.Produtos.Update(produtoSelecionado);
                            _context.Estoques.Update(Estoque);
                            _context.SaveChanges();
                        }
                        MessageBox.Show("Registro atualizado com sucesso!");
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao atualizar registro: {ex.Message}");
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
                        Produto = produtoSelecionado,
                        Quantidade = int.Parse(txtQuantidade.Text),
                        DataEntradaSaida = dataHoraEntradaSaida,
                        Entrada = cbTipoMovimento.SelectedValue?.ToString() == "1",
                        Observacao = txtObservacao.Text
                    };
                    try
                    {
                        using (var _context = new EstoqueContext())
                        {
                            if (Estoque.Entrada)
                            {
                                produtoSelecionado.QuantidadeTotal += Estoque.Quantidade;
                            }
                            else
                            {
                                produtoSelecionado.QuantidadeTotal -= Estoque.Quantidade;
                            }
                            produtoSelecionado.Alteracao = DateTime.Now;
                            _context.Produtos.Update(produtoSelecionado);
                            _context.Estoques.Add(Estoque);
                            _context.SaveChanges();
                        }
                        MessageBox.Show("Registro salvo com sucesso!");
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao salvar registro: {ex.Message}");
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
