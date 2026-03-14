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

        [Category("Dimensões")]
        [DisplayName("Extensão das Vias")]
        [Description("Comprimento do trecho de rua adicional em cada acesso")]
        public float ExtensaoVias { get; set; } = 40f;

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
            float ext = Math.Max(0f, ExtensaoVias);

            // Calçada (cantos + extensões)
            if (TemCalcada)
            {
                using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                {
                    float calc = LarguraCalcada;

                    // Cantos
                    g.FillRectangle(brush, -meiaTamanho, -meiaTamanho, calc, calc);
                    g.FillRectangle(brush, meiaRua, -meiaTamanho, calc, calc);
                    g.FillRectangle(brush, -meiaTamanho, meiaRua, calc, calc);
                    g.FillRectangle(brush, meiaRua, meiaRua, calc, calc);

                    // Extensões horizontais (faixas superior/inferior de calçada)
                    g.FillRectangle(brush, -meiaTamanho - ext, -meiaRua - calc, ext, calc);
                    g.FillRectangle(brush, -meiaTamanho - ext, meiaRua, ext, calc);
                    g.FillRectangle(brush, meiaTamanho, -meiaRua - calc, ext, calc);
                    g.FillRectangle(brush, meiaTamanho, meiaRua, ext, calc);

                    // Extensões verticais (faixas esquerda/direita de calçada)
                    g.FillRectangle(brush, -meiaRua - calc, -meiaTamanho - ext, calc, ext);
                    g.FillRectangle(brush, meiaRua, -meiaTamanho - ext, calc, ext);
                    g.FillRectangle(brush, -meiaRua - calc, meiaTamanho, calc, ext);
                    g.FillRectangle(brush, meiaRua, meiaTamanho, calc, ext);
                }
            }

            // Asfalto (cruz + extensões)
            using (var brush = new SolidBrush(Color.FromArgb(CorAsfaltoArgb)))
            {
                g.FillRectangle(brush, -(meiaTamanho + ext), -meiaRua, (meiaTamanho + ext) * 2, LarguraRua);
                g.FillRectangle(brush, -meiaRua, -(meiaTamanho + ext), LarguraRua, (meiaTamanho + ext) * 2);
            }

            // Textura de asfalto
            using (var brush = new HatchBrush(HatchStyle.Percent10,
                Color.FromArgb(20, 0, 0, 0), Color.Transparent))
            {
                g.FillRectangle(brush, -(meiaTamanho + ext), -meiaRua, (meiaTamanho + ext) * 2, LarguraRua);
                g.FillRectangle(brush, -meiaRua, -(meiaTamanho + ext), LarguraRua, (meiaTamanho + ext) * 2);
            }

            // Meio-fio sem atravessar a pista
            DesenharMeioFioBracos(g, meiaRua, meiaTamanho, ext,
                cima: true, baixo: true, esquerda: true, direita: true);

            // Faixas de pedestre (afastadas da área central)
            if (TemFaixaPedestre)
            {
                DesenharFaixasPedestre(g, meiaRua, meiaTamanho);
            }
        }

        private void DesenharT(Graphics g, float meiaRua, float meiaTamanho)
        {
            bool temCima = TipoCruzamento != TipoCruzamento.TParaBaixo;
            bool temBaixo = TipoCruzamento != TipoCruzamento.TParaCima;
            bool temEsquerda = TipoCruzamento != TipoCruzamento.TParaDireita;
            bool temDireita = TipoCruzamento != TipoCruzamento.TParaEsquerda;
            float ext = Math.Max(0f, ExtensaoVias);

            if (TemCalcada)
            {
                using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                {
                    float calc = LarguraCalcada;

                    if (!temCima)
                        g.FillRectangle(brush, -meiaTamanho, -meiaTamanho, meiaTamanho * 2, calc);
                    else
                    {
                        g.FillRectangle(brush, -meiaTamanho, -meiaTamanho, calc, calc);
                        g.FillRectangle(brush, meiaRua, -meiaTamanho, calc, calc);
                    }

                    if (!temBaixo)
                        g.FillRectangle(brush, -meiaTamanho, meiaRua, meiaTamanho * 2, calc);
                    else
                    {
                        g.FillRectangle(brush, -meiaTamanho, meiaRua, calc, calc);
                        g.FillRectangle(brush, meiaRua, meiaRua, calc, calc);
                    }

                    // Extensões de calçada acompanhando os braços existentes
                    if (temEsquerda)
                    {
                        g.FillRectangle(brush, -meiaTamanho - ext, -meiaRua - calc, ext, calc);
                        g.FillRectangle(brush, -meiaTamanho - ext, meiaRua, ext, calc);
                    }

                    if (temDireita)
                    {
                        g.FillRectangle(brush, meiaTamanho, -meiaRua - calc, ext, calc);
                        g.FillRectangle(brush, meiaTamanho, meiaRua, ext, calc);
                    }

                    if (temCima)
                    {
                        g.FillRectangle(brush, -meiaRua - calc, -meiaTamanho - ext, calc, ext);
                        g.FillRectangle(brush, meiaRua, -meiaTamanho - ext, calc, ext);
                    }

                    if (temBaixo)
                    {
                        g.FillRectangle(brush, -meiaRua - calc, meiaTamanho, calc, ext);
                        g.FillRectangle(brush, meiaRua, meiaTamanho, calc, ext);
                    }
                }
            }

            using (var brush = new SolidBrush(Color.FromArgb(CorAsfaltoArgb)))
            {
                g.FillRectangle(brush, -meiaTamanho, -meiaRua, meiaTamanho * 2, LarguraRua);

                if (temEsquerda)
                    g.FillRectangle(brush, -meiaTamanho - ext, -meiaRua, ext, LarguraRua);
                if (temDireita)
                    g.FillRectangle(brush, meiaTamanho, -meiaRua, ext, LarguraRua);

                if (temCima)
                    g.FillRectangle(brush, -meiaRua, -meiaTamanho - ext, LarguraRua, ext + meiaTamanho);
                if (temBaixo)
                    g.FillRectangle(brush, -meiaRua, 0, LarguraRua, meiaTamanho + ext);
            }

            using (var brush = new HatchBrush(HatchStyle.Percent10,
                Color.FromArgb(20, 0, 0, 0), Color.Transparent))
            {
                g.FillRectangle(brush, -meiaTamanho, -meiaRua, meiaTamanho * 2, LarguraRua);

                if (temEsquerda)
                    g.FillRectangle(brush, -meiaTamanho - ext, -meiaRua, ext, LarguraRua);
                if (temDireita)
                    g.FillRectangle(brush, meiaTamanho, -meiaRua, ext, LarguraRua);

                if (temCima)
                    g.FillRectangle(brush, -meiaRua, -meiaTamanho - ext, LarguraRua, ext + meiaTamanho);
                if (temBaixo)
                    g.FillRectangle(brush, -meiaRua, 0, LarguraRua, meiaTamanho + ext);
            }

            // Meio-fio sem atravessar a pista
            DesenharMeioFioBracos(g, meiaRua, meiaTamanho, ext,
                temCima, temBaixo, temEsquerda, temDireita);

            if (TemFaixaPedestre)
            {
                DesenharFaixasPedestreT(g, meiaRua, meiaTamanho, temCima, temBaixo, temEsquerda, temDireita);
            }
        }

        private void DesenharFaixasPedestre(Graphics g, float meiaRua, float meiaTamanho)
        {
            using (var brush = new SolidBrush(Color.FromArgb(CorFaixaArgb)))
            {
                float larguraLista = 4f;
                float espacamento = 3f;
                float distancia = meiaTamanho + 5f;

                DesenharFaixaUnica(g, brush, 0, -distancia, LarguraFaixaPedestre, true, larguraLista, espacamento);
                DesenharFaixaUnica(g, brush, 0, distancia, LarguraFaixaPedestre, true, larguraLista, espacamento);
                DesenharFaixaUnica(g, brush, distancia, 0, LarguraFaixaPedestre, false, larguraLista, espacamento);
                DesenharFaixaUnica(g, brush, -distancia, 0, LarguraFaixaPedestre, false, larguraLista, espacamento);
            }
        }

        private void DesenharFaixasPedestreT(Graphics g, float meiaRua, float meiaTamanho,
            bool cima, bool baixo, bool esquerda, bool direita)
        {
            using (var brush = new SolidBrush(Color.FromArgb(CorFaixaArgb)))
            {
                float larguraLista = 4f;
                float espacamento = 3f;
                float distancia = meiaTamanho + 5f;

                if (cima)
                    DesenharFaixaUnica(g, brush, 0, -distancia, LarguraFaixaPedestre, true, larguraLista, espacamento);
                if (baixo)
                    DesenharFaixaUnica(g, brush, 0, distancia, LarguraFaixaPedestre, true, larguraLista, espacamento);
                if (direita)
                    DesenharFaixaUnica(g, brush, distancia, 0, LarguraFaixaPedestre, false, larguraLista, espacamento);
                if (esquerda)
                    DesenharFaixaUnica(g, brush, -distancia, 0, LarguraFaixaPedestre, false, larguraLista, espacamento);
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
            float tamanho = LarguraRua + (TemCalcada ? LarguraCalcada * 2 : 0) + ExtensaoVias * 2;
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
            float dist = LarguraRua / 2 + (TemCalcada ? LarguraCalcada : 0) + ExtensaoVias;

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
            ExtensaoVias = Math.Max(0f, ExtensaoVias * media);
            Posicao = EscalarPonto(Posicao, centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }

        private void DesenharMeioFioBracos(Graphics g, float meiaRua, float meiaTamanho, float ext,
            bool cima, bool baixo, bool esquerda, bool direita)
        {
            using (var pen = new Pen(Color.FromArgb(150, 150, 140), 2f))
            {
                // Braço esquerdo: bordas superior e inferior
                if (esquerda)
                {
                    g.DrawLine(pen, -meiaTamanho - ext, -meiaRua, -meiaRua, -meiaRua);
                    g.DrawLine(pen, -meiaTamanho - ext, meiaRua, -meiaRua, meiaRua);
                }

                // Braço direito: bordas superior e inferior
                if (direita)
                {
                    g.DrawLine(pen, meiaRua, -meiaRua, meiaTamanho + ext, -meiaRua);
                    g.DrawLine(pen, meiaRua, meiaRua, meiaTamanho + ext, meiaRua);
                }

                // Braço superior: bordas esquerda e direita
                if (cima)
                {
                    g.DrawLine(pen, -meiaRua, -meiaTamanho - ext, -meiaRua, -meiaRua);
                    g.DrawLine(pen, meiaRua, -meiaTamanho - ext, meiaRua, -meiaRua);
                }

                // Braço inferior: bordas esquerda e direita
                if (baixo)
                {
                    g.DrawLine(pen, -meiaRua, meiaRua, -meiaRua, meiaTamanho + ext);
                    g.DrawLine(pen, meiaRua, meiaRua, meiaRua, meiaTamanho + ext);
                }
            }
        }
    }
}