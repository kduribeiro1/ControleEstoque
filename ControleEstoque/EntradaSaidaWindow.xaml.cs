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
        private Estoque? Estoque { get; set; } = new Estoque();

        public EntradaSaidaWindow(int? idProduto = null)
        {
            InitializeComponent();
            Estoque = new Estoque();
            IdEstoque = 0;
            try
            {
                using var context = new EstoqueContext();
                List<ProdutoCmbViewModel> produtos =
                [
                    new ProdutoCmbViewModel { Id = null, Nome = "Selecione um produto", Unidade = string.Empty },
                    .. context.Produtos
                        .Where(p => p.Ativo)
                        .Select(p => new ProdutoCmbViewModel { Id = p.Id, Nome = p.Nome, Unidade = p.TipoUnidade.Nome })
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
                if (cbProduto.SelectedItem is ProdutoCmbViewModel produto && produto.Unidade != null)
                {
                    txtTituloQtde.Text = $"Quantidade: ({produto.Unidade})";
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
                List<ProdutoCmbViewModel> produtos =
                [
                    new ProdutoCmbViewModel { Id = 0, Nome = "Selecione um produto", Unidade = string.Empty },
                        .. context.Produtos
                            .Where(p => p.Ativo)
                            .Select(p => new ProdutoCmbViewModel { Id = p.Id, Nome = p.Nome, Unidade = p.TipoUnidade.Nome })
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
                Estoque = context.Estoques.FirstOrDefault(e => e.Id == id);
                if (Estoque != null)
                {
                    dpDataEntradaSaida.SelectedDate = Estoque.DataEntradaSaida;
                    txtHoraEntradaSaida.Text = Estoque.DataEntradaSaida.ToString("HH:mm");
                    cbProduto.SelectedValue = Estoque.IdProduto;
                    txtQuantidade.Text = Estoque.Quantidade.ToString();
                    cbTipoMovimento.SelectedValue = Estoque.Entrada ? "1" : "0";
                    txtObservacao.Text = Estoque.Observacao;
                    // Atualiza o título da quantidade com a unidade do produto selecionado
                    if (cbProduto.SelectedItem is ProdutoCmbViewModel produto && produto.Unidade != null)
                    {
                        txtTituloQtde.Text = $"Quantidade: ({produto.Unidade})";
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
            if (cbProduto.SelectedItem is ProdutoCmbViewModel produto && produto.Unidade != null)
            {
                txtTituloQtde.Text = $"Quantidade: ({produto.Unidade})";
            }
            else
            {
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
                if (cbProduto.SelectedValue == null)
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
                if (IdEstoque > 0)
                {
                    if (Estoque == null)
                    {
                        using var context1 = new EstoqueContext();
                        Estoque = context1.Estoques.FirstOrDefault(e => e.Id == IdEstoque);
                        if (Estoque == null)
                        {
                            MessageBox.Show("Registro não encontrado.");
                            return;
                        }
                    }

                    DateTime data = dpDataEntradaSaida.SelectedDate ?? DateTime.UtcNow.AddHours(-3);
                    TimeSpan hora;
                    if (!TimeSpan.TryParse(txtHoraEntradaSaida.Text, out hora))
                        hora = TimeSpan.Zero; // ou trate como desejar

                    DateTime dataHoraEntradaSaida = data.Date + hora;

                    Estoque.DataEntradaSaida = dataHoraEntradaSaida;
                    Estoque.IdProduto = (int)cbProduto.SelectedValue;
                    Estoque.Quantidade = int.Parse(txtQuantidade.Text);
                    Estoque.Entrada = cbTipoMovimento.SelectedValue?.ToString() == "1";
                    Estoque.Observacao = txtObservacao.Text;
                    try
                    {
                        Produto? produtoEdt = null;
                        using (var context = new EstoqueContext())
                        {
                            produtoEdt = context.Produtos.FirstOrDefault(p => p.Id == Estoque.IdProduto) ?? null;
                        }
                        using (var _context = new EstoqueContext())
                        {
                            if (produtoEdt != null)
                            {
                                if (Estoque.Entrada)
                                {
                                    produtoEdt.QuantidadeTotal += Estoque.Quantidade;
                                }
                                else
                                {
                                    produtoEdt.QuantidadeTotal -= Estoque.Quantidade;
                                }
                                produtoEdt.Alteracao = DateTime.Now;
                                _context.Produtos.Update(produtoEdt);
                            }
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
                        IdProduto = (int)cbProduto.SelectedValue,
                        Quantidade = int.Parse(txtQuantidade.Text),
                        DataEntradaSaida = dataHoraEntradaSaida,
                        Entrada = cbTipoMovimento.SelectedValue?.ToString() == "1",
                        Observacao = txtObservacao.Text
                    };
                    try
                    {
                        Produto? produtoEdt = null;
                        using (var context = new EstoqueContext())
                        {
                            produtoEdt = context.Produtos.FirstOrDefault(p => p.Id == Estoque.IdProduto) ?? null;
                        }

                        using (var _context = new EstoqueContext())
                        {
                            if (produtoEdt != null)
                            {
                                if (Estoque.Entrada)
                                {
                                    produtoEdt.QuantidadeTotal += Estoque.Quantidade;
                                }
                                else
                                {
                                    produtoEdt.QuantidadeTotal -= Estoque.Quantidade;
                                }
                                produtoEdt.Alteracao = DateTime.Now;
                                _context.Produtos.Update(produtoEdt);
                            }
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
