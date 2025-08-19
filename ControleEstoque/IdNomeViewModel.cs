using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleEstoque
{
    public class IdNomeViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public IdNomeViewModel()
        {
            Id = 0;
            Nome = string.Empty;
        }

        public IdNomeViewModel(int id, string nome)
        {
            Id = id;
            Nome = nome;
        }

        public IdNomeViewModel(Produto produto)
        {
            Id = produto.Id;
            Nome = $"Cód.: {produto.Codigo}; Modelo: {produto.Modelo}; Fio: {produto.Fio}; mm: {produto.Milimetros}; Tam.: {produto.Tamanho}";
        }

        public IdNomeViewModel(Fornecedor fornecedor)
        {
            Id = fornecedor.Id;
            Nome = fornecedor.Nome;
        }

        public IdNomeViewModel(TipoUnidade tipoUnidade)
        {
            Id = tipoUnidade.Id;
            Nome = tipoUnidade.Nome;
        }

        public override string ToString()
        {
            return Nome;
        }
    }
}
