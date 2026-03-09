// Objects/StickFigure.cs - Boneco Articulável
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class StickFigure : BaseSketchObject
    {
        // Articulações (ângulos em graus)
        public float AnguloCabeca { get; set; } = 0f;
        public float AnguloBracoDireito { get; set; } = 45f;
        public float AnguloAntebracoDir { get; set; } = 0f;
        public float AnguloBracoEsquerdo { get; set; } = -45f;
        public float AnguloAntebracoEsq { get; set; } = 0f;
        public float AnguloPernaDireita { get; set; } = 20f;
        public float AnguloCanelaDireita { get; set; } = 0f;
        public float AnguloPernaEsquerda { get; set; } = -20f;
        public float AnguloCanelaEsquerda { get; set; } = 0f;
        public float AnguloTronco { get; set; } = 0f;

        // Tamanhos proporcionais
        public float TamanhoCabeca { get; set; } = 12f;
        public float TamanhoTronco { get; set; } = 35f;
        public float TamanhoBraco { get; set; } = 20f;
        public float TamanhoAntebraco { get; set; } = 18f;
        public float TamanhoPerna { get; set; } = 25f;
        public float TamanhoCanela { get; set; } = 22f;

        public float EscalaCorpo { get; set; } = 1f;
        public string Rotulo { get; set; } = ""; // ex: "Vítima", "Suspeito"
        public bool Contorno { get; set; } = false;
        public bool Preenchido { get; set; } = false;

        // Estado: Normal, Deitado, Sentado
        public string Estado { get; set; } = "Normal";

        // Articulações selecionáveis
        [System.Text.Json.Serialization.JsonIgnore]
        public int ArticulacaoSelecionada { get; set; } = -1;

        public StickFigure()
        {
            Tipo = "Corpo";
            CorContorno = Color.DarkRed;
            EspessuraContorno = 2.5f;
        }

        /// <summary>
        /// Define pose pré-configurada
        /// </summary>
        public void DefinirPose(string pose)
        {
            switch (pose)
            {
                case "EmPe":
                    AnguloBracoDireito = 30f;
                    AnguloBracoEsquerdo = -30f;
                    AnguloPernaDireita = 10f;
                    AnguloPernaEsquerda = -10f;
                    AnguloAntebracoDir = 0f;
                    AnguloAntebracoEsq = 0f;
                    AnguloCanelaDireita = 0f;
                    AnguloCanelaEsquerda = 0f;
                    break;

                case "Deitado":
                    AnguloTronco = 90f;
                    AnguloBracoDireito = 60f;
                    AnguloBracoEsquerdo = -40f;
                    AnguloPernaDireita = 85f;
                    AnguloPernaEsquerda = 95f;
                    AnguloCanelaDireita = -20f;
                    AnguloCanelaEsquerda = 10f;
                    break;

                case "DeitadoBrucos":
                    AnguloTronco = 90f;
                    AnguloBracoDireito = 120f;
                    AnguloBracoEsquerdo = -130f;
                    AnguloPernaDireita = 80f;
                    AnguloPernaEsquerda = 100f;
                    break;

                case "Sentado":
                    AnguloPernaDireita = 85f;
                    AnguloPernaEsquerda = 85f;
                    AnguloCanelaDireita = -85f;
                    AnguloCanelaEsquerda = -85f;
                    AnguloBracoDireito = 20f;
                    AnguloBracoEsquerdo = -20f;
                    break;

                case "Ajoelhado":
                    AnguloPernaDireita = 85f;
                    AnguloCanelaDireita = 170f;
                    AnguloPernaEsquerda = 0f;
                    AnguloCanelaEsquerda = -85f;
                    break;
            }
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            var state = g.Save();
            g.TranslateTransform(Posicao.X, Posicao.Y);
            g.RotateTransform(Rotacao);
            g.ScaleTransform(EscalaCorpo * EscalaX, EscalaCorpo * EscalaY);

            float escala = 1f;

            using (var pen = new Pen(CorContorno, EspessuraContorno))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                // Cabeça (no topo)
                float headY = -TamanhoTronco / 2 - TamanhoCabeca;
                PointF headCenter = new PointF(0, headY);

                if (Preenchido)
                {
                    using (var brush = new SolidBrush(
                        Color.FromArgb(100, CorContorno)))
                    {
                        g.FillEllipse(brush,
                            -TamanhoCabeca / 2, headY - TamanhoCabeca / 2,
                            TamanhoCabeca, TamanhoCabeca);
                    }
                }
                g.DrawEllipse(pen,
                    -TamanhoCabeca / 2, headY - TamanhoCabeca / 2,
                    TamanhoCabeca, TamanhoCabeca);

                // Tronco
                PointF ombro = new PointF(0, -TamanhoTronco / 2);
                PointF quadril = new PointF(0, TamanhoTronco / 2);
                g.DrawLine(pen, ombro, quadril);

                // Braço direito
                DesenharMembro(g, pen, ombro, AnguloBracoDireito,
                    TamanhoBraco, AnguloAntebracoDir, TamanhoAntebraco, 0);

                // Braço esquerdo
                DesenharMembro(g, pen, ombro, AnguloBracoEsquerdo,
                    TamanhoBraco, AnguloAntebracoEsq, TamanhoAntebraco, 1);

                // Perna direita
                DesenharMembro(g, pen, quadril, AnguloPernaDireita,
                    TamanhoPerna, AnguloCanelaDireita, TamanhoCanela, 2);

                // Perna esquerda
                DesenharMembro(g, pen, quadril, AnguloPernaEsquerda,
                    TamanhoPerna, AnguloCanelaEsquerda, TamanhoCanela, 3);
            }

            // Articulações (pontos de controle quando selecionado)
            if (Selecionado)
            {
                DesenharArticulacoes(g);
            }

            // Rótulo
            if (!string.IsNullOrEmpty(Rotulo))
            {
                using (var font = new Font("Segoe UI", 7f))
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    g.DrawString(Rotulo, font, new SolidBrush(CorContorno),
                        0, TamanhoTronco / 2 + TamanhoPerna +
                        TamanhoCanela + 5, format);
                }
            }

            g.Restore(state);

            if (Selecionado) DesenharSelecao(g);
        }

        private void DesenharMembro(Graphics g, Pen pen, PointF origem,
            float angulo1, float comp1, float angulo2, float comp2,
            int membroIndex)
        {
            float rad1 = angulo1 * (float)Math.PI / 180f;
            var articulacao = new PointF(
                origem.X + (float)Math.Sin(rad1) * comp1,
                origem.Y + (float)Math.Cos(rad1) * comp1);
            g.DrawLine(pen, origem, articulacao);

            float rad2 = (angulo1 + angulo2) * (float)Math.PI / 180f;
            var extremidade = new PointF(
                articulacao.X + (float)Math.Sin(rad2) * comp2,
                articulacao.Y + (float)Math.Cos(rad2) * comp2);
            g.DrawLine(pen, articulacao, extremidade);
        }

        private void DesenharArticulacoes(Graphics g)
        {
            float dotSize = 5f;
            var articulacoes = GetPosicoesArticulacoes();

            for (int i = 0; i < articulacoes.Count; i++)
            {
                var cor = (i == ArticulacaoSelecionada) ?
                    Color.Red : Color.Orange;
                using (var brush = new SolidBrush(cor))
                {
                    g.FillEllipse(brush,
                        articulacoes[i].X - dotSize / 2,
                        articulacoes[i].Y - dotSize / 2,
                        dotSize, dotSize);
                }
            }
        }

        public List<PointF> GetPosicoesArticulacoes()
        {
            var lista = new List<PointF>();
            // 0: Ombro, 1: CotoveloDir, 2: MaoDir
            // 3: CotoveloEsq, 4: MaoEsq
            // 5: QuadrilDir, 6: JoelhoDir, 7: PeDir
            // 8: QuadrilEsq, 9: JoelhoEsq, 10: PeEsq

            PointF ombro = new PointF(0, -TamanhoTronco / 2);
            PointF quadril = new PointF(0, TamanhoTronco / 2);

            lista.Add(ombro);
            AdicionarArticulacoesMembro(lista, ombro,
                AnguloBracoDireito, TamanhoBraco,
                AnguloAntebracoDir, TamanhoAntebraco);
            AdicionarArticulacoesMembro(lista, ombro,
                AnguloBracoEsquerdo, TamanhoBraco,
                AnguloAntebracoEsq, TamanhoAntebraco);
            lista.Add(quadril);
            AdicionarArticulacoesMembro(lista, quadril,
                AnguloPernaDireita, TamanhoPerna,
                AnguloCanelaDireita, TamanhoCanela);
            AdicionarArticulacoesMembro(lista, quadril,
                AnguloPernaEsquerda, TamanhoPerna,
                AnguloCanelaEsquerda, TamanhoCanela);

            return lista;
        }

        private void AdicionarArticulacoesMembro(List<PointF> lista,
            PointF origem, float ang1, float comp1, float ang2, float comp2)
        {
            float rad1 = ang1 * (float)Math.PI / 180f;
            var meio = new PointF(
                origem.X + (float)Math.Sin(rad1) * comp1,
                origem.Y + (float)Math.Cos(rad1) * comp1);
            lista.Add(meio);

            float rad2 = (ang1 + ang2) * (float)Math.PI / 180f;
            var fim = new PointF(
                meio.X + (float)Math.Sin(rad2) * comp2,
                meio.Y + (float)Math.Cos(rad2) * comp2);
            lista.Add(fim);
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            var bounds = GetBounds();
            bounds.Inflate(tolerancia, tolerancia);
            return bounds.Contains(ponto);
        }

        public override RectangleF GetBounds()
        {
            float totalHeight = TamanhoCabeca * 2 + TamanhoTronco +
                TamanhoPerna + TamanhoCanela;
            float totalWidth = Math.Max(TamanhoBraco + TamanhoAntebraco, 0) * 2;
            float scale = EscalaCorpo * EscalaX;

            return new RectangleF(
                Posicao.X - totalWidth * scale / 2,
                Posicao.Y - (TamanhoTronco / 2 + TamanhoCabeca * 2) * scale,
                totalWidth * scale,
                totalHeight * scale);
        }
    }
}