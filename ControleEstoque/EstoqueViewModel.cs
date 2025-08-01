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
        public int IdProduto { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public string Unidade { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public DateTime DataEntradaSaida { get; set; }
        public string TipoMovimento { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
    }
}
