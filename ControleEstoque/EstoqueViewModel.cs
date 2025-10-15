using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleEstoque
{
    public class EstoqueViewModel
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoCodigo { get; set; } = string.Empty;
        public string ProdutoModelo { get; set; } = string.Empty;
        public string ProdutoFio { get; set; } = string.Empty;
        public string ProdutoMilimetro { get; set; } = string.Empty;
        public string ProdutoTamanho { get; set; } = string.Empty;
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public string Unidade { get; set; } = string.Empty;
        public double Quantidade { get; set; }
        public DateTime DataEntradaSaida { get; set; }
        public string TipoMovimento { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;

        public EstoqueViewModel()
        {
            Id = 0;
            ProdutoId = 0;
            ProdutoCodigo = string.Empty;
            ProdutoModelo = string.Empty;
            ProdutoFio = string.Empty;
            ProdutoMilimetro = string.Empty;
            ProdutoTamanho = string.Empty;
            FornecedorId = 0;
            FornecedorNome = string.Empty;
            Unidade = string.Empty;
            Quantidade = 0;
            DataEntradaSaida = DateTime.Now;
            TipoMovimento = string.Empty;
            Observacao = string.Empty;
        }

        public EstoqueViewModel(Estoque estoque)
        {
            Id = estoque.Id;
            ProdutoId = estoque.ProdutoId;
            ProdutoCodigo = estoque.Produto.Codigo;
            ProdutoModelo = estoque.Produto.Modelo;
            ProdutoFio = estoque.Produto.Fio;
            ProdutoMilimetro = estoque.Produto.Milimetros;
            ProdutoTamanho = estoque.Produto.Tamanho;
            FornecedorId = estoque.Produto.FornecedorId;
            FornecedorNome = estoque.Produto.Fornecedor.Nome;
            Unidade = estoque.Produto.TipoUnidade.Nome;
            Quantidade = estoque.Quantidade;
            DataEntradaSaida = estoque.DataEntradaSaida;
            TipoMovimento = estoque.Entrada ? "Entrada" : "Saída";
            Observacao = estoque.Observacao ?? string.Empty;
        }
    }
}
