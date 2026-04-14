// Objects/DimensionLine.cs - Linha de Cota/Medida
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class DimensionLine : BaseSketchObject
    {
        private float _tamanhoFonte = 12f;

        [Browsable(false)]
        public PointF PontoInicial { get; set; }

        [Browsable(false)]
        public PointF PontoFinal { get; set; }

        [Category("Aparência")]
        [DisplayName("Offset da Linha")]
        [Description("Distância da linha de cota em relação ao objeto medido")]
        public float Offset { get; set; } = 25f;

        [Category("Aparência")]
        [DisplayName("Extensão da Linha")]
        [Description("Comprimento das linhas de extensão")]
        public float ExtensaoLinha { get; set; } = 8f;

        [Category("Texto")]
        [DisplayName("Texto Customizado")]
        [Description("Texto customizado para a medida (deixe em branco para automático)")]
        public string TextoCustomizado { get; set; } = null;

        [Category("Texto")]
        [DisplayName("Mostrar Texto")]
        [Description("Exibe o texto da medida")]
        public bool MostrarTexto { get; set; } = true;

        [Category("Texto")]
        [DisplayName("Tamanho da Fonte")]
        [Description("Tamanho da fonte do texto da medida")]
        public float TamanhoFonte
        {
            get => _tamanhoFonte;
            set => _tamanhoFonte = Math.Max(6f, value);
        }

        [Browsable(false)]
        public int CorTextoArgb { get; set; } = Color.FromArgb(200, 0, 0).ToArgb();

        [Browsable(false)]
        public int CorFundoTextoArgb { get; set; } = Color.FromArgb(60, 255, 255, 255).ToArgb();

        [Category("Texto")]
        [DisplayName("Cor do Texto")]
        [Description("Cor do texto da medida")]
        [JsonIgnore]
        public Color CorTexto
        {
            get => Color.FromArgb(CorTextoArgb);
            set => CorTextoArgb = value.ToArgb();
        }

        [Category("Texto")]
        [DisplayName("Cor do Fundo")]
        [Description("Cor da caixa de fundo do texto da medida")]
        [JsonIgnore]
        public Color CorFundoTexto
        {
            get => Color.FromArgb(CorFundoTextoArgb);
            set => CorFundoTextoArgb = value.ToArgb();
        }

        // Referência à escala para converter pixels em medida real
        [Browsable(false)]
        [System.Text.Json.Serialization.JsonIgnore]
        public Core.ScaleManager Escala { get; set; }

        public DimensionLine()
        {
            Tipo = "Cota";
            CorContorno = Color.FromArgb(200, 0, 0);
            EspessuraContorno = 1f;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            float dx = PontoFinal.X - PontoInicial.X;
            float dy = PontoFinal.Y - PontoInicial.Y;
            float comp = (float)Math.Sqrt(dx * dx + dy * dy);
            if (comp < 1) return;

            // Direção perpendicular (para offset)
            float perpX = -dy / comp * Offset;
            float perpY = dx / comp * Offset;

            // Pontos da linha de cota (com offset)
            var cotaP1 = new PointF(
                PontoInicial.X + perpX, PontoInicial.Y + perpY);
            var cotaP2 = new PointF(
                PontoFinal.X + perpX, PontoFinal.Y + perpY);

            using (var pen = new Pen(CorContorno, EspessuraContorno))
            {
                // Linhas de extensão
                float extPerpX = perpX > 0 ?
                    perpX + ExtensaoLinha * (-dy / comp) :
                    perpX - ExtensaoLinha * (-dy / comp);
                float extPerpY = perpY > 0 ?
                    perpY + ExtensaoLinha * (dx / comp) :
                    perpY - ExtensaoLinha * (dx / comp);

                g.DrawLine(pen, PontoInicial,
                    new PointF(PontoInicial.X + extPerpX,
                               PontoInicial.Y + extPerpY));
                g.DrawLine(pen, PontoFinal,
                    new PointF(PontoFinal.X + extPerpX,
                               PontoFinal.Y + extPerpY));

                // Linha de cota principal
                g.DrawLine(pen, cotaP1, cotaP2);

                // Setas nas extremidades
                float arrowSize = 8f;
                float arrowAngle = 25f * (float)Math.PI / 180f;
                float dirX = dx / comp;
                float dirY = dy / comp;

                DesenharSeta(g, pen, cotaP1, dirX, dirY,
                    arrowSize, arrowAngle);
                DesenharSeta(g, pen, cotaP2, -dirX, -dirY,
                    arrowSize, arrowAngle);
            }

            // Texto da medida
            if (MostrarTexto)
            {
                string texto = TextoCustomizado ??
                    (Escala != null ? Escala.FormatarMedidaTransito(comp) :
                     $"{comp:F1} px");

                var centro = new PointF(
                    (cotaP1.X + cotaP2.X) / 2,
                    (cotaP1.Y + cotaP2.Y) / 2);

                var state = g.Save();
                g.TranslateTransform(centro.X, centro.Y);

                float angulo = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
                if (angulo > 90 || angulo < -90) angulo += 180;
                g.RotateTransform(angulo);

                using (var font = new Font("Segoe UI", TamanhoFonte))
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Far;

                    var size = g.MeasureString(texto, font);
                    using (var bg = new SolidBrush(Color.FromArgb(CorFundoTextoArgb)))
                    {
                        g.FillRectangle(bg,
                            -size.Width / 2 - 2, -size.Height - 1,
                            size.Width + 4, size.Height + 2);
                    }

                    using (var brushTexto = new SolidBrush(Color.FromArgb(CorTextoArgb)))
                    {
                        g.DrawString(texto, font, brushTexto, 0, 0, format);
                    }
                }

                g.Restore(state);
            }

            if (Selecionado) DesenharSelecao(g);
        }

        private void DesenharSeta(Graphics g, Pen pen, PointF ponta,
            float dirX, float dirY, float size, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            var p1 = new PointF(
                ponta.X + size * (dirX * cos - dirY * sin),
                ponta.Y + size * (dirY * cos + dirX * sin));
            var p2 = new PointF(
                ponta.X + size * (dirX * cos + dirY * sin),
                ponta.Y + size * (dirY * cos - dirX * sin));

            g.FillPolygon(new SolidBrush(CorContorno),
                new[] { ponta, p1, p2 });
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            float perpX = -(PontoFinal.Y - PontoInicial.Y);
            float perpY = PontoFinal.X - PontoInicial.X;
            float comp = (float)Math.Sqrt(perpX * perpX + perpY * perpY);
            if (comp == 0) return false;
            perpX = perpX / comp * Offset;
            perpY = perpY / comp * Offset;

            var cotaP1 = new PointF(
                PontoInicial.X + perpX, PontoInicial.Y + perpY);
            var cotaP2 = new PointF(
                PontoFinal.X + perpX, PontoFinal.Y + perpY);

            return Utils.GeometryHelper.DistanciaPontoSegmento(
                ponto, cotaP1, cotaP2) <= tolerancia + 5;
        }

        private static void ExpandirBounds(ref float minX, ref float minY, ref float maxX, ref float maxY, PointF p)
        {
            minX = Math.Min(minX, p.X);
            minY = Math.Min(minY, p.Y);
            maxX = Math.Max(maxX, p.X);
            maxY = Math.Max(maxY, p.Y);
        }

        private static PointF RotacionarPontoLocal(PointF p, float cos, float sin)
        {
            return new PointF(
                p.X * cos - p.Y * sin,
                p.X * sin + p.Y * cos);
        }

        public override RectangleF GetBounds()
        {
            float dx = PontoFinal.X - PontoInicial.X;
            float dy = PontoFinal.Y - PontoInicial.Y;
            float comp = (float)Math.Sqrt(dx * dx + dy * dy);

            if (comp < 0.001f)
                return new RectangleF(PontoInicial.X - 2f, PontoInicial.Y - 2f, 4f, 4f);

            float perpX = -dy / comp * Offset;
            float perpY = dx / comp * Offset;

            var cotaP1 = new PointF(PontoInicial.X + perpX, PontoInicial.Y + perpY);
            var cotaP2 = new PointF(PontoFinal.X + perpX, PontoFinal.Y + perpY);

            float extPerpX = perpX > 0 ?
                perpX + ExtensaoLinha * (-dy / comp) :
                perpX - ExtensaoLinha * (-dy / comp);
            float extPerpY = perpY > 0 ?
                perpY + ExtensaoLinha * (dx / comp) :
                perpY - ExtensaoLinha * (dx / comp);

            var extP1 = new PointF(PontoInicial.X + extPerpX, PontoInicial.Y + extPerpY);
            var extP2 = new PointF(PontoFinal.X + extPerpX, PontoFinal.Y + extPerpY);

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, PontoInicial);
            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, PontoFinal);
            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, extP1);
            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, extP2);
            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, cotaP1);
            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, cotaP2);

            float arrowSize = 8f;
            float arrowAngle = 25f * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(arrowAngle);
            float sin = (float)Math.Sin(arrowAngle);
            float dirX = dx / comp;
            float dirY = dy / comp;

            var p1a = new PointF(
                cotaP1.X + arrowSize * (dirX * cos - dirY * sin),
                cotaP1.Y + arrowSize * (dirY * cos + dirX * sin));
            var p1b = new PointF(
                cotaP1.X + arrowSize * (dirX * cos + dirY * sin),
                cotaP1.Y + arrowSize * (dirY * cos - dirX * sin));

            var p2a = new PointF(
                cotaP2.X + arrowSize * ((-dirX) * cos - (-dirY) * sin),
                cotaP2.Y + arrowSize * ((-dirY) * cos + (-dirX) * sin));
            var p2b = new PointF(
                cotaP2.X + arrowSize * ((-dirX) * cos + (-dirY) * sin),
                cotaP2.Y + arrowSize * ((-dirY) * cos - (-dirX) * sin));

            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, p1a);
            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, p1b);
            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, p2a);
            ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, p2b);

            if (MostrarTexto)
            {
                string texto = TextoCustomizado ??
                    (Escala != null ? Escala.FormatarMedidaTransito(comp) :
                     $"{comp:F1} px");

                using (var bmp = new Bitmap(1, 1))
                using (var g = Graphics.FromImage(bmp))
                using (var font = new Font("Segoe UI", TamanhoFonte))
                {
                    var size = g.MeasureString(texto, font);
                    var centro = new PointF((cotaP1.X + cotaP2.X) / 2f, (cotaP1.Y + cotaP2.Y) / 2f);

                    float angulo = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
                    if (angulo > 90 || angulo < -90) angulo += 180;
                    float rad = angulo * (float)Math.PI / 180f;
                    float cosA = (float)Math.Cos(rad);
                    float sinA = (float)Math.Sin(rad);

                    var locais = new[]
                    {
                        new PointF(-size.Width / 2f - 2f, -size.Height - 1f),
                        new PointF(size.Width / 2f + 2f, -size.Height - 1f),
                        new PointF(size.Width / 2f + 2f, 1f),
                        new PointF(-size.Width / 2f - 2f, 1f)
                    };

                    foreach (var local in locais)
                    {
                        var rot = RotacionarPontoLocal(local, cosA, sinA);
                        var mundo = new PointF(centro.X + rot.X, centro.Y + rot.Y);
                        ExpandirBounds(ref minX, ref minY, ref maxX, ref maxY, mundo);
                    }
                }
            }

            float margem = Math.Max(2f, EspessuraContorno + 1f);
            return new RectangleF(minX - margem, minY - margem, (maxX - minX) + margem * 2, (maxY - minY) + margem * 2);
        }

        public override void Mover(float dx, float dy)
        {
            PontoInicial = new PointF(PontoInicial.X + dx, PontoInicial.Y + dy);
            PontoFinal = new PointF(PontoFinal.X + dx, PontoFinal.Y + dy);
            Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy);
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            PontoInicial = EscalarPonto(PontoInicial, centro, fatorX, fatorY);
            PontoFinal = EscalarPonto(PontoFinal, centro, fatorX, fatorY);
            float media = (Math.Abs(fatorX) + Math.Abs(fatorY)) / 2f;
            Offset *= media;
            ExtensaoLinha *= media;
            TamanhoFonte = Math.Max(6f, TamanhoFonte * media);
            base.EscalarAoRedor(centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            PontoInicial = RotacionarPonto(PontoInicial, centro, deltaGraus);
            PontoFinal = RotacionarPonto(PontoFinal, centro, deltaGraus);
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }
    }
}