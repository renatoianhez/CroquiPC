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
        VistaAerea = 1,
        DeLado = 2
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

        [Category("Pose")]
        [DisplayName("De Costas")]
        [Description("Quando ativo, representa o corpo visto de costas")]
        public bool DeCostas { get; set; } = false;

        private float _escalaCorpo = 1f;

        // Proporções (em pixels)
        public float EscalaCorpo
        {
            get => _escalaCorpo;
            set => _escalaCorpo = Math.Max(0.05f, value);
        }

        // Dimensões base (serão escaladas)
        public float LarguraCabeca { get; set; } = 20f;
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
        public float AnguloBracoDireito { get; set; } = -15f;
        public float AnguloBracoEsquerdo { get; set; } = 15f;
        public float AnguloCotoveloDireito { get; set; } = 12f;
        public float AnguloCotoveloEsquerdo { get; set; } = -12f;
        public float AnguloPernaDireita { get; set; } = -5f;
        public float AnguloPernaEsquerda { get; set; } = 5f;
        public float AnguloJoelhoDireito { get; set; } = 8f;
        public float AnguloJoelhoEsquerdo { get; set; } = -8f;

        // Vista aérea
        [Category("Pose")]
        [DisplayName("Braços Estendidos")]
        [Description("Na visão aérea, mantém os braços totalmente abertos")]
        public bool BracosEstendidos { get; set; } = false;

        // Ângulo geral de rotação do corpo
        public float AnguloCorpo { get; set; } = 0f;

        [Category("Articulações")]
        [DisplayName("Ângulo da Cabeça")]
        [Description("Rotação da cabeça em torno do pescoço")]
        public float AnguloCabeca { get; set; } = 0f;

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
        public int CorTroncoArgb { get; set; } = Color.FromArgb(70, 130, 180).ToArgb();
        [Browsable(false)]
        public int CorBracoArgb { get; set; } = Color.FromArgb(70, 130, 180).ToArgb();
        [Browsable(false)]
        public int CorAntebracoArgb { get; set; } = Color.FromArgb(70, 130, 180).ToArgb();
        [Browsable(false)]
        public int CorCoxaArgb { get; set; } = Color.FromArgb(70, 130, 180).ToArgb();
        [Browsable(false)]
        public int CorCanelaArgb { get; set; } = Color.FromArgb(70, 130, 180).ToArgb();
        [Browsable(false)]
        public int CorCabeloArgb { get; set; } = Color.FromArgb(60, 40, 30).ToArgb();
        [Browsable(false)]
        public int CorSapatoArgb { get; set; } = Color.FromArgb(40, 40, 40).ToArgb();

        // Visualização
        public bool MostrarContorno { get; set; } = true;
        public bool Preenchido { get; set; } = true;

        [Category("Camadas")]
        [DisplayName("Antebraço Direito à Frente da Cabeça")]
        [Description("Quando ativo, o antebraço direito é desenhado à frente da cabeça")]
        public bool AntebracoDireitoFrenteCabeca { get; set; } = false;

        [Category("Camadas")]
        [DisplayName("Antebraço Esquerdo à Frente da Cabeça")]
        [Description("Quando ativo, o antebraço esquerdo é desenhado à frente da cabeça")]
        public bool AntebracoEsquerdoFrenteCabeca { get; set; } = false;

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
        [Category("Aparência"), DisplayName("Cor da Roupa (Base)")]
        public Color CorRoupa
        {
            get => Color.FromArgb(CorRoupaArgb);
            set
            {
                int argb = value.ToArgb();
                CorRoupaArgb = argb;
                CorTroncoArgb = argb;
                CorBracoArgb = argb;
                CorAntebracoArgb = argb;
                CorCoxaArgb = argb;
                CorCanelaArgb = argb;
            }
        }

        [JsonIgnore]
        [Category("Aparência - Vestimenta"), DisplayName("Cor do Tronco")]
        public Color CorTronco
        {
            get => Color.FromArgb(CorTroncoArgb);
            set => CorTroncoArgb = value.ToArgb();
        }

        [JsonIgnore]
        [Category("Aparência - Vestimenta"), DisplayName("Cor dos Braços")]
        public Color CorBraco
        {
            get => Color.FromArgb(CorBracoArgb);
            set => CorBracoArgb = value.ToArgb();
        }

        [JsonIgnore]
        [Category("Aparência - Vestimenta"), DisplayName("Cor dos Antebraços")]
        public Color CorAntebraco
        {
            get => Color.FromArgb(CorAntebracoArgb);
            set => CorAntebracoArgb = value.ToArgb();
        }

        [JsonIgnore]
        [Category("Aparência - Vestimenta"), DisplayName("Cor das Coxas")]
        public Color CorCoxa
        {
            get => Color.FromArgb(CorCoxaArgb);
            set => CorCoxaArgb = value.ToArgb();
        }

        [JsonIgnore]
        [Category("Aparência - Vestimenta"), DisplayName("Cor das Canelas")]
        public Color CorCanela
        {
            get => Color.FromArgb(CorCanelaArgb);
            set => CorCanelaArgb = value.ToArgb();
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

                if (Pose == PoseCorpo.DeLado)
                    return (AlturaCabeca + AlturaTronco + AlturaPerna + AlturaPe * 0.65f) * EscalaCorpo;

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

                if (Pose == PoseCorpo.DeLado)
                {
                    float larguraPerfilCabeca = LarguraCabeca * 0.72f;
                    float larguraPerfilTronco = LarguraCintura * 0.55f;
                    float larguraPerfilBraco = 14f;
                    return Math.Max(Math.Max(larguraPerfilCabeca, larguraPerfilTronco), larguraPerfilBraco) * EscalaCorpo * 1.5f;
                }

                float largMax = Math.Max(LarguraOmbros, LarguraQuadril);
                return largMax * EscalaCorpo * 1.3f;
            }
        }

        #endregion

        public StickFigure()
        {
            Tipo = "Corpo";
            EspessuraContorno = 1.5f;
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
                LarguraOmbros = 30f;
                LarguraCintura = 22f;
                LarguraQuadril = 32f;
                AlturaTronco = 45f;
                LarguraCabeca = 19f;
                AlturaCabeca = 26f;
                LarguraPerna = 11f;
                AlturaPerna = 43f;
            }
            else // Masculino
            {
                LarguraOmbros = 42f;
                LarguraCintura = 34f;
                LarguraQuadril = 34f;
                AlturaTronco = 52f;
                LarguraCabeca = 20f;
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
                    AnguloCabeca = 0f;
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
                    AnguloCabeca = 0f;
                    BracosEstendidos = true;
                    break;

                case "delado":
                case "de lado":
                case "lado":
                case "lateral":
                    Pose = PoseCorpo.DeLado;
                    AnguloBracoDireito = 18f;
                    AnguloBracoEsquerdo = 10f;
                    AnguloCotoveloDireito = 16f;
                    AnguloCotoveloEsquerdo = 10f;
                    AnguloPernaDireita = 2f;
                    AnguloPernaEsquerda = -4f;
                    AnguloJoelhoDireito = 10f;
                    AnguloJoelhoEsquerdo = 12f;
                    AnguloCabeca = 2f;
                    BracosEstendidos = false;
                    break;
            }
        }

        #endregion

        #region Desenho Principal

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            GarantirParametrosValidos();

            var state = g.Save();

            // Posicionar no centro do corpo
            g.TranslateTransform(Posicao.X, Posicao.Y);
            g.RotateTransform(Rotacao + AnguloCorpo);
            g.ScaleTransform(EscalaCorpo * EscalaX, EscalaCorpo * EscalaY);

            // Aplicar opacidade se necessário
            float opacidade = Opacidade;

            if (Pose == PoseCorpo.VistaAerea)
                DesenharCorpoVistaAerea(g, opacidade);
            else if (Pose == PoseCorpo.DeLado)
                DesenharCorpoDeLado(g, opacidade);
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
            float yPescoco = yTroncoTop - 2f;
            float yQuadril = yTroncoBottom;

            DesenharPe(g, LarguraQuadril / 4, yQuadril, AnguloPernaDireita, AnguloJoelhoDireito, opacidade);
            DesenharPe(g, -LarguraQuadril / 4, yQuadril, AnguloPernaEsquerda, AnguloJoelhoEsquerdo, opacidade);

            DesenharPerna(g, LarguraQuadril / 4, yQuadril, AnguloPernaDireita, AnguloJoelhoDireito, opacidade);
            DesenharPerna(g, -LarguraQuadril / 4, yQuadril, AnguloPernaEsquerda, AnguloJoelhoEsquerdo, opacidade);

            DesenharTronco(g, yTroncoTop, opacidade);

            float xOmbroDir = LarguraOmbros / 2;
            float xOmbroEsq = -LarguraOmbros / 2;
            float yOmbro = yTroncoTop + 8;

            DesenharBracoSuperiorECotovelo(g, xOmbroDir, yOmbro, AnguloBracoDireito, opacidade);
            DesenharBracoSuperiorECotovelo(g, xOmbroEsq, yOmbro, AnguloBracoEsquerdo, opacidade);

            if (!AntebracoDireitoFrenteCabeca)
                DesenharAntebracoEMao(g, xOmbroDir, yOmbro, AnguloBracoDireito, AnguloCotoveloDireito, opacidade);

            if (!AntebracoEsquerdoFrenteCabeca)
                DesenharAntebracoEMao(g, xOmbroEsq, yOmbro, AnguloBracoEsquerdo, AnguloCotoveloEsquerdo, opacidade);

            DesenharCabecaComRotacao(g, 0, yCabecaCenter, 0, yPescoco, AnguloCabeca, opacidade);

            if (AntebracoDireitoFrenteCabeca)
                DesenharAntebracoEMao(g, xOmbroDir, yOmbro, AnguloBracoDireito, AnguloCotoveloDireito, opacidade);

            if (AntebracoEsquerdoFrenteCabeca)
                DesenharAntebracoEMao(g, xOmbroEsq, yOmbro, AnguloBracoEsquerdo, AnguloCotoveloEsquerdo, opacidade);
        }

        private void DesenharCorpoDeLado(Graphics g, float opacidade)
        {
            float alturaTotal = AlturaCabeca + AlturaTronco + AlturaPerna + AlturaPe * 0.65f;
            float yTopo = -alturaTotal / 2f;

            float yCabeca = yTopo + AlturaCabeca * 0.5f;
            float yTroncoTop = yTopo + AlturaCabeca - 2f;
            float yQuadril = yTroncoTop + AlturaTronco;

            float larguraTronco = Math.Max(10f, LarguraCintura * 0.5f);
            float larguraQuadril = Math.Max(9f, LarguraQuadril * 0.45f);
            float larguraCabeca = Math.Max(8f, LarguraCabeca * 0.68f);
            float alturaCabeca = AlturaCabeca;

            float compCoxa = AlturaPerna * 0.55f;
            float compCanela = AlturaPerna * 0.45f;

            float xPernaTras = -1.5f;
            float xPernaFrente = 1.5f;

            void DesenharPernaPerfil(float xBase, float anguloPerna, float anguloJoelho, Color corBase, float fator)
            {
                var st = g.Save();
                g.TranslateTransform(xBase, yQuadril);
                g.RotateTransform(anguloPerna);

                float largCoxa = LarguraPerna * 0.78f * fator;
                float largCanela = LarguraPerna * 0.68f * fator;

                using (var coxa = CriarRetanguloArredondado(-largCoxa / 2f, 0, largCoxa, compCoxa, 2.2f))
                {
                    if (Preenchido)
                    {
                        using (var brush = new SolidBrush(AplicarOpacidade(corBase, opacidade)))
                            g.FillPath(brush, coxa);
                    }
                    if (MostrarContorno)
                    {
                        using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(50, 50, 60), opacidade), EspessuraContorno * 0.7f))
                            g.DrawPath(pen, coxa);
                    }
                }

                g.TranslateTransform(0, compCoxa);
                g.RotateTransform(anguloJoelho);

                using (var canela = CriarRetanguloArredondado(-largCanela / 2f, 0, largCanela, compCanela, 2f))
                {
                    if (Preenchido)
                    {
                        using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(corBase, 0.12f), opacidade)))
                            g.FillPath(brush, canela);
                    }
                    if (MostrarContorno)
                    {
                        using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(50, 50, 60), opacidade), EspessuraContorno * 0.68f))
                            g.DrawPath(pen, canela);
                    }
                }

                float larguraPe = LarguraPe * 1.6f * fator;
                float alturaPe = AlturaPe * 0.52f * fator;
                using (var brush = new SolidBrush(AplicarOpacidade(CorSapato, opacidade)))
                {
                    g.FillEllipse(brush, -largCanela * 0.45f, compCanela - alturaPe * 0.2f, larguraPe, alturaPe);
                }

                g.Restore(st);
            }

            void DesenharBracoPerfil(float xOmbro, float yOmbro, float angulo, float anguloCotovelo, Color corBase, float fator)
            {
                var st = g.Save();
                g.TranslateTransform(xOmbro, yOmbro);
                g.RotateTransform(angulo);

                float larg = 7.0f * fator;
                float compSup = 16f * fator;
                float compAnte = 14f * fator;

                using (var sup = CriarRetanguloArredondado(-larg / 2f, 0, larg, compSup, 2.2f))
                {
                    if (Preenchido)
                    {
                        using (var brush = new SolidBrush(AplicarOpacidade(corBase, opacidade)))
                            g.FillPath(brush, sup);
                    }
                    if (MostrarContorno)
                    {
                        using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(55, 55, 70), opacidade), EspessuraContorno * 0.7f))
                            g.DrawPath(pen, sup);
                    }
                }

                g.TranslateTransform(0, compSup);
                g.RotateTransform(anguloCotovelo);

                using (var ante = CriarRetanguloArredondado(-larg / 2f, 0, larg, compAnte, 2.2f))
                {
                    if (Preenchido)
                    {
                        using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(corBase, 0.08f), opacidade)))
                            g.FillPath(brush, ante);
                    }
                    if (MostrarContorno)
                    {
                        using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(55, 55, 70), opacidade), EspessuraContorno * 0.68f))
                            g.DrawPath(pen, ante);
                    }
                }

                using (var brush = new SolidBrush(AplicarOpacidade(CorPele, opacidade)))
                    g.FillEllipse(brush, -3f * fator, compAnte - 2f, 6f * fator, 7f * fator);

                g.Restore(st);
            }

            DesenharPernaPerfil(xPernaTras, AnguloPernaEsquerda, AnguloJoelhoEsquerdo, EscurecerCor(CorCanela, 0.18f), 0.92f);

            float xOmbro = larguraTronco * 0.42f;
            float yOmbro = yTroncoTop + 9f;

            DesenharBracoPerfil(xOmbro - 1.4f, yOmbro + 1f, AnguloBracoEsquerdo, AnguloCotoveloEsquerdo, EscurecerCor(CorBraco, 0.14f), 0.92f);

            using (var tronco = CriarRetanguloArredondado(-larguraTronco / 2f, yTroncoTop, larguraTronco, AlturaTronco, 4.5f))
            {
                if (Preenchido)
                {
                    using (var brush = new SolidBrush(AplicarOpacidade(CorTronco, opacidade)))
                        g.FillPath(brush, tronco);
                }
                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(60, 60, 80), opacidade), EspessuraContorno))
                        g.DrawPath(pen, tronco);
                }
            }

            using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorTronco, 0.12f), opacidade)))
                g.FillEllipse(brush, -larguraQuadril / 2f, yQuadril - 4f, larguraQuadril, 8f);

            DesenharBracoPerfil(xOmbro, yOmbro, AnguloBracoDireito, AnguloCotoveloDireito, CorBraco, 1f);

            DesenharPernaPerfil(xPernaFrente, AnguloPernaDireita, AnguloJoelhoDireito, CorCanela, 1f);

            var head = g.Save();
            g.TranslateTransform(0, yCabeca);
            g.RotateTransform(AnguloCabeca);

            if (Preenchido)
            {
                using (var face = new SolidBrush(AplicarOpacidade(CorPele, opacidade)))
                    g.FillEllipse(face, -larguraCabeca / 2f, -alturaCabeca / 2f + 1.2f, larguraCabeca, alturaCabeca - 2.2f);

                if (Genero == GeneroCorpo.Feminino)
                {
                    using (var hair = new SolidBrush(AplicarOpacidade(CorCabelo, opacidade)))
                    using (var path = new GraphicsPath())
                    {
                        path.AddPolygon(new[]
                        {
                            new PointF(-larguraCabeca / 1.8f - 0.4f, -alturaCabeca / 2f + 2.4f), // Ponto inicial (x,y) da porção superior do trapézio do cabelo
                            new PointF(larguraCabeca / 2f - 2.4f, -alturaCabeca / 2f + 3.2f), // Ponto final (x,y) da porção superior do trapézio do cabelo
                            new PointF(larguraCabeca / 2f - 3.8f, alturaCabeca / 2.8f + 6.8f), // Ponto inicial (x,y) da porção inferior do trapézio do cabelo
                            new PointF(-larguraCabeca / 2f - 5.2f, alturaCabeca / 2.8f + 7.6f) // Ponto final (x,y) da porção inferior do trapézio do cabelo
                        });
                        g.FillPath(hair, path);
                        g.FillEllipse(hair,
                            -larguraCabeca / 2f - 1.8f, // Posição horizontal da elipse do cabelo
                            -alturaCabeca / 2f - 1.4f, // Posição vertical da elipse do cabelo
                            larguraCabeca + 1.8f, // Largura da elipse do cabelo
                            alturaCabeca * 0.6f); // Altura da elipse do cabelo para criar um efeito de cabelo mais cheio e volumoso
                    }
                }
                else
                {
                    using (var hair = new SolidBrush(AplicarOpacidade(CorCabelo, opacidade)))
                    {
                        g.FillPie(hair,
                            -larguraCabeca / 2f - 0.2f,
                            -alturaCabeca / 2f - 1.2f,
                            larguraCabeca * 0.96f,
                            alturaCabeca * 0.64f,
                            180f,
                            180f);

                        var hairState = g.Save();
                        g.TranslateTransform(-larguraCabeca * 0.02f, -alturaCabeca * 0.01f);
                        g.RotateTransform(-86f);
                        g.FillPie(hair,
                            -larguraCabeca * 0.43f, // Posição horizontal da elipse da parte de trás do cabelo
                            -alturaCabeca * 0.30f, // Posição vertical da elipse da parte de trás do cabelo
                            larguraCabeca * 1.4f, // Altura da elipse da parte de trás do cabelo
                            alturaCabeca * 0.38f, // Largura da elipse da parte de trás da cabeça
                            180f, // Início do arco na parte inferior
                            180f); // Arco completo para criar um efeito de cabelo penteado para o lado
                        g.Restore(hairState);
                    }
                }
            }

            using (var skin = new SolidBrush(AplicarOpacidade(CorPele, opacidade)))
            {
                var nariz = new[]
                {
                    new PointF(larguraCabeca / 2f - 0.5f, -2f),
                    new PointF(larguraCabeca / 2f + 3.2f, 0f),
                    new PointF(larguraCabeca / 2f - 0.5f, 2f)
                };
                g.FillPolygon(skin, nariz);
            }

            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(80, 60, 50), opacidade), EspessuraContorno * 0.85f))
                {
                    g.DrawEllipse(pen, -larguraCabeca / 2f, -alturaCabeca / 2f, larguraCabeca, alturaCabeca);
                    g.DrawLine(pen, larguraCabeca / 2f - 0.5f, -2f, larguraCabeca / 2f + 3.2f, 0f);
                    g.DrawLine(pen, larguraCabeca / 2f + 3.2f, 0f, larguraCabeca / 2f - 0.5f, 2f);
                }
            }

            g.Restore(head);
        }

        private void DesenharCorpoVistaAerea(Graphics g, float opacidade)
        {
            float yOmbros = 0f;
            float larguraOmbrosTopo = LarguraOmbros * 1.05f;
            float alturaOmbrosTopo = AlturaTronco * 0.35f;

            if (Preenchido)
            {
                using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorTronco, 0.1f), opacidade)))
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
            float yPescoco = yCabeca + AlturaCabeca * 0.35f;

            float anguloDir = BracosEstendidos ? 90f : AnguloBracoDireito;
            float anguloEsq = BracosEstendidos ? -90f : AnguloBracoEsquerdo;
            float cotoveloDir = BracosEstendidos ? 0f : AnguloCotoveloDireito;
            float cotoveloEsq = BracosEstendidos ? 0f : AnguloCotoveloEsquerdo;

            float xOmbroDir = larguraOmbrosTopo / 2 - 2;
            float xOmbroEsq = -larguraOmbrosTopo / 2 + 2;
            float yOmbro = yOmbros - 3;

            DesenharBracoSuperiorECotovelo(g, xOmbroDir, yOmbro, anguloDir, opacidade);
            DesenharBracoSuperiorECotovelo(g, xOmbroEsq, yOmbro, anguloEsq, opacidade);

            if (!AntebracoDireitoFrenteCabeca)
                DesenharAntebracoEMao(g, xOmbroDir, yOmbro, anguloDir, cotoveloDir, opacidade);

            if (!AntebracoEsquerdoFrenteCabeca)
                DesenharAntebracoEMao(g, xOmbroEsq, yOmbro, anguloEsq, cotoveloEsq, opacidade);

            DesenharTopoCabecaComRotacao(g, 0, yCabeca, 0, yPescoco, AnguloCabeca, opacidade);

            if (AntebracoDireitoFrenteCabeca)
                DesenharAntebracoEMao(g, xOmbroDir, yOmbro, anguloDir, cotoveloDir, opacidade);

            if (AntebracoEsquerdoFrenteCabeca)
                DesenharAntebracoEMao(g, xOmbroEsq, yOmbro, anguloEsq, cotoveloEsq, opacidade);
        }

        #endregion

        #region Partes do Corpo

        private void DesenharCabeca(Graphics g, float cx, float cy, float opacidade)
        {
            DesenharCabeloFemininoTrapazio(g, cx, cy, opacidade);

            if (DeCostas)
            {
                using (var brushCabelo = new SolidBrush(AplicarOpacidade(CorCabelo, opacidade)))
                {
                    float cabeloExtra = 3f;
                    g.FillEllipse(brushCabelo,
                        cx - LarguraCabeca / 2 - cabeloExtra / 2,
                        cy - AlturaCabeca / 2 - cabeloExtra / 2,
                        LarguraCabeca + cabeloExtra,
                        AlturaCabeca + cabeloExtra);
                }

                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(70, 50, 40), opacidade), EspessuraContorno))
                    {
                        g.DrawEllipse(pen,
                            cx - LarguraCabeca / 2,
                            cy - AlturaCabeca / 2,
                            LarguraCabeca,
                            AlturaCabeca);
                    }
                }

                return;
            }

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
            DesenharCabeloFemininoTrapazio(g, cx, cy, opacidade);

            if (DeCostas)
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

                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(70, 50, 40), opacidade), EspessuraContorno))
                    {
                        float diametro = Math.Max(LarguraCabeca, AlturaCabeca) + 1f;
                        g.DrawEllipse(pen,
                            cx - diametro / 2,
                            cy - diametro / 2,
                            diametro,
                            diametro);
                    }
                }

                return;
            }

            using (var brushCabelo = new SolidBrush(AplicarOpacidade(CorCabelo, opacidade)))
            {
                float diametro = Math.Max(LarguraCabeca, AlturaCabeca) + 3f;
                g.FillEllipse(brushCabelo,
                    cx - diametro / 2,
                    cy - diametro / 2,
                    diametro,
                    diametro);
            }

            if (MostrarContorno)
            {
                using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(80, 60, 50), opacidade), EspessuraContorno))
                {
                    float diametro = Math.Max(LarguraCabeca, AlturaCabeca) + 1f;
                    g.DrawEllipse(pen,
                        cx - diametro / 2,
                        cy - diametro / 2,
                        diametro,
                        diametro);
                }
            }
        }

        private void DesenharCabecaComRotacao(Graphics g, float cx, float cy, float px, float py, float angulo, float opacidade)
        {
            var state = g.Save();
            g.TranslateTransform(px, py);
            g.RotateTransform(angulo);
            g.TranslateTransform(-px, -py);
            DesenharCabeca(g, cx, cy, opacidade);
            g.Restore(state);
        }

        private void DesenharTopoCabecaComRotacao(Graphics g, float cx, float cy, float px, float py, float angulo, float opacidade)
        {
            var state = g.Save();
            g.TranslateTransform(px, py);
            g.RotateTransform(angulo);
            g.TranslateTransform(-px, -py);
            DesenharTopoCabeca(g, cx, cy, opacidade);
            g.Restore(state);
        }

        private void DesenharCabeloFemininoTrapazio(Graphics g, float cx, float cy, float opacidade)
        {
            if (Genero != GeneroCorpo.Feminino)
                return;

            float yTopo = cy - AlturaCabeca / 2f + 2f;
            float altura = AlturaCabeca * 0.95f;
            float larguraTopo = LarguraCabeca * 0.56f;
            float larguraBase = LarguraCabeca * 1.18f;

            var p1 = new PointF(cx - larguraTopo / 2f, yTopo);
            var p2 = new PointF(cx + larguraTopo / 2f, yTopo);
            var p3 = new PointF(cx + larguraBase / 2f, yTopo + altura);
            var p4 = new PointF(cx - larguraBase / 2f, yTopo + altura);

            using (var path = new GraphicsPath())
            {
                path.AddPolygon(new[] { p1, p2, p3, p4 });

                using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorCabelo, 0.06f), opacidade)))
                {
                    g.FillPath(brush, path);
                }

                if (MostrarContorno)
                {
                    using (var pen = new Pen(AplicarOpacidade(Color.FromArgb(70, 50, 40), opacidade), EspessuraContorno * 0.8f))
                    {
                        g.DrawPath(pen, path);
                    }
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
                    using (var brush = new SolidBrush(AplicarOpacidade(CorTronco, opacidade)))
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
            DesenharBracoSuperiorECotovelo(g, xOmbro, yOmbro, angulo, opacidade);
            DesenharAntebracoEMao(g, xOmbro, yOmbro, angulo, anguloCotovelo, opacidade);
        }

        private void DesenharBracoSuperiorECotovelo(Graphics g, float xOmbro, float yOmbro, float angulo,
            float opacidade)
        {
            var state = g.Save();
            g.TranslateTransform(xOmbro, yOmbro);
            g.RotateTransform(angulo);

            float larguraBraco = 8.5f;
            float compBracoSuperior = 18f;
            float diametroCotovelo = larguraBraco + 2f;

            using (var path = CriarRetanguloArredondado(-larguraBraco / 2, 0, larguraBraco, compBracoSuperior, 3.5f))
            {
                if (Preenchido)
                {
                    using (var brush = new SolidBrush(AplicarOpacidade(CorBraco, opacidade)))
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

            using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorBraco, 0.2f), opacidade)))
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

            g.Restore(state);
        }

        private void DesenharAntebracoEMao(Graphics g, float xOmbro, float yOmbro, float angulo,
            float anguloCotovelo, float opacidade)
        {
            var state = g.Save();
            g.TranslateTransform(xOmbro, yOmbro);
            g.RotateTransform(angulo);

            float larguraBraco = 8.5f;
            float compBracoSuperior = 18f;
            float compAntebraco = 17f;
            float larguraMao = 8f;
            float alturaMao = 9f;

            g.TranslateTransform(0, compBracoSuperior);
            g.RotateTransform(anguloCotovelo);

            using (var path = CriarRetanguloArredondado(-larguraBraco / 2, 0, larguraBraco, compAntebraco, 3.5f))
            {
                if (Preenchido)
                {
                    using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorAntebraco, 0.08f), opacidade)))
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
                    using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorCoxa, 0.15f), opacidade)))
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

            using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorCoxa, 0.22f), opacidade)))
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
                    using (var brush = new SolidBrush(AplicarOpacidade(EscurecerCor(CorCanela, 0.2f), opacidade)))
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
            GarantirParametrosValidos();

            float baseLargura = LarguraTotal / Math.Max(0.0001f, EscalaCorpo);
            float baseAltura = AlturaTotal / Math.Max(0.0001f, EscalaCorpo);

            float sx = EscalaCorpo * EscalaX;
            float sy = EscalaCorpo * EscalaY;
            if (Math.Abs(sx) < 0.0001f || Math.Abs(sy) < 0.0001f)
                return false;

            float angulo = Rotacao + AnguloCorpo;

            float dx = ponto.X - Posicao.X;
            float dy = ponto.Y - Posicao.Y;

            double rad = -angulo * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float xr = dx * cos - dy * sin;
            float yr = dx * sin + dy * cos;

            float lx = xr / sx;
            float ly = yr / sy;

            var local = new RectangleF(-baseLargura / 2f, -baseAltura / 2f, baseLargura, baseAltura);
            local.Inflate(tolerancia, tolerancia);
            return local.Contains(lx, ly);
        }

        public override RectangleF GetBounds()
        {
            GarantirParametrosValidos();

            float baseLargura = LarguraTotal / Math.Max(0.0001f, EscalaCorpo);
            float baseAltura = AlturaTotal / Math.Max(0.0001f, EscalaCorpo);

            float sx = EscalaCorpo * EscalaX;
            float sy = EscalaCorpo * EscalaY;
            float angulo = Rotacao + AnguloCorpo;

            double rad = angulo * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            PointF[] local = new[]
            {
                new PointF(-baseLargura / 2f, -baseAltura / 2f),
                new PointF(baseLargura / 2f, -baseAltura / 2f),
                new PointF(baseLargura / 2f, baseAltura / 2f),
                new PointF(-baseLargura / 2f, baseAltura / 2f)
            };

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var p in local)
            {
                float x = p.X * sx;
                float y = p.Y * sy;

                float wx = Posicao.X + (x * cos - y * sin);
                float wy = Posicao.Y + (x * sin + y * cos);

                minX = Math.Min(minX, wx);
                minY = Math.Min(minY, wy);
                maxX = Math.Max(maxX, wx);
                maxY = Math.Max(maxY, wy);
            }

            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }

        private void GarantirParametrosValidos()
        {
            LarguraCabeca = Math.Max(4f, LarguraCabeca);
            AlturaCabeca = Math.Max(4f, AlturaCabeca);
            LarguraOmbros = Math.Max(4f, LarguraOmbros);
            LarguraCintura = Math.Max(4f, LarguraCintura);
            LarguraQuadril = Math.Max(4f, LarguraQuadril);
            AlturaTronco = Math.Max(4f, AlturaTronco);
            LarguraPerna = Math.Max(2f, LarguraPerna);
            AlturaPerna = Math.Max(2f, AlturaPerna);
            LarguraPe = Math.Max(2f, LarguraPe);
            AlturaPe = Math.Max(2f, AlturaPe);
            EscalaCorpo = Math.Max(0.05f, EscalaCorpo);
            EspessuraContorno = Math.Max(0.2f, EspessuraContorno);
        }

        #endregion
    }
}

























