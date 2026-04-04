// Objects/RoundaboutObject.cs
using CrimeSketcher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    /// <summary>
    /// Rotatória
    /// </summary>
    [Serializable]
    public class RoundaboutObject : BaseSketchObject
    {
        [Category("Dimensões")]
        [DisplayName("Raio Externo (m)")]
        [Description("Raio externo da rotatória em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float RaioExterno { get; set; } = 120f;

        [Category("Dimensões")]
        [DisplayName("Raio Interno (m)")]
        [Description("Raio interno da ilha central em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float RaioInterno { get; set; } = 80f;

        [Category("Dimensões")]
        [DisplayName("Largura da Rua (m)")]
        [Description("Largura das vias de acesso em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraRua { get; set; } = 80f;

        [Category("Configuração")]
        [DisplayName("Número de Saídas")]
        [Description("Quantidade de vias conectadas à rotatória")]
        public int NumeroSaidas { get; set; } = 4;

        [Category("Configuração")]
        [DisplayName("Ângulo Inicial (graus)")]
        [Description("Ângulo de rotação da primeira saída")]
        public float AnguloInicialSaidas { get; set; } = 0f;

        [Category("Calçada")]
        [DisplayName("Possui Calçada")]
        [Description("Desenha calçadas ao redor da rotatória")]
        public bool TemCalcada { get; set; } = true;

        [Category("Calçada")]
        [DisplayName("Largura da Calçada (m)")]
        [Description("Largura das calçadas em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraCalcada { get; set; } = 15f;

        [Category("Configuração")]
        [DisplayName("Possui Ilha Central")]
        [Description("Desenha ilha central com vegetação")]
        public bool TemIlhaCentral { get; set; } = true;

        [Browsable(false)]
        public int CorAsfaltoArgb { get; set; } = Color.FromArgb(180, 180, 180).ToArgb();

        [Browsable(false)]
        public int CorContornoExternoArgb { get; set; } = Color.FromArgb(180, 180, 180).ToArgb();

        [Browsable(false)]
        public int CorCalcadaArgb { get; set; } = Color.FromArgb(210, 210, 200).ToArgb();

        [Browsable(false)]
        public int CorIlhaArgb { get; set; } = Color.FromArgb(120, 180, 120).ToArgb();

        [Browsable(false)]
        public int CorFaixaArgb { get; set; } = Color.White.ToArgb();

        [Category("Aparência")]
        [DisplayName("Cor do Asfalto")]
        [Description("Cor do asfalto da rotatória")]
        [JsonIgnore]
        public Color CorAsfalto
        {
            get => Color.FromArgb(CorAsfaltoArgb);
            set => CorAsfaltoArgb = value.ToArgb();
        }

        [Category("Aparência")]
        [DisplayName("Cor do Contorno Externo")]
        [Description("Cor da linha circular de contorno externo da rotatória")]
        [JsonIgnore]
        public Color CorContornoExterno
        {
            get => Color.FromArgb(CorContornoExternoArgb);
            set => CorContornoExternoArgb = value.ToArgb();
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
        [DisplayName("Cor da Ilha Central")]
        [Description("Cor da ilha central (vegetação)")]
        [JsonIgnore]
        public Color CorIlha
        {
            get => Color.FromArgb(CorIlhaArgb);
            set => CorIlhaArgb = value.ToArgb();
        }

        [JsonIgnore]
        public Color CorFaixa
        {
            get => Color.FromArgb(CorFaixaArgb);
            set => CorFaixaArgb = value.ToArgb();
        }

        public List<string> RuasConectadas { get; set; } = new List<string>();

        public RoundaboutObject()
        {
            Tipo = "Rotatória";
            CorContornoExternoArgb = CorAsfaltoArgb;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            var state = g.Save();
            g.TranslateTransform(Posicao.X, Posicao.Y);
            g.RotateTransform(Rotacao);

            float raioTotal = RaioExterno + (TemCalcada ? LarguraCalcada : 0);

            // 1. Calçada externa (círculo maior)
            if (TemCalcada)
            {
                using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                {
                    g.FillEllipse(brush, -raioTotal, -raioTotal,
                        raioTotal * 2, raioTotal * 2);
                }
            }

            // 2. Saídas (extensões para ruas) - desenhar cedo para o asfalto circular sobrepor
            DesenharSaidas(g, raioTotal);

            // 3. Asfalto circular (anel)
            using (var brush = new SolidBrush(Color.FromArgb(CorAsfaltoArgb)))
            {
                g.FillEllipse(brush, -RaioExterno, -RaioExterno,
                    RaioExterno * 2, RaioExterno * 2);
            }

            // Textura de asfalto
            using (var brush = new HatchBrush(HatchStyle.Percent10,
                Color.FromArgb(20, 0, 0, 0), Color.Transparent))
            {
                g.FillEllipse(brush, -RaioExterno, -RaioExterno,
                    RaioExterno * 2, RaioExterno * 2);
            }

            // 4. Ilha central
            if (TemIlhaCentral)
            {
                // Meio-fio da ilha
                using (var pen = new Pen(Color.FromArgb(150, 150, 140), 3f))
                {
                    g.DrawEllipse(pen, -RaioInterno - 2, -RaioInterno - 2,
                        (RaioInterno + 2) * 2, (RaioInterno + 2) * 2);
                }

                // Grama/área verde
                using (var brush = new SolidBrush(Color.FromArgb(CorIlhaArgb)))
                {
                    g.FillEllipse(brush, -RaioInterno, -RaioInterno,
                        RaioInterno * 2, RaioInterno * 2);
                }

                // Textura de grama
                using (var brush = new HatchBrush(HatchStyle.DiagonalCross,
                    Color.FromArgb(40, 0, 100, 0),
                    Color.Transparent))
                {
                    g.FillEllipse(brush, -RaioInterno, -RaioInterno,
                        RaioInterno * 2, RaioInterno * 2);
                }
            }
            else
            {
                // Marca central simples
                using (var brush = new SolidBrush(Color.FromArgb(CorFaixaArgb)))
                {
                    g.FillEllipse(brush, -RaioInterno, -RaioInterno,
                        RaioInterno * 2, RaioInterno * 2);
                }
            }

            // 5. Faixa tracejada circular
            DesenharFaixaCircular(g);

            // 6. Setas de direção
            DesenharSetasDirecao(g);

            // 7. Meio-fio externo
            using (var pen = new Pen(Color.FromArgb(CorContornoExternoArgb), 2f))
            {
                g.DrawEllipse(pen, -RaioExterno, -RaioExterno,
                    RaioExterno * 2, RaioExterno * 2);
            }

            g.Restore(state);

            if (Selecionado) DesenharSelecao(g);
        }

        private void DesenharFaixaCircular(Graphics g)
        {
            float raioMedio = (RaioExterno + RaioInterno) / 2;

            using (var pen = new Pen(Color.FromArgb(CorFaixaArgb), 2f))
            {
                pen.DashStyle = DashStyle.Custom;
                pen.DashPattern = new float[] { 8, 6 };
                g.DrawEllipse(pen, -raioMedio, -raioMedio,
                    raioMedio * 2, raioMedio * 2);
            }
        }

        private void DesenharSaidas(Graphics g, float raioTotal)
        {
            float anguloEntreS = 360f / NumeroSaidas;
            float meiaLargura = LarguraRua / 2;
            float extensao = LarguraRua; // Comprimento da extensão

            using (var brushAsfalto = new SolidBrush(Color.FromArgb(CorAsfaltoArgb)))
            using (var brushCalcada = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
            {
                for (int i = 0; i < NumeroSaidas; i++)
                {
                    float angulo = AnguloInicialSaidas + i * anguloEntreS;
                    float rad = angulo * (float)Math.PI / 180f;

                    // Direção da saída
                    float dx = (float)Math.Cos(rad);
                    float dy = (float)Math.Sin(rad);

                    // Perpendicular
                    float px = -dy;
                    float py = dx;

                    // Ponto inicial (na borda da rotatória)
                    float x0 = dx * (RaioExterno - 15);
                    float y0 = dy * (RaioExterno - 15);

                    // Ponto final (extensão)
                    float x1 = dx * (RaioExterno + extensao);
                    float y1 = dy * (RaioExterno + extensao);

                    // Calçada da saída
                    if (TemCalcada)
                    {
                        float lc = meiaLargura + LarguraCalcada;
                        var pontosCalc = new PointF[]
                        {
                            new PointF(x0 + px * lc, y0 + py * lc),
                            new PointF(x1 + px * lc, y1 + py * lc),
                            new PointF(x1 - px * lc, y1 - py * lc),
                            new PointF(x0 - px * lc, y0 - py * lc)
                        };
                        g.FillPolygon(brushCalcada, pontosCalc);
                    }

                    // Asfalto da saída
                    var pontosAsfalto = new PointF[]
                    {
                        new PointF(x0 + px * meiaLargura, y0 + py * meiaLargura),
                        new PointF(x1 + px * meiaLargura, y1 + py * meiaLargura),
                        new PointF(x1 - px * meiaLargura, y1 - py * meiaLargura),
                        new PointF(x0 - px * meiaLargura, y0 - py * meiaLargura)
                    };
                    g.FillPolygon(brushAsfalto, pontosAsfalto);

                    // Faixa central da saída
                    using (var pen = new Pen(Color.Yellow, 2f))
                    {
                        pen.DashStyle = DashStyle.Custom;
                        pen.DashPattern = new float[] { 8, 6 };
                        g.DrawLine(pen,
                            (x0 + x1) / 2, (y0 + y1) / 2,
                            x1, y1);
                    }
                }
            }
        }

        private void DesenharSetasDirecao(Graphics g)
        {
            float raioSeta = (RaioExterno + RaioInterno) / 2;
            int numSetas = 8;

            using (var pen = new Pen(Color.FromArgb(CorFaixaArgb), 2f))
            {
                for (int i = 0; i < numSetas; i++)
                {
                    float angulo = i * (360f / numSetas);
                    float rad = angulo * (float)Math.PI / 180f;

                    float x = (float)Math.Cos(rad) * raioSeta;
                    float y = (float)Math.Sin(rad) * raioSeta;

                    // Direção tangente (sentido anti-horário)
                    float tx = (float)Math.Sin(rad);
                    float ty = -(float)Math.Cos(rad);

                    // Desenhar seta
                    float tamanhoSeta = 8f;
                    var ponta = new PointF(x + tx * tamanhoSeta / 2, y + ty * tamanhoSeta / 2);

                    g.DrawLine(pen,
                        x - tx * tamanhoSeta / 2, y - ty * tamanhoSeta / 2,
                        ponta.X, ponta.Y);

                    // Ponta da seta
                    float anguloSeta = 25f * (float)Math.PI / 180f;
                    float cs = (float)Math.Cos(anguloSeta);
                    float sn = (float)Math.Sin(anguloSeta);

                    var p1 = new PointF(
                        ponta.X - 5f * (tx * cs - (-ty) * sn),
                        ponta.Y - 5f * (ty * cs + (-tx) * sn));
                    var p2 = new PointF(
                        ponta.X - 5f * (tx * cs + (-ty) * sn),
                        ponta.Y - 5f * (ty * cs - (-tx) * sn));

                    g.DrawLine(pen, ponta, p1);
                    g.DrawLine(pen, ponta, p2);
                }
            }
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            float dx = ponto.X - Posicao.X;
            float dy = ponto.Y - Posicao.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            float raioTotal = RaioExterno + (TemCalcada ? LarguraCalcada : 0) + LarguraRua;
            return dist <= raioTotal + tolerancia;
        }

        public override RectangleF GetBounds()
        {
            float raioTotal = RaioExterno + (TemCalcada ? LarguraCalcada : 0) + LarguraRua;
            return new RectangleF(
                Posicao.X - raioTotal,
                Posicao.Y - raioTotal,
                raioTotal * 2,
                raioTotal * 2);
        }

        /// <summary>
        /// Pontos de conexão para ruas
        /// </summary>
        public PointF[] GetPontosConexao()
        {
            var pontos = new PointF[NumeroSaidas];
            float anguloEntreS = 360f / NumeroSaidas;
            float distancia = RaioExterno + LarguraRua;

            for (int i = 0; i < NumeroSaidas; i++)
            {
                float angulo = AnguloInicialSaidas + i * anguloEntreS;
                float rad = angulo * (float)Math.PI / 180f;

                pontos[i] = new PointF(
                    Posicao.X + (float)Math.Cos(rad) * distancia,
                    Posicao.Y + (float)Math.Sin(rad) * distancia);
            }

            return pontos;
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            float media = (Math.Abs(fatorX) + Math.Abs(fatorY)) / 2f;
            RaioExterno = Math.Max(10f, RaioExterno * media);
            RaioInterno = Math.Max(5f, RaioInterno * media);
            LarguraRua = Math.Max(10f, LarguraRua * media);
            LarguraCalcada = Math.Max(2f, LarguraCalcada * media);
            Posicao = EscalarPonto(Posicao, centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }
    }
}