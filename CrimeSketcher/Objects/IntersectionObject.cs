// Objects/IntersectionObject.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    /// <summary>
    /// Cruzamento de ruas (Cruz ou T)
    /// </summary>
    [Serializable]
    public class IntersectionObject : BaseSketchObject
    {
        [Category("Configuração")]
        [DisplayName("Tipo de Cruzamento")]
        [Description("Tipo de interseção: Cruz (4 vias) ou T (3 vias)")]
        public TipoCruzamento TipoCruzamento { get; set; } = TipoCruzamento.Cruz;

        [Category("Dimensões")]
        [DisplayName("Largura da Rua")]
        [Description("Largura das vias no cruzamento")]
        public float LarguraRua { get; set; } = 80f;

        [Category("Calçada")]
        [DisplayName("Largura da Calçada")]
        [Description("Largura das calçadas no cruzamento")]
        public float LarguraCalcada { get; set; } = 15f;

        [Category("Calçada")]
        [DisplayName("Possui Calçada")]
        [Description("Desenha calçadas nos cantos do cruzamento")]
        public bool TemCalcada { get; set; } = true;

        [Category("Faixa de Pedestre")]
        [DisplayName("Possui Faixa de Pedestre")]
        [Description("Desenha faixas de pedestre no cruzamento")]
        public bool TemFaixaPedestre { get; set; } = true;

        [Category("Faixa de Pedestre")]
        [DisplayName("Largura da Faixa")]
        [Description("Largura das faixas de pedestres")]
        public float LarguraFaixaPedestre { get; set; } = 25f;

        [Browsable(false)]
        public int CorAsfaltoArgb { get; set; } = Color.FromArgb(180, 180, 180).ToArgb();

        [Browsable(false)]
        public int CorCalcadaArgb { get; set; } = Color.FromArgb(210, 210, 200).ToArgb();

        [Browsable(false)]
        public int CorFaixaArgb { get; set; } = Color.White.ToArgb();

        [Category("Aparência")]
        [DisplayName("Cor do Asfalto")]
        [Description("Cor do asfalto no cruzamento")]
        [JsonIgnore]
        public Color CorAsfalto
        {
            get => Color.FromArgb(CorAsfaltoArgb);
            set => CorAsfaltoArgb = value.ToArgb();
        }

        [Category("Aparência")]
        [DisplayName("Cor da Calçada")]
        [Description("Cor das calçadas")]
        [JsonIgnore]
        public Color CorCalcada
        {
            get => Color.FromArgb(CorCalcadaArgb);
            set => CorCalcadaArgb = value.ToArgb();
        }

        [Category("Aparência")]
        [DisplayName("Cor da Faixa")]
        [Description("Cor das faixas de pedestre")]
        [JsonIgnore]
        public Color CorFaixa
        {
            get => Color.FromArgb(CorFaixaArgb);
            set => CorFaixaArgb = value.ToArgb();
        }

        // IDs das ruas conectadas (até 4)
        [Browsable(false)]
        public List<string> RuasConectadas { get; set; } = new List<string>();

        public IntersectionObject()
        {
            Tipo = "Cruzamento";
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            var state = g.Save();
            g.TranslateTransform(Posicao.X, Posicao.Y);
            g.RotateTransform(Rotacao);

            float tamanho = LarguraRua + (TemCalcada ? LarguraCalcada * 2 : 0);
            float meiaRua = LarguraRua / 2;
            float meiaTamanho = tamanho / 2;

            // Desenhar baseado no tipo
            switch (TipoCruzamento)
            {
                case TipoCruzamento.Cruz:
                    DesenharCruz(g, meiaRua, meiaTamanho);
                    break;
                case TipoCruzamento.TParaCima:
                case TipoCruzamento.TParaBaixo:
                case TipoCruzamento.TParaEsquerda:
                case TipoCruzamento.TParaDireita:
                    DesenharT(g, meiaRua, meiaTamanho);
                    break;
            }

            g.Restore(state);

            if (Selecionado) DesenharSelecao(g);
        }

        private void DesenharCruz(Graphics g, float meiaRua, float meiaTamanho)
        {
            // Calçada (área total)
            if (TemCalcada)
            {
                using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                {
                    // Quatro cantos de calçada
                    float calc = LarguraCalcada;

                    // Canto superior esquerdo
                    g.FillRectangle(brush, -meiaTamanho, -meiaTamanho, calc, calc);
                    // Canto superior direito
                    g.FillRectangle(brush, meiaRua, -meiaTamanho, calc, calc);
                    // Canto inferior esquerdo
                    g.FillRectangle(brush, -meiaTamanho, meiaRua, calc, calc);
                    // Canto inferior direito
                    g.FillRectangle(brush, meiaRua, meiaRua, calc, calc);
                }
            }

            // Asfalto (cruz)
            using (var brush = new SolidBrush(Color.FromArgb(CorAsfaltoArgb)))
            {
                // Faixa horizontal
                g.FillRectangle(brush, -meiaTamanho, -meiaRua, meiaTamanho * 2, LarguraRua);
                // Faixa vertical
                g.FillRectangle(brush, -meiaRua, -meiaTamanho, LarguraRua, meiaTamanho * 2);
            }

            // Textura de asfalto
            using (var brush = new HatchBrush(HatchStyle.Percent10,
                Color.FromArgb(20, 0, 0, 0), Color.Transparent))
            {
                g.FillRectangle(brush, -meiaTamanho, -meiaRua, meiaTamanho * 2, LarguraRua);
                g.FillRectangle(brush, -meiaRua, -meiaTamanho, LarguraRua, meiaTamanho * 2);
            }

            // Faixas de pedestre
            if (TemFaixaPedestre)
            {
                DesenharFaixasPedestre(g, meiaRua, 4); // 4 direções
            }

            // Meio-fio (contorno)
            using (var pen = new Pen(Color.FromArgb(150, 150, 140), 2f))
            {
                // Contorno externo
                var path = CriarContornoCruz(meiaRua);
                g.DrawPath(pen, path);
            }
        }

        private void DesenharT(Graphics g, float meiaRua, float meiaTamanho)
        {
            bool temCima = TipoCruzamento != TipoCruzamento.TParaBaixo;
            bool temBaixo = TipoCruzamento != TipoCruzamento.TParaCima;
            bool temEsquerda = TipoCruzamento != TipoCruzamento.TParaDireita;
            bool temDireita = TipoCruzamento != TipoCruzamento.TParaEsquerda;

            // Calçadas nos cantos
            if (TemCalcada)
            {
                using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                {
                    float calc = LarguraCalcada;

                    if (!temCima)
                    {
                        // Topo fechado - calçada reta
                        g.FillRectangle(brush, -meiaTamanho, -meiaTamanho,
                            meiaTamanho * 2, calc);
                    }
                    else
                    {
                        // Cantos superiores
                        g.FillRectangle(brush, -meiaTamanho, -meiaTamanho, calc, calc);
                        g.FillRectangle(brush, meiaRua, -meiaTamanho, calc, calc);
                    }

                    if (!temBaixo)
                    {
                        g.FillRectangle(brush, -meiaTamanho, meiaRua,
                            meiaTamanho * 2, calc);
                    }
                    else
                    {
                        g.FillRectangle(brush, -meiaTamanho, meiaRua, calc, calc);
                        g.FillRectangle(brush, meiaRua, meiaRua, calc, calc);
                    }
                }
            }

            // Asfalto
            using (var brush = new SolidBrush(Color.FromArgb(CorAsfaltoArgb)))
            {
                // Horizontal sempre
                g.FillRectangle(brush, -meiaTamanho, -meiaRua, meiaTamanho * 2, LarguraRua);

                // Vertical conforme tipo
                if (temCima)
                    g.FillRectangle(brush, -meiaRua, -meiaTamanho, LarguraRua, meiaTamanho);
                if (temBaixo)
                    g.FillRectangle(brush, -meiaRua, 0, LarguraRua, meiaTamanho);
            }

            // Faixas de pedestre
            if (TemFaixaPedestre)
            {
                int numFaixas = (temCima ? 1 : 0) + (temBaixo ? 1 : 0) +
                    (temEsquerda ? 1 : 0) + (temDireita ? 1 : 0);
                DesenharFaixasPedestreT(g, meiaRua, temCima, temBaixo, temEsquerda, temDireita);
            }
        }

        private void DesenharFaixasPedestre(Graphics g, float meiaRua, int numDirecoes)
        {
            using (var brush = new SolidBrush(Color.FromArgb(CorFaixaArgb)))
            {
                float larguraLista = 4f;
                float espacamento = 3f;
                float distancia = meiaRua + 5f;

                // Norte
                DesenharFaixaUnica(g, brush, 0, -distancia, LarguraFaixaPedestre, true,
                    larguraLista, espacamento);
                // Sul
                DesenharFaixaUnica(g, brush, 0, distancia, LarguraFaixaPedestre, true,
                    larguraLista, espacamento);
                // Leste
                DesenharFaixaUnica(g, brush, distancia, 0, LarguraFaixaPedestre, false,
                    larguraLista, espacamento);
                // Oeste
                DesenharFaixaUnica(g, brush, -distancia, 0, LarguraFaixaPedestre, false,
                    larguraLista, espacamento);
            }
        }

        private void DesenharFaixasPedestreT(Graphics g, float meiaRua,
            bool cima, bool baixo, bool esquerda, bool direita)
        {
            using (var brush = new SolidBrush(Color.FromArgb(CorFaixaArgb)))
            {
                float larguraLista = 4f;
                float espacamento = 3f;
                float distancia = meiaRua + 5f;

                if (cima)
                    DesenharFaixaUnica(g, brush, 0, -distancia, LarguraFaixaPedestre, true,
                        larguraLista, espacamento);
                if (baixo)
                    DesenharFaixaUnica(g, brush, 0, distancia, LarguraFaixaPedestre, true,
                        larguraLista, espacamento);
                if (direita)
                    DesenharFaixaUnica(g, brush, distancia, 0, LarguraFaixaPedestre, false,
                        larguraLista, espacamento);
                if (esquerda)
                    DesenharFaixaUnica(g, brush, -distancia, 0, LarguraFaixaPedestre, false,
                        larguraLista, espacamento);
            }
        }

        private void DesenharFaixaUnica(Graphics g, Brush brush, float cx, float cy,
            float largura, bool horizontal, float espLista, float espaco)
        {
            int numListas = (int)(LarguraRua / (espLista + espaco));

            for (int i = 0; i < numListas; i++)
            {
                float offset = -LarguraRua / 2 + i * (espLista + espaco) + espLista / 2;

                if (horizontal)
                {
                    g.FillRectangle(brush,
                        cx + offset - espLista / 2, cy - largura / 2,
                        espLista, largura);
                }
                else
                {
                    g.FillRectangle(brush,
                        cx - largura / 2, cy + offset - espLista / 2,
                        largura, espLista);
                }
            }
        }

        private GraphicsPath CriarContornoCruz(float meiaRua)
        {
            var path = new GraphicsPath();
            float m = meiaRua;
            float t = meiaRua + (TemCalcada ? LarguraCalcada : 0);

            // Contorno em forma de cruz
            path.AddPolygon(new PointF[]
            {
                new PointF(-m, -t),
                new PointF(m, -t),
                new PointF(m, -m),
                new PointF(t, -m),
                new PointF(t, m),
                new PointF(m, m),
                new PointF(m, t),
                new PointF(-m, t),
                new PointF(-m, m),
                new PointF(-t, m),
                new PointF(-t, -m),
                new PointF(-m, -m)
            });

            return path;
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            var bounds = GetBounds();
            bounds.Inflate(tolerancia, tolerancia);
            return bounds.Contains(ponto);
        }

        public override RectangleF GetBounds()
        {
            float tamanho = LarguraRua + (TemCalcada ? LarguraCalcada * 2 : 0);
            return new RectangleF(
                Posicao.X - tamanho / 2,
                Posicao.Y - tamanho / 2,
                tamanho, tamanho);
        }

        /// <summary>
        /// Pontos de conexão para ruas (em coordenadas locais)
        /// </summary>
        public PointF[] GetPontosConexao()
        {
            float dist = LarguraRua / 2 + (TemCalcada ? LarguraCalcada : 0);

            switch (TipoCruzamento)
            {
                case TipoCruzamento.Cruz:
                    return new PointF[]
                    {
                        new PointF(Posicao.X, Posicao.Y - dist),  // Norte
                        new PointF(Posicao.X + dist, Posicao.Y),  // Leste
                        new PointF(Posicao.X, Posicao.Y + dist),  // Sul
                        new PointF(Posicao.X - dist, Posicao.Y)   // Oeste
                    };

                case TipoCruzamento.TParaCima:
                    return new PointF[]
                    {
                        new PointF(Posicao.X, Posicao.Y - dist),
                        new PointF(Posicao.X + dist, Posicao.Y),
                        new PointF(Posicao.X - dist, Posicao.Y)
                    };

                case TipoCruzamento.TParaBaixo:
                    return new PointF[]
                    {
                        new PointF(Posicao.X + dist, Posicao.Y),
                        new PointF(Posicao.X, Posicao.Y + dist),
                        new PointF(Posicao.X - dist, Posicao.Y)
                    };

                case TipoCruzamento.TParaEsquerda:
                    return new PointF[]
                    {
                        new PointF(Posicao.X, Posicao.Y - dist),
                        new PointF(Posicao.X, Posicao.Y + dist),
                        new PointF(Posicao.X - dist, Posicao.Y)
                    };

                case TipoCruzamento.TParaDireita:
                    return new PointF[]
                    {
                        new PointF(Posicao.X, Posicao.Y - dist),
                        new PointF(Posicao.X + dist, Posicao.Y),
                        new PointF(Posicao.X, Posicao.Y + dist)
                    };

                default:
                    return new PointF[0];
            }
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            float media = (Math.Abs(fatorX) + Math.Abs(fatorY)) / 2f;
            LarguraRua = Math.Max(10f, LarguraRua * media);
            LarguraCalcada = Math.Max(2f, LarguraCalcada * media);
            LarguraFaixaPedestre = Math.Max(6f, LarguraFaixaPedestre * media);
            Posicao = EscalarPonto(Posicao, centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }
    }
}