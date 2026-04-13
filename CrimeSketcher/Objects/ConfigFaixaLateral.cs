// Objects/ConfigFaixaLateral.cs
using System.ComponentModel;

namespace CrimeSketcher.Objects
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigFaixaLateral
    {
        [DisplayName("Tipo de Linha")]
        [Description("Tipo da linha divisória entre faixas")]
        public TipoFaixaCentral TipoLinha { get; set; } = TipoFaixaCentral.TracejadaSimples;

        [DisplayName("Cor")]
        [Description("Cor da sinalização desta divisória")]
        public CorSinalizacaoViaria Cor { get; set; } = CorSinalizacaoViaria.Branca;

        public override string ToString() => $"{TipoLinha}, {Cor}";
    }
}
