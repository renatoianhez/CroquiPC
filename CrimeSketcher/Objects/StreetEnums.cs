// Objects/StreetEnums.cs
namespace CrimeSketcher.Objects
{
    /// <summary>
    /// Tipo de linha de sinalização horizontal
    /// </summary>
    public enum TipoFaixaCentral
    {
        Nenhuma,
        TracejadaSimples,           // - - - - -
        ContinuaSimples,            // ─────────
        ContinuaDupla,              // ═════════
        ContinuaEsquerdaTracejadaDireita,  // ───  - - -
        TracejadaEsquerdaContinuaDireita   // - - -  ───
    }

    /// <summary>
    /// Tipo de linha divisória da faixa de estacionamento / acostamento
    /// </summary>
    public enum TipoLinhaEstacionamento
    {
        Tracejada,      // - - - -
        Continua,       // ──────
        Nenhuma         // Sem linha divisória
    }

    /// <summary>
    /// Cor padrão da sinalização horizontal da via
    /// </summary>
    public enum CorSinalizacaoViaria
    {
        Laranja,
        Branca
    }

    /// <summary>
    /// Tipo de conexão nas extremidades da rua
    /// </summary>
    public enum TipoExtremidade
    {
        Livre,          // Sem conexão
        Conectada,      // Conectada a outra rua
        Fechada         // Fim de rua (cul-de-sac)
    }

    /// <summary>
    /// Tipo de cruzamento
    /// </summary>
    public enum TipoCruzamento
    {
        Cruz,           // 4 vias (+)
        TParaCima,      // 3 vias (⊥) - saída para cima
        TParaBaixo,     // 3 vias (T) - saída para baixo
        TParaEsquerda,  // 3 vias (⊣) - saída para esquerda
        TParaDireita    // 3 vias (⊢) - saída para direita
    }
}