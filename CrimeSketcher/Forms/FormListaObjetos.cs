using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Forms
{
    public class FormListaObjetos : Form
    {
        private ListView listViewObjetos;
        private Button btnSubir;
        private Button btnDescer;
        private SketchDocument _documento;

        public event Action<BaseSketchObject> ObjetoSelecionado;
        public event Action<BaseSketchObject> ObjetoExcluido;
        public event Action OrdemAlterada;

        public FormListaObjetos(SketchDocument documento)
        {
            _documento = documento;
            InitializeComponent();
            AtualizarLista();
        }

        private void InitializeComponent()
        {
            Text = "Objetos na Cena";
            Size = new Size(750, 420);

            listViewObjetos = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewObjetos.Columns.Add("Nome", 150);
            listViewObjetos.Columns.Add("Tipo", 100);
            listViewObjetos.Columns.Add("Camada", 80);
            listViewObjetos.Columns.Add("Posição", 190);
            listViewObjetos.Columns.Add("Ação", 90);
            listViewObjetos.SelectedIndexChanged += ListViewObjetos_SelectedIndexChanged;
            listViewObjetos.MouseClick += ListViewObjetos_MouseClick;

            var painelLateral = new Panel
            {
                Dock = DockStyle.Right,
                Width = 120,
                Padding = new Padding(8)
            };

            btnSubir = new Button
            {
                Text = "⬆ Subir",
                Dock = DockStyle.Top,
                Height = 34
            };
            btnSubir.Click += (s, e) => MoverSelecionado(+1);

            btnDescer = new Button
            {
                Text = "⬇ Descer",
                Dock = DockStyle.Top,
                Height = 34,
                Margin = new Padding(0, 6, 0, 0)
            };
            btnDescer.Click += (s, e) => MoverSelecionado(-1);

            painelLateral.Controls.Add(btnDescer);
            painelLateral.Controls.Add(btnSubir);

            Controls.Add(listViewObjetos);
            Controls.Add(painelLateral);
        }

        public void AtualizarLista(BaseSketchObject objetoSelecionado = null)
        {
            listViewObjetos.Items.Clear();

            var esc = ScaleManager.Atual;
            int total = _documento.Objetos.Count;

            for (int i = 0; i < total; i++)
            {
                var obj = _documento.Objetos[i];
                var item = new ListViewItem(obj.Nome);
                item.SubItems.Add(obj.Tipo);
                item.SubItems.Add((i + 1).ToString());

                if (esc != null)
                {
                    float xm = esc.PixelsParaReal(obj.Posicao.X);
                    float ym = esc.PixelsParaReal(obj.Posicao.Y);
                    item.SubItems.Add($"({xm:F2} {esc.UnidadeReal}, {ym:F2} {esc.UnidadeReal})");
                }
                else
                {
                    item.SubItems.Add($"({obj.Posicao.X:F0}, {obj.Posicao.Y:F0})");
                }

                item.SubItems.Add("🗑 Excluir");
                item.Tag = obj;
                listViewObjetos.Items.Add(item);

                if (objetoSelecionado != null && ReferenceEquals(obj, objetoSelecionado))
                    item.Selected = true;
            }
        }

        private void ListViewObjetos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewObjetos.SelectedItems.Count > 0)
            {
                var obj = listViewObjetos.SelectedItems[0].Tag as BaseSketchObject;
                ObjetoSelecionado?.Invoke(obj);
            }
        }

        private void ListViewObjetos_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var hit = listViewObjetos.HitTest(e.Location);
            if (hit?.Item == null || hit.SubItem == null)
                return;

            int subItemIndex = hit.Item.SubItems.IndexOf(hit.SubItem);
            if (subItemIndex != 4)
                return;

            if (hit.Item.Tag is not BaseSketchObject obj)
                return;

            var confirm = MessageBox.Show(
                $"Deseja excluir o objeto '{obj.Nome}'?",
                "Excluir objeto",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            _documento.RemoverObjeto(obj);
            ObjetoExcluido?.Invoke(obj);
            AtualizarLista();
        }

        private void MoverSelecionado(int delta)
        {
            if (listViewObjetos.SelectedItems.Count == 0)
                return;

            if (listViewObjetos.SelectedItems[0].Tag is not BaseSketchObject obj)
                return;

            int indexAtual = _documento.Objetos.IndexOf(obj);
            if (indexAtual < 0)
                return;

            int novoIndex = indexAtual + delta;
            if (novoIndex < 0 || novoIndex >= _documento.Objetos.Count)
                return;

            _documento.Objetos.RemoveAt(indexAtual);
            _documento.Objetos.Insert(novoIndex, obj);
            _documento.NotificarAlteracao();

            AtualizarLista(obj);
            OrdemAlterada?.Invoke();
        }
    }
}
