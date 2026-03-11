// Objects/StickFigure.cs
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    public enum GeneroCorpo
    {
        Masculino,
        Feminino
    }

    public enum PoseCorpo
    {
        EmPe = 0,
        VistaAerea = 1
    }

    /// <summary>
    /// Representação de corpo humano para croqui de local de crime.
    /// Vista superior (planta baixa).
    /// </summary>
    [Serializable]
    public class StickFigure : BaseSketchObject
    {
        #region Propriedades

        public GeneroCorpo Genero { get; set; } = GeneroCorpo.Masculino;
        public PoseCorpo Pose { get; set; } = PoseCorpo.EmPe;

        // Proporções (em pixels)
        public float EscalaCorpo { get; set; } = 1f;

        // Dimensões base (serão escaladas)
        public float LarguraCabeca { get; set; } = 22f;
        public float AlturaCabeca { get; set; } = 26f;

        public float LarguraOmbros { get; set; } = 40f;
        public float LarguraCintura { get; set; } = 32f;
        public float LarguraQuadril { get; set; } = 36f;
        public float AlturaTronco { get; set; } = 50f;

        public float LarguraPerna { get; set; } = 12f;
        public float AlturaPerna { get; set; } = 45f;

        public float LarguraPe { get; set; } = 10f;
        public float AlturaPe { get; set; } = 16f;

        // Ângulos de articulação (em graus)
        public float AnguloBracoDireito { get; set; } = 15f;
        public float AnguloBracoEsquerdo { get; set; } = -15f;
        public float AnguloCotoveloDireito { get; set; } = 12f;
        public float AnguloCotoveloEsquerdo { get; set; } = -12f;
        public float AnguloPernaDireita { get; set; } = 5f;
        public float AnguloPernaEsquerda { get; set; } = -5f;
        public float AnguloJoelhoDireito { get; set; } = 8f;
        public float AnguloJoelhoEsquerdo { get; set; } = -8f;

        // Vista aérea
        [Category("Pose")]
        [DisplayName("Braços Estendidos")]
        [Description("Na visão aérea, mantém os braços totalmente abertos")]
        public bool BracosEstendidos { get; set; } = false;

        // Ângulo geral de rotação do corpo
        public float AnguloCorpo { get; set; } = 0f;

        // Identificação
        public string Rotulo { get; set; } = "Vítima";
        public bool MostrarRotulo { get; set; } = true;
        public int NumeroMarcador { get; set; } = 0; // 0 = sem número

        // Cores
        [Browsable(false)]
        public int CorPeleArgb { get; set; } = Color.FromArgb(255, 224, 189).ToArgb();
        [Browsable(false)]
        public int CorRoupaArgb { get; set; } = Color.FromArgb(70, 130, 180).ToArgb();
        [Browsable(false)]
        public int CorCabeloArgb { get; set; } = Color.FromArgb(60, 40, 30).ToArgb();
        [Browsable(false)]
        public int CorSapatoArgb { get; set; } = Color.FromArgb(40, 40, 40).ToArgb();

        // Visualização
        public bool MostrarContorno { get; set; } = true;
        public bool Preenchido { get; set; } = true;
        public float EspessuraContorno { get; set; } = 1.5f;

        #endregion

        #region Propriedades Calculadas

        [JsonIgnore]
        [Category("Aparência"), DisplayName("Cor da Pele")]
        public Color CorPele 
        { 
            get => Color.FromArgb(CorPeleArgb); 
            set => CorPeleArgb = value.ToArgb(); 
        }

        [JsonIgnore]
        [Category("Aparência"), DisplayName("Cor da Roupa")]
        public Color CorRoupa 
        { 
            get => Color.FromArgb(CorRoupaArgb); 
            set => CorRoupaArgb = value.ToArgb(); 
        }

        [JsonIgnore]
        [Category("Aparência"), DisplayName("Cor do Cabelo")]
        public Color CorCabelo 
        { 
            get => Color.FromArgb(CorCabeloArgb); 
            set => CorCabeloArgb = value.ToArgb(); 
        }

        [JsonIgnore]
        [Category("Aparência"), DisplayName("Cor do Sapato")]
        public Color CorSapato 
        { 
            get => Color.FromArgb(CorSapatoArgb); 
            set => CorSapatoArgb = value.ToArgb(); 
        }

        [JsonIgnore]
        public float AlturaTotal
        {
            get
            {
                if (Pose == PoseCorpo.VistaAerea)
                    return Math.Max(AlturaCabeca + 16f, AlturaTronco * 0.45f) * EscalaCorpo;

                return (AlturaCabeca + AlturaTronco + AlturaPerna + AlturaPe) * EscalaCorpo;
            }
        }

        [JsonIgnore]
        public float LarguraTotal
        {
            get
            {
                if (Pose == PoseCorpo.VistaAerea)
                {
                    float larguraBracos = BracosEstendidos ? (LarguraOmbros + 78f) : (LarguraOmbros + 26f);
                    return larguraBracos * EscalaCorpo;
                }

                float largMax = Math.Max(LarguraOmbros, LarguraQuadril);
                return largMax * EscalaCorpo * 1.3f;
            }
        }

        #endregion

        public StickFigure()
        {
            Tipo = "Corpo";
            AplicarProporcoesGenero();
        }

        #region Configuração

        /// <summary>
        /// Aplica proporções baseadas no gênero
        /// </summary>
        public void AplicarProporcoesGenero()
        {
            if (Genero == GeneroCorpo.Feminino)
            {
                LarguraOmbros = 36f;
                LarguraCintura = 28f;
                LarguraQuadril = 38f;
                AlturaTronco = 45f;
                LarguraCabeca = 20f;
                AlturaCabeca = 24f;
                LarguraPerna = 11f;
                AlturaPerna = 43f;
            }
            else // Masculino
            {
                LarguraOmbros = 42f;
                LarguraCintura = 34f;
                LarguraQuadril = 34f;
                AlturaTronco = 52f;
                LarguraCabeca = 22f;
                AlturaCabeca = 26f;
                LarguraPerna = 13f;
                AlturaPerna = 46f;
            }
        }

        /// <summary>
        /// Define a pose
        /// </summary>
        public void DefinirPose(string pose)
        {
            switch (pose.ToLower())
            {
                case "empe":
                case "deitado":
                    Pose = PoseCorpo.EmPe;
                    AnguloBracoDireito = 15f;
                    AnguloBracoEsquerdo = -15f;
                    AnguloCotoveloDireito = 12f;
                    AnguloCotoveloEsquerdo = -12f;
                    AnguloPernaDireita = 5f;
                    AnguloPernaEsquerda = -5f;
                    AnguloJoelhoDireito = 8f;
                    AnguloJoelhoEsquerdo = -8f;
                    BracosEstendidos = false;
                    break;

                case "aerea":
                case "vistaaerea":
                case "porcima":
                    Pose = PoseCorpo.VistaAerea;
                    AnguloBracoDireito = 90f;
                    AnguloBracoEsquerdo = -90f;
                    AnguloCotoveloDireito = 0f;
                    AnguloCotoveloEsquerdo = 0f;
                    BracosEstendidos = true;
                    break;
            }
        }

        #endregion

        #region Desenho Principal

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            var state = g.Save();

            // Posicionar no centro do corpo
            g.TranslateTransform(Posicao.X, Posicao.Y);
            g.RotateTransform(Rotacao + AnguloCorpo);
            g.ScaleTransform(EscalaCorpo * EscalaX, EscalaCorpo * EscalaY);

            // Aplicar opacidade se necessário
            float opacidade = Opacidade;

            if (Pose == PoseCorpo.VistaAerea)
                DesenharCorpoVistaAerea(g, opacidade);
            else
                DesenharCorpoEmPe(g, opacidade);

            g.Restore(state);

            // Rótulo (fora da transformação)
            if (MostrarRotulo && !string.IsNullOrEmpty(Rotulo))
            {
                DesenharRotulo(g);
            }

            if (Selecionado)
            {
                DesenharSelecao(g);
            }
        }

        /// <summary>
        /// Corpo em pé visto de cima (deitado na planta baixa)
        /// Ordem: Pés → Pernas → Tronco → Braços → Cabeça
        /// </summary>
        private void DesenharCorpoEmPe(Graphics g, float opacidade)
        {
            float yTroncoTop = -AlturaTronco / 2;
            float yTroncoBottom = AlturaTronco / 2;
            float yCabecaCenter = yTroncoTop - AlturaCabeca / 2 - 3;
            float yQuadril = yTroncoBottom;

            DesenharPe(g, LarguraQuadril / 4, yQuadril, AnguloPernaDireita, AnguloJoelhoDireito, opacidade);
            DesenharPe(g, -LarguraQuadril / 4, yQuadril, AnguloPernaEsquerda, AnguloJoelhoEsquerdo, opacidade);

            DesenharPerna(g, LarguraQuadril / 4, yQuadril, AnguloPernaDireita, AnguloJoelhoDireito, opacidade);
            DesenharPerna(g, -LarguraQuadril / 4, yQuadril, AnguloPernaEsquerda, AnguloJoelhoEsquerdo, opacidade);

            DesenharTronco(g, yTroncoTop, opacidade);

            DesenharBraco(g, LarguraOmbros / 2, yTroncoTop + 8, AnguloBracoDireito, AnguloCotoveloDireito, opacidade);
            DesenharBraco(g, -LarguraOmbros / 2, yTroncoTop + 8, AnguloBracoEsquerdo, AnguloCotoveloEsquerdo, opacidade);

            DesenharCabeca(g, 0, yCabecaCenter, opacidade);
        }

        private void DesenharCorpoVistaAerea(Graphics g, float opacidade)
        {
            float yOmbros = 0f;
            float larguraOmbrosTopo = LarguraOmbros * 1.05f;
            float alturaOmbrosTopo = AlturaTronco * 0.35f;

            if (Preenchido)
            {
                using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorRoupa, 0.1f), opacidade)))
                {
                    g.FillEllipse(brush,
                        -larguraOmbrosTopo / 2,
                        yOmbros - alturaOmbrosTopo / 2,
                        larguraOmbrosTopo,
                        alturaOmbrosTopo);
                }
            }

            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(60, 60, 80), opacidade), EspessuraContorno))
                {
                    g.DrawEllipse(pen,
                        -larguraOmbrosTopo / 2,
                        yOmbros - alturaOmbrosTopo / 2,
                        larguraOmbrosTopo,
                        alturaOmbrosTopo);
                }
            }

            float yCabeca = yOmbros - alturaOmbrosTopo / 2 - AlturaCabeca * 0.3f;
            DesenharTopoCabeca(g, 0, yCabeca, opacidade);

            float anguloDir = BracosEstendidos ? 90f : AnguloBracoDireito;
            float anguloEsq = BracosEstendidos ? -90f : AnguloBracoEsquerdo;
            float cotoveloDir = BracosEstendidos ? 0f : AnguloCotoveloDireito;
            float cotoveloEsq = BracosEstendidos ? 0f : AnguloCotoveloEsquerdo;

            DesenharBraco(g, larguraOmbrosTopo / 2 - 2, yOmbros - 3, anguloDir, cotoveloDir, opacidade);
            DesenharBraco(g, -larguraOmbrosTopo / 2 + 2, yOmbros - 3, anguloEsq, cotoveloEsq, opacidade);
        }

        #endregion

        #region Partes do Corpo

        private void DesenharCabeca(Graphics g, float cx, float cy, float opacidade)
        {
            // Cabelo (oval maior atrás)
            using (var brushCabelo = new SolidBrush(AplicarOpacidade(CorCabelo, opacidade)))
            {
                float cabeloExtra = 3f;
                g.FillEllipse(brushCabelo,
                    cx - LarguraCabeca / 2 - cabeloExtra / 2,
                    cy - AlturaCabeca / 2 - cabeloExtra / 2,
                    LarguraCabeca + cabeloExtra,
                    AlturaCabeca + cabeloExtra);
            }

            // Rosto (oval)
            using (var brushPele = new SolidBrush(AplicarOpacidade(CorPele, opacidade)))
            {
                g.FillEllipse(brushPele,
                    cx - LarguraCabeca / 2,
                    cy - AlturaCabeca / 2 + 2,
                    LarguraCabeca,
                    AlturaCabeca - 2);
            }

            // Contorno
            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(80, 60, 50), opacidade),
                    EspessuraContorno))
                {
                    g.DrawEllipse(pen,
                        cx - LarguraCabeca / 2,
                        cy - AlturaCabeca / 2,
                        LarguraCabeca,
                        AlturaCabeca);
                }
            }

            // Indicação simplificada de face (parte superior da cabeça)
            using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(60, 0, 0, 0), opacidade), 1f))
            {
                // Sobrancelhas simplificadas
                float yOlhos = cy - AlturaCabeca / 6;
                g.DrawArc(pen, cx - 6, yOlhos - 2, 5, 3, 0, 180);
                g.DrawArc(pen, cx + 1, yOlhos - 2, 5, 3, 0, 180);
            }
        }

        private void DesenharTopoCabeca(Graphics g, float cx, float cy, float opacidade)
        {
            using (var brushCabelo = new SolidBrush(AplicarOpacidade(CorCabelo, opacidade)))
            {
                float diametro = Math.Max(LarguraCabeca, AlturaCabeca) + 3f;
                g.FillEllipse(brushCabelo,
                    cx - diametro / 2,
                    cy - diametro / 2,
                    diametro,
                    diametro);
            }

            using (var brushPele = new SolidBrush(AplicarOpacidade(CorPele, opacidade)))
            {
                float diametro = Math.Max(LarguraCabeca, AlturaCabeca) * 0.84f;
                g.FillEllipse(brushPele,
                    cx - diametro / 2,
                    cy - diametro / 2,
                    diametro,
                    diametro);
            }

            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(80, 60, 50), opacidade), EspessuraContorno))
                {
                    float diametro = Math.Max(LarguraCabeca, AlturaCabeca) * 0.84f;
                    g.DrawEllipse(pen,
                        cx - diametro / 2,
                        cy - diametro / 2,
                        diametro,
                        diametro);
                }
            }
        }

        private void DesenharTronco(Graphics g, float yTop, float opacidade)
        {
            // Criar forma de tronco (trapézio com curvas)
            using (var path = new GraphicsPath())
            {
                float yOmbros = yTop;
                float yCintura = yTop + AlturaTronco * 0.45f;
                float yQuadril = yTop + AlturaTronco;

                if (Genero == GeneroCorpo.Feminino)
                {
                    // Feminino: ombros estreitos, cintura fina, quadril largo
                    path.AddBezier(
                        new PointF(-LarguraOmbros / 2, yOmbros),
                        new PointF(-LarguraOmbros / 2 - 2, yCintura - 10),
                        new PointF(-LarguraCintura / 2 - 3, yCintura),
                        new PointF(-LarguraCintura / 2, yCintura));

                    path.AddBezier(
                        new PointF(-LarguraCintura / 2, yCintura),
                        new PointF(-LarguraCintura / 2 - 5, yCintura + 10),
                        new PointF(-LarguraQuadril / 2 - 3, yQuadril - 5),
                        new PointF(-LarguraQuadril / 2, yQuadril));

                    path.AddLine(-LarguraQuadril / 2, yQuadril, LarguraQuadril / 2, yQuadril);

                    path.AddBezier(
                        new PointF(LarguraQuadril / 2, yQuadril),
                        new PointF(LarguraQuadril / 2 + 3, yQuadril - 5),
                        new PointF(LarguraCintura / 2 + 5, yCintura + 10),
                        new PointF(LarguraCintura / 2, yCintura));

                    path.AddBezier(
                        new PointF(LarguraCintura / 2, yCintura),
                        new PointF(LarguraCintura / 2 + 3, yCintura),
                        new PointF(LarguraOmbros / 2 + 2, yCintura - 10),
                        new PointF(LarguraOmbros / 2, yOmbros));
                }
                else
                {
                    // Masculino: ombros largos, tronco maisreto
                    path.AddBezier(
                        new PointF(-LarguraOmbros / 2, yOmbros),
                        new PointF(-LarguraOmbros / 2 + 2, yCintura),
                        new PointF(-LarguraCintura / 2 - 2, yCintura),
                        new PointF(-LarguraCintura / 2, yCintura));

                    path.AddBezier(
                        new PointF(-LarguraCintura / 2, yCintura),
                        new PointF(-LarguraCintura / 2, yQuadril - 5),
                        new PointF(-LarguraQuadril / 2, yQuadril),
                        new PointF(-LarguraQuadril / 2, yQuadril));

                    path.AddLine(-LarguraQuadril / 2, yQuadril, LarguraQuadril / 2, yQuadril);

                    path.AddBezier(
                        new PointF(LarguraQuadril / 2, yQuadril),
                        new PointF(LarguraQuadril / 2, yQuadril),
                        new PointF(LarguraCintura / 2, yQuadril - 5),
                        new PointF(LarguraCintura / 2, yCintura));

                    path.AddBezier(
                        new PointF(LarguraCintura / 2, yCintura),
                        new PointF(LarguraCintura / 2 + 2, yCintura),
                        new PointF(LarguraOmbros / 2 - 2, yCintura),
                        new PointF(LarguraOmbros / 2, yOmbros));
                }

                path.CloseFigure();

                // Preencher
                if (Preenchido)
                {
                    using (var brush = new SolidBrush(AplicarOpacidade(CorRoupa, opacidade)))
                    {
                        g.FillPath(brush, path);
                    }

                    // Gradiente sutil para dar volume
                    using (var brush = new LinearGradientBrush(
                        new PointF(0, yTop),
                        new PointF(0, yQuadril),
                        AplicarOpacidade(Color.FromArgb(30, 255, 255, 255), opacidade),
                        AplicarOpacidade(Color.FromArgb(30, 0, 0, 0), opacidade)))
                    {
                        g.FillPath(brush, path);
                    }
                }

                // Contorno
                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(60, 60, 80), opacidade),
                        EspessuraContorno))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            // Detalhe: gola/pescoço
            using (var brushPele = new SolidBrush(AplicarOpacidade(CorPele, opacidade)))
            {
                float larguraPescoco = LarguraCabeca * 0.6f;
                g.FillEllipse(brushPele,
                    -larguraPescoco / 2,
                    yTop - 6,
                    larguraPescoco,
                    12);
            }
        }

        private void DesenharBraco(Graphics g, float xOmbro, float yOmbro, float angulo,
            float anguloCotovelo, float opacidade)
        {
            var state = g.Save();
            g.TranslateTransform(xOmbro, yOmbro);
            g.RotateTransform(angulo);

            float larguraBraco = 8.5f;
            float compBracoSuperior = 18f;
            float compAntebraco = 17f;
            float diametroCotovelo = larguraBraco + 2f;
            float larguraMao = 8f;
            float alturaMao = 9f;

            using (var path = CriarRetanguloArredondado(-larguraBraco / 2, 0, larguraBraco, compBracoSuperior, 3.5f))
            {
                if (Preenchido)
                {
                    using (var brush = new SolidBrush(AplicarOpacidade(CorRoupa, opacidade)))
                    {
                        g.FillPath(brush, path);
                    }
                }
                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(60, 60, 80), opacidade),
                        EspessuraContorno * 0.8f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorRoupa, 0.2f), opacidade)))
            {
                g.FillEllipse(brush,
                    -diametroCotovelo / 2,
                    compBracoSuperior - diametroCotovelo / 2,
                    diametroCotovelo,
                    diametroCotovelo);
            }
            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(50, 50, 60), opacidade), EspessuraContorno * 0.7f))
                {
                    g.DrawEllipse(pen,
                        -diametroCotovelo / 2,
                        compBracoSuperior - diametroCotovelo / 2,
                        diametroCotovelo,
                        diametroCotovelo);
                }
            }

            g.TranslateTransform(0, compBracoSuperior);
            g.RotateTransform(anguloCotovelo);

            using (var path = CriarRetanguloArredondado(-larguraBraco / 2, 0, larguraBraco, compAntebraco, 3.5f))
            {
                if (Preenchido)
                {
                    using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorRoupa, 0.08f), opacidade)))
                    {
                        g.FillPath(brush, path);
                    }
                }
                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(60, 60, 80), opacidade),
                        EspessuraContorno * 0.8f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            using (var brush = new SolidBrush(AplicarOpacidade(CorPele, opacidade)))
            {
                g.FillEllipse(brush,
                    -larguraMao / 2,
                    compAntebraco - 2,
                    larguraMao,
                    alturaMao);
            }
            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(150, 120, 100), opacidade), 0.8f))
                {
                    g.DrawEllipse(pen, -larguraMao / 2, compAntebraco - 2, larguraMao, alturaMao);
                }
            }

            g.Restore(state);
        }

        private void DesenharPerna(Graphics g, float xQuadril, float yQuadril, float angulo,
            float anguloJoelho, float opacidade)
        {
            var state = g.Save();
            g.TranslateTransform(xQuadril, yQuadril);
            g.RotateTransform(angulo);

            float compCoxa = AlturaPerna * 0.55f;
            float compCanela = AlturaPerna * 0.45f;
            float diametroJoelho = LarguraPerna + 1.5f;

            using (var path = new GraphicsPath())
            {
                float largTop = LarguraPerna;
                float largBot = LarguraPerna * 0.9f;

                path.AddLine(-largTop / 2, 0, largTop / 2, 0);
                path.AddLine(largTop / 2, 0, largBot / 2, compCoxa);
                path.AddLine(largBot / 2, compCoxa, -largBot / 2, compCoxa);
                path.AddLine(-largBot / 2, compCoxa, -largTop / 2, 0);
                path.CloseFigure();

                if (Preenchido)
                {
                    using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorRoupa, 0.15f), opacidade)))
                    {
                        g.FillPath(brush, path);
                    }
                }
                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(50, 50, 60), opacidade),
                        EspessuraContorno * 0.8f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorRoupa, 0.22f), opacidade)))
            {
                g.FillEllipse(brush,
                    -diametroJoelho / 2,
                    compCoxa - diametroJoelho / 2,
                    diametroJoelho,
                    diametroJoelho);
            }
            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(45, 45, 55), opacidade), EspessuraContorno * 0.7f))
                {
                    g.DrawEllipse(pen,
                        -diametroJoelho / 2,
                        compCoxa - diametroJoelho / 2,
                        diametroJoelho,
                        diametroJoelho);
                }
            }

            g.TranslateTransform(0, compCoxa);
            g.RotateTransform(anguloJoelho);

            using (var path = new GraphicsPath())
            {
                float largTop = LarguraPerna * 0.85f;
                float largBot = LarguraPerna * 0.75f;

                path.AddLine(-largTop / 2, 0, largTop / 2, 0);
                path.AddLine(largTop / 2, 0, largBot / 2, compCanela);
                path.AddLine(largBot / 2, compCanela, -largBot / 2, compCanela);
                path.AddLine(-largBot / 2, compCanela, -largTop / 2, 0);
                path.CloseFigure();

                if (Preenchido)
                {
                    using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorRoupa, 0.2f), opacidade)))
                    {
                        g.FillPath(brush, path);
                    }
                }
                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(50, 50, 60), opacidade),
                        EspessuraContorno * 0.8f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            g.Restore(state);
        }

        private void DesenharPe(Graphics g, float xQuadril, float yQuadril, float angulo,
            float anguloJoelho, float opacidade)
        {
            var state = g.Save();
            g.TranslateTransform(xQuadril, yQuadril);
            g.RotateTransform(angulo);

            float compCoxa = AlturaPerna * 0.55f;
            float compCanela = AlturaPerna * 0.45f;

            g.TranslateTransform(0, compCoxa);
            g.RotateTransform(anguloJoelho);
            g.TranslateTransform(0, compCanela);

            using (var brush = new SolidBrush(AplicarOpacidade(CorSapato, opacidade)))
            {
                g.FillEllipse(brush, -LarguraPe / 2, -2, LarguraPe, AlturaPe);
            }

            using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(80, 255, 255, 255), opacidade), 1f))
            {
                g.DrawArc(pen, -LarguraPe / 2 + 2, -2 + AlturaPe / 4, LarguraPe - 4, AlturaPe / 2, 0, 180);
            }

            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(30, 30, 30), opacidade),
                    EspessuraContorno * 0.7f))
                {
                    g.DrawEllipse(pen, -LarguraPe / 2, -2, LarguraPe, AlturaPe);
                }
            }

            g.Restore(state);
        }

        // Rótulo (fora da transformação)
        private void DesenharRotulo(Graphics g)
        {
            using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Near;

                string texto = Rotulo;
                if (NumeroMarcador > 0)
                    texto = $"[{NumeroMarcador}] {texto}";

                var size = g.MeasureString(texto, font);
                float yLabel = Posicao.Y + AlturaTotal / 2 + 8;

                using (var bg = new SolidBrush(Color.FromArgb(220, 30, 30, 30)))
                {
                    g.FillRectangle(bg,
                        Posicao.X - size.Width / 2 - 4,
                        yLabel - 2,
                        size.Width + 8,
                        size.Height + 4);
                }

                Color corBorda = Rotulo.ToLower().Contains("vítima") ? Color.Red :
                                 Rotulo.ToLower().Contains("suspeito") ? Color.Orange :
                                 Color.DodgerBlue;
                using (var pen = new Pen(corBorda, 2f))
                {
                    g.DrawRectangle(pen,
                        Posicao.X - size.Width / 2 - 4,
                        yLabel - 2,
                        size.Width + 8,
                        size.Height + 4);
                }

                g.DrawString(texto, font, Brushes.White, Posicao.X, yLabel, format);
            }
        }

        #endregion

        #region Utilitários

        private GraphicsPath CriarRetanguloArredondado(float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Color AplicarOpacidade(Color cor, float opacidade)
        {
            return Color.FromArgb((int)(cor.A * opacidade), cor.R, cor.G, cor.B);
        }

        private Color EscurecerCor(Color cor, float fator)
        {
            return Color.FromArgb(
                cor.A,
                Math.Max(0, (int)(cor.R * (1 - fator))),
                Math.Max(0, (int)(cor.G * (1 - fator))),
                Math.Max(0, (int)(cor.B * (1 - fator))));
        }

        #endregion

        #region BaseSketchObject

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            var bounds = GetBounds();
            bounds.Inflate(tolerancia, tolerancia);
            return bounds.Contains(ponto);
        }

        public override RectangleF GetBounds()
        {
            float largura = LarguraTotal;
            float altura = AlturaTotal;

            return new RectangleF(
                Posicao.X - largura / 2,
                Posicao.Y - altura / 2,
                largura,
                altura);
        }

        #endregion
    }
}