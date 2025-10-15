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
    /// Lógica interna para ProdutoWindow.xaml
    /// </summary>
    public partial class ProdutoWindow : Window
    {
        private int? _fornecedorId;
        private Produto? _produtoEditando;

        // Construtor para novo produto
        public ProdutoWindow(int? fornecedorId)
        {
            InitializeComponent();
            DataObject.AddPastingHandler(txtQtdeMinima, TxtQtdeMinima_Pasting);
            DataObject.AddPastingHandler(txtPesoUnitarioGrama, TxtPesoUnitarioGrama_Pasting);
            DataObject.AddPastingHandler(txtQtdeTotal, TxtQtdeTotal_Pasting);
            _fornecedorId = fornecedorId;
            _produtoEditando = null;
            try
            {
                cbFornecedor.ItemsSource = EstoqueEntityManager.ObterFornecedores()
                            .Select(p => new IdNomeViewModel(p))
                            .OrderBy(p => p.Nome).ToList();
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
                var fornecedor = EstoqueEntityManager.ObterFornecedorPorId(_fornecedorId.Value);
                if (fornecedor != null)
                {
                    cbFornecedor.SelectedValue = fornecedor.Id;
                }
                else
                {
                    _fornecedorId = null;
                    MessageBox.Show("Fornecedor não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            try
            {
                cbUnidade.ItemsSource = EstoqueEntityManager.ObterTiposUnidades()
                            .Select(t => new IdNomeViewModel(t))
                            .OrderBy(t => t.Nome).ToList();
                cbUnidade.DisplayMemberPath = "Nome";
                cbUnidade.SelectedValuePath = "Id";
                cbUnidade.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar unidades: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            txtQtdeTotal.IsEnabled = true;
            txtQtdeTotal.IsReadOnly = false;
            chkAtivo.IsChecked = true;
            txtQtdeMinima.Text = "0";
            txtPesoUnitarioGrama.Text = "0";
            txtQtdeTotal.Text = "0";
            this.Title = "Cadastro de Produto";
            lblTitulo.Content = "Cadastro de Produto";
        }

        // Construtor para edição
        public ProdutoWindow(int? fornecedorId, Produto produto) : this(fornecedorId)
        {
            _produtoEditando = produto;
            this.Title = "Edição de Produto";
            lblTitulo.Content = "Edição de Produto";
            PreencherCampos(produto);
        }

        private void PreencherCampos(Produto produto)
        {
            txtQtdeTotal.IsEnabled = false;
            txtQtdeTotal.IsReadOnly = true;
            txtCodigo.Text = produto.Codigo;
            txtModelo.Text = produto.Modelo;
            txtFio.Text = produto.Fio;
            txtMilimetros.Text = produto.Milimetros;
            txtTamanho.Text = produto.Tamanho;
            txtQtdeMinima.Text = produto.QuantidadeMinima.ToString();
            txtPesoUnitarioGrama.Text = produto.PesoUnitarioGrama.ToString();
            cbUnidade.SelectedValue = produto.TipoUnidadeId;
            cbFornecedor.SelectedValue = produto.FornecedorId;
            txtQtdeTotal.Text = produto.QuantidadeTotal.ToString();
            chkAtivo.IsChecked = produto.Ativo;
            lblAlteracao.Content = produto.Alteracao.ToString("dd/MM/yyyy HH:mm");
            txtDescricao.Text = produto.Descricao;
        }

        private void RetornarCampos(ref Produto produto, double qtdeMinima, double pesoUnitarioGrama, int tipounidadeid, int fornecedorid)
        {
            produto.Codigo = txtCodigo.Text;
            produto.Modelo = txtModelo.Text;
            produto.Fio = txtFio.Text;
            produto.Milimetros = txtMilimetros.Text;
            produto.Tamanho = txtTamanho.Text;
            produto.QuantidadeMinima = qtdeMinima;
            produto.PesoUnitarioGrama = pesoUnitarioGrama;
            if (produto.TipoUnidadeId != tipounidadeid)
            {
                produto.TipoUnidadeId = tipounidadeid;
                produto.TipoUnidade = null!;
            }
            if (produto.FornecedorId != fornecedorid)
            {
                produto.FornecedorId = fornecedorid;
                produto.Fornecedor = null!;
            }
            produto.Ativo = chkAtivo.IsChecked ?? false;
            produto.Alteracao = DateTime.UtcNow.AddHours(-3);
            produto.Descricao = txtDescricao.Text.Trim();
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("Digite o código do produto.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtModelo.Text))
            {
                MessageBox.Show("Digite o modelo do produto.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fornecedorSelecionado = cbFornecedor.SelectedItem as IdNomeViewModel;
            if (fornecedorSelecionado == null)
            {
                MessageBox.Show("Selecione um fornecedor.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (fornecedorSelecionado.Id < 1) {
                MessageBox.Show("Selecione um fornecedor válido.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var tipoUnidadeSelecionado = cbUnidade.SelectedItem as IdNomeViewModel;
            if (tipoUnidadeSelecionado == null)
            {
                MessageBox.Show("Selecione uma unidade de medida.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (tipoUnidadeSelecionado.Id < 1) {
                MessageBox.Show("Selecione uma unidade de medida válida.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double quantidadeMinima = 0;
            double pesoUnitarioGrama = 0;
            double quantidadeTotal = 0;

            string qtdeMinimaText = NormalizarDecimal(txtQtdeMinima.Text.Trim());
            string pesoUnitarioText = NormalizarDecimal(txtPesoUnitarioGrama.Text.Trim());
            string qtdeTotalText = NormalizarDecimal(txtQtdeTotal.Text.Trim());

            if (string.IsNullOrWhiteSpace(qtdeMinimaText) || !double.TryParse(qtdeMinimaText, NumberStyles.Any, CultureInfo.InvariantCulture, out quantidadeMinima))
            {
                quantidadeMinima = 0;
            }
            if (quantidadeMinima < 0)
            {
                quantidadeMinima = 0;
            }

            if (string.IsNullOrWhiteSpace(pesoUnitarioText) || !double.TryParse(pesoUnitarioText, NumberStyles.Any, CultureInfo.InvariantCulture, out pesoUnitarioGrama))
            {
                pesoUnitarioGrama = 0;
            }
            if (pesoUnitarioGrama < 0)
            {
                pesoUnitarioGrama = 0;
            }

            if (string.IsNullOrWhiteSpace(qtdeTotalText) || !double.TryParse(qtdeTotalText, NumberStyles.Any, CultureInfo.InvariantCulture, out quantidadeTotal))
            {
                quantidadeTotal = 0;
            }
            if (quantidadeTotal < 0)
            {
                quantidadeTotal = 0;
            }

            try
            {
                Produto produto = new();
                if (_produtoEditando == null)
                {
                    // Novo produto
                    produto = new();
                }
                else
                {
                    var produtoExiste = EstoqueEntityManager.ObterProdutoPorId(_produtoEditando.Id);
                    if (produtoExiste == null)
                    {
                        MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    produto = produtoExiste;
                }
                RetornarCampos(ref produto, quantidadeMinima, pesoUnitarioGrama, tipoUnidadeSelecionado.Id, fornecedorSelecionado.Id);

                if (EstoqueEntityManager.ExisteProduto(produto.Codigo, produto.Fio, produto.Modelo, produto.Milimetros, produto.Tamanho, produto.FornecedorId, _produtoEditando?.Id))
                {
                    MessageBox.Show("Já existe um produto cadastrado com estes dados para o fornecedor selecionado.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (EstoqueEntityManager.LancarProduto(produto))
                {
                    if (_produtoEditando == null)
                    {
                        if (quantidadeTotal > 0)
                        {
                            var produtoAdicionado = EstoqueEntityManager.ObterProdutoPorDadosFornecedorId(produto.Codigo, produto.Fio, produto.Modelo, produto.Milimetros, produto.Tamanho, produto.FornecedorId);
                            if (produtoAdicionado != null)
                            {
                                Estoque estoque = new Estoque
                                {
                                    ProdutoId = produtoAdicionado.Id,
                                    Quantidade = quantidadeTotal,
                                    DataEntradaSaida = DateTime.UtcNow.AddHours(-3),
                                    Entrada = true,
                                    Observacao = "Estoque inicial do produto."
                                };
                                if (EstoqueEntityManager.LancarEstoque(estoque))
                                {
                                    MessageBox.Show("Produto e estoque cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                                    DialogResult = true;
                                    Close();
                                }
                                else
                                {
                                    MessageBox.Show("Produto cadastrado com sucesso, porém houve um erro ao cadastrar o estoque.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                    DialogResult = true;
                                    Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Erro ao localizar o produto cadastrado ao tentar cadastrar o estoque. Verifique na tabela de produtos se o produto foi cadastrado corretamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                DialogResult = true;
                                Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Produto cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                            Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Produto salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("Erro ao salvar produto. Verifique os dados e tente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private void TxtQtdeMinima_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite apenas dígitos e um separador decimal (vírgula ou ponto)
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d,.]+$");
        }

        private void TxtQtdeTotal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite apenas dígitos e um separador decimal (vírgula ou ponto)
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d,.]+$");
        }

        private void TxtPesoUnitarioGrama_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite apenas dígitos e um separador decimal (vírgula ou ponto)
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d,.]+$");
        }

        private void TxtQtdeMinima_Pasting(object sender, DataObjectPastingEventArgs e)
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

        private void TxtQtdeTotal_Pasting(object sender, DataObjectPastingEventArgs e)
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
