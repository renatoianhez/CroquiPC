using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CrimeSketcher.Forms
{
    public class FormTemplatesTrevo : Form
    {
        private readonly List<TemplateTrevoInfo> _templates;
        private ComboBox cmbTemplate;
        private NumericUpDown nudCentroX;
        private NumericUpDown nudCentroY;
        private NumericUpDown nudEscala;
        private Label lblDescricao;
        private Panel painelPreview;

        public List<BaseSketchObject> ObjetosCriados { get; private set; } = [];

        public FormTemplatesTrevo(PointF centroInicial)
        {
            _templates = TrevoTemplateFactory.ListarTemplates();
            InitializeComponent();
            nudCentroX.Value = AjustarValorNumerico(centroInicial.X, nudCentroX.Minimum, nudCentroX.Maximum);
            nudCentroY.Value = AjustarValorNumerico(centroInicial.Y, nudCentroY.Minimum, nudCentroY.Maximum);

            if (cmbTemplate.Items.Count > 0)
            {
                if (cmbTemplate.SelectedIndex < 0)
                    cmbTemplate.SelectedIndex = 0;

                AtualizarDescricao();
            }
            else
            {
                lblDescricao.Text = "Nenhum template .csk foi encontrado na pasta Templates.";
            }
        }

        private void InitializeComponent()
        {
            Text = "Templates de Trevos";
            Size = new Size(620, 500);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.White;

            Controls.Add(new Label
            {
                Text = "Template:",
                Location = new Point(20, 22),
                AutoSize = true
            });

            cmbTemplate = new ComboBox
            {
                Location = new Point(120, 18),
                Size = new Size(220, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                DataSource = _templates,
                DisplayMember = nameof(TemplateTrevoInfo.NomeExibicao)
            };
            cmbTemplate.SelectedIndexChanged += (_, _) =>
            {
                AtualizarDescricao();
                painelPreview.Invalidate();
            };
            Controls.Add(cmbTemplate);

            Controls.Add(new Label
            {
                Text = "Centro X (m):",
                Location = new Point(20, 68),
                AutoSize = true
            });
            nudCentroX = CriarNumero(new Point(120, 64), -50000, 50000, 2, 0.5m, 0);
            Controls.Add(nudCentroX);

            Controls.Add(new Label
            {
                Text = "Centro Y (m):",
                Location = new Point(20, 108),
                AutoSize = true
            });
            nudCentroY = CriarNumero(new Point(120, 104), -50000, 50000, 2, 0.5m, 0);
            Controls.Add(nudCentroY);

            Controls.Add(new Label
            {
                Text = "Escala do template:",
                Location = new Point(20, 148),
                AutoSize = true
            });
            nudEscala = CriarNumero(new Point(120, 144), 0.25m, 4m, 2, 0.05m, 1m);
            nudEscala.ValueChanged += (_, _) => painelPreview.Invalidate();
            Controls.Add(nudEscala);

            lblDescricao = new Label
            {
                Location = new Point(20, 190),
                Size = new Size(560, 70),
                ForeColor = Color.Gainsboro
            };
            Controls.Add(lblDescricao);

            painelPreview = new Panel
            {
                Location = new Point(20, 270),
                Size = new Size(560, 140),
                BackColor = Color.FromArgb(37, 37, 38),
                BorderStyle = BorderStyle.FixedSingle
            };
            painelPreview.Paint += PainelPreview_Paint;
            Controls.Add(painelPreview);

            var lblNota = new Label
            {
                Text = "Origem: arquivos .csk da pasta Templates. O preview usa o conteúdo salvo pela própria aplicação.",
                Location = new Point(20, 420),
                Size = new Size(560, 34),
                ForeColor = Color.Silver
            };
            Controls.Add(lblNota);

            var btnInserir = new Button
            {
                Text = "Inserir template",
                Location = new Point(364, 18),
                Size = new Size(110, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Enabled = _templates.Count > 0
            };
            btnInserir.FlatAppearance.BorderSize = 0;
            btnInserir.Click += (_, _) => InserirTemplate();
            Controls.Add(btnInserir);

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(480, 18),
                Size = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            Controls.Add(btnCancelar);

            AcceptButton = btnInserir;
            CancelButton = btnCancelar;
        }

        private NumericUpDown CriarNumero(
            Point location,
            decimal minimum,
            decimal maximum,
            int decimalPlaces,
            decimal increment,
            decimal value)
        {
            return new NumericUpDown
            {
                Location = location,
                Size = new Size(120, 26),
                Minimum = minimum,
                Maximum = maximum,
                DecimalPlaces = decimalPlaces,
                Increment = increment,
                Value = value,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
        }

        private void AtualizarDescricao()
        {
            lblDescricao.Text = ObterTemplateSelecionado()?.Descricao ?? "Selecione um template .csk.";
        }

        private void PainelPreview_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(painelPreview.BackColor);

            var template = ObterTemplateSelecionado();
            if (template != null)
            {
                var preview = TrevoTemplateFactory.Criar(
                    template,
                    new PointF(painelPreview.Width / 2f, painelPreview.Height / 2f),
                    (float)nudEscala.Value * 0.18f);

                foreach (var obj in preview)
                {
                    obj.Desenhar(e.Graphics);
                }
            }

            using var pen = new Pen(Color.FromArgb(120, Color.White)) { DashStyle = DashStyle.Dash };
            e.Graphics.DrawLine(pen, painelPreview.Width / 2f, 0, painelPreview.Width / 2f, painelPreview.Height);
            e.Graphics.DrawLine(pen, 0, painelPreview.Height / 2f, painelPreview.Width, painelPreview.Height / 2f);
        }

        private void InserirTemplate()
        {
            var template = ObterTemplateSelecionado();
            if (template == null)
                return;

            ObjetosCriados = TrevoTemplateFactory.Criar(
                template,
                new PointF((float)nudCentroX.Value, (float)nudCentroY.Value),
                (float)nudEscala.Value);

            if (ObjetosCriados.Count == 0)
            {
                MessageBox.Show(
                    "Não foi possível carregar o template selecionado.",
                    "Templates de Trevos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private TemplateTrevoInfo ObterTemplateSelecionado()
        {
            return cmbTemplate.SelectedItem as TemplateTrevoInfo;
        }

        private static decimal AjustarValorNumerico(float valor, decimal minimo, decimal maximo)
        {
            decimal convertido = (decimal)valor;
            if (convertido < minimo)
                return minimo;
            if (convertido > maximo)
                return maximo;
            return convertido;
        }
    }
}
