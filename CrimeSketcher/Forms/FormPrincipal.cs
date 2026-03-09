// Forms/FormPrincipal.cs
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Library;
using CrimeSketcher.Objects;
using CrimeSketcher.Tools;

namespace CrimeSketcher.Forms
{
    public class FormPrincipal : Form
    {
        #region Campos

        // Componentes principais
        private SketchCanvas canvas;
        private SketchDocument documento;
        private ScaleManager escala;
        private GridManager grid;
        private SymbolLibrary biblioteca;
        private UndoRedoManager undoRedo;

        // Ferramentas
        private SelectTool selectTool;
        private WallTool wallTool;
        private RoomTool roomTool;
        private StreetTool streetTool;
        private DimensionTool dimensionTool;
        private StampTool stampTool;
        private StickFigureTool stickFigureTool;
        private TextTool textTool;
        private ArrowTool arrowTool;

        // UI Principal
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;

        // Layout
        private SplitContainer splitPrincipal;      // Esquerda | Centro+Direita
        private SplitContainer splitCentroDireita;  // Centro | Direita

        // Painel Esquerdo
        private Panel painelEsquerdo;
        private Panel painelFerramentas;
        private TabControl tabBiblioteca;

        // Painel Direito
        private Panel painelDireito;
        private PropertyGrid propGrid;

        // Status
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel statusCoord;
        private ToolStripStatusLabel statusEscala;
        private ToolStripStatusLabel statusZoom;
        private ToolStripStatusLabel statusSnap;

        // Controle
        private string _arquivoAtual;
        private Button _botaoFerramentaAtiva;

        #endregion

        public FormPrincipal()
        {
            InitializeComponent();
            InicializarSistema();
        }

        #region Inicialização da Interface

        private void InitializeComponent()
        {
            // ===== FORM =====
            this.Text = "🔍 CrimeSketcher - Croqui de Local de Crime";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.KeyPreview = true;
            this.BackColor = Color.FromArgb(45, 45, 48);

            // ===== MENU =====
            CriarMenu();

            // ===== TOOLBAR =====
            CriarToolbar();

            // ===== STATUS BAR =====
            CriarStatusBar();

            // ===== LAYOUT PRINCIPAL =====
            CriarLayout();

            // Adicionar controles ao form na ordem correta
            this.Controls.Add(splitPrincipal);
            splitPrincipal.Panel1MinSize = 200;
            splitPrincipal.Panel2MinSize = 400;
            splitPrincipal.SplitterDistance = 240;
            this.Controls.Add(toolStrip);
            this.Controls.Add(menuStrip);
            this.Controls.Add(statusStrip);
            this.MainMenuStrip = menuStrip;
            
            // Agora é seguro definir o SplitterDistance
            splitPrincipal.SplitterDistance = 240;
        }

        private void CriarMenu()
        {
            menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.FromArgb(45, 45, 48);
            menuStrip.ForeColor = Color.White;

            // Arquivo
            var mnuArquivo = new ToolStripMenuItem("&Arquivo");
            mnuArquivo.DropDownItems.AddRange(new ToolStripItem[]
            {
                CriarMenuItem("&Novo", "Ctrl+N", NovoDocumento),
                CriarMenuItem("&Abrir...", "Ctrl+O", AbrirDocumento),
                CriarMenuItem("&Salvar", "Ctrl+S", SalvarDocumento),
                CriarMenuItem("Salvar &Como...", "Ctrl+Shift+S", SalvarComo),
                new ToolStripSeparator(),
                CriarMenuItem("Exportar como &Imagem...", "", ExportarImagem),
                CriarMenuItem("Exportar como &PDF...", "", ExportarPDF),
                new ToolStripSeparator(),
                CriarMenuItem("&Imprimir...", "Ctrl+P", Imprimir),
                new ToolStripSeparator(),
                CriarMenuItem("&Sair", "Alt+F4", () => Close())
            });

            // Editar
            var mnuEditar = new ToolStripMenuItem("&Editar");
            mnuEditar.DropDownItems.AddRange(new ToolStripItem[]
            {
                CriarMenuItem("&Desfazer", "Ctrl+Z", () => undoRedo?.Desfazer()),
                CriarMenuItem("&Refazer", "Ctrl+Y", () => undoRedo?.Refazer()),
                new ToolStripSeparator(),
                CriarMenuItem("&Copiar", "Ctrl+C", Copiar),
                CriarMenuItem("C&olar", "Ctrl+V", Colar),
                CriarMenuItem("Recor&tar", "Ctrl+X", Recortar),
                new ToolStripSeparator(),
                CriarMenuItem("&Excluir", "Delete", ExcluirSelecao),
                CriarMenuItem("Selecionar &Tudo", "Ctrl+A", SelecionarTudo)
            });

            // Exibir
            var mnuExibir = new ToolStripMenuItem("E&xibir");

            var mnuGrid = new ToolStripMenuItem("Mostrar &Grade");
            mnuGrid.Checked = true;
            mnuGrid.Click += (s, e) =>
            {
                mnuGrid.Checked = !mnuGrid.Checked;
                grid.Visivel = mnuGrid.Checked;
                canvas.Invalidate();
            };

            var mnuSnap = new ToolStripMenuItem("&Snap to Grid");
            mnuSnap.Checked = true;
            mnuSnap.Click += (s, e) =>
            {
                mnuSnap.Checked = !mnuSnap.Checked;
                grid.SnapAtivo = mnuSnap.Checked;
                AtualizarStatusSnap();
            };

            mnuExibir.DropDownItems.AddRange(new ToolStripItem[]
            {
                mnuGrid,
                mnuSnap,
                new ToolStripSeparator(),
                CriarMenuItem("&Centralizar Vista", "", () => canvas.CentralizarVista()),
                CriarMenuItem("Zoom para &Tudo", "Ctrl+0", () => canvas.ZoomParaMostrarTudo()),
                CriarMenuItem("Zoom &100%", "Ctrl+1", () => DefinirZoom(1f)),
                new ToolStripSeparator(),
                CriarMenuItem("Configurar &Escala...", "", ConfigurarEscala)
            });

            // Inserir
            var mnuInserir = new ToolStripMenuItem("&Inserir");
            mnuInserir.DropDownItems.AddRange(new ToolStripItem[]
            {
                CriarMenuItem("&Rosa dos Ventos", "", InserirRosaDosVentos),
                CriarMenuItem("&Legenda/Carimbo", "", InserirCarimbo),
                new ToolStripSeparator(),
                CriarMenuItem("&Imagem do Arquivo...", "", InserirImagemArquivo)
            });

            // Ajuda
            var mnuAjuda = new ToolStripMenuItem("A&juda");
            mnuAjuda.DropDownItems.AddRange(new ToolStripItem[]
            {
                CriarMenuItem("&Atalhos de Teclado", "F1", MostrarAtalhos),
                new ToolStripSeparator(),
                CriarMenuItem("&Sobre", "", MostrarSobre)
            });

            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                mnuArquivo, mnuEditar, mnuExibir, mnuInserir, mnuAjuda
            });
        }

        private ToolStripMenuItem CriarMenuItem(string texto, string atalho, Action acao)
        {
            var item = new ToolStripMenuItem(texto);
            if (!string.IsNullOrEmpty(atalho))
                item.ShortcutKeyDisplayString = atalho;
            item.Click += (s, e) => acao();
            return item;
        }

        private void CriarToolbar()
        {
            toolStrip = new ToolStrip();
            toolStrip.BackColor = Color.FromArgb(62, 62, 66);
            toolStrip.ForeColor = Color.White;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Padding = new Padding(5, 2, 5, 2);
            toolStrip.ImageScalingSize = new Size(20, 20);

            // ===== ARQUIVO =====
            toolStrip.Items.Add(CriarBotaoToolbar("📄", "Novo (Ctrl+N)", NovoDocumento));
            toolStrip.Items.Add(CriarBotaoToolbar("📂", "Abrir (Ctrl+O)", AbrirDocumento));
            toolStrip.Items.Add(CriarBotaoToolbar("💾", "Salvar (Ctrl+S)", SalvarDocumento));

            toolStrip.Items.Add(new ToolStripSeparator());

            // ===== EDIÇÃO =====
            toolStrip.Items.Add(CriarBotaoToolbar("↩️", "Desfazer (Ctrl+Z)", () => undoRedo?.Desfazer()));
            toolStrip.Items.Add(CriarBotaoToolbar("↪️", "Refazer (Ctrl+Y)", () => undoRedo?.Refazer()));

            toolStrip.Items.Add(new ToolStripSeparator());

            toolStrip.Items.Add(CriarBotaoToolbar("📋", "Copiar (Ctrl+C)", Copiar));
            toolStrip.Items.Add(CriarBotaoToolbar("📌", "Colar (Ctrl+V)", Colar));
            toolStrip.Items.Add(CriarBotaoToolbar("✂️", "Recortar (Ctrl+X)", Recortar));

            toolStrip.Items.Add(new ToolStripSeparator());

            // ===== AGRUPAR =====
            toolStrip.Items.Add(CriarBotaoToolbar("🔗", "Agrupar (Ctrl+G)", Agrupar));
            toolStrip.Items.Add(CriarBotaoToolbar("⛓️‍💥", "Desagrupar (Ctrl+Shift+G)", Desagrupar));

            toolStrip.Items.Add(new ToolStripSeparator());

            // ===== ZOOM =====
            toolStrip.Items.Add(CriarBotaoToolbar("🔍+", "Zoom + (Ctrl++)", () => AlterarZoom(1.25f)));
            toolStrip.Items.Add(CriarBotaoToolbar("🔍-", "Zoom - (Ctrl+-)", () => AlterarZoom(1f / 1.25f)));
            toolStrip.Items.Add(CriarBotaoToolbar("🔍◻", "Zoom Tudo (Ctrl+0)", () => canvas.ZoomParaMostrarTudo()));

            // ComboBox de Zoom
            var cmbZoom = new ToolStripComboBox();
            cmbZoom.Items.AddRange(new object[] { "25%", "50%", "75%", "100%", "150%", "200%", "400%" });
            cmbZoom.Text = "100%";
            cmbZoom.Width = 70;
            cmbZoom.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbZoom.SelectedIndexChanged += (s, e) =>
            {
                string val = cmbZoom.Text.Replace("%", "");
                if (float.TryParse(val, out float zoom))
                    DefinirZoom(zoom / 100f);
            };
            toolStrip.Items.Add(cmbZoom);

            toolStrip.Items.Add(new ToolStripSeparator());

            // ===== ORDENAÇÃO =====
            toolStrip.Items.Add(CriarBotaoToolbar("⬆️", "Trazer para Frente", TrazerParaFrente));
            toolStrip.Items.Add(CriarBotaoToolbar("⬇️", "Enviar para Trás", EnviarParaTras));
            toolStrip.Items.Add(CriarBotaoToolbar("🔼", "Avançar Uma Camada", AvancarCamada));
            toolStrip.Items.Add(CriarBotaoToolbar("🔽", "Recuar Uma Camada", RecuarCamada));

            toolStrip.Items.Add(new ToolStripSeparator());

            // ===== ALINHAMENTO =====
            toolStrip.Items.Add(CriarBotaoToolbar("⬅", "Alinhar à Esquerda", () => Alinhar("esquerda")));
            toolStrip.Items.Add(CriarBotaoToolbar("⬌", "Centralizar Horizontal", () => Alinhar("centro_h")));
            toolStrip.Items.Add(CriarBotaoToolbar("➡", "Alinhar à Direita", () => Alinhar("direita")));
            toolStrip.Items.Add(CriarBotaoToolbar("⬆", "Alinhar ao Topo", () => Alinhar("topo")));
            toolStrip.Items.Add(CriarBotaoToolbar("⬍", "Centralizar Vertical", () => Alinhar("centro_v")));
            toolStrip.Items.Add(CriarBotaoToolbar("⬇", "Alinhar à Base", () => Alinhar("base")));

            toolStrip.Items.Add(new ToolStripSeparator());

            // ===== SNAP =====
            var btnSnap = new ToolStripButton("⊞ SNAP");
            btnSnap.CheckOnClick = true;
            btnSnap.Checked = true;
            btnSnap.ToolTipText = "Snap to Grid (G)";
            btnSnap.ForeColor = Color.LightGreen;
            btnSnap.CheckedChanged += (s, e) =>
            {
                grid.SnapAtivo = btnSnap.Checked;
                btnSnap.ForeColor = btnSnap.Checked ? Color.LightGreen : Color.Gray;
                AtualizarStatusSnap();
                canvas.Invalidate();
            };
            toolStrip.Items.Add(btnSnap);
        }

        private ToolStripButton CriarBotaoToolbar(string emoji, string tooltip, Action acao)
        {
            var btn = new ToolStripButton(emoji);
            btn.ToolTipText = tooltip;
            btn.Font = new Font("Segoe UI Emoji", 10);
            btn.ForeColor = Color.White;
            btn.Click += (s, e) => acao();
            return btn;
        }

        private void CriarStatusBar()
        {
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(0, 122, 204);

            statusLabel = new ToolStripStatusLabel("Pronto")
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White
            };

            statusCoord = new ToolStripStatusLabel("X: 0  Y: 0")
            {
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                ForeColor = Color.White
            };

            statusEscala = new ToolStripStatusLabel("Escala: 1:100")
            {
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                ForeColor = Color.White
            };

            statusZoom = new ToolStripStatusLabel("Zoom: 100%")
            {
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                ForeColor = Color.White
            };

            statusSnap = new ToolStripStatusLabel("SNAP: ON")
            {
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            statusStrip.Items.AddRange(new ToolStripItem[]
            {
                statusLabel, statusCoord, statusEscala, statusZoom, statusSnap
            });
        }

        private void CriarLayout()
        {
            // ===== SPLIT PRINCIPAL: Esquerda | (Centro + Direita) =====
            splitPrincipal = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 3,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            // Definir os MinSize DEPOIS ou configurar após adicionar ao form

            // ===== SPLIT CENTRO/DIREITA: Canvas | Propriedades =====
            splitCentroDireita = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 3,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // ===== PAINEL ESQUERDO =====
            CriarPainelEsquerdo();
            splitPrincipal.Panel1.Controls.Add(painelEsquerdo);

            // ===== CANVAS (Centro) =====
            canvas = new SketchCanvas
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 245)
            };
            canvas.CursorMoved += Canvas_CursorMoved;
            canvas.ZoomChanged += Canvas_ZoomChanged;

            var borderCanvas = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                BackColor = Color.FromArgb(80, 80, 85)
            };
            borderCanvas.Controls.Add(canvas);
            splitCentroDireita.Panel1.Controls.Add(borderCanvas);

            // ===== PAINEL DIREITO (Propriedades) =====
            CriarPainelDireito();
            splitCentroDireita.Panel2.Controls.Add(painelDireito);

            // Montar hierarquia
            splitPrincipal.Panel2.Controls.Add(splitCentroDireita);
        }

        private void CriarPainelEsquerdo()
        {
            painelEsquerdo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38)
            };

            // Split interno: Ferramentas (topo) | Biblioteca (baixo)
            var splitEsquerdo = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 380,
                SplitterWidth = 3,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // ===== PAINEL FERRAMENTAS =====
            CriarPainelFerramentas();
            splitEsquerdo.Panel1.Controls.Add(painelFerramentas);

            // ===== BIBLIOTECA =====
            CriarPainelBiblioteca();
            splitEsquerdo.Panel2.Controls.Add(tabBiblioteca);

            painelEsquerdo.Controls.Add(splitEsquerdo);
        }

        private void CriarPainelFerramentas()
        {
            painelFerramentas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38),
                AutoScroll = true
            };

            // Título
            var lblTitulo = new Label
            {
                Text = "🔧 FERRAMENTAS",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Height = 32
            };

            // Container das ferramentas
            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(5),
                BackColor = Color.FromArgb(37, 37, 38)
            };

            // ===== GRUPO: SELEÇÃO =====
            container.Controls.Add(CriarGrupoFerramentas("Seleção", new[]
            {
                ("🖱️ Selecionar", "Selecionar", "V"),
            }));

            // ===== GRUPO: CONSTRUÇÃO =====
            container.Controls.Add(CriarGrupoFerramentas("Construção", new[]
            {
                ("🧱 Parede", "Parede", "W"),
                ("🚪 Parede + Porta", "ParedePorta", ""),
                ("🪟 Parede + Janela", "ParedeJanela", ""),
                ("🏠 Cômodo", "Comodo", "R"),
            }));

            // ===== GRUPO: VIAS =====
            container.Controls.Add(CriarGrupoFerramentas("Vias e Externos", new[]
            {
                ("🛣️ Rua", "Rua", "S"),
            }));

            // ===== GRUPO: MEDIÇÕES =====
            container.Controls.Add(CriarGrupoFerramentas("Medições e Indicações", new[]
            {
                ("📏 Cota/Medida", "Cota", "D"),
                ("➡️ Seta", "Seta", "A"),
                ("🏷️ Texto", "Texto", "T"),
            }));

            // ===== GRUPO: CORPOS =====
            container.Controls.Add(CriarGrupoFerramentas("Representação de Corpos", new[]
            {
                ("🧍 Em Pé", "CorpoEmPe", ""),
                ("🛌 Deitado (Supino)", "CorpoDeitado", ""),
                ("🙇 Deitado (Bruços)", "CorpoBrucos", ""),
                ("🪑 Sentado", "CorpoSentado", ""),
                ("🧎 Ajoelhado", "CorpoAjoelhado", ""),
            }));

            painelFerramentas.Controls.Add(container);
            painelFerramentas.Controls.Add(lblTitulo);
        }

        private GroupBox CriarGrupoFerramentas(string titulo, (string texto, string tool, string atalho)[] ferramentas)
        {
            var grp = new GroupBox
            {
                Text = titulo,
                Width = 220,
                Height = 25 + ferramentas.Length * 34,
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 5)
            };

            int y = 20;
            foreach (var (texto, tool, atalho) in ferramentas)
            {
                var btn = new Button
                {
                    Text = texto + (!string.IsNullOrEmpty(atalho) ? $"  ({atalho})" : ""),
                    Location = new Point(8, y),
                    Size = new Size(200, 30),
                    TextAlign = ContentAlignment.MiddleLeft,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(55, 55, 58),
                    Cursor = Cursors.Hand,
                    Tag = tool
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 70, 75);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 122, 204);

                btn.Click += (s, e) =>
                {
                    DefinirFerramenta(tool);
                    MarcarBotaoAtivo(btn);
                };

                grp.Controls.Add(btn);
                y += 32;
            }

            return grp;
        }

        private void MarcarBotaoAtivo(Button btn)
        {
            // Desmarcar anterior
            if (_botaoFerramentaAtiva != null)
            {
                _botaoFerramentaAtiva.BackColor = Color.FromArgb(55, 55, 58);
                _botaoFerramentaAtiva.ForeColor = Color.White;
            }

            // Marcar novo
            _botaoFerramentaAtiva = btn;
            btn.BackColor = Color.FromArgb(0, 122, 204);
            btn.ForeColor = Color.White;
        }

        private void CriarPainelBiblioteca()
        {
            tabBiblioteca = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5f)
            };

            // Título sobre as tabs
            var lblTitulo = new Label
            {
                Text = "📚 BIBLIOTECA DE SÍMBOLOS",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(104, 33, 122),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Height = 32
            };

            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38)
            };

            container.Controls.Add(tabBiblioteca);
            container.Controls.Add(lblTitulo);

            // Substituir tabBiblioteca pelo container
            tabBiblioteca = new TabControl { Dock = DockStyle.Fill };

            var wrapper = new Panel { Dock = DockStyle.Fill };
            wrapper.Controls.Add(tabBiblioteca);
            wrapper.Controls.Add(lblTitulo);

            // Limpar e usar wrapper
            tabBiblioteca.Parent?.Controls.Remove(tabBiblioteca);
            tabBiblioteca = new TabControl { Dock = DockStyle.Fill };
        }

        private void CriarPainelDireito()
        {
            painelDireito = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38)
            };

            // Título
            var lblTitulo = new Label
            {
                Text = "📋 PROPRIEDADES",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Height = 32
            };

            // PropertyGrid
            propGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38),
                ViewBackColor = Color.FromArgb(45, 45, 48),
                ViewForeColor = Color.White,
                LineColor = Color.FromArgb(60, 60, 65),
                CategoryForeColor = Color.LightGray,
                PropertySort = PropertySort.Categorized,
                HelpVisible = true,
                HelpBackColor = Color.FromArgb(45, 45, 48),
                HelpForeColor = Color.Silver
            };
            propGrid.PropertyValueChanged += PropGrid_ValueChanged;

            // Info quando nada selecionado
            var lblInfo = new Label
            {
                Text = "Selecione um objeto para ver suas propriedades",
                Dock = DockStyle.Fill,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            painelDireito.Controls.Add(propGrid);
            painelDireito.Controls.Add(lblTitulo);
        }

        #endregion

        #region Inicialização do Sistema

        private void InicializarSistema()
        {
            // Escala e Grade
            escala = new ScaleManager();
            grid = new GridManager(escala);

            // Documento
            documento = new SketchDocument();
            undoRedo = documento.UndoRedo;
            undoRedo.EstadoAlterado += (s, e) => canvas.Invalidate();

            // Canvas
            canvas.Escala = escala;
            canvas.Grid = grid;
            canvas.Documento = documento;

            // Criar todas as ferramentas
            CriarFerramentas();

            // Definir ferramenta inicial
            canvas.FerramentaAtual = selectTool;

            // Biblioteca
            string symbolsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Symbols");
            biblioteca = new SymbolLibrary(symbolsPath);
            PreencherBiblioteca();

            // Atualizar status
            statusEscala.Text = $"Escala: {escala.TextoEscala}";
            AtualizarStatusSnap();
        }

        private void CriarFerramentas()
        {
            selectTool = new SelectTool(documento, undoRedo);
            selectTool.SelectionChanged += (s, obj) =>
            {
                propGrid.SelectedObject = obj;
                statusLabel.Text = obj != null
                    ? $"Selecionado: {obj.Tipo} - {obj.Nome}"
                    : "Pronto";
            };

            wallTool = new WallTool(documento, grid);
            roomTool = new RoomTool(documento, grid);
            streetTool = new StreetTool(documento, grid);
            dimensionTool = new DimensionTool(documento, grid, escala);
            stampTool = new StampTool(documento, grid);
            stickFigureTool = new StickFigureTool(documento, grid);
            textTool = new TextTool(documento, grid);
            arrowTool = new ArrowTool(documento, grid);
        }

        private void RecriarFerramentas()
        {
            undoRedo = documento.UndoRedo;
            undoRedo.EstadoAlterado += (s, e) => canvas.Invalidate();

            selectTool = new SelectTool(documento, undoRedo);
            selectTool.SelectionChanged += (s, obj) =>
            {
                propGrid.SelectedObject = obj;
                statusLabel.Text = obj != null
                    ? $"Selecionado: {obj.Tipo} - {obj.Nome}"
                    : "Pronto";
            };

            wallTool = new WallTool(documento, grid);
            roomTool = new RoomTool(documento, grid);
            streetTool = new StreetTool(documento, grid);
            dimensionTool = new DimensionTool(documento, grid, escala);
            stampTool = new StampTool(documento, grid);
            stickFigureTool = new StickFigureTool(documento, grid);
            textTool = new TextTool(documento, grid);
            arrowTool = new ArrowTool(documento, grid);

            canvas.FerramentaAtual = selectTool;
        }

        private void PreencherBiblioteca()
        {
            tabBiblioteca.TabPages.Clear();

            foreach (var cat in biblioteca.Categorias)
            {
                var tabPage = new TabPage(cat.Nome);
                tabPage.BackColor = Color.FromArgb(45, 45, 48);

                var listView = new ListView
                {
                    Dock = DockStyle.Fill,
                    View = View.LargeIcon,
                    BackColor = Color.FromArgb(45, 45, 48),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    LargeImageList = new ImageList
                    {
                        ImageSize = new Size(48, 48),
                        ColorDepth = ColorDepth.Depth32Bit
                    }
                };

                int imgIndex = 0;
                foreach (var item in cat.Itens)
                {
                    if (item.Thumbnail != null)
                    {
                        listView.LargeImageList.Images.Add(item.Nome, item.Thumbnail);
                        listView.Items.Add(new ListViewItem(item.Nome, imgIndex++)
                        {
                            Tag = item
                        });
                    }
                }

                listView.ItemActivate += (s, e) =>
                {
                    if (listView.SelectedItems.Count > 0)
                    {
                        var simbolo = (SymbolItem)listView.SelectedItems[0].Tag;
                        stampTool.SimboloAtual = simbolo;
                        canvas.FerramentaAtual = stampTool;
                        statusLabel.Text = $"Símbolo selecionado: {simbolo.Nome} - Clique para posicionar";
                    }
                };

                // Botão importar
                var btnImportar = new Button
                {
                    Text = "➕ Importar Símbolo...",
                    Dock = DockStyle.Bottom,
                    Height = 28,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(60, 60, 65),
                    ForeColor = Color.White
                };
                btnImportar.FlatAppearance.BorderSize = 0;

                string catNome = cat.Nome;
                btnImportar.Click += (s, e) =>
                {
                    using (var dlg = new OpenFileDialog())
                    {
                        dlg.Filter = "Imagens PNG|*.png|Todas|*.*";
                        dlg.Title = "Importar Símbolo";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            string nome = Path.GetFileNameWithoutExtension(dlg.FileName);
                            biblioteca.ImportarSimbolo(dlg.FileName, catNome, nome);
                            PreencherBiblioteca();
                        }
                    }
                };

                tabPage.Controls.Add(listView);
                tabPage.Controls.Add(btnImportar);
                tabBiblioteca.TabPages.Add(tabPage);
            }
        }

        #endregion

        #region Ações de Ferramentas

        private void DefinirFerramenta(string ferramenta)
        {
            switch (ferramenta)
            {
                case "Selecionar":
                    canvas.FerramentaAtual = selectTool;
                    break;

                case "Parede":
                    wallTool.ComPorta = false;
                    wallTool.ComJanela = false;
                    canvas.FerramentaAtual = wallTool;
                    break;

                case "ParedePorta":
                    wallTool.ComPorta = true;
                    wallTool.ComJanela = false;
                    canvas.FerramentaAtual = wallTool;
                    break;

                case "ParedeJanela":
                    wallTool.ComPorta = false;
                    wallTool.ComJanela = true;
                    canvas.FerramentaAtual = wallTool;
                    break;

                case "Comodo":
                    string nomeComodo = InputBox("Nome do cômodo:", "Novo Cômodo", "Sala");
                    if (!string.IsNullOrEmpty(nomeComodo))
                    {
                        roomTool.NomeComodo = nomeComodo;
                        canvas.FerramentaAtual = roomTool;
                    }
                    break;

                case "Rua":
                    string nomeRua = InputBox("Nome da rua:", "Nova Rua", "");
                    streetTool.NomeRua = nomeRua;
                    canvas.FerramentaAtual = streetTool;
                    break;

                case "Cota":
                    canvas.FerramentaAtual = dimensionTool;
                    break;

                case "Texto":
                    canvas.FerramentaAtual = textTool;
                    break;

                case "Seta":
                    canvas.FerramentaAtual = arrowTool;
                    break;

                case "CorpoEmPe":
                    ConfigurarCorpo("EmPe");
                    break;

                case "CorpoDeitado":
                    ConfigurarCorpo("Deitado");
                    break;

                case "CorpoBrucos":
                    ConfigurarCorpo("DeitadoBrucos");
                    break;

                case "CorpoSentado":
                    ConfigurarCorpo("Sentado");
                    break;

                case "CorpoAjoelhado":
                    ConfigurarCorpo("Ajoelhado");
                    break;
            }

            statusLabel.Text = $"Ferramenta: {ferramenta}";
        }

        private void ConfigurarCorpo(string pose)
        {
            string rotulo = InputBox("Identificação do corpo:", "Corpo", "Vítima");
            stickFigureTool.PoseInicial = pose;
            stickFigureTool.Rotulo = rotulo;
            canvas.FerramentaAtual = stickFigureTool;
        }

        #endregion

        #region Ações de Menu/Toolbar

        private void NovoDocumento()
        {
            if (documento.Objetos.Count > 0)
            {
                var result = MessageBox.Show(
                    "Criar novo croqui?\nAlterações não salvas serão perdidas.",
                    "Novo Croqui",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }

            documento = new SketchDocument();
            canvas.Documento = documento;
            RecriarFerramentas();

            _arquivoAtual = null;
            this.Text = "🔍 CrimeSketcher - Novo Croqui";
            propGrid.SelectedObject = null;
            canvas.CentralizarVista();
            canvas.Invalidate();
        }

        private void AbrirDocumento()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Croqui CrimeSketcher|*.csk|Todos|*.*";
                dlg.Title = "Abrir Croqui";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        documento = SketchDocument.Carregar(dlg.FileName);
                        canvas.Documento = documento;
                        RecriarFerramentas();

                        _arquivoAtual = dlg.FileName;
                        this.Text = $"🔍 CrimeSketcher - {Path.GetFileName(dlg.FileName)}";
                        canvas.ZoomParaMostrarTudo();
                        statusLabel.Text = "Arquivo carregado com sucesso!";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao abrir arquivo:\n{ex.Message}",
                            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SalvarDocumento()
        {
            if (string.IsNullOrEmpty(_arquivoAtual))
            {
                SalvarComo();
                return;
            }

            try
            {
                documento.Salvar(_arquivoAtual);
                statusLabel.Text = "✓ Salvo com sucesso!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SalvarComo()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Croqui CrimeSketcher|*.csk";
                dlg.Title = "Salvar Croqui Como";
                dlg.DefaultExt = "csk";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _arquivoAtual = dlg.FileName;
                    SalvarDocumento();
                    this.Text = $"🔍 CrimeSketcher - {Path.GetFileName(dlg.FileName)}";
                }
            }
        }

        private void ExportarImagem()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp";
                dlg.Title = "Exportar como Imagem";
                dlg.DefaultExt = "png";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var bmp = canvas.ExportarImagem(3000, 2000))
                        {
                            var format = System.Drawing.Imaging.ImageFormat.Png;
                            if (dlg.FileName.ToLower().EndsWith(".jpg"))
                                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            else if (dlg.FileName.ToLower().EndsWith(".bmp"))
                                format = System.Drawing.Imaging.ImageFormat.Bmp;

                            bmp.Save(dlg.FileName, format);
                        }
                        statusLabel.Text = $"✓ Exportado: {Path.GetFileName(dlg.FileName)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao exportar:\n{ex.Message}",
                            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportarPDF()
        {
            MessageBox.Show(
                "Para exportar como PDF, exportue primeiro como imagem (PNG)\n" +
                "e depois converta usando um editor de PDF.\n\n" +
                "Para integração nativa com PDF, instale o pacote\n" +
                "Syncfusion.Pdf.Net.Core via NuGet.",
                "Exportar PDF",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ExportarImagem();
        }

        private void Imprimir()
        {
            using (var pd = new System.Drawing.Printing.PrintDocument())
            using (var dlg = new PrintDialog { Document = pd })
            {
                pd.PrintPage += (s, e) =>
                {
                    using (var bmp = canvas.ExportarImagem(
                        (int)e.PageBounds.Width * 2,
                        (int)e.PageBounds.Height * 2))
                    {
                        e.Graphics.DrawImage(bmp, e.MarginBounds);
                    }
                };

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    pd.Print();
                    statusLabel.Text = "Enviado para impressão";
                }
            }
        }

        private BaseSketchObject _objetoCopiado;

        private void Copiar()
        {
            if (selectTool.ObjetoSelecionado != null)
            {
                _objetoCopiado = selectTool.ObjetoSelecionado;
                statusLabel.Text = $"Copiado: {_objetoCopiado.Tipo}";
            }
        }

        private void Colar()
        {
            // Implementação simplificada - seria necessário clonar o objeto
            if (_objetoCopiado != null)
            {
                statusLabel.Text = "Função Colar em desenvolvimento";
            }
        }

        private void Recortar()
        {
            if (selectTool.ObjetoSelecionado != null)
            {
                _objetoCopiado = selectTool.ObjetoSelecionado;
                documento.RemoverObjeto(selectTool.ObjetoSelecionado);
                propGrid.SelectedObject = null;
                canvas.Invalidate();
                statusLabel.Text = $"Recortado: {_objetoCopiado.Tipo}";
            }
        }

        private void ExcluirSelecao()
        {
            if (selectTool.ObjetoSelecionado != null)
            {
                documento.RemoverObjeto(selectTool.ObjetoSelecionado);
                propGrid.SelectedObject = null;
                canvas.Invalidate();
                statusLabel.Text = "Objeto excluído";
            }
        }

        private void SelecionarTudo()
        {
            foreach (var obj in documento.Objetos)
            {
                obj.Selecionado = true;
            }
            canvas.Invalidate();
            statusLabel.Text = $"{documento.Objetos.Count} objetos selecionados";
        }

        private void Agrupar()
        {
            statusLabel.Text = "Função Agrupar em desenvolvimento";
        }

        private void Desagrupar()
        {
            statusLabel.Text = "Função Desagrupar em desenvolvimento";
        }

        private void AlterarZoom(float fator)
        {
            escala.ZoomLevel = Math.Max(0.1f, Math.Min(10f, escala.ZoomLevel * fator));
            statusZoom.Text = $"Zoom: {escala.ZoomLevel * 100:F0}%";
            canvas.Invalidate();
        }

        private void DefinirZoom(float zoom)
        {
            escala.ZoomLevel = Math.Max(0.1f, Math.Min(10f, zoom));
            statusZoom.Text = $"Zoom: {escala.ZoomLevel * 100:F0}%";
            canvas.Invalidate();
        }

        private void TrazerParaFrente()
        {
            if (selectTool.ObjetoSelecionado != null)
            {
                var obj = selectTool.ObjetoSelecionado;
                documento.Objetos.Remove(obj);
                documento.Objetos.Add(obj);
                canvas.Invalidate();
            }
        }

        private void EnviarParaTras()
        {
            if (selectTool.ObjetoSelecionado != null)
            {
                var obj = selectTool.ObjetoSelecionado;
                documento.Objetos.Remove(obj);
                documento.Objetos.Insert(0, obj);
                canvas.Invalidate();
            }
        }

        private void AvancarCamada()
        {
            if (selectTool.ObjetoSelecionado != null)
            {
                var obj = selectTool.ObjetoSelecionado;
                int index = documento.Objetos.IndexOf(obj);
                if (index < documento.Objetos.Count - 1)
                {
                    documento.Objetos.RemoveAt(index);
                    documento.Objetos.Insert(index + 1, obj);
                    canvas.Invalidate();
                }
            }
        }

        private void RecuarCamada()
        {
            if (selectTool.ObjetoSelecionado != null)
            {
                var obj = selectTool.ObjetoSelecionado;
                int index = documento.Objetos.IndexOf(obj);
                if (index > 0)
                {
                    documento.Objetos.RemoveAt(index);
                    documento.Objetos.Insert(index - 1, obj);
                    canvas.Invalidate();
                }
            }
        }

        private void Alinhar(string tipo)
        {
            // Pega todos os objetos selecionados ou o objeto atual
            var selecionados = documento.Objetos.Where(o => o.Selecionado).ToList();
            if (selecionados.Count < 2)
            {
                statusLabel.Text = "Selecione pelo menos 2 objetos para alinhar";
                return;
            }

            var bounds = selecionados.Select(o => o.GetBounds()).ToList();

            switch (tipo)
            {
                case "esquerda":
                    float minX = bounds.Min(b => b.Left);
                    foreach (var obj in selecionados)
                    {
                        float dx = minX - obj.GetBounds().Left;
                        obj.Mover(dx, 0);
                    }
                    break;

                case "direita":
                    float maxX = bounds.Max(b => b.Right);
                    foreach (var obj in selecionados)
                    {
                        float dx = maxX - obj.GetBounds().Right;
                        obj.Mover(dx, 0);
                    }
                    break;

                case "centro_h":
                    float centerX = bounds.Average(b => b.Left + b.Width / 2);
                    foreach (var obj in selecionados)
                    {
                        float dx = centerX - (obj.GetBounds().Left + obj.GetBounds().Width / 2);
                        obj.Mover(dx, 0);
                    }
                    break;

                case "topo":
                    float minY = bounds.Min(b => b.Top);
                    foreach (var obj in selecionados)
                    {
                        float dy = minY - obj.GetBounds().Top;
                        obj.Mover(0, dy);
                    }
                    break;

                case "base":
                    float maxY = bounds.Max(b => b.Bottom);
                    foreach (var obj in selecionados)
                    {
                        float dy = maxY - obj.GetBounds().Bottom;
                        obj.Mover(0, dy);
                    }
                    break;

                case "centro_v":
                    float centerY = bounds.Average(b => b.Top + b.Height / 2);
                    foreach (var obj in selecionados)
                    {
                        float dy = centerY - (obj.GetBounds().Top + obj.GetBounds().Height / 2);
                        obj.Mover(0, dy);
                    }
                    break;
            }

            canvas.Invalidate();
            statusLabel.Text = $"Objetos alinhados ({tipo})";
        }

        private void ConfigurarEscala()
        {
            using (var dlg = new FormConfiguracaoEscala(escala, grid))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    statusEscala.Text = $"Escala: {escala.TextoEscala}";
                    AtualizarStatusSnap();
                    canvas.Invalidate();
                }
            }
        }

        private void InserirRosaDosVentos()
        {
            var symbolPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Symbols", "Sinais", "Norte.png");

            if (File.Exists(symbolPath))
            {
                stampTool.SimboloAtual = new SymbolItem
                {
                    Nome = "Norte",
                    CaminhoImagem = symbolPath,
                    LarguraPadrao = 60,
                    AlturaPadrao = 60,
                    Thumbnail = Image.FromFile(symbolPath)
                };
            }
            else
            {
                // Criar um marcador de norte simples via texto
                var arrow = new ArrowObject
                {
                    PontoInicial = new PointF(50, 100),
                    PontoFinal = new PointF(50, 30),
                    Rotulo = "N",
                    TamanhoSeta = 15
                };
                documento.AdicionarObjeto(arrow);
                canvas.Invalidate();
                statusLabel.Text = "Rosa dos Ventos inserida";
                return;
            }

            canvas.FerramentaAtual = stampTool;
            statusLabel.Text = "Clique para posicionar a Rosa dos Ventos";
        }

        private void InserirCarimbo()
        {
            using (var form = new Form())
            {
                form.Text = "Carimbo / Legenda";
                form.Size = new Size(450, 400);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.BackColor = Color.FromArgb(45, 45, 48);
                form.ForeColor = Color.White;

                var campos = new[]
                {
                    ("Nº Procedimento:", "NumeroProcedimento"),
                    ("Perito:", "Perito"),
                    ("Data:", "Data"),
                    ("Endereço:", "Endereco"),
                    ("Natureza:", "Natureza"),
                    ("Observações:", "Observacoes")
                };

                var textBoxes = new TextBox[campos.Length];
                int y = 20;

                for (int i = 0; i < campos.Length; i++)
                {
                    var lbl = new Label
                    {
                        Text = campos[i].Item1,
                        Location = new Point(15, y + 3),
                        AutoSize = true
                    };
                    form.Controls.Add(lbl);

                    textBoxes[i] = new TextBox
                    {
                        Location = new Point(140, y),
                        Size = new Size(280, 25),
                        BackColor = Color.FromArgb(60, 60, 65),
                        ForeColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    form.Controls.Add(textBoxes[i]);
                    y += 40;
                }

                textBoxes[2].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                var btnOk = new Button
                {
                    Text = "Inserir Legenda",
                    Location = new Point(140, y + 20),
                    Size = new Size(140, 35),
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;
                form.Controls.Add(btnOk);

                var btnCancelar = new Button
                {
                    Text = "Cancelar",
                    Location = new Point(290, y + 20),
                    Size = new Size(100, 35),
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(60, 60, 65),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnCancelar.FlatAppearance.BorderSize = 0;
                form.Controls.Add(btnCancelar);

                form.AcceptButton = btnOk;
                form.CancelButton = btnCancelar;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    documento.NumeroProcedimento = textBoxes[0].Text;
                    documento.Perito = textBoxes[1].Text;
                    documento.Endereco = textBoxes[3].Text;

                    string textoLegenda =
                        $"══════════════════════════════\n" +
                        $"       CROQUI DE LOCAL DE CRIME\n" +
                        $"══════════════════════════════\n" +
                        $"Procedimento: {textBoxes[0].Text}\n" +
                        $"Data/Hora: {textBoxes[2].Text}\n" +
                        $"Perito: {textBoxes[1].Text}\n" +
                        $"Endereço: {textBoxes[3].Text}\n" +
                        $"Natureza: {textBoxes[4].Text}\n" +
                        (string.IsNullOrEmpty(textBoxes[5].Text) ? "" : $"Obs: {textBoxes[5].Text}\n") +
                        $"Escala: {escala.TextoEscala}\n" +
                        $"══════════════════════════════";

                    var label = new TextLabel
                    {
                        Posicao = new PointF(20, 20),
                        Texto = textoLegenda,
                        FonteTamanho = 9,
                        ComFundo = true,
                        CorFundoArgb = Color.FromArgb(240, 255, 255, 240).ToArgb(),
                        Negrito = false
                    };
                    documento.AdicionarObjeto(label);
                    canvas.Invalidate();
                    statusLabel.Text = "Legenda inserida";
                }
            }
        }

        private void InserirImagemArquivo()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Imagens|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                dlg.Title = "Inserir Imagem";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var img = Image.FromFile(dlg.FileName);
                    stampTool.SimboloAtual = new SymbolItem
                    {
                        Nome = Path.GetFileNameWithoutExtension(dlg.FileName),
                        CaminhoImagem = dlg.FileName,
                        LarguraPadrao = Math.Min(img.Width, 200),
                        AlturaPadrao = Math.Min(img.Height, 200),
                        Thumbnail = img
                    };
                    canvas.FerramentaAtual = stampTool;
                    statusLabel.Text = "Clique para posicionar a imagem";
                }
            }
        }

        private void MostrarAtalhos()
        {
            string atalhos =
                "═══════════════════════════════════════\n" +
                "              ATALHOS DE TECLADO\n" +
                "═══════════════════════════════════════\n\n" +
                "ARQUIVO:\n" +
                "  Ctrl+N    Novo documento\n" +
                "  Ctrl+O    Abrir\n" +
                "  Ctrl+S    Salvar\n" +
                "  Ctrl+P    Imprimir\n\n" +
                "EDIÇÃO:\n" +
                "  Ctrl+Z    Desfazer\n" +
                "  Ctrl+Y    Refazer\n" +
                "  Ctrl+C    Copiar\n" +
                "  Ctrl+V    Colar\n" +
                "  Delete    Excluir seleção\n\n" +
                "FERRAMENTAS:\n" +
                "  V / Esc   Selecionar\n" +
                "  W         Parede\n" +
                "  R         Cômodo\n" +
                "  S         Rua\n" +
                "  D         Cota/Medida\n" +
                "  T         Texto\n" +
                "  A         Seta\n\n" +
                "VISUALIZAÇÃO:\n" +
                "  G         Toggle Snap\n" +
                "  Ctrl+0    Zoom para ver tudo\n" +
                "  Ctrl+1    Zoom 100%\n" +
                "  Mouse     Scroll = Zoom\n" +
                "            Botão do meio = Pan\n" +
                "═══════════════════════════════════════";

            MessageBox.Show(atalhos, "Atalhos de Teclado",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MostrarSobre()
        {
            MessageBox.Show(
                "🔍 CrimeSketcher v1.0\n\n" +
                "Aplicação para elaboração de croquis\n" +
                "técnicos de locais de crime.\n\n" +
                "Desenvolvido para uso pericial.\n\n" +
                "© 2024 - Todos os direitos reservados.",
                "Sobre o CrimeSketcher",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion

        #region Eventos

        private void Canvas_CursorMoved(object sender, PointF pos)
        {
            statusCoord.Text = $"X: {pos.X:F1}  Y: {pos.Y:F1}";
        }

        private void Canvas_ZoomChanged(object sender, float zoom)
        {
            statusZoom.Text = $"Zoom: {zoom * 100:F0}%";
        }

        private void PropGrid_ValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            canvas.Invalidate();
            documento.NotificarAlteracao();
        }

        private void AtualizarStatusSnap()
        {
            statusSnap.Text = grid.SnapAtivo ? "SNAP: ON" : "SNAP: OFF";
            statusSnap.ForeColor = grid.SnapAtivo ? Color.LightGreen : Color.Gray;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Atalhos com Ctrl
            if (e.Control && !e.Alt && !e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.N: NovoDocumento(); e.Handled = true; break;
                    case Keys.O: AbrirDocumento(); e.Handled = true; break;
                    case Keys.S: SalvarDocumento(); e.Handled = true; break;
                    case Keys.P: Imprimir(); e.Handled = true; break;
                    case Keys.Z: undoRedo?.Desfazer(); e.Handled = true; break;
                    case Keys.Y: undoRedo?.Refazer(); e.Handled = true; break;
                    case Keys.C: Copiar(); e.Handled = true; break;
                    case Keys.V: Colar(); e.Handled = true; break;
                    case Keys.X: Recortar(); e.Handled = true; break;
                    case Keys.A: SelecionarTudo(); e.Handled = true; break;
                    case Keys.G: Agrupar(); e.Handled = true; break;
                    case Keys.D0: canvas.ZoomParaMostrarTudo(); e.Handled = true; break;
                    case Keys.D1: DefinirZoom(1f); e.Handled = true; break;
                    case Keys.Oemplus: AlterarZoom(1.25f); e.Handled = true; break;
                    case Keys.OemMinus: AlterarZoom(1f / 1.25f); e.Handled = true; break;
                }
            }
            // Ctrl+Shift
            else if (e.Control && e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.S: SalvarComo(); e.Handled = true; break;
                    case Keys.G: Desagrupar(); e.Handled = true; break;
                }
            }
            // Sem modificadores
            else if (!e.Control && !e.Alt && !e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.V:
                    case Keys.Escape:
                        DefinirFerramenta("Selecionar");
                        e.Handled = true;
                        break;
                    case Keys.W: DefinirFerramenta("Parede"); e.Handled = true; break;
                    case Keys.R: DefinirFerramenta("Comodo"); e.Handled = true; break;
                    case Keys.S: DefinirFerramenta("Rua"); e.Handled = true; break;
                    case Keys.D: DefinirFerramenta("Cota"); e.Handled = true; break;
                    case Keys.T: DefinirFerramenta("Texto"); e.Handled = true; break;
                    case Keys.A: DefinirFerramenta("Seta"); e.Handled = true; break;
                    case Keys.G:
                        grid.SnapAtivo = !grid.SnapAtivo;
                        AtualizarStatusSnap();
                        canvas.Invalidate();
                        e.Handled = true;
                        break;
                    case Keys.Delete:
                        ExcluirSelecao();
                        e.Handled = true;
                        break;
                    case Keys.F1:
                        MostrarAtalhos();
                        e.Handled = true;
                        break;
                }
            }

            if (e.Handled)
                canvas.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (documento.Objetos.Count > 0)
            {
                var result = MessageBox.Show(
                    "Deseja salvar o croqui antes de sair?",
                    "Sair do CrimeSketcher",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SalvarDocumento();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(e);
        }

        #endregion

        #region Utilitários

        private string InputBox(string prompt, string title, string defaultValue = "")
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            form.BackColor = Color.FromArgb(45, 45, 48);
            form.ForeColor = Color.White;

            label.Text = prompt;
            textBox.Text = defaultValue;
            textBox.BackColor = Color.FromArgb(60, 60, 65);
            textBox.ForeColor = Color.White;

            buttonOk.Text = "OK";
            buttonOk.BackColor = Color.FromArgb(0, 122, 204);
            buttonOk.ForeColor = Color.White;
            buttonOk.FlatStyle = FlatStyle.Flat;
            buttonOk.FlatAppearance.BorderSize = 0;

            buttonCancel.Text = "Cancelar";
            buttonCancel.BackColor = Color.FromArgb(60, 60, 65);
            buttonCancel.ForeColor = Color.White;
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.FlatAppearance.BorderSize = 0;

            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(15, 20, 350, 20);
            textBox.SetBounds(15, 45, 350, 28);
            buttonOk.SetBounds(180, 85, 90, 32);
            buttonCancel.SetBounds(275, 85, 90, 32);

            form.ClientSize = new Size(385, 130);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        #endregion
    }
}