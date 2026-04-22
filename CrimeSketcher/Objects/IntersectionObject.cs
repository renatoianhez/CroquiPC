// Objects/IntersectionObject.cs
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
    /// Cruzamento de ruas (Cruz ou T)
    /// </summary>
    [Serializable]
    public class IntersectionObject : BaseSketchObject
    {
        private const float EXTENSAO_BRACOS_ADICIONAL = 20f;
        private float _larguraRua = 80f;
        private bool _temCanteiroCentral = false;
        private float _larguraCanteiroCentral = 12f;
        private bool _estacionamentoRuaNorte = true;
        private bool _estacionamentoRuaSul = true;
        private bool _estacionamentoRuaLeste = true;
        private bool _estacionamentoRuaOeste = true;
        private bool _temFaixaEstacionamento = false;
        private float _larguraFaixaEstacionamento = 30f;

        [Category("Configuração")]
        [DisplayName("Tipo de Cruzamento")]
        [Description("Tipo de interseção: Cruz (4 vias) ou T (3 vias)")]
        public TipoCruzamento TipoCruzamento { get; set; } = TipoCruzamento.Cruz;

        [Category("Dimensões")]
        [DisplayName("Largura da Rua (m)")]
        [Description("Largura total das vias no cruzamento em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraRua
        {
            get => _larguraRua;
            set => _larguraRua = Math.Max(10f, value);
        }

        [Category("Canteiro Central")]
        [DisplayName("Possui Canteiro Central")]
        [Description("Desenha canteiro central nos braços do cruzamento")]
        public bool TemCanteiroCentral
        {
            get => _temCanteiroCentral;
            set
            {
                float larguraUtil = ObterLarguraUtilPista();
                _temCanteiroCentral = value;
                _larguraRua = Math.Max(10f, larguraUtil + (_temCanteiroCentral ? _larguraCanteiroCentral : 0f));
            }
        }

        [Category("Canteiro Central")]
        [DisplayName("Largura do Canteiro (m)")]
        [Description("Largura do canteiro central em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraCanteiroCentral
        {
            get => _larguraCanteiroCentral;
            set
            {
                float larguraUtil = ObterLarguraUtilPista();
                _larguraCanteiroCentral = Math.Max(2f, value);
                if (_temCanteiroCentral)
                    _larguraRua = Math.Max(10f, larguraUtil + _larguraCanteiroCentral);
            }
        }

        [Category("Canteiro Central")]
        [DisplayName("Canteiro na Rua Norte")]
        [Description("Aplica canteiro no acesso norte")]
        public bool CanteiroRuaNorte { get; set; } = true;

        [Category("Canteiro Central")]
        [DisplayName("Canteiro na Rua Leste")]
        [Description("Aplica canteiro no acesso leste")]
        public bool CanteiroRuaLeste { get; set; } = true;

        [Category("Canteiro Central")]
        [DisplayName("Canteiro na Rua Sul")]
        [Description("Aplica canteiro no acesso sul")]
        public bool CanteiroRuaSul { get; set; } = true;

        [Category("Canteiro Central")]
        [DisplayName("Canteiro na Rua Oeste")]
        [Description("Aplica canteiro no acesso oeste")]
        public bool CanteiroRuaOeste { get; set; } = true;

        [Category("Calçada")]
        [DisplayName("Largura da Calçada (m)")]
        [Description("Largura das calçadas no cruzamento em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
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
        [DisplayName("Largura da Faixa (m)")]
        [Description("Largura das faixas de pedestres em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraFaixaPedestre { get; set; } = 25f;

        [Category("Estacionamento")]
        [DisplayName("Possui Faixa de Estacionamento")]
        [Description("Preenche as bordas de cada acesso com cor de calçada, representando a faixa de estacionamento")]
        public bool TemFaixaEstacionamento
        {
            get => _temFaixaEstacionamento;
            set => _temFaixaEstacionamento = value;
        }

        [Category("Estacionamento")]
        [DisplayName("Largura da Faixa (m)")]
        [Description("Largura de cada faixa de estacionamento lateral em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraFaixaEstacionamento
        {
            get => _larguraFaixaEstacionamento;
            set => _larguraFaixaEstacionamento = Math.Max(5f, value);
        }

        [Category("Estacionamento")]
        [DisplayName("Estacionamento Rua Norte")]
        [Description("Ativa faixa de estacionamento no acesso norte (sincronizado com Sul)")]
        public bool EstacionamentoRuaNorte
        {
            get => _estacionamentoRuaNorte;
            set
            {
                _estacionamentoRuaNorte = value;
                _estacionamentoRuaSul = value;
            }
        }

        [Category("Estacionamento")]
        [DisplayName("Estacionamento Rua Sul")]
        [Description("Ativa faixa de estacionamento no acesso sul (sincronizado com Norte)")]
        public bool EstacionamentoRuaSul
        {
            get => _estacionamentoRuaSul;
            set
            {
                _estacionamentoRuaSul = value;
                _estacionamentoRuaNorte = value;
            }
        }

        [Category("Estacionamento")]
        [DisplayName("Estacionamento Rua Leste")]
        [Description("Ativa faixa de estacionamento no acesso leste (sincronizado com Oeste)")]
        public bool EstacionamentoRuaLeste
        {
            get => _estacionamentoRuaLeste;
            set
            {
                _estacionamentoRuaLeste = value;
                _estacionamentoRuaOeste = value;
            }
        }

        [Category("Estacionamento")]
        [DisplayName("Estacionamento Rua Oeste")]
        [Description("Ativa faixa de estacionamento no acesso oeste (sincronizado com Leste)")]
        public bool EstacionamentoRuaOeste
        {
            get => _estacionamentoRuaOeste;
            set
            {
                _estacionamentoRuaOeste = value;
                _estacionamentoRuaLeste = value;
            }
        }

        [Category("Dimensões")]
        [DisplayName("Extensão das Vias (m)")]
        [Description("Comprimento do trecho de rua adicional em cada acesso em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float ExtensaoVias { get; set; } = 40f;

        [Browsable(false)]
        public int CorAsfaltoArgb { get; set; } = Color.FromArgb(180, 180, 180).ToArgb();

        [Browsable(false)]
        public int CorCalcadaArgb { get; set; } = Color.FromArgb(210, 210, 200).ToArgb();

        [Browsable(false)]
        public int CorFaixaArgb { get; set; } = Color.White.ToArgb();

        [Browsable(false)]
        public int CorCanteiroArgb { get; set; } = Color.FromArgb(120, 160, 90).ToArgb();

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

        [Category("Aparência")]
        [DisplayName("Cor do Canteiro Central")]
        [Description("Cor de preenchimento do canteiro central")]
        [JsonIgnore]
        public Color CorCanteiro
        {
            get => Color.FromArgb(CorCanteiroArgb);
            set => CorCanteiroArgb = value.ToArgb();
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
        [DisplayName("PARE na Rua Sul")]
        [Description("Desenha PARE no acesso da rua sul")]
        public bool PareRuaSul { get; set; } = true;

        [Category("Sinalização")]
        [DisplayName("PARE na Rua Leste")]
        [Description("Desenha PARE no acesso da rua leste")]
        public bool PareRuaLeste { get; set; } = true;

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

            DesenharCruzamento(g, meiaRua, meiaTamanho);

            g.Restore(state);

            if (Selecionado) DesenharSelecao(g);
        }

        private void DesenharCruzamento(Graphics g, float meiaRua, float meiaTamanho)
        {
            bool temCima = TipoCruzamento != TipoCruzamento.TParaBaixo;
            bool temBaixo = TipoCruzamento != TipoCruzamento.TParaCima;
            bool temEsquerda = TipoCruzamento != TipoCruzamento.TParaDireita;
            bool temDireita = TipoCruzamento != TipoCruzamento.TParaEsquerda;
            float ext = Math.Max(0f, ExtensaoVias) + EXTENSAO_BRACOS_ADICIONAL;

            float extraX = ObterExtraEstacionamentoEixoX();
            float extraY = ObterExtraEstacionamentoEixoY();
            float meiaRuaCalcX = meiaRua + extraX;
            float meiaRuaCalcY = meiaRua + extraY;
            float meiaTamanhoX = meiaTamanho + extraX;
            float meiaTamanhoY = meiaTamanho + extraY;

            if (TemCalcada)
            {
                using (var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb)))
                {
                    float calc = LarguraCalcada;

                    if (!temCima)
                        g.FillRectangle(brush, -meiaTamanhoX, -meiaTamanhoY, meiaTamanhoX * 2, calc);
                    else
                    {
                        g.FillRectangle(brush, -meiaTamanhoX, -meiaTamanhoY, calc, calc);
                        g.FillRectangle(brush, meiaRuaCalcX, -meiaTamanhoY, calc, calc);
                    }

                    if (!temBaixo)
                        g.FillRectangle(brush, -meiaTamanhoX, meiaRuaCalcY, meiaTamanhoX * 2, calc);
                    else
                    {
                        g.FillRectangle(brush, -meiaTamanhoX, meiaRuaCalcY, calc, calc);
                        g.FillRectangle(brush, meiaRuaCalcX, meiaRuaCalcY, calc, calc);
                    }

                    if (!temEsquerda)
                        g.FillRectangle(brush, -meiaTamanhoX, -meiaTamanhoY, calc, meiaTamanhoY * 2);

                    if (!temDireita)
                        g.FillRectangle(brush, meiaRuaCalcX, -meiaTamanhoY, calc, meiaTamanhoY * 2);

                    if (temEsquerda)
                    {
                        g.FillRectangle(brush, -meiaTamanhoX - ext, -meiaRuaCalcY - calc, ext, calc);
                        g.FillRectangle(brush, -meiaTamanhoX - ext, meiaRuaCalcY, ext, calc);
                    }
                    if (temDireita)
                    {
                        g.FillRectangle(brush, meiaTamanhoX, -meiaRuaCalcY - calc, ext, calc);
                        g.FillRectangle(brush, meiaTamanhoX, meiaRuaCalcY, ext, calc);
                    }
                    if (temCima)
                    {
                        g.FillRectangle(brush, -meiaRuaCalcX - calc, -meiaTamanhoY - ext, calc, ext);
                        g.FillRectangle(brush, meiaRuaCalcX, -meiaTamanhoY - ext, calc, ext);
                    }
                    if (temBaixo)
                    {
                        g.FillRectangle(brush, -meiaRuaCalcX - calc, meiaTamanhoY, calc, ext);
                        g.FillRectangle(brush, meiaRuaCalcX, meiaTamanhoY, calc, ext);
                    }
                }
            }

            using (var brush = new SolidBrush(Color.FromArgb(CorAsfaltoArgb)))
            {
                g.FillRectangle(brush, -meiaTamanho, -meiaRua, meiaTamanho * 2, LarguraRua);
                if (temEsquerda) g.FillRectangle(brush, -meiaTamanho - ext, -meiaRua, ext, LarguraRua);
                if (temDireita) g.FillRectangle(brush, meiaTamanho, -meiaRua, ext, LarguraRua);
                if (temCima) g.FillRectangle(brush, -meiaRua, -meiaTamanho - ext, LarguraRua, ext + meiaTamanho);
                if (temBaixo) g.FillRectangle(brush, -meiaRua, 0, LarguraRua, meiaTamanho + ext);
            }

            using (var brush = new HatchBrush(HatchStyle.Percent10, Color.FromArgb(20, 0, 0, 0), Color.Transparent))
            {
                g.FillRectangle(brush, -meiaTamanho, -meiaRua, meiaTamanho * 2, LarguraRua);
                if (temEsquerda) g.FillRectangle(brush, -meiaTamanho - ext, -meiaRua, ext, LarguraRua);
                if (temDireita) g.FillRectangle(brush, meiaTamanho, -meiaRua, ext, LarguraRua);
                if (temCima) g.FillRectangle(brush, -meiaRua, -meiaTamanho - ext, LarguraRua, ext + meiaTamanho);
                if (temBaixo) g.FillRectangle(brush, -meiaRua, 0, LarguraRua, meiaTamanho + ext);
            }

            AjustarBracosSemCanteiro(g, meiaRua, meiaTamanho, ext,
                temCima, temBaixo, temEsquerda, temDireita,
                usarNorte: CanteiroRuaNorte, usarSul: CanteiroRuaSul,
                usarOeste: CanteiroRuaOeste, usarLeste: CanteiroRuaLeste);

            if (TemFaixaEstacionamento)
            {
                DesenharFaixasEstacionamento(g, meiaRua, meiaTamanho, ext,
                    cima: temCima && EstacionamentoRuaNorte,
                    baixo: temBaixo && EstacionamentoRuaSul,
                    esquerda: temEsquerda && EstacionamentoRuaOeste,
                    direita: temDireita && EstacionamentoRuaLeste);
            }

            DesenharPoligonoCentralCruz(g, meiaRua);

            if (TemCalcada && TipoCruzamento != TipoCruzamento.Cruz)
                ReforcarCalcadaLadoFechadoT(g, meiaRua, meiaTamanho, temCima, temBaixo, temEsquerda, temDireita);

            if (TemFaixaPedestre)
                DesenharFaixasPedestreT(g, meiaRua, meiaTamanho, temCima, temBaixo, temEsquerda, temDireita);

            DesenharMeioFioBracos(g, meiaRua, meiaTamanho, ext, temCima, temBaixo, temEsquerda, temDireita);

            if (TemSinalPare)
            {
                DesenharSinalPare(g, meiaRua, meiaTamanho,
                    temCima && PareRuaNorte,
                    temBaixo && PareRuaSul,
                    temEsquerda && PareRuaOeste,
                    temDireita && PareRuaLeste);
            }

            if (TemCanteiroCentral)
            {
                DesenharCanteiroCentral(g, meiaRua, meiaTamanho, ext,
                    cima: temCima && CanteiroRuaNorte,
                    baixo: temBaixo && CanteiroRuaSul,
                    esquerda: temEsquerda && CanteiroRuaOeste,
                    direita: temDireita && CanteiroRuaLeste);
            }
        }

        private void ReforcarCalcadaLadoFechadoT(Graphics g, float meiaRua, float meiaTamanho, bool temCima, bool temBaixo, bool temEsquerda, bool temDireita)
        {
            float extraX = ObterExtraEstacionamentoEixoX();
            float extraY = ObterExtraEstacionamentoEixoY();
            float meiaRuaCalcX = meiaRua + extraX;
            float meiaRuaCalcY = meiaRua + extraY;
            float meiaTamanhoX = meiaTamanho + extraX;
            float meiaTamanhoY = meiaTamanho + extraY;
            float calc = LarguraCalcada;

            using var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb));
            using var penMeioFio = new Pen(Color.FromArgb(150, 150, 140), 2f);

            if (!temCima)
            {
                g.FillRectangle(brush, -meiaTamanhoX, -meiaTamanhoY, meiaTamanhoX * 2, calc);
                g.DrawLine(penMeioFio, -meiaRuaCalcX, -meiaRuaCalcY, meiaRuaCalcX, -meiaRuaCalcY);
            }

            if (!temBaixo)
            {
                g.FillRectangle(brush, -meiaTamanhoX, meiaRuaCalcY, meiaTamanhoX * 2, calc);
                g.DrawLine(penMeioFio, -meiaRuaCalcX, meiaRuaCalcY, meiaRuaCalcX, meiaRuaCalcY);
            }

            if (!temEsquerda)
            {
                g.FillRectangle(brush, -meiaTamanhoX, -meiaTamanhoY, calc, meiaTamanhoY * 2);
                g.DrawLine(penMeioFio, -meiaRuaCalcX, -meiaRuaCalcY, -meiaRuaCalcX, meiaRuaCalcY);
            }

            if (!temDireita)
            {
                g.FillRectangle(brush, meiaRuaCalcX, -meiaTamanhoY, calc, meiaTamanhoY * 2);
                g.DrawLine(penMeioFio, meiaRuaCalcX, -meiaRuaCalcY, meiaRuaCalcX, meiaRuaCalcY);
            }
        }

        private void DesenharPoligonoCentralCruz(Graphics g, float meiaRua)
        {
            using (var path = CriarContornoCruz(meiaRua))
            using (var brushAsfalto = new SolidBrush(Color.FromArgb(CorAsfaltoArgb)))
            using (var hatch = new HatchBrush(HatchStyle.Percent10, Color.FromArgb(20, 0, 0, 0), Color.Transparent))
            {
                g.FillPath(brushAsfalto, path);
                g.FillPath(hatch, path);
            }
        }

        private void DesenharFaixasEstacionamento(Graphics g, float meiaRua, float meiaTamanho, float ext, bool cima, bool baixo, bool esquerda, bool direita)
        {
            if (!TemFaixaEstacionamentoEfetiva()) return;
            float le = LarguraFaixaEstacionamento;
            if (le < 1f) return;

            Color corCalcada = Color.FromArgb(CorCalcadaArgb);
            Color corEstacionamento = Color.FromArgb(Math.Max(0, corCalcada.R - 18), Math.Max(0, corCalcada.G - 18), Math.Max(0, corCalcada.B - 18));

            using var brush = new SolidBrush(corEstacionamento);
            using var pen = new Pen(Color.FromArgb(CorFaixaArgb), 1.2f);
            using var penBorda = new Pen(Color.FromArgb(140, 140, 130), 1.8f);
            pen.DashStyle = DashStyle.Custom;
            pen.DashPattern = new float[] { 5f, 4f };

            if (cima)
            {
                g.FillRectangle(brush, -meiaRua - le, -(meiaTamanho + ext), le, ext);
                g.FillRectangle(brush, meiaRua, -(meiaTamanho + ext), le, ext);
                g.FillRectangle(brush, -meiaRua - le, -meiaTamanho, le, meiaTamanho);
                g.FillRectangle(brush, meiaRua, -meiaTamanho, le, meiaTamanho);
                g.DrawLine(pen, -meiaRua, -(meiaTamanho + ext), -meiaRua, 0f);
                g.DrawLine(pen, meiaRua, -(meiaTamanho + ext), meiaRua, 0f);
                g.DrawLine(penBorda, -meiaRua - le, -(meiaTamanho + ext), -meiaRua - le, 0f);
                g.DrawLine(penBorda, meiaRua + le, -(meiaTamanho + ext), meiaRua + le, 0f);
            }
            if (baixo)
            {
                g.FillRectangle(brush, -meiaRua - le, 0f, le, meiaTamanho);
                g.FillRectangle(brush, meiaRua, 0f, le, meiaTamanho);
                g.FillRectangle(brush, -meiaRua - le, meiaTamanho, le, ext);
                g.FillRectangle(brush, meiaRua, meiaTamanho, le, ext);
                g.DrawLine(pen, -meiaRua, 0f, -meiaRua, meiaTamanho + ext);
                g.DrawLine(pen, meiaRua, 0f, meiaRua, meiaTamanho + ext);
                g.DrawLine(penBorda, -meiaRua - le, 0f, -meiaRua - le, meiaTamanho + ext);
                g.DrawLine(penBorda, meiaRua + le, 0f, meiaRua + le, meiaTamanho + ext);
            }
            if (esquerda)
            {
                g.FillRectangle(brush, -(meiaTamanho + ext), -meiaRua - le, ext, le);
                g.FillRectangle(brush, -(meiaTamanho + ext), meiaRua, ext, le);
                g.FillRectangle(brush, -meiaTamanho, -meiaRua - le, meiaTamanho, le);
                g.FillRectangle(brush, -meiaTamanho, meiaRua, meiaTamanho, le);
                g.DrawLine(pen, -(meiaTamanho + ext), -meiaRua, 0f, -meiaRua);
                g.DrawLine(pen, -(meiaTamanho + ext), meiaRua, 0f, meiaRua);
                g.DrawLine(penBorda, -(meiaTamanho + ext), -meiaRua - le, 0f, -meiaRua - le);
                g.DrawLine(penBorda, -(meiaTamanho + ext), meiaRua + le, 0f, meiaRua + le);
            }
            if (direita)
            {
                g.FillRectangle(brush, 0f, -meiaRua - le, meiaTamanho, le);
                g.FillRectangle(brush, 0f, meiaRua, meiaTamanho, le);
                g.FillRectangle(brush, meiaTamanho, -meiaRua - le, ext, le);
                g.FillRectangle(brush, meiaTamanho, meiaRua, ext, le);
                g.DrawLine(pen, 0f, -meiaRua, meiaTamanho + ext, -meiaRua);
                g.DrawLine(pen, 0f, meiaRua, meiaTamanho + ext, meiaRua);
                g.DrawLine(penBorda, 0f, -meiaRua - le, meiaTamanho + ext, -meiaRua - le);
                g.DrawLine(penBorda, 0f, meiaRua + le, meiaTamanho + ext, meiaRua + le);
            }

            using var penMeio = new Pen(Color.FromArgb(150, 150, 140), 2f);
            if (cima) { g.DrawLine(penMeio, -meiaRua, -meiaTamanho, -meiaRua, 0f); g.DrawLine(penMeio, meiaRua, -meiaTamanho, meiaRua, 0f); }
            if (baixo) { g.DrawLine(penMeio, -meiaRua, 0f, -meiaRua, meiaTamanho); g.DrawLine(penMeio, meiaRua, 0f, meiaRua, meiaTamanho); }
            if (esquerda) { g.DrawLine(penMeio, -meiaTamanho, -meiaRua, 0f, -meiaRua); g.DrawLine(penMeio, -meiaTamanho, meiaRua, 0f, meiaRua); }
            if (direita) { g.DrawLine(penMeio, 0f, -meiaRua, meiaTamanho, -meiaRua); g.DrawLine(penMeio, 0f, meiaRua, meiaTamanho, meiaRua); }
        }

        private void DesenharFaixasPedestreT(Graphics g, float meiaRua, float meiaTamanho, bool cima, bool baixo, bool esquerda, bool direita)
        {
            using var brush = new SolidBrush(Color.FromArgb(CorFaixaArgb));
            float larguraLista = 4f;
            float espacamento = 3f;
            float distancia = meiaTamanho + 5f;
            if (cima) DesenharFaixaUnica(g, brush, 0, -distancia, LarguraFaixaPedestre, true, larguraLista, espacamento);
            if (baixo) DesenharFaixaUnica(g, brush, 0, distancia, LarguraFaixaPedestre, true, larguraLista, espacamento);
            if (direita) DesenharFaixaUnica(g, brush, distancia, 0, LarguraFaixaPedestre, false, larguraLista, espacamento);
            if (esquerda) DesenharFaixaUnica(g, brush, -distancia, 0, LarguraFaixaPedestre, false, larguraLista, espacamento);
        }

        private void DesenharFaixaUnica(Graphics g, Brush brush, float cx, float cy, float largura, bool horizontal, float espLista, float espaco)
        {
            int numListas = (int)(LarguraRua / (espLista + espaco));
            for (int i = 0; i < numListas; i++)
            {
                float offset = -LarguraRua / 2 + i * (espLista + espaco) + espLista / 2;
                if (horizontal) g.FillRectangle(brush, cx + offset - espLista / 2, cy - largura / 2, espLista, largura);
                else g.FillRectangle(brush, cx - largura / 2, cy + offset - espLista / 2, largura, espLista);
            }
        }

        private void DesenharSinalPare(Graphics g, float meiaRua, float meiaTamanho, bool cima, bool baixo, bool esquerda, bool direita)
        {
            using var pen = new Pen(Color.FromArgb(CorFaixaArgb), 3f);
            using var brush = new SolidBrush(Color.FromArgb(CorFaixaArgb));
            using var fonte = new Font("Arial", 12f, FontStyle.Bold, GraphicsUnit.Pixel);
            float deslocLinha = ObterDeslocamentoLinhaPare(meiaTamanho);
            float deslocTexto = 10f;
            float meiaExt = Math.Min(meiaRua, 35f) + 3;
            SizeF sz = g.MeasureString("PARE", fonte);
            float halfW = sz.Width / 2f;
            if (cima)
            {
                float yLinha = -meiaTamanho - deslocLinha;
                float yTexto = yLinha - deslocTexto;
                float xTexto = -meiaExt + halfW - 5;
                if (ViaDuasMaos) g.DrawLine(pen, -meiaExt, yLinha, 0f, yLinha);
                else g.DrawLine(pen, -meiaExt, yLinha, meiaExt, yLinha);
                DesenharTextoCentralizadoRotacionado(g, "PARE", fonte, brush, xTexto, yTexto, 180f);
            }
            if (baixo)
            {
                float yLinha = meiaTamanho + deslocLinha;
                float yTexto = yLinha + deslocTexto;
                float xTexto = meiaExt - halfW + 5;
                if (ViaDuasMaos) g.DrawLine(pen, 0f, yLinha, meiaExt, yLinha);
                else g.DrawLine(pen, -meiaExt, yLinha, meiaExt, yLinha);
                DesenharTextoCentralizadoRotacionado(g, "PARE", fonte, brush, xTexto, yTexto, 0f);
            }
            if (esquerda)
            {
                float xLinha = -meiaTamanho - deslocLinha;
                float xTexto = xLinha - deslocTexto;
                float yTexto = meiaExt - halfW + 5;
                if (ViaDuasMaos) g.DrawLine(pen, xLinha, 0f, xLinha, meiaExt);
                else g.DrawLine(pen, xLinha, -meiaExt, xLinha, meiaExt);
                DesenharTextoCentralizadoRotacionado(g, "PARE", fonte, brush, xTexto, yTexto, 90f);
            }
            if (direita)
            {
                float xLinha = meiaTamanho + deslocLinha;
                float xTexto = xLinha + deslocTexto;
                float yTexto = -meiaExt + halfW - 5;
                if (ViaDuasMaos) g.DrawLine(pen, xLinha, -meiaExt, xLinha, 0f);
                else g.DrawLine(pen, xLinha, -meiaExt, xLinha, meiaExt);
                DesenharTextoCentralizadoRotacionado(g, "PARE", fonte, brush, xTexto, yTexto, -90f);
            }
        }

        private float ObterDeslocamentoLinhaPare(float meiaTamanho)
        {
            const float deslocamentoPadrao = 20f;
            const float margemEntreFaixaEPare = 8f;

            if (!TemFaixaPedestre)
                return deslocamentoPadrao;

            float distanciaFaixa = meiaTamanho + 5f;
            float bordaExternaFaixa = distanciaFaixa + (LarguraFaixaPedestre / 2f);
            float deslocamentoMinimo = bordaExternaFaixa - meiaTamanho + margemEntreFaixaEPare;
            return Math.Max(deslocamentoPadrao, deslocamentoMinimo);
        }

        private GraphicsPath CriarContornoCruz(float meiaRua)
        {
            var path = new GraphicsPath();
            float m = meiaRua;
            float t = meiaRua + (TemCalcada ? LarguraCalcada : 0);
            path.AddPolygon(new PointF[]
            {
                new PointF(-m, -t), new PointF(m, -t), new PointF(m, -m),
                new PointF(t, -m), new PointF(t, m), new PointF(m, m),
                new PointF(m, t), new PointF(-m, t), new PointF(-m, m),
                new PointF(-t, m), new PointF(-t, -m), new PointF(-m, -m)
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
            bool temCima = TipoCruzamento != TipoCruzamento.TParaBaixo;
            bool temBaixo = TipoCruzamento != TipoCruzamento.TParaCima;
            bool temEsquerda = TipoCruzamento != TipoCruzamento.TParaDireita;
            bool temDireita = TipoCruzamento != TipoCruzamento.TParaEsquerda;

            float meiaRua = LarguraRua / 2f;
            float meiaTamanho = (LarguraRua + (TemCalcada ? LarguraCalcada * 2f : 0f)) / 2f;
            float ext = Math.Max(0f, ExtensaoVias);

            float localMinX = -meiaTamanho;
            float localMaxX = meiaTamanho;
            float localMinY = -meiaTamanho;
            float localMaxY = meiaTamanho;

            if (temEsquerda) localMinX = Math.Min(localMinX, -meiaTamanho - ext);
            if (temDireita) localMaxX = Math.Max(localMaxX, meiaTamanho + ext);
            if (temCima) localMinY = Math.Min(localMinY, -meiaTamanho - ext);
            if (temBaixo) localMaxY = Math.Max(localMaxY, meiaTamanho + ext);

            if (TemFaixaEstacionamentoEfetiva())
            {
                float le = Math.Max(0f, LarguraFaixaEstacionamento);

                if (temCima && EstacionamentoRuaNorte)
                {
                    localMinY = Math.Min(localMinY, -meiaTamanho - ext);
                    localMinX = Math.Min(localMinX, -meiaRua - le);
                    localMaxX = Math.Max(localMaxX, meiaRua + le);
                }

                if (temBaixo && EstacionamentoRuaSul)
                {
                    localMaxY = Math.Max(localMaxY, meiaTamanho + ext);
                    localMinX = Math.Min(localMinX, -meiaRua - le);
                    localMaxX = Math.Max(localMaxX, meiaRua + le);
                }

                if (temEsquerda && EstacionamentoRuaOeste)
                {
                    localMinX = Math.Min(localMinX, -meiaTamanho - ext);
                    localMinY = Math.Min(localMinY, -meiaRua - le);
                    localMaxY = Math.Max(localMaxY, meiaRua + le);
                }

                if (temDireita && EstacionamentoRuaLeste)
                {
                    localMaxX = Math.Max(localMaxX, meiaTamanho + ext);
                    localMinY = Math.Min(localMinY, -meiaRua - le);
                    localMaxY = Math.Max(localMaxY, meiaRua + le);
                }
            }

            if (TemFaixaPedestre)
            {
                float distancia = meiaTamanho + 5f;
                float meioComprimentoFaixa = LarguraFaixaPedestre / 2f;

                if (temCima) localMinY = Math.Min(localMinY, -distancia - meioComprimentoFaixa);
                if (temBaixo) localMaxY = Math.Max(localMaxY, distancia + meioComprimentoFaixa);
                if (temEsquerda) localMinX = Math.Min(localMinX, -distancia - meioComprimentoFaixa);
                if (temDireita) localMaxX = Math.Max(localMaxX, distancia + meioComprimentoFaixa);
            }

            float rad = Rotacao * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            PointF Transformar(float x, float y) => new PointF(
                Posicao.X + x * cos - y * sin,
                Posicao.Y + x * sin + y * cos);

            var p1 = Transformar(localMinX, localMinY);
            var p2 = Transformar(localMaxX, localMinY);
            var p3 = Transformar(localMaxX, localMaxY);
            var p4 = Transformar(localMinX, localMaxY);

            float minX = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
            float minY = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
            float maxX = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
            float maxY = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));

            const float margem = 2f;
            return new RectangleF(minX - margem, minY - margem, (maxX - minX) + margem * 2f, (maxY - minY) + margem * 2f);
        }

        public PointF[] GetPontosConexao()
        {
            float dist = LarguraRua / 2 + (TemCalcada ? LarguraCalcada : 0) + ExtensaoVias;
            var locais = GetPontosConexaoLocais(dist);
            var globais = new PointF[locais.Length];
            for (int i = 0; i < locais.Length; i++) globais[i] = TransformarLocalParaGlobal(locais[i]);
            return globais;
        }

        public bool TryObterConexaoProxima(PointF ponto, float tolerancia, out PointF pontoConexao, out PointF direcaoSaida, out bool temCanteiro)
        {
            float dist = LarguraRua / 2 + (TemCalcada ? LarguraCalcada : 0) + ExtensaoVias;
            var pontosLocais = GetPontosConexaoLocais(dist);
            var direcoesLocais = GetDirecoesConexaoLocais();
            var canteiroLocais = GetCanteiroConexaoLocais();
            pontoConexao = PointF.Empty;
            direcaoSaida = new PointF(1f, 0f);
            temCanteiro = false;
            if (pontosLocais.Length == 0 || pontosLocais.Length != direcoesLocais.Length) return false;

            float melhor = float.MaxValue;
            int indice = -1;
            PointF melhorPonto = PointF.Empty;
            for (int i = 0; i < pontosLocais.Length; i++)
            {
                var p = TransformarLocalParaGlobal(pontosLocais[i]);
                float dx = p.X - ponto.X, dy = p.Y - ponto.Y;
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d <= tolerancia && d < melhor) { melhor = d; indice = i; melhorPonto = p; }
            }
            if (indice < 0) return false;
            pontoConexao = melhorPonto;
            direcaoSaida = TransformarDirecaoLocalParaGlobal(direcoesLocais[indice]);
            temCanteiro = TemCanteiroCentral && indice < canteiroLocais.Length && canteiroLocais[indice];
            return true;
        }

        private PointF[] GetPontosConexaoLocais(float dist)
        {
            return TipoCruzamento switch
            {
                TipoCruzamento.Cruz => new[] { new PointF(0, -dist), new PointF(dist, 0), new PointF(0, dist), new PointF(-dist, 0) },
                TipoCruzamento.TParaCima => new[] { new PointF(0, -dist), new PointF(dist, 0), new PointF(-dist, 0) },
                TipoCruzamento.TParaBaixo => new[] { new PointF(dist, 0), new PointF(0, dist), new PointF(-dist, 0) },
                TipoCruzamento.TParaEsquerda => new[] { new PointF(0, -dist), new PointF(0, dist), new PointF(-dist, 0) },
                TipoCruzamento.TParaDireita => new[] { new PointF(0, -dist), new PointF(dist, 0), new PointF(0, dist) },
                _ => Array.Empty<PointF>()
            };
        }

        private PointF[] GetDirecoesConexaoLocais()
        {
            return TipoCruzamento switch
            {
                TipoCruzamento.Cruz => new[] { new PointF(0, -1), new PointF(1, 0), new PointF(0, 1), new PointF(-1, 0) },
                TipoCruzamento.TParaCima => new[] { new PointF(0, -1), new PointF(1, 0), new PointF(-1, 0) },
                TipoCruzamento.TParaBaixo => new[] { new PointF(1, 0), new PointF(0, 1), new PointF(-1, 0) },
                TipoCruzamento.TParaEsquerda => new[] { new PointF(0, -1), new PointF(0, 1), new PointF(-1, 0) },
                TipoCruzamento.TParaDireita => new[] { new PointF(0, -1), new PointF(1, 0), new PointF(0, 1) },
                _ => Array.Empty<PointF>()
            };
        }

        private bool[] GetCanteiroConexaoLocais()
        {
            return TipoCruzamento switch
            {
                TipoCruzamento.Cruz => new[] { CanteiroRuaNorte, CanteiroRuaLeste, CanteiroRuaSul, CanteiroRuaOeste },
                TipoCruzamento.TParaCima => new[] { CanteiroRuaNorte, CanteiroRuaLeste, CanteiroRuaOeste },
                TipoCruzamento.TParaBaixo => new[] { CanteiroRuaLeste, CanteiroRuaSul, CanteiroRuaOeste },
                TipoCruzamento.TParaEsquerda => new[] { CanteiroRuaNorte, CanteiroRuaSul, CanteiroRuaOeste },
                TipoCruzamento.TParaDireita => new[] { CanteiroRuaNorte, CanteiroRuaLeste, CanteiroRuaSul },
                _ => Array.Empty<bool>()
            };
        }

        private PointF TransformarLocalParaGlobal(PointF local)
        {
            float rad = Rotacao * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad), sin = (float)Math.Sin(rad);
            return new PointF(Posicao.X + local.X * cos - local.Y * sin, Posicao.Y + local.X * sin + local.Y * cos);
        }

        private PointF TransformarDirecaoLocalParaGlobal(PointF local)
        {
            float rad = Rotacao * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad), sin = (float)Math.Sin(rad);
            float x = local.X * cos - local.Y * sin;
            float y = local.X * sin + local.Y * cos;
            float len = (float)Math.Sqrt(x * x + y * y);
            return len <= 0.0001f ? new PointF(1f, 0f) : new PointF(x / len, y / len);
        }

        private float ObterExtraEstacionamentoEixoX()
        {
            if (!_temFaixaEstacionamento || !(_estacionamentoRuaNorte || _estacionamentoRuaSul)) return 0f;
            return _larguraFaixaEstacionamento;
        }

        private float ObterExtraEstacionamentoEixoY()
        {
            if (!_temFaixaEstacionamento || !(_estacionamentoRuaLeste || _estacionamentoRuaOeste)) return 0f;
            return _larguraFaixaEstacionamento;
        }

        private float ObterLarguraUtilPista()
        {
            float canteiroAtual = _temCanteiroCentral ? _larguraCanteiroCentral : 0f;
            return Math.Max(6f, _larguraRua - canteiroAtual);
        }

        private bool TemFaixaEstacionamentoEfetiva() => _temFaixaEstacionamento && (_estacionamentoRuaNorte || _estacionamentoRuaSul || _estacionamentoRuaLeste || _estacionamentoRuaOeste);

        private void AjustarBracosSemCanteiro(Graphics g, float meiaRua, float meiaTamanho, float ext, bool cima, bool baixo, bool esquerda, bool direita, bool usarNorte, bool usarSul, bool usarOeste, bool usarLeste)
        {
            if (!TemCanteiroCentral || LarguraCanteiroCentral <= 0.1f) return;
            float meiaSemCanteiro = Math.Max(4f, (LarguraRua - LarguraCanteiroCentral) / 2f);
            if (meiaSemCanteiro >= meiaRua) return;
            float faixaAjuste = meiaRua - meiaSemCanteiro;
            if (faixaAjuste <= 0.1f) return;
            using var brush = new SolidBrush(Color.FromArgb(CorCalcadaArgb));
            if (esquerda && !usarOeste) { g.FillRectangle(brush, -meiaTamanho - ext, -meiaRua, ext, faixaAjuste); g.FillRectangle(brush, -meiaTamanho - ext, meiaSemCanteiro, ext, faixaAjuste); }
            if (direita && !usarLeste) { g.FillRectangle(brush, meiaTamanho, -meiaRua, ext, faixaAjuste); g.FillRectangle(brush, meiaTamanho, meiaSemCanteiro, ext, faixaAjuste); }
            if (cima && !usarNorte) { g.FillRectangle(brush, -meiaRua, -meiaTamanho - ext, faixaAjuste, ext); g.FillRectangle(brush, meiaSemCanteiro, -meiaTamanho - ext, faixaAjuste, ext); }
            if (baixo && !usarSul) { g.FillRectangle(brush, -meiaRua, meiaTamanho, faixaAjuste, ext); g.FillRectangle(brush, meiaSemCanteiro, meiaTamanho, faixaAjuste, ext); }
        }

        private void DesenharMeioFioBracos(Graphics g, float meiaRua, float meiaTamanho, float ext, bool cima, bool baixo, bool esquerda, bool direita)
        {
            using var pen = new Pen(Color.FromArgb(150, 150, 140), 2f);
            if (cima)
            {
                g.DrawLine(pen, -meiaRua, -meiaTamanho - ext, -meiaRua, -meiaTamanho);
                g.DrawLine(pen, meiaRua, -meiaTamanho - ext, meiaRua, -meiaTamanho);
            }
            if (baixo)
            {
                g.DrawLine(pen, -meiaRua, meiaTamanho, -meiaRua, meiaTamanho + ext);
                g.DrawLine(pen, meiaRua, meiaTamanho, meiaRua, meiaTamanho + ext);
            }
            if (esquerda)
            {
                g.DrawLine(pen, -meiaTamanho - ext, -meiaRua, -meiaTamanho, -meiaRua);
                g.DrawLine(pen, -meiaTamanho - ext, meiaRua, -meiaTamanho, meiaRua);
            }
            if (direita)
            {
                g.DrawLine(pen, meiaTamanho, -meiaRua, meiaTamanho + ext, -meiaRua);
                g.DrawLine(pen, meiaTamanho, meiaRua, meiaTamanho + ext, meiaRua);
            }
        }

        private void DesenharCanteiroCentral(Graphics g, float meiaRua, float meiaTamanho, float ext, bool cima, bool baixo, bool esquerda, bool direita)
        {
            if (!TemCanteiroCentral) return;

            float largura = Math.Min(LarguraCanteiroCentral, Math.Max(2f, LarguraRua - 4f));
            if (largura <= 0.1f) return;

            float metade = largura / 2f;
            using var brush = new SolidBrush(Color.FromArgb(CorCanteiroArgb));
            using var pen = new Pen(Color.FromArgb(90, 120, 70), 1.2f);

            if (cima)
            {
                var r = new RectangleF(-metade, -(meiaTamanho + ext), largura, ext + meiaTamanho);
                g.FillRectangle(brush, r);
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
            }
            if (baixo)
            {
                var r = new RectangleF(-metade, 0f, largura, meiaTamanho + ext);
                g.FillRectangle(brush, r);
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
            }
            if (esquerda)
            {
                var r = new RectangleF(-(meiaTamanho + ext), -metade, ext + meiaTamanho, largura);
                g.FillRectangle(brush, r);
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
            }
            if (direita)
            {
                var r = new RectangleF(0f, -metade, meiaTamanho + ext, largura);
                g.FillRectangle(brush, r);
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
            }
        }

        private void DesenharTextoCentralizadoRotacionado(Graphics g, string texto, Font fonte, Brush brush, float x, float y, float angulo)
        {
            var state = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(angulo);
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(texto, fonte, brush, 0f, 0f, format);
            }
            g.Restore(state);
        }
    }
}

