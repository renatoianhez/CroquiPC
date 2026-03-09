// Library/SymbolItem.cs
using System.Drawing;

namespace CrimeSketcher.Library
{
    public class SymbolItem
    {
        public string Nome { get; set; }
        public string CaminhoImagem { get; set; }
        public string Categoria { get; set; }
        public Image Thumbnail { get; set; }
        public float LarguraPadrao { get; set; } = 40f;
        public float AlturaPadrao { get; set; } = 40f;
        public string Descricao { get; set; }
    }

    public class SymbolCategory
    {
        public string Nome { get; set; }
        public string Icone { get; set; }
        public System.Collections.Generic.List<SymbolItem> Itens { get; set; }
            = new System.Collections.Generic.List<SymbolItem>();
    }
}