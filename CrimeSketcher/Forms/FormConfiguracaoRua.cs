// Forms/FormConfiguracaoRua.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using CrimeSketcher.Tools;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Forms
{
    public class FormConfiguracaoRua : Form
    {
        private NumericUpDown nudLargura;
        private NumericUpDown nudFaixas;
        private TextBox txtNome;
        private CheckBox chkCalcada;
        private NumericUpDown nudLarguraCalcada;
        private ComboBox cmbTipoFaixa;
        private CheckBox chkMaoUnica;
        private CheckBox chkCanteiroCentral;
        private NumericUpDown nudLarguraCanteiroCentral;
        private CheckBox chkFaixaEstacionamento;
        private ComboBox cmbTipoLinhaEstacionamento;
        private Button btnCorEstacionamento;
        private Button btnCorLinhaEstacionamento;
        private CheckBox chkCiclofaixa;
        private Panel painelPreview;
        private Color _corEstacionamento = Color.Transparent;
        private Color _corLinhaEstacionamento = Color.White;

        public StreetTool StreetTool { get; private set; }

        public FormConfiguracaoRua(StreetTool tool)
        {
            StreetTool = tool;
            InitializeComponent();
            CarregarValores();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuração de Rua";
            this.Size = new Size(450, 620);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            int y = 20;
            int labelWidth = 150;
            int controlX = 165;

            // Nome da rua
            Controls.Add(new Label
            {
                Text = "Nome da rua:",
                Location = new Point(20, y + 3),
                AutoSize = true
            });
            txtNome = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(240, 25),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(txtNome);
            y += 40;

            // Largura
            Controls.Add(new Label
            {
                Text = "Largura (m):",
                Location = new Point(20, y + 3),
                AutoSize = true
            });
            nudLargura = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 100,
                DecimalPlaces = 2,
                Increment = 0.5m,
                Value = 5,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
            Controls.Add(nudLargura);
            y += 40;

            // Número de faixas
            Controls.Add(new Label
            {
                Text = "Número de faixas:",
                Location = new Point(20, y + 3),
                AutoSize = true
            });
            nudFaixas = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 6,
                Value = 2,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
            Controls.Add(nudFaixas);
            y += 40;

            // Tipo de faixa central
            Controls.Add(new Label
            {
                Text = "Faixa central:",
                Location = new Point(20, y + 3),
                AutoSize = true
            });
            cmbTipoFaixa = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
            cmbTipoFaixa.Items.AddRange(new object[]
            {
                "Nenhuma",
                "Tracejada simples (- - - -)",
                "Contínua simples (────)",
                "Contínua dupla (════)",
                "Contínua esq. / Tracejada dir.",
                "Tracejada esq. / Contínua dir."
            });
            cmbTipoFaixa.SelectedIndex = 1;
            Controls.Add(cmbTipoFaixa);
            y += 40;

            // Mão única
            chkMaoUnica = new CheckBox
            {
                Text = "Mão única",
                Location = new Point(20, y),
                AutoSize = true
            };
            Controls.Add(chkMaoUnica);
            y += 35;

            // Separador
            var sep = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(20, y),
                Size = new Size(390, 2)
            };
            Controls.Add(sep);
            y += 15;

            // Calçada
            chkCalcada = new CheckBox
            {
                Text = "Desenhar calçadas",
                Location = new Point(20, y),
                AutoSize = true,
                Checked = true
            };
            chkCalcada.CheckedChanged += (s, e) =>
            {
                nudLarguraCalcada.Enabled = chkCalcada.Checked;
                AtualizarPreview();
            };
            Controls.Add(chkCalcada);
            y += 35;

            // Largura calçada
            Controls.Add(new Label
            {
                Text = "Largura calçada (m):",
                Location = new Point(40, y + 3),
                AutoSize = true
            });
            nudLarguraCalcada = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(80, 25),
                Minimum = 0.1m,
                Maximum = 20,
                DecimalPlaces = 2,
                Increment = 0.1m,
                Value = 1,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
            Controls.Add(nudLarguraCalcada);
            y += 40;

            // Canteiro central
            chkCanteiroCentral = new CheckBox
            {
                Text = "Possui canteiro central",
                Location = new Point(20, y),
                AutoSize = true,
                Checked = false
            };
            chkCanteiroCentral.CheckedChanged += (s, e) =>
            {
                nudLarguraCanteiroCentral.Enabled = chkCanteiroCentral.Checked;
                AtualizarPreview();
            };
            Controls.Add(chkCanteiroCentral);
            y += 35;

            Controls.Add(new Label
            {
                Text = "Largura canteiro (m):",
                Location = new Point(40, y + 3),
                AutoSize = true
            });
            nudLarguraCanteiroCentral = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(80, 25),
                Minimum = 0.1m,
                Maximum = 20,
                DecimalPlaces = 2,
                Increment = 0.1m,
                Value = 1,
                Enabled = false,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
            Controls.Add(nudLarguraCanteiroCentral);
            y += 45;

            chkFaixaEstacionamento = new CheckBox
            {
                Text = "Possui faixa de estacionamento",
                Location = new Point(20, y),
                AutoSize = true,
                Checked = false
            };
            chkFaixaEstacionamento.CheckedChanged += (s, e) => AtualizarPreview();
            Controls.Add(chkFaixaEstacionamento);
            y += 30;

            chkCiclofaixa = new CheckBox
            {
                Text = "Possui ciclofaixa",
                Location = new Point(20, y),
                AutoSize = true,
                Checked = false
            };
            chkCiclofaixa.CheckedChanged += (s, e) => AtualizarPreview();
            Controls.Add(chkCiclofaixa);
            y += 40;

            // Preview
            Controls.Add(new Label
            {
                Text = "Preview:",
                Location = new Point(20, y),
                AutoSize = true
            });
            y += 25;

            painelPreview = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(390, 80),
                BackColor = Color.FromArgb(250, 250, 245),
                BorderStyle = BorderStyle.FixedSingle
            };
            painelPreview.Paint += PainelPreview_Paint;
            Controls.Add(painelPreview);
            y += 95;

            // Botões
            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(220, y),
                Size = new Size(90, 32),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += BtnOk_Click;
            Controls.Add(btnOk);

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(320, y),
                Size = new Size(90, 32),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            Controls.Add(btnCancelar);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancelar;

            // Eventos para atualizar preview
            nudLargura.ValueChanged += (s, e) => AtualizarPreview();
            nudFaixas.ValueChanged += (s, e) => AtualizarPreview();
            cmbTipoFaixa.SelectedIndexChanged += (s, e) => AtualizarPreview();
            nudLarguraCalcada.ValueChanged += (s, e) => AtualizarPreview();
            nudLarguraCanteiroCentral.ValueChanged += (s, e) => AtualizarPreview();
        }

        private float PxParaM(float px)
        {
            var esc = ScaleManager.Atual;
            return esc != null ? esc.PixelsParaReal(px) : px;
        }

        private float MParaPx(float m)
        {
            var esc = ScaleManager.Atual;
            return esc != null ? esc.RealParaPixels(m) : m;
        }

        private void CarregarValores()
        {
            txtNome.Text = StreetTool.NomeRua;
            nudLargura.Value = (decimal)Math.Max((float)nudLargura.Minimum,
                Math.Min((float)nudLargura.Maximum, PxParaM(StreetTool.Largura)));
            nudFaixas.Value = StreetTool.NumeroFaixas;
            chkCalcada.Checked = StreetTool.TemCalcada;
            nudLarguraCalcada.Value = (decimal)Math.Max((float)nudLarguraCalcada.Minimum,
                Math.Min((float)nudLarguraCalcada.Maximum, PxParaM(StreetTool.LarguraCalcada)));
            cmbTipoFaixa.SelectedIndex = (int)StreetTool.TipoFaixa;
            chkMaoUnica.Checked = StreetTool.MaoUnica;
            chkCanteiroCentral.Checked = StreetTool.TemCanteiroCentral;
            nudLarguraCanteiroCentral.Value = (decimal)Math.Max((float)nudLarguraCanteiroCentral.Minimum,
                Math.Min((float)nudLarguraCanteiroCentral.Maximum, PxParaM(StreetTool.LarguraCanteiroCentral)));
            nudLarguraCanteiroCentral.Enabled = chkCanteiroCentral.Checked;
            chkFaixaEstacionamento.Checked = StreetTool.TemFaixaEstacionamento;
            chkCiclofaixa.Checked = StreetTool.TemCiclofaixa;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            StreetTool.NomeRua = txtNome.Text;
            StreetTool.Largura = MParaPx((float)nudLargura.Value);
            StreetTool.NumeroFaixas = (int)nudFaixas.Value;
            StreetTool.TemCalcada = chkCalcada.Checked;
            StreetTool.LarguraCalcada = MParaPx((float)nudLarguraCalcada.Value);
            StreetTool.TipoFaixa = (TipoFaixaCentral)cmbTipoFaixa.SelectedIndex;
            StreetTool.MaoUnica = chkMaoUnica.Checked;
            StreetTool.TemCanteiroCentral = chkCanteiroCentral.Checked;
            StreetTool.LarguraCanteiroCentral = MParaPx((float)nudLarguraCanteiroCentral.Value);
            StreetTool.TemFaixaEstacionamento = chkFaixaEstacionamento.Checked;
            StreetTool.TemCiclofaixa = chkCiclofaixa.Checked;
        }

        private void AtualizarPreview()
        {
            painelPreview.Invalidate();
        }

        private void PainelPreview_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float largura = (float)nudLargura.Value;
            float scale = Math.Min(
                (painelPreview.Width - 20) / 200f,
                (painelPreview.Height - 10) / (largura + 40));

            g.TranslateTransform(10, painelPreview.Height / 2);
            g.ScaleTransform(scale, scale);

            var preview = new StreetObject
            {
                PontoInicial = new PointF(0, 0),
                PontoFinal = new PointF(200, 0),
                TemCalcada = chkCalcada.Checked,
                LarguraCalcada = (float)nudLarguraCalcada.Value,
                TipoFaixaCentral = (TipoFaixaCentral)cmbTipoFaixa.SelectedIndex
            };

            preview.TemCanteiroCentral = chkCanteiroCentral.Checked;
            preview.LarguraCanteiroCentral = (float)nudLarguraCanteiroCentral.Value;
            preview.TemFaixaEstacionamento = chkFaixaEstacionamento.Checked;
            preview.TemCiclofaixa = chkCiclofaixa.Checked;
            preview.NumeroFaixas = (int)nudFaixas.Value;
            preview.Largura = largura;

            preview.Desenhar(g);
        }
    }
}