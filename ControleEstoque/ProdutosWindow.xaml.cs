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
    /// Lógica interna para ProdutosWindow.xaml
    /// </summary>
    public partial class ProdutosWindow : Window
    {
        private int? _fornecedorId;
        private bool _atualizandoTabs = false;

        public ProdutosWindow(int? fornecedorId)
        {
            InitializeComponent();
            _atualizandoTabs = true; // Inicia trava

            _fornecedorId = fornecedorId;
            CarregarFornecedoresTabs();
            CarregarProdutos();
            cbFiltroTipo.SelectedIndex = 0;
            _atualizandoTabs = false; // Libera trava
        }

        private void TxtFiltroNome_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_atualizandoTabs)
                return; // Ignora evento durante atualização das abas

            CarregarProdutos();
        }

        private void TabFornecedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_atualizandoTabs)
                return; // Ignora evento durante atualização das abas

            var item = tabFornecedores.SelectedItem as TabItem;
            if (item != null)
            {
                _fornecedorId = item.Tag as int?;
            }
            else
            {
                _fornecedorId = null;
            }   
            CarregarProdutos();
        }

        private void CarregarFornecedoresTabs()
        {
            var fornecedores = EstoqueEntityManager.ObterFornecedores();

            tabFornecedores.Items.Clear();

            // Aba "Todos"
            var tabTodos = new TabItem { Header = "Todos", Tag = null };
            tabFornecedores.Items.Add(tabTodos);

            foreach (var fornecedor in fornecedores)
            {
                var tab = new TabItem { Header = fornecedor.Nome, Tag = fornecedor.Id };
                tabFornecedores.Items.Add(tab);
            }
            
            if (_fornecedorId == null)
            {
                // Seleciona a aba "Todos" se nenhum fornecedor estiver selecionado
                tabFornecedores.SelectedIndex = 0;
            }
            else
            {
                // Seleciona a aba do fornecedor específico
                var tabSelecionada = tabFornecedores.Items.Cast<TabItem>().FirstOrDefault(t => (int?)t.Tag == _fornecedorId);
                if (tabSelecionada != null)
                {
                    tabFornecedores.SelectedItem = tabSelecionada;
                }
                else
                {
                    // Se o fornecedor não estiver na lista, seleciona a aba "Todos"
                    tabFornecedores.SelectedIndex = 0;
                }
            }

        }

        private void CarregarProdutos()
        {
            try
            {
                string? tipofiltro = cbFiltroTipo.SelectedValue as string;
                string? filtro = txtFiltroNome?.Text;
                lstProdutos.ItemsSource = EstoqueEntityManager.ObterProdutosPorFornecedorFiltroOrdemAcabando(_fornecedorId, tipofiltro, filtro, null).Select(p => new ProdutoViewModel(p)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            ProdutoWindow produtoWindow = new ProdutoWindow(_fornecedorId);
            if (produtoWindow.ShowDialog() == true)
            {
                CarregarProdutos();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var produtoSelecionado = lstProdutos.SelectedItem as ProdutoViewModel;
                if (produtoSelecionado == null)
                {
                    MessageBox.Show("Selecione um produto para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Produto? produto = EstoqueEntityManager.ObterProdutoPorId(produtoSelecionado.Id);
                if (produto == null)
                {
                    MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                ProdutoWindow produtoWindow = new ProdutoWindow(_fornecedorId, produto);
                if (produtoWindow.ShowDialog() == true)
                {
                    CarregarProdutos();
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
                ProdutoViewModel? itemSelecionado = lstProdutos.SelectedItem as ProdutoViewModel;
                if (itemSelecionado != null)
                {
                    int idProduto = itemSelecionado.Id;
                    Produto? produto = EstoqueEntityManager.ObterProdutoPorId(idProduto);
                    if (produto == null)
                    {
                        MessageBox.Show("Produto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (MessageBox.Show("Tem certeza que deseja deletar este item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (EstoqueEntityManager.ExisteProdutoEstoque(produto.Id))
                        {
                            if (MessageBox.Show("Este produto possui estoques associados. Deseja deletar inclusive os estoques?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                if (EstoqueEntityManager.DeletarEstoquePorProduto(produto.Id))
                                {
                                    if (EstoqueEntityManager.DeletarProduto(produto))
                                    {
                                        MessageBox.Show("Produto e estoques deletados com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                                        CarregarProdutos();
                                        return;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Erro ao deletar produto.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Erro ao deletar estoques associados.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Produto não pode ser deletado pois possui estoques associados.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                        else
                        {
                            if (EstoqueEntityManager.DeletarProduto(produto))
                            {
                                MessageBox.Show("Produto deletado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                                CarregarProdutos();
                                return;
                            }
                            else
                            {
                                MessageBox.Show("Erro ao deletar produto.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
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
                MessageBox.Show($"Erro ao deletar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CarregarProdutos();
                MessageBox.Show("Produtos atualizados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnGerarModelo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("ModeloProdutos");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(1).SetCellValue("Modelo");
                headerRow.CreateCell(0).SetCellValue("Código");
                headerRow.CreateCell(2).SetCellValue("Fio");
                headerRow.CreateCell(3).SetCellValue("Milímetros");
                headerRow.CreateCell(4).SetCellValue("Tamanho");
                headerRow.CreateCell(5).SetCellValue("Fornecedor");
                headerRow.CreateCell(6).SetCellValue("Unidade");
                headerRow.CreateCell(7).SetCellValue("Quantidade Total");
                headerRow.CreateCell(8).SetCellValue("Quantidade Mínima");
                headerRow.CreateCell(9).SetCellValue("Descrição");

                // Sugestão: Adicionar exemplos de valores na segunda linha
                var exampleRow = sheet.CreateRow(1);
                exampleRow.CreateCell(1).SetCellValue("Modelo Exemplo");
                exampleRow.CreateCell(0).SetCellValue("Código Exemplo");
                exampleRow.CreateCell(2).SetCellValue("Fio Exemplo");
                exampleRow.CreateCell(3).SetCellValue("Milímetros Exemplo");
                exampleRow.CreateCell(4).SetCellValue("Tamanho Exemplo");
                exampleRow.CreateCell(5).SetCellValue("Fornecedor Exemplo");
                exampleRow.CreateCell(6).SetCellValue("Unidade Exemplo");
                exampleRow.CreateCell(7).SetCellValue("10"); // Quantidade Total Exemplo
                exampleRow.CreateCell(8).SetCellValue("2"); // Quantidade Mínima Exemplo
                exampleRow.CreateCell(9).SetCellValue("Descrição Exemplo");

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "ModeloProdutos.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fs);
                    }
                    MessageBox.Show("Modelo gerado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar modelo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx"
                };

                if (openFileDialog.ShowDialog() != true)
                    return;

                var workbook = new XSSFWorkbook(openFileDialog.OpenFile());
                var sheet = workbook.GetSheetAt(0);
                if (sheet == null)
                {
                    MessageBox.Show("A planilha está vazia ou não foi encontrada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (sheet.LastRowNum < 1)
                {
                    MessageBox.Show("A planilha não contém dados para importar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var headerRow = sheet.GetRow(0);
                headerRow.CreateCell(10).SetCellValue("Status");


                int importados = 0;
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    string modelo = row.GetCell(0)?.ToString() ?? "";
                    string codigo = row.GetCell(1)?.ToString() ?? "";
                    string fio = row.GetCell(2)?.ToString() ?? "";
                    string milimetros = row.GetCell(3)?.ToString() ?? "";
                    string tamanho = row.GetCell(4)?.ToString() ?? "";
                    string fornecedor = row.GetCell(5)?.ToString() ?? "";
                    string unidade = row.GetCell(6)?.ToString() ?? "";
                    string quantidadeTotalStr = row.GetCell(7)?.ToString() ?? "0";
                    string quantidadeMinimaStr = row.GetCell(8)?.ToString() ?? "0";
                    string descricao = row.GetCell(9)?.ToString() ?? "";


                    if (string.IsNullOrWhiteSpace(modelo) || string.IsNullOrWhiteSpace(modelo))
                    {
                        row.CreateCell(10).SetCellValue("Ignorado: Modelo inválido");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(codigo))
                    {
                        row.CreateCell(10).SetCellValue("Ignorado: Código inválido");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(unidade))
                    {
                        row.CreateCell(10).SetCellValue("Ignorado: Unidade inválida");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(fornecedor))
                    {
                        row.CreateCell(10).SetCellValue("Ignorado: Fornecedor inválido");
                        continue;
                    }

                    // Busca ou cadastra o tipo de unidade
                    var tipoUnidade = EstoqueEntityManager.ObterTipoUnidadePorNome(unidade);
                    if (tipoUnidade == null)
                    {
                        tipoUnidade = new TipoUnidade
                        {
                            Nome = unidade,
                            QuantidadeMinima = 0
                        };
                        if (EstoqueEntityManager.LancarTipoUnidade(tipoUnidade))
                        {
                            tipoUnidade = EstoqueEntityManager.ObterTipoUnidadePorNome(unidade);
                        }
                        else
                        {
                            row.CreateCell(10).SetCellValue("Erro ao cadastrar unidade");
                            continue;
                        }
                    }
                    if (tipoUnidade == null)
                    {
                        row.CreateCell(10).SetCellValue("Erro ao obter unidade");
                        continue;
                    }

                    // Busca ou cadastra o fornecedor
                    var fornecedorEntity = EstoqueEntityManager.ObterFornecedorPorNome(fornecedor);
                    if (fornecedorEntity == null)
                    {
                        fornecedorEntity = new Fornecedor
                        {
                            Nome = fornecedor,
                            Ativo = true,
                            Descricao = ""
                        };
                        if (!EstoqueEntityManager.LancarFornecedor(fornecedorEntity))
                        {
                            row.CreateCell(10).SetCellValue("Erro ao cadastrar fornecedor");
                            continue;
                        }
                        else
                        {
                            fornecedorEntity = EstoqueEntityManager.ObterFornecedorPorNome(fornecedor);
                        }
                    }
                    if (fornecedorEntity == null)
                    {
                        row.CreateCell(10).SetCellValue("Erro ao obter fornecedor");
                        continue;
                    }

                    if (EstoqueEntityManager.ExisteProduto(codigo, fio, modelo, milimetros, tamanho, fornecedorEntity.Id))
                    {
                        row.CreateCell(10).SetCellValue("Ignorado: produto já existe");
                        continue;
                    }

                    string msgStatus = "";

                    if (!int.TryParse(quantidadeTotalStr, out int quantidadeTotal))
                    {
                        quantidadeTotal = 0;
                        msgStatus += "Qtde Total inválida, definida como 0;";
                    }
                    if (quantidadeTotal < 0)
                    {
                        quantidadeTotal = 0; // Evita valores negativos
                        msgStatus += "Qtde Total não pode ser negativa, definida como 0;";
                    }

                    if (!int.TryParse(quantidadeMinimaStr, out int quantidadeMinima))
                    {
                        quantidadeMinima = 0;
                        msgStatus += "Qtde Mínima inválida, definida como 0;";
                    }
                    if (quantidadeMinima < 0)
                    {
                        quantidadeMinima = 0; // Evita valores negativos
                        msgStatus += "Qtde Mínima não pode ser negativa, definida como 0;";
                    }

                    var produto = new Produto
                    {
                        Modelo = modelo,
                        Fio = fio,
                        Milimetros = milimetros,
                        Tamanho = tamanho,
                        Codigo = codigo,
                        FornecedorId = fornecedorEntity.Id,
                        TipoUnidadeId = tipoUnidade.Id,
                        QuantidadeTotal = 0,
                        QuantidadeMinima = quantidadeMinima,
                        Ativo = true,
                        Alteracao = DateTime.Now,
                        Descricao = descricao
                    };

                    if (EstoqueEntityManager.LancarProduto(produto))
                    {
                        msgStatus += "Produto cadastrado com sucesso;";

                        if (quantidadeTotal > 0)
                        {
                            produto = EstoqueEntityManager.ObterProdutoPorDadosFornecedorId(codigo, fio, modelo, milimetros, tamanho, fornecedorEntity.Id);

                            if (produto == null)
                            {
                                msgStatus += "Erro ao obter produto, estoque não cadastrado;";
                            }
                            else
                            {
                                var estoque = new Estoque
                                {
                                    ProdutoId = produto.Id,
                                    Quantidade = quantidadeTotal,
                                    DataEntradaSaida = DateTime.Now,
                                    Entrada = true,
                                    Observacao = "Cadastro inicial via importação"
                                };
                                if (EstoqueEntityManager.LancarEstoque(estoque))
                                {
                                    msgStatus += "Estoque cadastrado com sucesso;";
                                }
                                else
                                {
                                    msgStatus += "Erro ao cadastrar estoque;";
                                }
                            }
                        }
                        else
                        {
                            msgStatus += "Estoque não cadastrado devido Qtde Total igual 0;";
                        }
                        row.CreateCell(10).SetCellValue(msgStatus);
                    }
                    else
                    {
                        row.CreateCell(10).SetCellValue("Erro ao cadastrar produto");
                        continue;
                    }
                    importados++;
                }

                MessageBox.Show($"{importados} produtos importados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                CarregarProdutos();

                // Salvar planilha com resultados
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "ImportacaoProdutosResultado.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fs);
                    }
                    MessageBox.Show("Arquivo de importação salvo com os resultados!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao importar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var produtos = lstProdutos.ItemsSource as IEnumerable<ProdutoViewModel>;
                if (produtos == null || !produtos.Any())
                {
                    MessageBox.Show("Não há produtos para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Produtos");

                // Cabeçalho
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Id");
                headerRow.CreateCell(1).SetCellValue("Modelo");
                headerRow.CreateCell(2).SetCellValue("Código");
                headerRow.CreateCell(3).SetCellValue("Fio");
                headerRow.CreateCell(4).SetCellValue("Milímetros");
                headerRow.CreateCell(5).SetCellValue("Tamanho");
                headerRow.CreateCell(6).SetCellValue("Fornecedor");
                headerRow.CreateCell(7).SetCellValue("Unidade");
                headerRow.CreateCell(8).SetCellValue("Quantidade Total");
                headerRow.CreateCell(9).SetCellValue("Quantidade Mínima");
                headerRow.CreateCell(10).SetCellValue("Ativo");
                headerRow.CreateCell(11).SetCellValue("Alteração");
                headerRow.CreateCell(12).SetCellValue("Descrição");

                int rowIndex = 1;
                foreach (var produto in produtos)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(produto.Id);
                    row.CreateCell(1).SetCellValue(produto.Modelo);
                    row.CreateCell(2).SetCellValue(produto.Codigo);
                    row.CreateCell(3).SetCellValue(produto.Fio);
                    row.CreateCell(4).SetCellValue(produto.Milimetros);
                    row.CreateCell(5).SetCellValue(produto.Tamanho);
                    row.CreateCell(6).SetCellValue(produto.FornecedorNome);
                    row.CreateCell(7).SetCellValue(produto.TipoUnidadeNome);
                    row.CreateCell(8).SetCellValue(produto.QuantidadeTotal);
                    row.CreateCell(9).SetCellValue(produto.QuantidadeMinima);
                    row.CreateCell(10).SetCellValue(produto.Ativo);
                    row.CreateCell(11).SetCellValue(produto.Alteracao.ToString("dd/MM/yyyy HH:mm"));
                    row.CreateCell(12).SetCellValue(produto.Descricao);
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                    FileName = "Produtos.xlsx"
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
                MessageBox.Show($"Erro ao exportar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_atualizandoTabs)
                return; // Ignora evento durante atualização das abas

            CarregarProdutos();
        }
    }
}
