using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleEstoque
{
    public class ProdutoViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public string Milimetros { get; set; } = string.Empty;
        public string Tamanho { get; set; } = string.Empty;
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public int TipoUnidadeId { get; set; }
        public string TipoUnidadeNome { get; set; } = string.Empty;
        public int QuantidadeTotal { get; set; }
        public int QuantidadeMinima { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Ativo { get; set; } = string.Empty;
        public DateTime Alteracao { get; set; }

        public ProdutoViewModel()
        {
            Id = 0;
            Codigo = string.Empty;
            Modelo = string.Empty;
            Fio = string.Empty;
            Milimetros = string.Empty;
            Tamanho = string.Empty;
            FornecedorId = 0;
            FornecedorNome = string.Empty;
            TipoUnidadeId = 0;
            TipoUnidadeNome = string.Empty;
            QuantidadeTotal = 0;
            QuantidadeMinima = 0;
            Descricao = string.Empty;
            Ativo = string.Empty;
            Alteracao = DateTime.Now;
        }

        public ProdutoViewModel(Produto produto)
        {
            Id = produto.Id;
            Codigo = produto.Codigo;
            Modelo = produto.Modelo;
            Fio = produto.Fio;
            Milimetros = produto.Milimetros;
            Tamanho = produto.Tamanho;
            FornecedorId = produto.FornecedorId;
            FornecedorNome = produto.Fornecedor.Nome;
            TipoUnidadeId = produto.TipoUnidadeId;
            TipoUnidadeNome = produto.TipoUnidade.Nome;
            QuantidadeTotal = produto.QuantidadeTotal;
            QuantidadeMinima = produto.QuantidadeMinima > 0 ? produto.QuantidadeMinima : produto.TipoUnidade.QuantidadeMinima;
            Descricao = produto.Descricao ?? string.Empty;
            Ativo = produto.Ativo ? "Sim" : "Não";
            Alteracao = produto.Alteracao;
        }
    }
}
