using CrimeSketcher.Core;
using CrimeSketcher.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class AutoIntersectionObject : BaseSketchObject
    {
        private float _larguraRua1 = 80f;
        private float _larguraRua2 = 80f;
        private float _angulo1 = 0f;
        private float _angulo2 = 90f;
        private string _idRua1 = "";
        private string _idRua2 = "";
        private bool _temCalcadaRua1 = true;
        private bool _temCalcadaRua2 = true;
        private float _larguraCalcadaRua1 = 15f;
        private float _larguraCalcadaRua2 = 15f;
        private bool _temCiclofaixaRua1 = false;
        private bool _temCiclofaixaRua2 = false;
        private bool _temEstacionamentoRua1 = false;
        private bool _temEstacionamentoRua2 = false;
        private float _deslocamentoRua2 = -50f;

        [Category("Configuração")]
        [DisplayName("Tipo de Cruzamento")]
        [Description("Tipo de interseção: Cruz (4 vias) ou T (3 vias)")]
        public Core.IntersectionType TipoCruzamento { get; set; } = Core.IntersectionType.Cruz;

        [Category("Dimensões")]
        [DisplayName("Largura Rua 1")]
        public float LarguraRua1
        {
            get => _larguraRua1;
            set => _larguraRua1 = Math.Max(10f, value);
        }

        [Category("Dimensões")]
        [DisplayName("Largura Rua 2")]
        public float LarguraRua2
        {
            get => _larguraRua2;
            set => _larguraRua2 = Math.Max(10f, value);
        }

        [Category("Dimensões")]
        [DisplayName("Ângulo Rua 1")]
        public float AnguloRua1
        {
            get => _angulo1;
            set => _angulo1 = value % 360f;
        }

        [Category("Dimensões")]
        [DisplayName("Ângulo Rua 2")]
        public float AnguloRua2
        {
            get => _angulo2;
            set => _angulo2 = value % 360f;
        }

        [Category("Dimensões")]
        [DisplayName("Deslocamento Rua 2")]
        [Description("Desloca R2 ao longo da Rua 2 (longitudinalmente). Positivo = afasta do centro, negativo = aproxima.")]
        public float DeslocamentoRua2
        {
            get => _deslocamentoRua2;
            set => _deslocamentoRua2 = value;
        }

        [Browsable(false)]
        public string IdRua1
        {
            get => _idRua1;
            set => _idRua1 = value ?? "";
        }

        [Browsable(false)]
        public string IdRua2
        {
            get => _idRua2;
            set => _idRua2 = value ?? "";
        }

        [Category("Propriedades")]
        [DisplayName("Tem Canteiro Central")]
        public bool TemCanteiroCentral { get; set; } = false;

        [Category("Propriedades")]
        [DisplayName("Largura Canteiro")]
        public float LarguraCanteiroCentral { get; set; } = 12f;

        [Browsable(false)]
        public bool TemCalcadaRua1
        {
            get => _temCalcadaRua1;
            set => _temCalcadaRua1 = value;
        }

        [Browsable(false)]
        public bool TemCalcadaRua2
        {
            get => _temCalcadaRua2;
            set => _temCalcadaRua2 = value;
        }

        [Browsable(false)]
        public float LarguraCalcadaRua1
        {
            get => _larguraCalcadaRua1;
            set => _larguraCalcadaRua1 = Math.Max(0f, value);
        }

        [Browsable(false)]
        public float LarguraCalcadaRua2
        {
            get => _larguraCalcadaRua2;
            set => _larguraCalcadaRua2 = Math.Max(0f, value);
        }

        [Browsable(false)]
        public bool TemCiclofaixaRua1
        {
            get => _temCiclofaixaRua1;
            set => _temCiclofaixaRua1 = value;
        }

        [Browsable(false)]
        public bool TemCiclofaixaRua2
        {
            get => _temCiclofaixaRua2;
            set => _temCiclofaixaRua2 = value;
        }

        [Browsable(false)]
        public bool TemEstacionamentoRua1
        {
            get => _temEstacionamentoRua1;
            set => _temEstacionamentoRua1 = value;
        }

        [Browsable(false)]
        public bool TemEstacionamentoRua2
        {
            get => _temEstacionamentoRua2;
            set => _temEstacionamentoRua2 = value;
        }

        [Browsable(false)]
        public int CorAsfaltoArgb { get; set; } = Color.FromArgb(60, 60, 60).ToArgb();

        [Browsable(false)]
        public int CorCalcadaArgb { get; set; } = Color.FromArgb(200, 200, 200).ToArgb();

        [Browsable(false)]
        public int CorCiclofaixaArgb { get; set; } = Color.FromArgb(255, 200, 0).ToArgb();

        [Category("Aparência")]
        [DisplayName("Cor do Asfalto")]
        [JsonIgnore]
        public Color CorAsfalto
        {
            get => Color.FromArgb(CorAsfaltoArgb);
            set => CorAsfaltoArgb = value.ToArgb();
        }

        [Category("Aparência")]
        [DisplayName("Cor da Calçada")]
        [JsonIgnore]
        public Color CorCalcada
        {
            get => Color.FromArgb(CorCalcadaArgb);
            set => CorCalcadaArgb = value.ToArgb();
        }

        [Category("Aparência")]
        [DisplayName("Cor da Ciclofaixa")]
        [JsonIgnore]
        public Color CorCiclofaixa
        {
            get => Color.FromArgb(CorCiclofaixaArgb);
            set => CorCiclofaixaArgb = value.ToArgb();
        }

        public AutoIntersectionObject()
        {
            Nome = "Cruzamento Automático";
            Tipo = "AutoCruzamento";
        }

        public override RectangleF GetBounds()
        {
            float maxLargura = Math.Max(_larguraRua1, _larguraRua2) +
                Math.Max(_larguraCalcadaRua1, _larguraCalcadaRua2) * 2 +
                (_temCiclofaixaRua1 || _temCiclofaixaRua2 ? 30f : 0f) +
                (_temEstacionamentoRua1 || _temEstacionamentoRua2 ? 60f : 0f);

            ObterDimensoesBaseCruzamento(out float semiComprimentoR1, out float comprimentoR2);
            float deslocamentoR1 = ObterDeslocamentoLongitudinalR1Escalar();
            float alcanceR1 = semiComprimentoR1 + Math.Abs(deslocamentoR1);
            float alcanceGeometrico = Math.Max(alcanceR1, comprimentoR2);
            float alcance = Math.Max(maxLargura, alcanceGeometrico) + Math.Abs(_deslocamentoRua2) + MARGEM_BRACO;

            return new RectangleF(
                Posicao.X - alcance,
                Posicao.Y - alcance,
                alcance * 2,
                alcance * 2);
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            var state = g.Save();
            g.TranslateTransform(Posicao.X, Posicao.Y);
            g.RotateTransform(Rotacao);

            var brushAsfalto = new SolidBrush(CorAsfalto);
            var brushCalcada = new SolidBrush(CorCalcada);
            var brushCiclofaixa = new SolidBrush(CorCiclofaixa);
            var penCanteiro = new Pen(Color.Yellow, 2f);

            try
            {
                bool ehCruz = TipoCruzamento == Core.IntersectionType.Cruz;

                PrepararVetores(
                    out var vet1, out var vet2,
                    out var perp1, out var perp2);
                ObterDimensoesBaseCruzamento(out float semiComprimentoR1, out float comprimentoR2);

                var pontoEntrada = new PointF(vet2.X * _deslocamentoRua2, vet2.Y * _deslocamentoRua2);
                float deslocamentoR1 = ObterDeslocamentoLongitudinalR1Escalar();
                var centroR1 = new PointF(vet1.X * deslocamentoR1, vet1.Y * deslocamentoR1);
                var vetPontaR2 = ObterDirecaoPontaR2(vet2, pontoEntrada);

                float l1Half = _larguraRua1 / 2f;
                float l2Half = _larguraRua2 / 2f;
                float c1Half = _larguraCalcadaRua1 / 2f;
                float c2Half = _larguraCalcadaRua2 / 2f;
                const float larguraCiclofaixa = 15f;

                // Camada 3 (mais externa): Ciclofaixas — união R1 ∪ R2
                if (_temCiclofaixaRua1 || _temCiclofaixaRua2)
                {
                    float cicloHW1 = l1Half
                        + (_temCalcadaRua1 ? c1Half : 0f)
                        + (_temCiclofaixaRua1 ? 2f + larguraCiclofaixa : 0f);
                    float cicloHW2 = l2Half
                        + (_temCalcadaRua2 ? c2Half : 0f)
                        + (_temCiclofaixaRua2 ? 2f + larguraCiclofaixa : 0f);

                    DesenharCamadaUniao(g, brushCiclofaixa, ehCruz,
                        vet1, vet2, vetPontaR2, perp1, perp2,
                        semiComprimentoR1, comprimentoR2, cicloHW1, cicloHW2, centroR1, pontoEntrada);
                }

                // Camada 2 (intermediária): Calçadas — união R1 ∪ R2
                if (_temCalcadaRua1 || _temCalcadaRua2)
                {
                    float calcHW1 = l1Half + (_temCalcadaRua1 ? c1Half : 0f);
                    float calcHW2 = l2Half + (_temCalcadaRua2 ? c2Half : 0f);

                    DesenharCamadaUniao(g, brushCalcada, ehCruz,
                        vet1, vet2, vetPontaR2, perp1, perp2,
                        semiComprimentoR1, comprimentoR2, calcHW1, calcHW2, centroR1, pontoEntrada);
                }

                // Camada 1 (mais interna): Asfalto — união R1 ∪ R2
                DesenharCamadaUniao(g, brushAsfalto, ehCruz,
                    vet1, vet2, vetPontaR2, perp1, perp2,
                    semiComprimentoR1, comprimentoR2, l1Half, l2Half, centroR1, pontoEntrada);

                // Canteiros (contorno) — união R1 ∪ R2
                if (TemCanteiroCentral)
                {
                    float largCanteiro = LarguraCanteiroCentral / 2f;
                    DesenharContornoUniao(g, penCanteiro, ehCruz,
                        vet1, vet2, vetPontaR2, perp1, perp2,
                        semiComprimentoR1, comprimentoR2, largCanteiro, largCanteiro, centroR1, pontoEntrada);
                }
            }
            finally
            {
                brushAsfalto.Dispose();
                brushCalcada.Dispose();
                brushCiclofaixa.Dispose();
                penCanteiro.Dispose();
                g.Restore(state);
            }

            if (Selecionado) DesenharSelecao(g);
        }

        /// <summary>
        /// Prepara os vetores de direção e perpendiculares de cada rua, normalizados.
        /// </summary>
        private void PrepararVetores(
            out PointF vet1, out PointF vet2,
            out PointF perp1, out PointF perp2)
        {
            vet1 = GetVetorRua1();
            vet2 = GetVetorRua2();
            perp1 = GetPerpendicular(vet1);
            perp2 = GetPerpendicular(vet2);

            NormalizarVetor(ref vet1);
            NormalizarVetor(ref vet2);
            NormalizarVetor(ref perp1);
            NormalizarVetor(ref perp2);
        }

        /// <summary>
        /// Margem em pixels que cada braço do cruzamento ultrapassa
        /// a borda do asfalto da rua oposta. Deve ser ≥ 0.
        /// </summary>
        private const float MARGEM_BRACO = 25f;

        /// <summary>
        /// Limite inferior do seno do ângulo de inserção para evitar comprimentos
        /// excessivos quando as ruas ficam quase paralelas.
        /// </summary>
        private const float SENO_MINIMO_INSERCAO = 0.2f;

        /// <summary>
        /// Define as dimensões geométricas dos dois retângulos-base do cruzamento:
        /// R1 (Rua 1) e R2 (Rua 2), ambos com largura do asfalto e comprimentos
        /// suficientes para a fusão do ponto de inserção.
        /// </summary>
        private void ObterDimensoesBaseCruzamento(out float semiComprimentoR1, out float comprimentoR2)
        {
            float margem = Math.Max(0f, MARGEM_BRACO);

            float anguloRelativoRad = (_angulo2 - _angulo1) * (float)Math.PI / 180f;
            float seno = Math.Abs((float)Math.Sin(anguloRelativoRad));
            float fatorAngularR1 = 1f / Math.Max(seno, SENO_MINIMO_INSERCAO);

            semiComprimentoR1 = (_larguraRua2 / 2f) * fatorAngularR1 + margem;
            comprimentoR2 = _larguraRua1 / 2f + margem;
        }

        private float ObterDeslocamentoLongitudinalR1Escalar()
        {
            float margem = Math.Max(0f, MARGEM_BRACO);
            float baseMinima = _larguraRua2 / 2f + margem;

            ObterDimensoesBaseCruzamento(out float semiComprimentoR1, out _);
            float extra = Math.Max(0f, semiComprimentoR1 - baseMinima);

            float ang1 = _angulo1 * (float)Math.PI / 180f;
            float ang2 = _angulo2 * (float)Math.PI / 180f;
            float dot = (float)(Math.Cos(ang1) * Math.Cos(ang2) + Math.Sin(ang1) * Math.Sin(ang2));
            float direcao = dot >= 0f ? 1f : -1f;

            return direcao * (extra / 2f) + 50;
        }

        private static PointF ObterDirecaoPontaR2(PointF vet2, PointF pontoEntrada)
        {
            var direcao = new PointF(-pontoEntrada.X, -pontoEntrada.Y);
            float comp = (float)Math.Sqrt(direcao.X * direcao.X + direcao.Y * direcao.Y);
            if (comp <= 0f)
            {
                return vet2;
            }

            return new PointF(direcao.X / comp, direcao.Y / comp);
        }

        private static PointF[] CriarRetanguloSimetrico(
            PointF centro, PointF vet, PointF perp, float semiComprimento, float halfWidth)
        {
            float far = semiComprimento;
            float near = -semiComprimento;

            return new PointF[]
            {
                new PointF(centro.X + vet.X * far  + perp.X * halfWidth, centro.Y + vet.Y * far  + perp.Y * halfWidth),
                new PointF(centro.X + vet.X * far  - perp.X * halfWidth, centro.Y + vet.Y * far  - perp.Y * halfWidth),
                new PointF(centro.X + vet.X * near - perp.X * halfWidth, centro.Y + vet.Y * near - perp.Y * halfWidth),
                new PointF(centro.X + vet.X * near + perp.X * halfWidth, centro.Y + vet.Y * near + perp.Y * halfWidth)
            };
        }

        private static PointF[] CriarRetanguloPonta(
            PointF pontoEntrada, PointF vet, PointF perp, float comprimento, float halfWidth)
        {
            float far = comprimento;
            float near = 0f;

            return new PointF[]
            {
                new PointF(pontoEntrada.X + vet.X * far  + perp.X * halfWidth, pontoEntrada.Y + vet.Y * far  + perp.Y * halfWidth),
                new PointF(pontoEntrada.X + vet.X * far  - perp.X * halfWidth, pontoEntrada.Y + vet.Y * far  - perp.Y * halfWidth),
                new PointF(pontoEntrada.X + vet.X * near - perp.X * halfWidth, pontoEntrada.Y + vet.Y * near - perp.Y * halfWidth),
                new PointF(pontoEntrada.X + vet.X * near + perp.X * halfWidth, pontoEntrada.Y + vet.Y * near + perp.Y * halfWidth)
            };
        }

        /// <summary>
        /// Desenha uma camada preenchida como a união de dois retângulos R1 ∪ R2.
        /// Cada camada (ciclofaixa, calçada, asfalto) usa meia-larguras diferentes,
        /// e a camada mais interna pinta por cima da mais externa, produzindo as
        /// faixas visuais corretas — incluindo os cantos do cruzamento.
        /// </summary>
        private static void DesenharCamadaUniao(
            Graphics g, Brush brush, bool ehCruz,
            PointF vet1, PointF vet2, PointF vet2Ponta, PointF perp1, PointF perp2,
            float dist1, float dist2, float halfW1, float halfW2, PointF centroR1, PointF pontoEntradaR2)
        {
            var r1 = CriarRetanguloSimetrico(centroR1, vet1, perp1, dist1, halfW1);
            var r2 = ehCruz
                ? CriarRetanguloSimetrico(centroR1, vet2, perp2, dist2, halfW2)
                : CriarRetanguloPonta(
                    pontoEntradaR2,
                    vet2Ponta,
                    new PointF(-vet2Ponta.Y, vet2Ponta.X),
                    dist2,
                    halfW2);

            var uniao = PolygonUnion.CalcularUniao(r1, r2);
            if (uniao != null && uniao.Length >= 3)
            {
                g.FillPolygon(brush, uniao);
            }
            else
            {
                g.FillPolygon(brush, r1);
                g.FillPolygon(brush, r2);
            }
        }

        /// <summary>
        /// Desenha o contorno (outline) da união de dois retângulos R1 ∪ R2.
        /// Usado para canteiros centrais, onde apenas a borda é desenhada.
        /// </summary>
        private static void DesenharContornoUniao(
            Graphics g, Pen pen, bool ehCruz,
            PointF vet1, PointF vet2, PointF vet2Ponta, PointF perp1, PointF perp2,
            float dist1, float dist2, float halfW1, float halfW2, PointF centroR1, PointF pontoEntradaR2)
        {
            var r1 = CriarRetanguloSimetrico(centroR1, vet1, perp1, dist1, halfW1);
            var r2 = ehCruz
                ? CriarRetanguloSimetrico(centroR1, vet2, perp2, dist2, halfW2)
                : CriarRetanguloPonta(
                    pontoEntradaR2,
                    vet2Ponta,
                    new PointF(-vet2Ponta.Y, vet2Ponta.X),
                    dist2,
                    halfW2);

            var uniao = PolygonUnion.CalcularUniao(r1, r2);
            if (uniao != null && uniao.Length >= 3)
            {
                g.DrawPolygon(pen, uniao);
            }
            else
            {
                g.DrawPolygon(pen, r1);
                g.DrawPolygon(pen, r2);
            }
        }

        private PointF GetVetorRua1()
        {
            float rad = _angulo1 * (float)Math.PI / 180f;
            return new PointF((float)Math.Cos(rad), (float)Math.Sin(rad));
        }

        private PointF GetVetorRua2()
        {
            float rad = _angulo2 * (float)Math.PI / 180f;
            return new PointF((float)Math.Cos(rad), (float)Math.Sin(rad));
        }

        private PointF GetPerpendicular(PointF vetor)
        {
            return new PointF(-vetor.Y, vetor.X);
        }

        private void NormalizarVetor(ref PointF vetor)
        {
            float comp = (float)Math.Sqrt(vetor.X * vetor.X + vetor.Y * vetor.Y);
            if (comp > 0)
            {
                vetor.X /= comp;
                vetor.Y /= comp;
            }
        }

        public override void Mover(float dx, float dy)
        {
            Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy);
        }

        public override bool ContemPonto(PointF ponto, float tolerancia = 5f)
        {
            return GetBounds().Contains(ponto);
        }

        private void DesenharSelecao(Graphics g)
        {
            var bounds = GetBounds();
            using (var pen = new Pen(Color.Blue, 2f) { DashStyle = DashStyle.Dash })
            {
                g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }
        }
    }
}
