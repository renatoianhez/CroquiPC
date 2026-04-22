// Objects/MarkEnums.cs - Enumerações para Marcas
using System;

namespace CrimeSketcher.Objects
{
    /// <summary>
    /// Tipos de marca para evidências em locais de crime
    /// </summary>
    [Serializable]
    public enum TipoMarca
    {
        /// <summary>
        /// Marca de frenagem - linhas paralelas escuras
        /// </summary>
        Frenagem = 0,

        /// <summary>
        /// Marca de derrapagem - marca contínua lateral
        /// </summary>
        Derrapagem = 1,

        /// <summary>
        /// Sulco - marca profunda no solo
        /// </summary>
        Sulco = 2,

        /// <summary>
        /// Arranhão - linha irregular e fina
        /// </summary>
        Arranhao = 3,

        /// <summary>
        /// Rastro - marca de arrasto
        /// </summary>
        Rastro = 4,

        /// <summary>
        /// Marca de impacto - padrão irregular
        /// </summary>
        Impacto = 5,

        /// <summary>
        /// Marca genérica personalizada
        /// </summary>
        Personalizada = 6,

        /// <summary>
        /// Risco - traço fino
        /// </summary>
        Risco = 7,

        /// <summary>
        /// Cerca - padrão traço-x-traço
        /// </summary>
        Cerca = 8,

        /// <summary>
        /// Muro - padrão de linha de tijolos
        /// </summary>
        Muro = 9,

        /// <summary>
        /// Canaleta - linha grossa com gradiente longitudinal
        /// </summary>
        Canaleta = 10,

        /// <summary>
        /// Meio-fio - linha única
        /// </summary>
        MeioFio = 11
    }

    /// <summary>
    /// Intensidade da marca
    /// </summary>
    [Serializable]
    public enum IntensidadeMarca
    {
        /// <summary>
        /// Marca leve, quase imperceptível
        /// </summary>
        Leve = 0,

        /// <summary>
        /// Marca média, bem visível
        /// </summary>
        Media = 1,

        /// <summary>
        /// Marca forte, muito evidente
        /// </summary>
        Forte = 2,

        /// <summary>
        /// Marca muito profunda
        /// </summary>
        MuitoForte = 3
    }
}
