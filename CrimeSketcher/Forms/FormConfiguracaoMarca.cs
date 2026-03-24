// Forms/FormConfiguracaoMarca.cs - Formulário de Configuração de Marca
using CrimeSketcher.Objects;
using CrimeSketcher.Tools;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Forms
{
    public class FormConfiguracaoMarca : Form
    {
        private ComboBox cmbTipoMarca;
        private NumericUpDown nudLargura;
        private ComboBox cmbIntensidade;
        private Button btnCor;
        private Panel painelCorAtual;
        private Panel painelPreview;

        public MarkTool MarkTool { get; private set; }

        public FormConfiguracaoMarca(MarkTool tool)
        {
            MarkTool = tool;
            InitializeComponent();
            CarregarValores();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuração de Marca";
            this.Size = new Size(450, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            int y = 20;
            int controlX = 165;

            // Tipo de marca
            Controls.Add(new Label
            {
                Text = "Tipo de marca:",
                Location = new Point(20, y + 3),
                AutoSize = true,
                ForeColor = Color.White
            });
            cmbTipoMarca = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
            cmbTipoMarca.Items.AddRange(new object[]
            {
                "Frenagem",
                "Derrapagem",
                "Sulco",
                "Arranhão",
                "Rastro",
                "Impacto",
                "Personalizada"
            });
            cmbTipoMarca.SelectedIndexChanged += (s, e) => AtualizarPreview();
            Controls.Add(cmbTipoMarca);
            y += 40;

            // Largura
            Controls.Add(new Label
            {
                Text = "Largura (pixels):",
                Location = new Point(20, y + 3),
                AutoSize = true,
                ForeColor = Color.White
            });
            nudLargura = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(100, 25),
                Minimum = 3,
                Maximum = 50,
                Value = 15,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
            nudLargura.ValueChanged += (s, e) => AtualizarPreview();
            Controls.Add(nudLargura);
            y += 40;

            // Intensidade
            Controls.Add(new Label
            {
                Text = "Intensidade:",
                Location = new Point(20, y + 3),
                AutoSize = true,
                ForeColor = Color.White
            });
            cmbIntensidade = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White
            };
            cmbIntensidade.Items.AddRange(new object[]
            {
                "Leve",
                "Média",
                "Forte",
                "Muito Forte"
            });
            cmbIntensidade.SelectedIndexChanged += (s, e) => AtualizarPreview();
            Controls.Add(cmbIntensidade);
            y += 40;

            // Cor
            Controls.Add(new Label
            {
                Text = "Cor da marca:",
                Location = new Point(20, y + 3),
                AutoSize = true,
                ForeColor = Color.White
            });
            painelCorAtual = new Panel
            {
                Location = new Point(controlX, y),
                Size = new Size(40, 25),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            Controls.Add(painelCorAtual);

            btnCor = new Button
            {
                Text = "Alterar Cor",
                Location = new Point(controlX + 50, y),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCor.FlatAppearance.BorderSize = 0;
            btnCor.Click += BtnCor_Click;
            Controls.Add(btnCor);
            y += 45;

            // Descrições dos tipos
            var lblDescricao = new Label
            {
                Text = GetDescricaoTipo(TipoMarca.Frenagem),
                Location = new Point(20, y),
                Size = new Size(390, 60),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8f, FontStyle.Italic)
            };
            Controls.Add(lblDescricao);
            cmbTipoMarca.SelectedIndexChanged += (s, e) =>
            {
                lblDescricao.Text = GetDescricaoTipo((TipoMarca)cmbTipoMarca.SelectedIndex);
            };
            y += 70;

            // Preview
            Controls.Add(new Label
            {
                Text = "Preview:",
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = Color.White
            });
            y += 25;

            painelPreview = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(390, 100),
                BackColor = Color.FromArgb(250, 250, 245),
                BorderStyle = BorderStyle.FixedSingle
            };
            painelPreview.Paint += PainelPreview_Paint;
            Controls.Add(painelPreview);
            y += 115;

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
        }

        private string GetDescricaoTipo(TipoMarca tipo)
        {
            return tipo switch
            {
                TipoMarca.Frenagem => "Marca de frenagem: Linhas paralelas escuras típicas de freadas bruscas. Comum em acidentes com veículos.",
                TipoMarca.Derrapagem => "Marca de derrapagem: Faixa larga contínua resultante de derrapagem lateral. Indica perda de controle.",
                TipoMarca.Sulco => "Sulco: Marca profunda no solo deixada por impacto ou arrasto pesado.",
                TipoMarca.Arranhao => "Arranhão: Linha fina e irregular, pode indicar raspagem ou atrito superficial.",
                TipoMarca.Rastro => "Rastro: Marca de arrasto, útil para indicar movimento de objetos ou corpos.",
                TipoMarca.Impacto => "Marca de impacto: Padrão irregular e disperso resultante de impacto ou queda.",
                TipoMarca.Personalizada => "Marca personalizada: Estilo básico para casos específicos.",
                _ => ""
            };
        }

        private void CarregarValores()
        {
            cmbTipoMarca.SelectedIndex = (int)MarkTool.TipoMarcaPadrao;
            nudLargura.Value = (decimal)MarkTool.LarguraPadrao;
            cmbIntensidade.SelectedIndex = (int)MarkTool.IntensidadePadrao;
            painelCorAtual.BackColor = MarkTool.CorPadrao;
        }

        private void BtnCor_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = painelCorAtual.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    painelCorAtual.BackColor = colorDialog.Color;
                    AtualizarPreview();
                }
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            MarkTool.TipoMarcaPadrao = (TipoMarca)cmbTipoMarca.SelectedIndex;
            MarkTool.LarguraPadrao = (float)nudLargura.Value;
            MarkTool.IntensidadePadrao = (IntensidadeMarca)cmbIntensidade.SelectedIndex;
            MarkTool.CorPadrao = painelCorAtual.BackColor;
        }

        private void AtualizarPreview()
        {
            painelPreview.Invalidate();
        }

        private void PainelPreview_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Criar marca de preview
            var preview = new MarkObject
            {
                PontoInicial = new PointF(30, painelPreview.Height / 2),
                PontoFinal = new PointF(painelPreview.Width - 30, painelPreview.Height / 2),
                TipoMarca = (TipoMarca)cmbTipoMarca.SelectedIndex,
                Largura = (float)nudLargura.Value,
                Intensidade = (IntensidadeMarca)cmbIntensidade.SelectedIndex,
                CorMarca = painelCorAtual.BackColor,
                Visivel = true
            };

            preview.Desenhar(g);
        }
    }
}
