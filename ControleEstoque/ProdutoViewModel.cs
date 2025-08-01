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
        public string Nome { get; set; } = string.Empty;
        public int IdTipoUnidade { get; set; }
        public string Unidade { get; set; } = string.Empty;
        public int PrecoUnidade { get; set; }
        public int QuantidadeTotal { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Ativo { get; set; } = string.Empty;
        public DateTime Alteracao { get; set; }
    }
}
