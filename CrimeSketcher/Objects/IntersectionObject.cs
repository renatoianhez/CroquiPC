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

        [Category("Sinalização")]
        [DisplayName("Possui PARE")]
        [Description("Desenha marcação PARE e faixa transversal antes do cruzamento")]
        public bool TemSinalPare { get; set; } = false;

        [Category("Sinalização")]
        [DisplayName("Via de Duas Mãos")]
        [Description("Quando ativo, a marcação PARE e a faixa transversal ocupam apenas meia pista")]
        public bool ViaDuasMaos { get; set; } = true;

        [Category("Sinalização")]
        [DisplayName("PARE na Rua Norte")]
        [Description("Desenha PARE no acesso da rua norte")]
        public bool PareRuaNorte { get; set; } = true;

        [Category("Sinalização")]
        [DisplayName("PARE na Rua Leste")]
        [Description("Desenha PARE no acesso da rua leste")]
        public bool PareRuaLeste { get; set; } = true;

        [Category("Sinalização")]
        [DisplayName("PARE na Rua Sul")]
        [Description("Desenha PARE no acesso da rua sul")]
        public bool PareRuaSul { get; set; } = true;

        [Category("Sinalização")]
        [DisplayName("PARE na Rua Oeste")]
        [Description("Desenha PARE no acesso da rua oeste")]
        public bool PareRuaOeste { get; set; } = true;

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

            if (TemSinalPare)
            {
                DesenharSinalPare(g, meiaRua, meiaTamanho,
                    cima: PareRuaNorte,
                    baixo: PareRuaSul,
                    esquerda: PareRuaOeste,
                    direita: PareRuaLeste);
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

                    if (!temEsquerda)
                        g.FillRectangle(brush, -meiaTamanho, -meiaTamanho, calc, meiaTamanho * 2);

                    if (!temDireita)
                        g.FillRectangle(brush, meiaRua, -meiaTamanho, calc, meiaTamanho * 2);

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

            if (TemCalcada)
            {
                using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                {
                    if (!temEsquerda)
                        g.FillRectangle(brush, -meiaTamanho, -meiaRua, LarguraCalcada, LarguraRua);

                    if (!temDireita)
                        g.FillRectangle(brush, meiaRua, -meiaRua, LarguraCalcada, LarguraRua);
                }
            }

            // Meio-fio sem atravessar a pista
            DesenharMeioFioBracos(g, meiaRua, meiaTamanho, ext,
                temCima, temBaixo, temEsquerda, temDireita);

            if (TemFaixaPedestre)
            {
                DesenharFaixasPedestreT(g, meiaRua, meiaTamanho, temCima, temBaixo, temEsquerda, temDireita);
            }

            if (TemSinalPare)
            {
                DesenharSinalPare(g, meiaRua, meiaTamanho,
                    temCima && PareRuaNorte,
                    temBaixo && PareRuaSul,
                    temEsquerda && PareRuaOeste,
                    temDireita && PareRuaLeste);
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

        private void DesenharSinalPare(Graphics g, float meiaRua, float meiaTamanho,
            bool cima, bool baixo, bool esquerda, bool direita)
        {
            using (var pen = new Pen(Color.FromArgb(CorFaixaArgb), 3f))
            using (var brush = new SolidBrush(Color.FromArgb(CorFaixaArgb)))
            using (var fonte = new Font("Arial", Math.Max(8f, LarguraRua * 0.18f), FontStyle.Bold, GraphicsUnit.Pixel))
            {
                float deslocLinha = 6f;
                float deslocTexto = 16f;

                if (cima)
                {
                    float yLinha = -meiaTamanho - deslocLinha;
                    float yTexto = yLinha - deslocTexto;
                    float xTexto = ViaDuasMaos ? -meiaRua * 0.5f : 0f;

                    if (ViaDuasMaos)
                        g.DrawLine(pen, -meiaRua, yLinha, 0f, yLinha);
                    else
                        g.DrawLine(pen, -meiaRua, yLinha, meiaRua, yLinha);

                    DesenharTextoCentralizadoRotacionado(g, "PARE", fonte, brush, xTexto, yTexto, 180f);
                }

                if (baixo)
                {
                    float yLinha = meiaTamanho + deslocLinha;
                    float yTexto = yLinha + deslocTexto;
                    float xTexto = ViaDuasMaos ? meiaRua * 0.5f : 0f;

                    if (ViaDuasMaos)
                        g.DrawLine(pen, 0f, yLinha, meiaRua, yLinha);
                    else
                        g.DrawLine(pen, -meiaRua, yLinha, meiaRua, yLinha);

                    DesenharTextoCentralizadoRotacionado(g, "PARE", fonte, brush, xTexto, yTexto, 0f);
                }

                if (esquerda)
                {
                    float xLinha = -meiaTamanho - deslocLinha;
                    float xTexto = xLinha - deslocTexto;
                    float yTexto = ViaDuasMaos ? meiaRua * 0.5f : 0f;

                    if (ViaDuasMaos)
                        g.DrawLine(pen, xLinha, 0f, xLinha, meiaRua);
                    else
                        g.DrawLine(pen, xLinha, -meiaRua, xLinha, meiaRua);

                    DesenharTextoCentralizadoRotacionado(g, "PARE", fonte, brush, xTexto, yTexto, 90f);
                }

                if (direita)
                {
                    float xLinha = meiaTamanho + deslocLinha;
                    float xTexto = xLinha + deslocTexto;
                    float yTexto = ViaDuasMaos ? -meiaRua * 0.5f : 0f;

                    if (ViaDuasMaos)
                        g.DrawLine(pen, xLinha, -meiaRua, xLinha, 0f);
                    else
                        g.DrawLine(pen, xLinha, -meiaRua, xLinha, meiaRua);

                    DesenharTextoCentralizadoRotacionado(g, "PARE", fonte, brush, xTexto, yTexto, -90f);
                }
            }
        }

        private void DesenharTextoCentralizado(Graphics g, string texto, Font fonte, Brush brush, float x, float y)
        {
            var tamanho = g.MeasureString(texto, fonte);
            g.DrawString(texto, fonte, brush, x - tamanho.Width / 2f, y - tamanho.Height / 2f);
        }

        private void DesenharTextoCentralizadoRotacionado(Graphics g, string texto, Font fonte, Brush brush, float x, float y, float angulo)
        {
            var state = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(angulo);

            var tamanho = g.MeasureString(texto, fonte);
            g.DrawString(texto, fonte, brush, -tamanho.Width / 2f, -tamanho.Height / 2f);

            g.Restore(state);
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

                // Fechamento da face sem braço no cruzamento em T
                if (!cima)
                    g.DrawLine(pen, -meiaTamanho, -meiaRua, meiaTamanho, -meiaRua);

                if (!baixo)
                    g.DrawLine(pen, -meiaTamanho, meiaRua, meiaTamanho, meiaRua);

                if (!esquerda)
                    g.DrawLine(pen, -meiaRua, -meiaTamanho, -meiaRua, meiaTamanho);

                if (!direita)
                    g.DrawLine(pen, meiaRua, -meiaTamanho, meiaRua, meiaTamanho);
            }
        }
    }
}