using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleEstoque
{
    public class FornecedorViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Ativo { get; set; }

        public FornecedorViewModel()
        {
            Id = 0;
            Nome = string.Empty;
            Ativo = string.Empty;
        }

        public FornecedorViewModel(Fornecedor fornecedor)
        {
            Id = fornecedor.Id;
            Nome = fornecedor.Nome;
            Ativo = fornecedor.Ativo ? "Sim" : "Não";
        }

        public Fornecedor ToFornecedor()
        {
            return new Fornecedor
            {
                Id = this.Id,
                Nome = this.Nome,
                Ativo = this.Ativo == "Sim"
            };
        }
    }
}
