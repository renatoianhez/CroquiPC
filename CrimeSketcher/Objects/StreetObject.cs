// Objects/StreetObject.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class StreetObject : BaseSketchObject
    {
        #region Propriedades Básicas

        private float _rotacaoAplicada = 0f;

        [Browsable(false)]
        public PointF PontoInicial { get; set; }

        [Browsable(false)]
        public PointF PontoFinal { get; set; }

        [Browsable(false)]
        public PointF? PontoCurva { get; set; } = null;

        [Category("Curvatura")]
        [DisplayName("Tem Curva")]
        [Description("Define se a rua possui curvatura")]
        public bool TemCurva
        {
            get => PontoCurva.HasValue;
            set
            {
                if (value && !PontoCurva.HasValue)
                {
                    // Define ponto de curva no meio
                    PontoCurva = new PointF(
                        (PontoInicial.X + PontoFinal.X) / 2,
                        (PontoInicial.Y + PontoFinal.Y) / 2);
                }
                else if (!value)
                {
                    PontoCurva = null;
                }
            }
        }

        [Category("Dimensões")]
        [DisplayName("Largura (pixels)")]
        [Description("Largura total da via em pixels")]
        public float Largura { get; set; } = 80f;

        [Category("Identificação")]
        [DisplayName("Nome da Rua")]
        [Description("Nome ou identificação da via")]
        public string NomeRua { get; set; } = "";

        [Category("Faixas")]
        [DisplayName("Número de Faixas")]
        [Description("Quantidade de faixas de rodagem")]
        public int NumeroFaixas { get; set; } = 2;

        [Category("Faixas")]
        [DisplayName("Mão Única")]
        [Description("Define se a via é de mão única")]
        public bool MaoUnica { get; set; } = false;

        #endregion

        #region Calçadas e Meio-fio

        [Category("Calçada")]
        [DisplayName("Possui Calçada")]
        [Description("Define se a rua terá calçadas laterais")]
        public bool TemCalcada { get; set; } = true;

        [Category("Calçada")]
        [DisplayName("Largura da Calçada")]
        [Description("Largura das calçadas em pixels")]
        public float LarguraCalcada { get; set; } = 15f;

        [Category("Calçada")]
        [DisplayName("Possui Meio-Fio")]
        [Description("Define se será desenhado o meio-fio")]
        public bool TemMeioFio { get; set; } = true;

        [Browsable(false)]
        public int CorCalcadaArgb { get; set; } = Color.FromArgb(210, 210, 200).ToArgb();

        [Browsable(false)]
        public int CorMeioFioArgb { get; set; } = Color.FromArgb(150, 150, 140).ToArgb();

        [Category("Calçada")]
        [DisplayName("Cor da Calçada")]
        [Description("Cor das calçadas")]
        [JsonIgnore]
        public Color CorCalcada
        {
            get => Color.FromArgb(CorCalcadaArgb);
            set => CorCalcadaArgb = value.ToArgb();
        }

        [Category("Calçada")]
        [DisplayName("Cor do Meio-Fio")]
        [Description("Cor do meio-fio")]
        [JsonIgnore]
        public Color CorMeioFio
        {
            get => Color.FromArgb(CorMeioFioArgb);
            set => CorMeioFioArgb = value.ToArgb();
        }

        #endregion

        #region Faixas de Sinalização

        [Category("Sinalização")]
        [DisplayName("Tipo de Faixa Central")]
        [Description("Tipo de faixa de separação no centro da via")]
        public TipoFaixaCentral TipoFaixaCentral { get; set; } = TipoFaixaCentral.TracejadaSimples;

        [Category("Sinalização")]
        [DisplayName("Mostrar Faixas Laterais")]
        [Description("Exibe as faixas de borda da via")]
        public bool MostrarFaixasLaterais { get; set; } = true;

        [Category("Sinalização")]
        [DisplayName("Espaçamento do Tracejado")]
        [Description("Espaçamento entre os traços da faixa tracejada")]
        public float EspacamentoTracejado { get; set; } = 15f;

        [Category("Sinalização")]
        [DisplayName("Comprimento do Tracejado")]
        [Description("Comprimento de cada traço da faixa")]
        public float ComprimentoTracejado { get; set; } = 25f;

        [Browsable(false)]
        public int CorFaixaAmarelaArgb { get; set; } = Color.FromArgb(255, 200, 0).ToArgb();

        [Browsable(false)]
        public int CorFaixaBrancaArgb { get; set; } = Color.White.ToArgb();

        [Category("Sinalização")]
        [DisplayName("Espessura da Faixa")]
        [Description("Espessura das linhas de sinalização")]
        public float EspessuraFaixa { get; set; } = 2f;

        #endregion

        #region Conexões

        [Browsable(false)]
        public TipoExtremidade ExtremidadeInicial { get; set; } = TipoExtremidade.Livre;

        [Browsable(false)]
        public TipoExtremidade ExtremidadeFinal { get; set; } = TipoExtremidade.Livre;

        [Browsable(false)]
        public string IdConexaoInicial { get; set; }

        [Browsable(false)]
        public string IdConexaoFinal { get; set; }

        #endregion

        #region Propriedades Calculadas

        [Category("Dimensões")]
        [DisplayName("Comprimento")]
        [Description("Comprimento total da via em pixels")]
        [JsonIgnore]
        public float Comprimento
        {
            get
            {
                float dx = PontoFinal.X - PontoInicial.X;
                float dy = PontoFinal.Y - PontoInicial.Y;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }
            set
            {
                float novoComprimento = Math.Max(1f, value);
                float comprimentoAtual = Comprimento;

                if (comprimentoAtual > 0.001f)
                {
                    float escala = novoComprimento / comprimentoAtual;

                    PontoFinal = new PointF(
                        PontoInicial.X + (PontoFinal.X - PontoInicial.X) * escala,
                        PontoInicial.Y + (PontoFinal.Y - PontoInicial.Y) * escala);

                    if (PontoCurva.HasValue)
                    {
                        PontoCurva = new PointF(
                            PontoInicial.X + (PontoCurva.Value.X - PontoInicial.X) * escala,
                            PontoInicial.Y + (PontoCurva.Value.Y - PontoInicial.Y) * escala);
                    }
                }
                else
                {
                    PontoFinal = new PointF(PontoInicial.X + novoComprimento, PontoInicial.Y);

                    if (PontoCurva.HasValue)
                    {
                        PontoCurva = new PointF(
                            (PontoInicial.X + PontoFinal.X) / 2,
                            (PontoInicial.Y + PontoFinal.Y) / 2);
                    }
                }
            }
        }

        [Category("Transformação")]
        [DisplayName("Ângulo (graus)")]
        [Description("Ângulo da via em relação ao eixo horizontal")]
        [ReadOnly(true)]
        [JsonIgnore]
        public float Angulo
        {
            get
            {
                float dx = PontoFinal.X - PontoInicial.X;
                float dy = PontoFinal.Y - PontoInicial.Y;
                return (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
            }
        }

        [Browsable(false)]
        [JsonIgnore]
        public PointF Centro => new PointF(
            (PontoInicial.X + PontoFinal.X) / 2,
            (PontoInicial.Y + PontoFinal.Y) / 2);

        /// <summary>
        /// Vetor unitário na direção da rua
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public PointF Direcao
        {
            get
            {
                float comp = Comprimento;
                if (comp == 0) return new PointF(1, 0);
                return new PointF(
                    (PontoFinal.X - PontoInicial.X) / comp,
                    (PontoFinal.Y - PontoInicial.Y) / comp);
            }
        }

        /// <summary>
        /// Vetor perpendicular (para largura)
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public PointF Perpendicular
        {
            get
            {
                var dir = Direcao;
                return new PointF(-dir.Y, dir.X);
            }
        }

        #endregion

        public StreetObject()
        {
            Tipo = "Rua";
            CorPreenchimento = Color.FromArgb(180, 180, 180);
            CorContorno = Color.FromArgb(100, 100, 100);
        }

        #region Desenho

        public override void Desenhar(Graphics g)
        {
            AplicarRotacaoPendente();

            if (!Visivel) return;

            float comp = Comprimento;
            if (comp < 1) return;

            var perp = Perpendicular;

            // 1. Calçadas (se houver)
            if (TemCalcada)
            {
                DesenharCalcadas(g, perp);
            }

            // 2. Asfalto
            DesenharAsfalto(g, perp);

            // 3. Meio-fio
            if (TemMeioFio)
            {
                DesenharMeioFio(g, perp);
            }

            // 4. Faixas de sinalização
            DesenharFaixas(g, perp);

            // 5. Nome da rua
            if (!string.IsNullOrEmpty(NomeRua))
            {
                DesenharNomeRua(g);
            }

            // 6. Seleção
            if (Selecionado)
            {
                DesenharSelecao(g);
                DesenharPontosConexao(g);
            }
        }

        private void DesenharCalcadas(Graphics g, PointF perp)
        {
            float totalWidth = Largura + LarguraCalcada * 2;

            if (!TemCurva || !PontoCurva.HasValue)
            {
                // Rua reta
                var pontos = GetPoligono(totalWidth, perp);
                using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                {
                    g.FillPolygon(brush, pontos);
                }
            }
            else
            {
                // Rua curva - desenhar com path
                using (var path = new GraphicsPath())
                {
                    // Criar polígono da calçada seguindo a curva
                    List<PointF> ladoSuperior = new List<PointF>();
                    List<PointF> ladoInferior = new List<PointF>();

                    int segmentos = 30;
                    for (int i = 0; i <= segmentos; i++)
                    {
                        float t = i / (float)segmentos;
                        var ponto = GetPontoNaCurva(t);
                        var perpLocal = GetPerpendicularNaCurva(t);
                        float halfW = totalWidth / 2;

                        ladoSuperior.Add(new PointF(
                            ponto.X + perpLocal.X * halfW,
                            ponto.Y + perpLocal.Y * halfW));
                        ladoInferior.Add(new PointF(
                            ponto.X - perpLocal.X * halfW,
                            ponto.Y - perpLocal.Y * halfW));
                    }

                    // Adicionar ambos os lados ao path
                    path.AddLines(ladoSuperior.ToArray());
                    ladoInferior.Reverse();
                    path.AddLines(ladoInferior.ToArray());
                    path.CloseFigure();

                    using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }

            // Textura de calçada (linhas finas) - simplificado para curvas
            using (var pen = new Pen(Color.FromArgb(40, 100, 100, 100), 0.5f))
            {
                float spacing = 8f;
                int numLinhas = (int)(Comprimento / spacing);

                for (int i = 0; i <= numLinhas; i++)
                {
                    float t = i / (float)numLinhas;
                    var ponto = GetPontoNaCurva(t);
                    var perpLocal = GetPerpendicularNaCurva(t);
                    float halfW = totalWidth / 2;

                    var p1 = new PointF(
                        ponto.X + perpLocal.X * halfW,
                        ponto.Y + perpLocal.Y * halfW);
                    var p2 = new PointF(
                        ponto.X - perpLocal.X * halfW,
                        ponto.Y - perpLocal.Y * halfW);
                    g.DrawLine(pen, p1, p2);
                }
            }
        }

        private void DesenharAsfalto(Graphics g, PointF perp)
        {
            if (!TemCurva || !PontoCurva.HasValue)
            {
                // Rua reta
                var pontos = GetPoligono(Largura, perp);
                using (var brush = new SolidBrush(CorPreenchimento))
                {
                    g.FillPolygon(brush, pontos);
                }

                // Textura de asfalto sutil
                using (var brush = new HatchBrush(
                    HatchStyle.Percent10,
                    Color.FromArgb(20, 0, 0, 0),
                    Color.Transparent))
                {
                    g.FillPolygon(brush, pontos);
                }
            }
            else
            {
                // Rua curva
                using (var path = new GraphicsPath())
                {
                    List<PointF> ladoSuperior = new List<PointF>();
                    List<PointF> ladoInferior = new List<PointF>();

                    int segmentos = 30;
                    for (int i = 0; i <= segmentos; i++)
                    {
                        float t = i / (float)segmentos;
                        var ponto = GetPontoNaCurva(t);
                        var perpLocal = GetPerpendicularNaCurva(t);
                        float halfW = Largura / 2;

                        ladoSuperior.Add(new PointF(
                            ponto.X + perpLocal.X * halfW,
                            ponto.Y + perpLocal.Y * halfW));
                        ladoInferior.Add(new PointF(
                            ponto.X - perpLocal.X * halfW,
                            ponto.Y - perpLocal.Y * halfW));
                    }

                    path.AddLines(ladoSuperior.ToArray());
                    ladoInferior.Reverse();
                    path.AddLines(ladoInferior.ToArray());
                    path.CloseFigure();

                    using (var brush = new SolidBrush(CorPreenchimento))
                    {
                        g.FillPath(brush, path);
                    }

                    // Textura de asfalto
                    using (var brush = new HatchBrush(
                        HatchStyle.Percent10,
                        Color.FromArgb(20, 0, 0, 0),
                        Color.Transparent))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        private void DesenharMeioFio(Graphics g, PointF perp)
        {
            using (var pen = new Pen(Color.FromArgb(CorMeioFioArgb), 2.5f))
            {
                float offset = Largura / 2;

                if (!TemCurva || !PontoCurva.HasValue)
                {
                    // Rua reta - código original
                    // Lado superior
                    g.DrawLine(pen,
                        PontoInicial.X + perp.X * offset,
                        PontoInicial.Y + perp.Y * offset,
                        PontoFinal.X + perp.X * offset,
                        PontoFinal.Y + perp.Y * offset);

                    // Lado inferior
                    g.DrawLine(pen,
                        PontoInicial.X - perp.X * offset,
                        PontoInicial.Y - perp.Y * offset,
                        PontoFinal.X - perp.X * offset,
                        PontoFinal.Y - perp.Y * offset);
                }
                else
                {
                    // Rua curva - desenhar meio-fio seguindo a curva
                    List<PointF> ladoSuperior = new List<PointF>();
                    List<PointF> ladoInferior = new List<PointF>();

                    int segmentos = 30;
                    for (int i = 0; i <= segmentos; i++)
                    {
                        float t = i / (float)segmentos;
                        var ponto = GetPontoNaCurva(t);
                        var perpLocal = GetPerpendicularNaCurva(t);

                        ladoSuperior.Add(new PointF(
                            ponto.X + perpLocal.X * offset,
                            ponto.Y + perpLocal.Y * offset));
                        ladoInferior.Add(new PointF(
                            ponto.X - perpLocal.X * offset,
                            ponto.Y - perpLocal.Y * offset));
                    }

                    // Desenhar ambos os lados
                    if (ladoSuperior.Count > 1)
                        g.DrawLines(pen, ladoSuperior.ToArray());
                    if (ladoInferior.Count > 1)
                        g.DrawLines(pen, ladoInferior.ToArray());
                }
            }
        }

        private void DesenharFaixas(Graphics g, PointF perp)
        {
            var dir = Direcao;
            float comp = Comprimento;

            // Faixa central
            DesenharFaixaCentral(g, dir, comp);

            // Faixas laterais (divisão de pistas)
            if (MostrarFaixasLaterais && NumeroFaixas > 2)
            {
                DesenharFaixasLaterais(g, dir, perp, comp);
            }
        }

        private void DesenharFaixaCentral(Graphics g, PointF dir, float comp)
        {
            if (TipoFaixaCentral == TipoFaixaCentral.Nenhuma) return;

            var corAmarela = Color.FromArgb(CorFaixaAmarelaArgb);
            float espessura = EspessuraFaixa;

            if (!TemCurva || !PontoCurva.HasValue)
            {
                // Rua reta - código original
                switch (TipoFaixaCentral)
                {
                    case TipoFaixaCentral.TracejadaSimples:
                        DesenharLinhaTracejada(g, PontoInicial, PontoFinal,
                            corAmarela, espessura, ComprimentoTracejado, EspacamentoTracejado);
                        break;

                    case TipoFaixaCentral.ContinuaSimples:
                        using (var pen = new Pen(corAmarela, espessura))
                        {
                            g.DrawLine(pen, PontoInicial, PontoFinal);
                        }
                        break;

                    case TipoFaixaCentral.ContinuaDupla:
                        DesenharLinhaDupla(g, dir, corAmarela, espessura, false, false);
                        break;

                    case TipoFaixaCentral.ContinuaEsquerdaTracejadaDireita:
                        DesenharLinhaDupla(g, dir, corAmarela, espessura, false, true);
                        break;

                    case TipoFaixaCentral.TracejadaEsquerdaContinuaDireita:
                        DesenharLinhaDupla(g, dir, corAmarela, espessura, true, false);
                        break;
                }
            }
            else
            {
                // Rua curva - desenhar faixa seguindo a curva
                List<PointF> pontosCurva = new List<PointF>();
                int segmentos = 30;
                for (int i = 0; i <= segmentos; i++)
                {
                    float t = i / (float)segmentos;
                    pontosCurva.Add(GetPontoNaCurva(t));
                }

                switch (TipoFaixaCentral)
                {
                    case TipoFaixaCentral.TracejadaSimples:
                        DesenharLinhaTracejadaCurva(g, pontosCurva, corAmarela, espessura,
                            ComprimentoTracejado, EspacamentoTracejado);
                        break;

                    case TipoFaixaCentral.ContinuaSimples:
                        using (var pen = new Pen(corAmarela, espessura))
                        {
                            g.DrawLines(pen, pontosCurva.ToArray());
                        }
                        break;

                    case TipoFaixaCentral.ContinuaDupla:
                        DesenharLinhaDuplaCurva(g, corAmarela, espessura, false, false);
                        break;

                    case TipoFaixaCentral.ContinuaEsquerdaTracejadaDireita:
                        DesenharLinhaDuplaCurva(g, corAmarela, espessura, false, true);
                        break;

                    case TipoFaixaCentral.TracejadaEsquerdaContinuaDireita:
                        DesenharLinhaDuplaCurva(g, corAmarela, espessura, true, false);
                        break;
                }
            }
        }

        /// <summary>
        /// Desenha linha tracejada ao longo da curva
        /// </summary>
        private void DesenharLinhaTracejadaCurva(Graphics g, List<PointF> pontos, Color cor,
            float espessura, float comprimento, float espacamento)
        {
            using (var pen = new Pen(cor, espessura))
            {
                pen.DashStyle = DashStyle.Custom;
                pen.DashPattern = new float[]
                {
                    comprimento / espessura,
                    espacamento / espessura
                };
                g.DrawLines(pen, pontos.ToArray());
            }
        }

        /// <summary>
        /// Desenha linha dupla ao longo da curva
        /// </summary>
        private void DesenharLinhaDuplaCurva(Graphics g, Color cor, float espessura,
            bool esquerdaTracejada, bool direitaTracejada)
        {
            float offset = 2.5f; // Espaçamento entre as linhas

            List<PointF> pontosEsquerda = new List<PointF>();
            List<PointF> pontosDireita = new List<PointF>();

            int segmentos = 30;
            for (int i = 0; i <= segmentos; i++)
            {
                float t = i / (float)segmentos;
                var ponto = GetPontoNaCurva(t);
                var perp = GetPerpendicularNaCurva(t);

                pontosEsquerda.Add(new PointF(
                    ponto.X + perp.X * offset,
                    ponto.Y + perp.Y * offset));
                pontosDireita.Add(new PointF(
                    ponto.X - perp.X * offset,
                    ponto.Y - perp.Y * offset));
            }

            // Linha da esquerda
            if (esquerdaTracejada)
            {
                DesenharLinhaTracejadaCurva(g, pontosEsquerda, cor, espessura,
                    ComprimentoTracejado, EspacamentoTracejado);
            }
            else
            {
                using (var pen = new Pen(cor, espessura))
                {
                    g.DrawLines(pen, pontosEsquerda.ToArray());
                }
            }

            // Linha da direita
            if (direitaTracejada)
            {
                DesenharLinhaTracejadaCurva(g, pontosDireita, cor, espessura,
                    ComprimentoTracejado, EspacamentoTracejado);
            }
            else
            {
                using (var pen = new Pen(cor, espessura))
                {
                    g.DrawLines(pen, pontosDireita.ToArray());
                }
            }
        }

        private void DesenharLinhaDupla(Graphics g, PointF dir, Color cor,
            float espessura, bool esquerdaTracejada, bool direitaTracejada)
        {
            var perp = Perpendicular;
            float offset = 2.5f; // Espaçamento entre as linhas

            // Linha da esquerda
            var p1Esq = new PointF(
                PontoInicial.X + perp.X * offset,
                PontoInicial.Y + perp.Y * offset);
            var p2Esq = new PointF(
                PontoFinal.X + perp.X * offset,
                PontoFinal.Y + perp.Y * offset);

            if (esquerdaTracejada)
            {
                DesenharLinhaTracejada(g, p1Esq, p2Esq, cor, espessura,
                    ComprimentoTracejado, EspacamentoTracejado);
            }
            else
            {
                using (var pen = new Pen(cor, espessura))
                {
                    g.DrawLine(pen, p1Esq, p2Esq);
                }
            }

            // Linha da direita
            var p1Dir = new PointF(
                PontoInicial.X - perp.X * offset,
                PontoInicial.Y - perp.Y * offset);
            var p2Dir = new PointF(
                PontoFinal.X - perp.X * offset,
                PontoFinal.Y - perp.Y * offset);

            if (direitaTracejada)
            {
                DesenharLinhaTracejada(g, p1Dir, p2Dir, cor, espessura,
                    ComprimentoTracejado, EspacamentoTracejado);
            }
            else
            {
                using (var pen = new Pen(cor, espessura))
                {
                    g.DrawLine(pen, p1Dir, p2Dir);
                }
            }
        }

        private void DesenharLinhaTracejada(Graphics g, PointF inicio, PointF fim,
            Color cor, float espessura, float comprimento, float espacamento)
        {
            using (var pen = new Pen(cor, espessura))
            {
                pen.DashStyle = DashStyle.Custom;
                pen.DashPattern = new float[]
                {
                    comprimento / espessura,
                    espacamento / espessura
                };
                g.DrawLine(pen, inicio, fim);
            }
        }

        private void DesenharFaixasLaterais(Graphics g, PointF dir, PointF perp, float comp)
        {
            var corBranca = Color.FromArgb(CorFaixaBrancaArgb);
            float larguraFaixa = Largura / NumeroFaixas;

            if (!TemCurva || !PontoCurva.HasValue)
            {
                // Rua reta - código original
                for (int i = 1; i < NumeroFaixas; i++)
                {
                    // Pula a faixa central
                    if (i == NumeroFaixas / 2) continue;

                    float offset = -Largura / 2 + i * larguraFaixa;

                    var p1 = new PointF(
                        PontoInicial.X + perp.X * offset,
                        PontoInicial.Y + perp.Y * offset);
                    var p2 = new PointF(
                        PontoFinal.X + perp.X * offset,
                        PontoFinal.Y + perp.Y * offset);

                    DesenharLinhaTracejada(g, p1, p2, corBranca, EspessuraFaixa,
                        ComprimentoTracejado * 0.8f, EspacamentoTracejado * 1.2f);
                }
            }
            else
            {
                // Rua curva - desenhar faixas seguindo a curva
                for (int i = 1; i < NumeroFaixas; i++)
                {
                    // Pula a faixa central
                    if (i == NumeroFaixas / 2) continue;

                    float offset = -Largura / 2 + i * larguraFaixa;

                    List<PointF> pontosFaixa = new List<PointF>();
                    int segmentos = 30;
                    for (int j = 0; j <= segmentos; j++)
                    {
                        float t = j / (float)segmentos;
                        var ponto = GetPontoNaCurva(t);
                        var perpLocal = GetPerpendicularNaCurva(t);

                        pontosFaixa.Add(new PointF(
                            ponto.X + perpLocal.X * offset,
                            ponto.Y + perpLocal.Y * offset));
                    }

                    DesenharLinhaTracejadaCurva(g, pontosFaixa, corBranca, EspessuraFaixa,
                        ComprimentoTracejado * 0.8f, EspacamentoTracejado * 1.2f);
                }
            }
        }

        private void DesenharNomeRua(Graphics g)
        {
            var state = g.Save();
            g.TranslateTransform(Centro.X, Centro.Y);

            float textAngle = Angulo;
            if (textAngle > 90 || textAngle < -90)
                textAngle += 180;
            g.RotateTransform(textAngle);

            using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                var size = g.MeasureString(NomeRua, font);

                // Fundo
                using (var bg = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                {
                    g.FillRectangle(bg,
                        -size.Width / 2 - 4, -size.Height / 2 - 2,
                        size.Width + 8, size.Height + 4);
                }

                // Texto
                g.DrawString(NomeRua, font, Brushes.DarkSlateGray, 0, 0, format);
            }

            g.Restore(state);
        }

        private void DesenharPontosConexao(Graphics g)
        {
            float radius = 6f;

            // Ponto inicial
            Color corInicial = ExtremidadeInicial == TipoExtremidade.Conectada
                ? Color.LimeGreen : Color.Orange;
            using (var brush = new SolidBrush(corInicial))
            {
                g.FillEllipse(brush,
                    PontoInicial.X - radius, PontoInicial.Y - radius,
                    radius * 2, radius * 2);
            }
            g.DrawEllipse(Pens.White,
                PontoInicial.X - radius, PontoInicial.Y - radius,
                radius * 2, radius * 2);

            // Ponto final
            Color corFinal = ExtremidadeFinal == TipoExtremidade.Conectada
                ? Color.LimeGreen : Color.Orange;
            using (var brush = new SolidBrush(corFinal))
            {
                g.FillEllipse(brush,
                    PontoFinal.X - radius, PontoFinal.Y - radius,
                    radius * 2, radius * 2);
            }
            g.DrawEllipse(Pens.White,
                PontoFinal.X - radius, PontoFinal.Y - radius,
                radius * 2, radius * 2);

            // Ponto de controle de curva (se houver)
            if (TemCurva && PontoCurva.HasValue)
            {
                var pc = PontoCurva.Value;

                // Linhas de controle tracejadas
                using (var pen = new Pen(Color.DodgerBlue, 1f))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawLine(pen, PontoInicial, pc);
                    g.DrawLine(pen, pc, PontoFinal);
                }

                // Ponto de controle (diamante)
                float curveRadius = 7f;
                PointF[] diamond = new PointF[]
                {
                    new PointF(pc.X, pc.Y - curveRadius),
                    new PointF(pc.X + curveRadius, pc.Y),
                    new PointF(pc.X, pc.Y + curveRadius),
                    new PointF(pc.X - curveRadius, pc.Y)
                };

                using (var brush = new SolidBrush(Color.Cyan))
                {
                    g.FillPolygon(brush, diamond);
                }
                using (var pen = new Pen(Color.DodgerBlue, 2f))
                {
                    g.DrawPolygon(pen, diamond);
                }
            }
        }

        private PointF[] GetPoligono(float largura, PointF perp)
        {
            float halfW = largura / 2;
            return new PointF[]
            {
                new PointF(PontoInicial.X + perp.X * halfW, PontoInicial.Y + perp.Y * halfW),
                new PointF(PontoFinal.X + perp.X * halfW, PontoFinal.Y + perp.Y * halfW),
                new PointF(PontoFinal.X - perp.X * halfW, PontoFinal.Y - perp.Y * halfW),
                new PointF(PontoInicial.X - perp.X * halfW, PontoInicial.Y - perp.Y * halfW)
            };
        }

        #endregion

        #region Métodos de Curva Bézier

        private void AplicarRotacaoPendente()
        {
            float delta = Rotacao - _rotacaoAplicada;
            if (Math.Abs(delta) < 0.001f)
                return;

            RotacionarPontoDaRua(delta);
            _rotacaoAplicada = Rotacao;
        }

        private void RotacionarPontoDaRua(float anguloGraus)
        {
            var centro = Centro;
            float rad = (float)(anguloGraus * Math.PI / 180.0);
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            PointF Rotacionar(PointF p)
            {
                float x = p.X - centro.X;
                float y = p.Y - centro.Y;
                return new PointF(
                    centro.X + x * cos - y * sin,
                    centro.Y + x * sin + y * cos);
            }

            PontoInicial = Rotacionar(PontoInicial);
            PontoFinal = Rotacionar(PontoFinal);

            if (PontoCurva.HasValue)
            {
                PontoCurva = Rotacionar(PontoCurva.Value);
            }
        }

        /// <summary>
        /// Calcula um ponto na curva Bézier quadrática
        /// </summary>
        /// <param name="t">Parâmetro entre 0 e 1</param>
        private PointF GetPontoNaCurva(float t)
        {
            if (!TemCurva || !PontoCurva.HasValue)
            {
                // Interpolação linear se não houver curva
                return new PointF(
                    PontoInicial.X + (PontoFinal.X - PontoInicial.X) * t,
                    PontoInicial.Y + (PontoFinal.Y - PontoInicial.Y) * t);
            }

            // Curva Bézier quadrática: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ut2 = 2 * u * t;

            PointF p = new PointF(
                uu * PontoInicial.X + ut2 * PontoCurva.Value.X + tt * PontoFinal.X,
                uu * PontoInicial.Y + ut2 * PontoCurva.Value.Y + tt * PontoFinal.Y);

            return p;
        }

        /// <summary>
        /// Calcula a derivada (tangente) da curva em um ponto
        /// </summary>
        private PointF GetTangenteNaCurva(float t)
        {
            if (!TemCurva || !PontoCurva.HasValue)
            {
                float deltaX = PontoFinal.X - PontoInicial.X;
                float deltaY = PontoFinal.Y - PontoInicial.Y;
                float length = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                return new PointF(deltaX / length, deltaY / length);
            }

            // Derivada da Bézier: B'(t) = 2(1-t)(P1-P0) + 2t(P2-P1)
            float u = 1 - t;
            float tangDx = 2 * u * (PontoCurva.Value.X - PontoInicial.X) + 2 * t * (PontoFinal.X - PontoCurva.Value.X);
            float tangDy = 2 * u * (PontoCurva.Value.Y - PontoInicial.Y) + 2 * t * (PontoFinal.Y - PontoCurva.Value.Y);

            float tangLen = (float)Math.Sqrt(tangDx * tangDx + tangDy * tangDy);
            if (tangLen > 0)
            {
                return new PointF(tangDx / tangLen, tangDy / tangLen);
            }
            return new PointF(1, 0);
        }

        /// <summary>
        /// Retorna o vetor perpendicular à curva em um ponto
        /// </summary>
        private PointF GetPerpendicularNaCurva(float t)
        {
            var tangente = GetTangenteNaCurva(t);
            return new PointF(-tangente.Y, tangente.X);
        }

        /// <summary>
        /// Cria um GraphicsPath para a curva
        /// </summary>
        private GraphicsPath GetCurvaPath()
        {
            var path = new GraphicsPath();

            if (!TemCurva || !PontoCurva.HasValue)
            {
                path.AddLine(PontoInicial, PontoFinal);
            }
            else
            {
                // Usar curva Bézier quadrática
                // GraphicsPath não tem Bézier quadrática direta, então aproximamos com pontos
                List<PointF> pontos = new List<PointF>();
                int segmentos = 30;
                for (int i = 0; i <= segmentos; i++)
                {
                    float t = i / (float)segmentos;
                    pontos.Add(GetPontoNaCurva(t));
                }
                if (pontos.Count > 1)
                {
                    path.AddLines(pontos.ToArray());
                }
            }

            return path;
        }

        #endregion

        #region Hit Test e Bounds

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            AplicarRotacaoPendente();

            float totalW = Largura + (TemCalcada ? LarguraCalcada * 2 : 0);
            return Utils.GeometryHelper.DistanciaPontoSegmento(
                ponto, PontoInicial, PontoFinal) <= totalW / 2 + tolerancia;
        }

        public override RectangleF GetBounds()
        {
            AplicarRotacaoPendente();

            float totalW = Largura + (TemCalcada ? LarguraCalcada * 2 : 0);

            if (!TemCurva || !PontoCurva.HasValue)
            {
                // Bounds para rua reta
                float minX = Math.Min(PontoInicial.X, PontoFinal.X) - totalW;
                float minY = Math.Min(PontoInicial.Y, PontoFinal.Y) - totalW;
                float maxX = Math.Max(PontoInicial.X, PontoFinal.X) + totalW;
                float maxY = Math.Max(PontoInicial.Y, PontoFinal.Y) + totalW;
                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }
            else
            {
                // Bounds para rua curva - considera todos os pontos da curva
                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;

                int segmentos = 20;
                for (int i = 0; i <= segmentos; i++)
                {
                    float t = i / (float)segmentos;
                    var ponto = GetPontoNaCurva(t);
                    var perp = GetPerpendicularNaCurva(t);
                    float halfW = totalW / 2;

                    // Pontos dos dois lados da rua
                    float x1 = ponto.X + perp.X * halfW;
                    float y1 = ponto.Y + perp.Y * halfW;
                    float x2 = ponto.X - perp.X * halfW;
                    float y2 = ponto.Y - perp.Y * halfW;

                    minX = Math.Min(minX, Math.Min(x1, x2));
                    minY = Math.Min(minY, Math.Min(y1, y2));
                    maxX = Math.Max(maxX, Math.Max(x1, x2));
                    maxY = Math.Max(maxY, Math.Max(y1, y2));
                }

                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }
        }

        public override void Mover(float dx, float dy)
        {
            PontoInicial = new PointF(PontoInicial.X + dx, PontoInicial.Y + dy);
            PontoFinal = new PointF(PontoFinal.X + dx, PontoFinal.Y + dy);
            Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy);
            if (PontoCurva.HasValue)
            {
                PontoCurva = new PointF(PontoCurva.Value.X + dx, PontoCurva.Value.Y + dy);
            }
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            PontoInicial = EscalarPonto(PontoInicial, centro, fatorX, fatorY);
            PontoFinal = EscalarPonto(PontoFinal, centro, fatorX, fatorY);
            Posicao = EscalarPonto(Posicao, centro, fatorX, fatorY);

            if (PontoCurva.HasValue)
            {
                PontoCurva = EscalarPonto(PontoCurva.Value, centro, fatorX, fatorY);
            }

            float media = (Math.Abs(fatorX) + Math.Abs(fatorY)) / 2f;
            Largura = Math.Max(10f, Largura * media);
            LarguraCalcada = Math.Max(2f, LarguraCalcada * media);
            ComprimentoTracejado = Math.Max(4f, ComprimentoTracejado * media);
            EspacamentoTracejado = Math.Max(2f, EspacamentoTracejado * media);
            EspessuraFaixa = Math.Max(1f, EspessuraFaixa * media);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            PontoInicial = RotacionarPonto(PontoInicial, centro, deltaGraus);
            PontoFinal = RotacionarPonto(PontoFinal, centro, deltaGraus);
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);

            if (PontoCurva.HasValue)
            {
                PontoCurva = RotacionarPonto(PontoCurva.Value, centro, deltaGraus);
            }

            Rotacao += deltaGraus;
            _rotacaoAplicada += deltaGraus;
        }

        /// <summary>
        /// Verifica se o ponto está próximo ao ponto de controle de curva
        /// </summary>
        public bool ContemPontoCurva(PointF ponto, float tolerancia = 10f)
        {
            if (!TemCurva || !PontoCurva.HasValue) return false;

            float dx = ponto.X - PontoCurva.Value.X;
            float dy = ponto.Y - PontoCurva.Value.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            return dist <= tolerancia;
        }

        /// <summary>
        /// Move o ponto de controle de curva
        /// </summary>
        public void MoverPontoCurva(PointF novaPosicao)
        {
            if (TemCurva)
            {
                PontoCurva = novaPosicao;
            }
        }

        #endregion

        #region Conexões

        /// <summary>
        /// Verifica se um ponto está próximo a uma extremidade
        /// </summary>
        public int GetExtremidadeProxima(PointF ponto, float tolerancia = 15f)
        {
            float distInicial = Utils.GeometryHelper.Distancia(ponto, PontoInicial);
            float distFinal = Utils.GeometryHelper.Distancia(ponto, PontoFinal);

            if (distInicial <= tolerancia && distInicial < distFinal)
                return 0; // Extremidade inicial
            if (distFinal <= tolerancia)
                return 1; // Extremidade final
            return -1; // Nenhuma
        }

        /// <summary>
        /// Retorna o ponto da extremidade
        /// </summary>
        public PointF GetPontoExtremidade(int extremidade)
        {
            return extremidade == 0 ? PontoInicial : PontoFinal;
        }

        /// <summary>
        /// Define o ponto de uma extremidade
        /// </summary>
        public void SetPontoExtremidade(int extremidade, PointF ponto)
        {
            if (extremidade == 0)
                PontoInicial = ponto;
            else
                PontoFinal = ponto;
        }

        #endregion
    }
}