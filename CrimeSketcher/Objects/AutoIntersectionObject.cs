using CrimeSketcher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    public enum AutoIntersectionParte
    {
        Ambas = 0,
        Rua1 = 1,
        Rua2 = 2
    }

    [Serializable]
    public class AutoIntersectionObject : BaseSketchObject, ICustomTypeDescriptor
    {
        private const float AJUSTE_LARGURA_RUA = 4f;
        private const float DESLOCAMENTO_RUA2_OFFSET = 50f;
        private const float ACRESCIMO_ESTACIONAMENTO = 30f;
        private const float ACRESCIMO_CICLOFAIXA = 17f;

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
        [DisplayName("Largura Rua 1 (m)")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraRua1
        {
            get => _larguraRua1;
            set => _larguraRua1 = Math.Max(10f, value) - AJUSTE_LARGURA_RUA;
        }

        [Category("Dimensões")]
        [DisplayName("Largura Rua 2 (m)")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraRua2
        {
            get => _larguraRua2;
            set => _larguraRua2 = Math.Max(10f, value) - AJUSTE_LARGURA_RUA;
        }

        [Category("Dimensões")]
        [DisplayName("Ângulo Rua 1")]
        public float AnguloRua1
        {
            get => _angulo1;
            set => _angulo1 = GeometryHelper.NormalizarAngulo360(value);
        }

        [Category("Dimensões")]
        [DisplayName("Ângulo Rua 2")]
        public float AnguloRua2
        {
            get => _angulo2;
            set => _angulo2 = GeometryHelper.NormalizarAngulo360(value);
        }

        [Category("Dimensões")]
        [DisplayName("Deslocamento Rua 2 (m)")]
        [Description("Desloca R2 ao longo da Rua 2 (longitudinalmente). Positivo = afasta do centro, negativo = aproxima.")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float DeslocamentoRua2
        {
            get => _deslocamentoRua2 - DESLOCAMENTO_RUA2_OFFSET;
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

        [Browsable(false)]
        public AutoIntersectionParte Parte { get; set; } = AutoIntersectionParte.Ambas;

        [Category("Propriedades")]
        [DisplayName("Tem Canteiro Central")]
        public bool TemCanteiroCentral { get; set; } = false;

        [Category("Propriedades")]
        [DisplayName("Largura Canteiro (m)")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraCanteiroCentral { get; set; } = 12f;

        [Category("Sinalização")]
        [DisplayName("PARE na Rua 1")]
        public bool TemPareRua1 { get; set; } = false;

        [Category("Sinalização")]
        [DisplayName("PARE na Rua 2")]
        public bool TemPareRua2 { get; set; } = false;

        [Category("Sinalização")]
        [DisplayName("Faixa de Pedestres Rua 1")]
        public bool TemFaixaPedestresRua1 { get; set; } = false;

        [Category("Sinalização")]
        [DisplayName("Faixa de Pedestres Rua 2")]
        public bool TemFaixaPedestresRua2 { get; set; } = false;

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
        public int CorAsfaltoArgb { get; set; } = Color.FromArgb(180, 180, 180).ToArgb();

        [Browsable(false)]
        public int CorCalcadaArgb { get; set; } = Color.FromArgb(200, 200, 200).ToArgb();

        [Browsable(false)]
        public int CorCiclofaixaArgb { get; set; } = Color.Transparent.ToArgb();

        [Category("Aparência")]
        [DisplayName("Cor do Asfalto")]
        [JsonIgnore]
        public Color CorAsfalto
        {
            get
            {
                if (CorAsfaltoArgb != 0)
                    return Color.FromArgb(CorAsfaltoArgb);

                if (CorPreenchimentoArgb != Color.Transparent.ToArgb())
                    return Color.FromArgb(CorPreenchimentoArgb);

                return Color.FromArgb(180, 180, 180);
            }
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
            CorPreenchimento = Color.FromArgb(180, 180, 180);
            CorContorno = Color.FromArgb(100, 100, 100);
        }

        public override RectangleF GetBounds()
        {
            ObterPoligonosExternosLocais(out var r1, out var r2);

            return Parte switch
            {
                AutoIntersectionParte.Rua1 => CalcularBoundsGlobal(r1),
                AutoIntersectionParte.Rua2 => CalcularBoundsGlobal(r2),
                _ => UnirBounds(CalcularBoundsGlobal(r1), CalcularBoundsGlobal(r2))
            };
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            var state = g.Save();
            g.TranslateTransform(Posicao.X, Posicao.Y);
            g.RotateTransform(Rotacao);

            var brushAsfalto = new SolidBrush(CorAsfalto);

            try
            {
                bool ehCruz = TipoCruzamento == Core.IntersectionType.Cruz;
                bool desenharRua1 = Parte != AutoIntersectionParte.Rua2;
                bool desenharRua2 = Parte != AutoIntersectionParte.Rua1;

                PrepararVetores(
                    out var vet1, out var vet2,
                    out var perp1, out var perp2);
                ObterDimensoesBaseCruzamento(out float semiComprimentoR1, out float comprimentoR2);

                var pontoEntradaBaseR2 = new PointF(vet2.X * _deslocamentoRua2, vet2.Y * _deslocamentoRua2);
                float deslocamentoR2 = ObterDeslocamentoLongitudinalR2Escalar();
                var dirDentroR2 = ObterDirecaoDentroRua2(vet2, pontoEntradaBaseR2);
                var pontoEntrada = new PointF(
                    pontoEntradaBaseR2.X + dirDentroR2.X * deslocamentoR2,
                    pontoEntradaBaseR2.Y + dirDentroR2.Y * deslocamentoR2);

                float deslocamentoR1 = ObterDeslocamentoLongitudinalR1Escalar();
                var centroR1 = new PointF(vet1.X * deslocamentoR1, vet1.Y * deslocamentoR1);
                var vetPontaR2 = ObterDirecaoPontaR2(vet2, pontoEntrada);

                float l1Half = ObterMeiaLarguraRua1Efetiva();
                float l2Half = ObterMeiaLarguraRua2Efetiva();

                var smoothingAnterior = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.None;
                try
                {
                    DesenharCamadaUniao(g, brushAsfalto, ehCruz,
                        vet1, vet2, vetPontaR2, perp1, perp2,
                        semiComprimentoR1, comprimentoR2, l1Half, l2Half, centroR1, pontoEntrada,
                        desenharRua1, desenharRua2);
                }
                finally
                {
                    g.SmoothingMode = smoothingAnterior;
                }

                if (desenharRua1)
                {
                    DesenharSinalizacaoVia(
                        g,
                        centroR1,
                        vet1,
                        perp1,
                        semiComprimentoR1,
                        l1Half,
                        TemPareRua1,
                        TemFaixaPedestresRua1,
                        true);
                }

                if (desenharRua2)
                {
                    var vetSinalizacaoR2 = ehCruz ? vet2 : vetPontaR2;
                    var perpSinalizacaoR2 = ehCruz ? perp2 : new PointF(-vetPontaR2.Y, vetPontaR2.X);
                    var referenciaR2 = ehCruz ? centroR1 : pontoEntrada;

                    DesenharSinalizacaoVia(
                        g,
                        referenciaR2,
                        vetSinalizacaoR2,
                        perpSinalizacaoR2,
                        comprimentoR2,
                        l2Half,
                        TemPareRua2,
                        TemFaixaPedestresRua2,
                        ehCruz,
                        ehCruz ? 1f : -1f);
                }
            }
            finally
            {
                brushAsfalto.Dispose();
                g.Restore(state);
            }

            if (Selecionado) DesenharSelecao(g);
        }

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

        private const float MARGEM_BRACO = 25f;
        private const float BASE_COMPRIMENTO_R2 = 35f;
        private const float TOLERANCIA_ORTOGONAL_GRAUS = 2f;
        private const float SENO_MINIMO_INSERCAO = 0.2f;

        private void ObterDimensoesBaseCruzamento(out float semiComprimentoR1, out float comprimentoR2)
        {
            float margem = Math.Max(0f, MARGEM_BRACO);

            float fatorAngular = ObterFatorAngularInsercao();
            float fatorAngularR1 = ObterFatorAngularR1();
            float acrescimoR1 = ObterAcrescimoComprimentoR1PorFaixasRua1();

            float halfW1 = ObterMeiaLarguraRua1Efetiva();
            float halfW2 = ObterMeiaLarguraRua2Efetiva();

            semiComprimentoR1 = halfW2 * fatorAngularR1 + margem + acrescimoR1;

            if (TipoCruzamento == Core.IntersectionType.Cruz && InsercaoQuaseOrtogonal())
            {
                float acrescimoR2 = ObterAcrescimoComprimentoR2PorFaixasRua2();
                comprimentoR2 = halfW1 + margem + acrescimoR2;
            }
            else
            {
                comprimentoR2 = (BASE_COMPRIMENTO_R2 + halfW1 + margem) * fatorAngular;
            }
        }

        private float ObterAcrescimoComprimentoR1PorFaixasRua1()
        {
            float acrescimo = 0f;
            if (_temEstacionamentoRua1) acrescimo += ACRESCIMO_ESTACIONAMENTO;
            if (_temCiclofaixaRua1) acrescimo += ACRESCIMO_CICLOFAIXA;
            return acrescimo;
        }

        private float ObterAcrescimoComprimentoR2PorFaixasRua2()
        {
            float acrescimo = 0f;
            if (_temEstacionamentoRua2) acrescimo += ACRESCIMO_ESTACIONAMENTO;
            if (_temCiclofaixaRua2) acrescimo += ACRESCIMO_CICLOFAIXA;
            return acrescimo;
        }

        private float ObterFatorAngularInsercao()
        {
            float anguloRelativoRad = (_angulo2 - _angulo1) * (float)Math.PI / 180f;
            float seno = Math.Abs((float)Math.Sin(anguloRelativoRad));
            return 1f / Math.Max(seno, SENO_MINIMO_INSERCAO);
        }

        private float ObterFatorAngularR1()
        {
            if (TipoCruzamento == Core.IntersectionType.Cruz && InsercaoQuaseOrtogonal())
                return 1f;

            return ObterFatorAngularInsercao() * 1.5f;
        }

        private bool InsercaoQuaseOrtogonal()
        {
            float delta = (_angulo2 - _angulo1) % 180f;
            if (delta < 0f) delta += 180f;
            return Math.Abs(delta - 90f) <= TOLERANCIA_ORTOGONAL_GRAUS;
        }

        private float ObterDeslocamentoLongitudinalR1Escalar()
        {
            if (TipoCruzamento == Core.IntersectionType.T && InsercaoQuaseOrtogonal())
                return 0f;

            if (TipoCruzamento == Core.IntersectionType.Cruz)
                return 1f;

            float margem = Math.Max(0f, MARGEM_BRACO);
            float baseMinima = ObterMeiaLarguraRua2Efetiva() + margem;

            ObterDimensoesBaseCruzamento(out float semiComprimentoR1, out _);
            float extra = Math.Max(0f, semiComprimentoR1 - baseMinima);

            float ang1 = _angulo1 * (float)Math.PI / 180f;
            float ang2 = _angulo2 * (float)Math.PI / 180f;
            float dot = (float)(Math.Cos(ang1) * Math.Cos(ang2) + Math.Sin(ang1) * Math.Sin(ang2));
            float direcao = dot >= 0f ? 1f : -1f;

            return -1.5f * direcao * (extra / 2f);
        }

        private float ObterDeslocamentoLongitudinalR2Escalar()
        {
            if (TipoCruzamento == Core.IntersectionType.Cruz)
                return 0f;

            float margem = Math.Max(0f, MARGEM_BRACO);
            float baseMinima = -10f + BASE_COMPRIMENTO_R2 + ObterMeiaLarguraRua1Efetiva() + margem;

            ObterDimensoesBaseCruzamento(out _, out float comprimentoR2);
            float extra = Math.Max(0f, comprimentoR2 - baseMinima);
            return 2.8f * (extra / 2f);
        }

        private static PointF ObterDirecaoPontaR2(PointF vet2, PointF pontoEntrada)
        {
            var direcao = new PointF(-pontoEntrada.X, -pontoEntrada.Y);
            float comp = (float)Math.Sqrt(direcao.X * direcao.X + direcao.Y * direcao.Y);
            if (comp <= 0f)
                return vet2;

            return new PointF(direcao.X / comp, direcao.Y / comp);
        }

        private static PointF ObterDirecaoDentroRua2(PointF vet2, PointF pontoEntrada)
        {
            var direcao = pontoEntrada;
            float comp = (float)Math.Sqrt(direcao.X * direcao.X + direcao.Y * direcao.Y);
            if (comp <= 0f)
                return vet2;

            return new PointF(direcao.X / comp, direcao.Y / comp);
        }

        private static PointF[] CriarRetanguloSimetrico(
            PointF centro, PointF vet, PointF perp, float semiComprimento, float halfWidth)
        {
            float far = semiComprimento + 20f; // Aqui que tem que atuar para aumentar ou diminuir os retângulos de cruzamento
            float near = -semiComprimento - 20f;

            return
            [
                new PointF(centro.X + vet.X * far  + perp.X * halfWidth, centro.Y + vet.Y * far  + perp.Y * halfWidth),
                new PointF(centro.X + vet.X * far  - perp.X * halfWidth, centro.Y + vet.Y * far  - perp.Y * halfWidth),
                new PointF(centro.X + vet.X * near - perp.X * halfWidth, centro.Y + vet.Y * near - perp.Y * halfWidth),
                new PointF(centro.X + vet.X * near + perp.X * halfWidth, centro.Y + vet.Y * near + perp.Y * halfWidth)
            ];
        }

        private static PointF[] CriarRetanguloPonta(
            PointF pontoEntrada, PointF vet, PointF perp, float comprimento, float halfWidth)
        {
            float far = comprimento;
            float near = 0f;

            return
            [
                new PointF(pontoEntrada.X + vet.X * far  + perp.X * halfWidth, pontoEntrada.Y + vet.Y * far  + perp.Y * halfWidth),
                new PointF(pontoEntrada.X + vet.X * far  - perp.X * halfWidth, pontoEntrada.Y + vet.Y * far  - perp.Y * halfWidth),
                new PointF(pontoEntrada.X + vet.X * near - perp.X * halfWidth, pontoEntrada.Y + vet.Y * near - perp.Y * halfWidth),
                new PointF(pontoEntrada.X + vet.X * near + perp.X * halfWidth, pontoEntrada.Y + vet.Y * near + perp.Y * halfWidth)
            ];
        }

        private static void DesenharCamadaUniao(
            Graphics g, Brush brush, bool ehCruz,
            PointF vet1, PointF vet2, PointF vet2Ponta, PointF perp1, PointF perp2,
            float dist1, float dist2, float halfW1, float halfW2, PointF centroR1, PointF pontoEntradaR2,
            bool desenharRua1, bool desenharRua2)
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

            if (desenharRua1)
                g.FillPolygon(brush, r1);
            if (desenharRua2)
                g.FillPolygon(brush, r2);
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
            var pontoLocal = TransformarGlobalParaLocal(ponto);
            ObterPoligonosExternosLocais(out var r1, out var r2);

            return Parte switch
            {
                AutoIntersectionParte.Rua1 => ContemPontoNoPoligono(r1, pontoLocal, tolerancia),
                AutoIntersectionParte.Rua2 => ContemPontoNoPoligono(r2, pontoLocal, tolerancia),
                _ => ContemPontoNoPoligono(r1, pontoLocal, tolerancia) || ContemPontoNoPoligono(r2, pontoLocal, tolerancia)
            };
        }

        private void ObterPoligonosExternosLocais(out PointF[] r1, out PointF[] r2)
        {
            bool ehCruz = TipoCruzamento == Core.IntersectionType.Cruz;

            PrepararVetores(
                out var vet1, out var vet2,
                out var perp1, out var perp2);
            ObterDimensoesBaseCruzamento(out float semiComprimentoR1, out float comprimentoR2);

            var pontoEntradaBaseR2 = new PointF(vet2.X * _deslocamentoRua2, vet2.Y * _deslocamentoRua2);
            float deslocamentoR2 = ObterDeslocamentoLongitudinalR2Escalar();
            var dirDentroR2 = ObterDirecaoDentroRua2(vet2, pontoEntradaBaseR2);
            var pontoEntrada = new PointF(
                pontoEntradaBaseR2.X + dirDentroR2.X * deslocamentoR2,
                pontoEntradaBaseR2.Y + dirDentroR2.Y * deslocamentoR2);

            float deslocamentoR1 = ObterDeslocamentoLongitudinalR1Escalar();
            var centroR1 = new PointF(vet1.X * deslocamentoR1, vet1.Y * deslocamentoR1);
            var vetPontaR2 = ObterDirecaoPontaR2(vet2, pontoEntrada);

            float halfW1 = ObterMeiaLarguraRua1Efetiva();
            float halfW2 = ObterMeiaLarguraRua2Efetiva();

            r1 = CriarRetanguloSimetrico(centroR1, vet1, perp1, semiComprimentoR1, halfW1);
            r2 = ehCruz
                ? CriarRetanguloSimetrico(centroR1, vet2, perp2, comprimentoR2, halfW2)
                : CriarRetanguloPonta(
                    pontoEntrada,
                    vetPontaR2,
                    new PointF(-vetPontaR2.Y, vetPontaR2.X),
                    comprimentoR2,
                    halfW2);
        }

        private RectangleF CalcularBoundsGlobal(PointF[] poligonoLocal)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var p in poligonoLocal)
            {
                var g = TransformarLocalParaGlobal(p);
                minX = Math.Min(minX, g.X);
                minY = Math.Min(minY, g.Y);
                maxX = Math.Max(maxX, g.X);
                maxY = Math.Max(maxY, g.Y);
            }

            const float margem = 2f;
            return new RectangleF(minX - margem, minY - margem, (maxX - minX) + margem * 2f, (maxY - minY) + margem * 2f);
        }

        private static RectangleF UnirBounds(RectangleF a, RectangleF b)
        {
            float minX = Math.Min(a.Left, b.Left);
            float minY = Math.Min(a.Top, b.Top);
            float maxX = Math.Max(a.Right, b.Right);
            float maxY = Math.Max(a.Bottom, b.Bottom);
            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }

        private PointF TransformarLocalParaGlobal(PointF local)
        {
            float rad = Rotacao * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            return new PointF(
                Posicao.X + local.X * cos - local.Y * sin,
                Posicao.Y + local.X * sin + local.Y * cos);
        }

        private PointF TransformarGlobalParaLocal(PointF global)
        {
            float rad = -Rotacao * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            float dx = global.X - Posicao.X;
            float dy = global.Y - Posicao.Y;
            return new PointF(
                dx * cos - dy * sin,
                dx * sin + dy * cos);
        }

        private static bool ContemPontoNoPoligono(PointF[] poligono, PointF pontoLocal, float tolerancia)
        {
            using var path = new GraphicsPath();
            path.AddPolygon(poligono);

            if (path.IsVisible(pontoLocal))
                return true;

            using var pen = new Pen(Color.Black, Math.Max(1f, tolerancia * 2f));
            return path.IsOutlineVisible(pontoLocal, pen);
        }

        private float ObterLarguraFaixasAdicionaisPorLadoRua1()
        {
            float extra = 0f;
            if (_temEstacionamentoRua1) extra += ACRESCIMO_ESTACIONAMENTO;
            if (_temCiclofaixaRua1) extra += ACRESCIMO_CICLOFAIXA;
            return extra;
        }

        private float ObterLarguraFaixasAdicionaisPorLadoRua2()
        {
            float extra = 0f;
            if (_temEstacionamentoRua2) extra += ACRESCIMO_ESTACIONAMENTO;
            if (_temCiclofaixaRua2) extra += ACRESCIMO_CICLOFAIXA;
            return extra;
        }

        private float ObterMeiaLarguraRua1Efetiva()
        {
            float meiaBase = _larguraRua1 / 2f;
            float extraPorLado = ObterLarguraFaixasAdicionaisPorLadoRua1();
            if (extraPorLado <= 0f)
                return Math.Max(5f, meiaBase);

            bool larguraJaIncluiExtras = (_larguraRua1 - extraPorLado * 2f) >= 10f;
            return Math.Max(5f, larguraJaIncluiExtras ? meiaBase : meiaBase + extraPorLado);
        }

        private float ObterMeiaLarguraRua2Efetiva()
        {
            float meiaBase = _larguraRua2 / 2f;
            float extraPorLado = ObterLarguraFaixasAdicionaisPorLadoRua2();
            if (extraPorLado <= 0f)
                return Math.Max(5f, meiaBase);

            bool larguraJaIncluiExtras = (_larguraRua2 - extraPorLado * 2f) >= 10f;
            return Math.Max(5f, larguraJaIncluiExtras ? meiaBase : meiaBase + extraPorLado);
        }

        private static void DesenharSinalizacaoVia(
            Graphics g,
            PointF referencia,
            PointF vet,
            PointF perp,
            float alcanceVia,
            float halfRoadWidth,
            bool desenharPare,
            bool desenharFaixaPedestre,
            bool duasExtremidades,
            float sentidoExtremidadeUnica = 1f)
        {
            if (!desenharPare && !desenharFaixaPedestre)
                return;

            using var penBranca = new Pen(Color.White, 3f);
            using var brushBranca = new SolidBrush(Color.White);
            using var fontePare = new Font("Arial", 10f, FontStyle.Bold);
            using var sfCentro = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            if (duasExtremidades)
            {
                DesenharSinalizacaoLado(g, referencia, vet, perp, 1f, alcanceVia, halfRoadWidth, desenharPare, desenharFaixaPedestre, penBranca, brushBranca, fontePare, sfCentro);
                DesenharSinalizacaoLado(g, referencia, vet, perp, -1f, alcanceVia, halfRoadWidth, desenharPare, desenharFaixaPedestre, penBranca, brushBranca, fontePare, sfCentro);
                return;
            }

            DesenharSinalizacaoLado(g, referencia, vet, perp, sentidoExtremidadeUnica, Math.Max(8f, alcanceVia * 0.18f), halfRoadWidth, desenharPare, desenharFaixaPedestre, penBranca, brushBranca, fontePare, sfCentro);
        }

        private static void DesenharSinalizacaoLado(
            Graphics g,
            PointF referencia,
            PointF vet,
            PointF perp,
            float sentido,
            float alcance,
            float halfRoadWidth,
            bool desenharPare,
            bool desenharFaixaPedestre,
            Pen penBranca,
            Brush brushBranca,
            Font fontePare,
            StringFormat sfCentro)
        {
            float baseOffset = Math.Max(2f, alcance - 19f); // deslocamento offset mínimo para que a sinalização não fique colada no cruzamento
            float crosswalkOffset = baseOffset + 9f; //Posição da faixa de pedestres em relação ao deslocamento offset
            float pareOffset = baseOffset + 30f; // Posição da marcação de PARE em relação ao deslocamento offset

            if (desenharFaixaPedestre)
            {
                var centroFaixa = new PointF(
                    referencia.X + vet.X * sentido * crosswalkOffset,
                    referencia.Y + vet.Y * sentido * crosswalkOffset);

                float larguraFaixa = Math.Max(halfRoadWidth * 1.8f, 28f);
                float comprimentoLinha = 14f;
                int qtd = Math.Max(5, (int)(larguraFaixa / 6f));
                float inicio = -larguraFaixa / 2f;
                float passo = qtd > 1 ? larguraFaixa / (qtd - 1) : 0f;

                using var penFaixa = new Pen(Color.White, 3f);
                for (int i = 0; i < qtd; i++)
                {
                    float deslocLateral = inicio + passo * i;
                    var c = new PointF(
                        centroFaixa.X + perp.X * deslocLateral,
                        centroFaixa.Y + perp.Y * deslocLateral);

                    g.DrawLine(
                        penFaixa,
                        c.X - vet.X * comprimentoLinha / 2f,
                        c.Y - vet.Y * comprimentoLinha / 2f,
                        c.X + vet.X * comprimentoLinha / 2f,
                        c.Y + vet.Y * comprimentoLinha / 2f);
                }
            }

            if (desenharPare)
            {
                var deslocEsquerda = new PointF(-perp.X * sentido * (halfRoadWidth * 0.5f), -perp.Y * sentido * (halfRoadWidth * 0.5f));

                var centroTexto = new PointF(
                    referencia.X + vet.X * sentido * pareOffset + deslocEsquerda.X,
                    referencia.Y + vet.Y * sentido * pareOffset + deslocEsquerda.Y);

                var centroLinha = new PointF(
                    centroTexto.X - vet.X * sentido * 10f,
                    centroTexto.Y - vet.Y * sentido * 10f);

                float meiaLinha = halfRoadWidth * 0.45f;
                var p1 = new PointF(
                    centroLinha.X - perp.X * meiaLinha,
                    centroLinha.Y - perp.Y * meiaLinha);
                var p2 = new PointF(
                    centroLinha.X + perp.X * meiaLinha,
                    centroLinha.Y + perp.Y * meiaLinha);
                g.DrawLine(penBranca, p1, p2);

                float anguloLinha = ObterAnguloTextoPare(perp, vet, sentido);
                var st = g.Save();
                g.TranslateTransform(centroTexto.X, centroTexto.Y);
                g.RotateTransform(anguloLinha);
                g.DrawString("PARE", fontePare, brushBranca, 0f, 0f, sfCentro);
                g.Restore(st);
            }
        }

        private static float ObterAnguloTextoPare(PointF perp, PointF vet, float sentido)
        {
            float anguloBase = (float)(Math.Atan2(perp.Y, perp.X) * 180f / Math.PI);
            float[] candidatos = { anguloBase, anguloBase + 180f };

            var direcaoAproximacao = new PointF(-vet.X * sentido, -vet.Y * sentido);
            float melhorDot = float.MinValue;
            float melhor = anguloBase;

            foreach (var ang in candidatos)
            {
                float rad = ang * (float)Math.PI / 180f;
                var up = new PointF((float)Math.Sin(rad), (float)-Math.Cos(rad));
                float dot = up.X * direcaoAproximacao.X + up.Y * direcaoAproximacao.Y;
                if (dot > melhorDot)
                {
                    melhorDot = dot;
                    melhor = ang;
                }
            }

            return melhor;
        }

        // ── ICustomTypeDescriptor: filtro dinâmico por Parte ───────────────────
        AttributeCollection ICustomTypeDescriptor.GetAttributes() => TypeDescriptor.GetAttributes(this, true);
        string ICustomTypeDescriptor.GetClassName() => TypeDescriptor.GetClassName(this, true);
        string ICustomTypeDescriptor.GetComponentName() => TypeDescriptor.GetComponentName(this, true);
        TypeConverter ICustomTypeDescriptor.GetConverter() => TypeDescriptor.GetConverter(this, true);
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() => TypeDescriptor.GetEvents(this, true);
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(this, attributes, true);
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() => ((ICustomTypeDescriptor)this).GetProperties(null);

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            var originem = TypeDescriptor.GetProperties(typeof(AutoIntersectionObject), attributes);
            var filtradas = new List<PropertyDescriptor>();

            foreach (PropertyDescriptor pd in originem)
            {
                if (DeveExibirPropriedade(pd.Name))
                    filtradas.Add(pd);
            }

            return new PropertyDescriptorCollection(filtradas.ToArray(), true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) => this;

        private bool DeveExibirPropriedade(string nomePropriedade)
        {
            if (Parte == AutoIntersectionParte.Ambas)
                return true;

            if (Parte == AutoIntersectionParte.Rua1)
                return !EhPropriedadeDaRua2(nomePropriedade);

            if (Parte == AutoIntersectionParte.Rua2)
                return !EhPropriedadeDaRua1(nomePropriedade);

            return true;
        }

        private static bool EhPropriedadeDaRua1(string nomePropriedade)
            => nomePropriedade.EndsWith("Rua1", StringComparison.Ordinal);

        private static bool EhPropriedadeDaRua2(string nomePropriedade)
            => nomePropriedade.EndsWith("Rua2", StringComparison.Ordinal)
               || string.Equals(nomePropriedade, nameof(DeslocamentoRua2), StringComparison.Ordinal);
    }
}
