// Library/SymbolLibrary.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace CrimeSketcher.Library
{
    public class SymbolLibrary
    {
        public List<SymbolCategory> Categorias { get; set; }
            = new List<SymbolCategory>();

        private string _basePath;
        private const float LarguraInsercaoPng = 30f;

        public SymbolLibrary(string basePath)
        {
            _basePath = basePath;
            InicializarCategorias();
        }

        private void InicializarCategorias()
        {
            // Criar estrutura de diretórios se não existe
            var categorias = new[]
            {
                ("Veículos", "Veiculos"),
                ("Móveis", "Moveis"),
                ("Vestígios", "Vestigios"),
                ("Armas", "Armas"),
                ("Corpos", "Corpos"),
                ("Sinais", "Sinais"),
                ("Diversos", "Diversos")
            };

            foreach (var (nome, pasta) in categorias)
            {
                string dir = Path.Combine(_basePath, pasta);
                Directory.CreateDirectory(dir);

                var cat = new SymbolCategory { Nome = nome };

                // Carregar PNGs existentes
                if (Directory.Exists(dir))
                {
                    foreach (var file in Directory.GetFiles(dir, "*.png"))
                    {
                        try
                        {
                            var img = Image.FromFile(file);
                            float alturaPadrao = img.Width > 0
                                ? Math.Max(1f, (float)Math.Round(img.Height * (LarguraInsercaoPng / img.Width)))
                                : LarguraInsercaoPng;

                            cat.Itens.Add(new SymbolItem
                            {
                                Nome = Path.GetFileNameWithoutExtension(file),
                                CaminhoImagem = file,
                                Categoria = nome,
                                Thumbnail = CriarThumbnail(img, 48, 48),
                                LarguraPadrao = LarguraInsercaoPng,
                                AlturaPadrao = alturaPadrao
                            });
                        }
                        catch { }
                    }
                }

                Categorias.Add(cat);
            }

            // Se não houver imagens, criar placeholders vetoriais
            GerarSimbolosPadrao();
        }

        /// <summary>
        /// Gera símbolos padrão como imagens PNG se a pasta estiver vazia
        /// </summary>
        private void GerarSimbolosPadrao()
        {
            // Veículos
            GerarSimboloSeNaoExiste("Veiculos", "Carro_Sedan",
                60, 30, (g, w, h) =>
                {
                    g.FillRoundedRectangle(Brushes.LightGray,
                        2, 2, w - 4, h - 4, 5);
                    g.DrawRoundedRectangle(Pens.Black,
                        2, 2, w - 4, h - 4, 5);
                    // Rodas
                    g.FillEllipse(Brushes.DarkGray, 8, -2, 8, 8);
                    g.FillEllipse(Brushes.DarkGray, 8, h - 6, 8, 8);
                    g.FillEllipse(Brushes.DarkGray, w - 18, -2, 8, 8);
                    g.FillEllipse(Brushes.DarkGray, w - 18, h - 6, 8, 8);
                    // Para-brisa
                    g.DrawLine(Pens.DarkGray, w - 15, 5, w - 15, h - 5);
                    g.DrawLine(Pens.DarkGray, 15, 5, 15, h - 5);
                });

            GerarSimboloSeNaoExiste("Veiculos", "Moto", 40, 20,
                (g, w, h) =>
                {
                    g.FillEllipse(Brushes.DarkGray, 2, h / 2 - 5, 10, 10);
                    g.FillEllipse(Brushes.DarkGray, w - 14, h / 2 - 5, 10, 10);
                    g.DrawLine(new Pen(Color.Black, 2), 7, h / 2,
                        w - 9, h / 2);
                    g.FillRectangle(Brushes.Gray, w / 2 - 6, 3, 12, h - 6);
                });

            // Móveis
            GerarSimboloSeNaoExiste("Moveis", "Mesa_Retangular",
                50, 30, (g, w, h) =>
                {
                    g.FillRectangle(new SolidBrush(Color.BurlyWood),
                        3, 3, w - 6, h - 6);
                    g.DrawRectangle(Pens.SaddleBrown, 3, 3, w - 6, h - 6);
                });

            GerarSimboloSeNaoExiste("Moveis", "Cadeira", 20, 20,
                (g, w, h) =>
                {
                    g.FillRectangle(new SolidBrush(Color.Tan),
                        3, 3, w - 6, h - 6);
                    g.DrawRectangle(Pens.SaddleBrown, 3, 3, w - 6, h - 6);
                    g.DrawLine(Pens.SaddleBrown, 3, 3, 3, h / 2);
                });

            GerarSimboloSeNaoExiste("Moveis", "Cama_Casal",
                50, 40, (g, w, h) =>
                {
                    g.FillRectangle(new SolidBrush(Color.LightBlue),
                        3, 3, w - 6, h - 6);
                    g.DrawRectangle(Pens.SteelBlue, 3, 3, w - 6, h - 6);
                    // Travesseiros
                    g.FillRectangle(Brushes.White, 6, 5, w / 2 - 6, 10);
                    g.FillRectangle(Brushes.White, w / 2 + 2, 5, w / 2 - 6, 10);
                    g.DrawRectangle(Pens.LightSteelBlue, 6, 5, w / 2 - 6, 10);
                    g.DrawRectangle(Pens.LightSteelBlue, w / 2 + 2, 5,
                        w / 2 - 6, 10);
                });

            GerarSimboloSeNaoExiste("Moveis", "Sofa", 60, 25,
                (g, w, h) =>
                {
                    g.FillRoundedRectangle(new SolidBrush(Color.CadetBlue),
                        3, 3, w - 6, h - 6, 4);
                    g.DrawRoundedRectangle(Pens.DarkSlateGray,
                        3, 3, w - 6, h - 6, 4);
                    g.FillRectangle(new SolidBrush(Color.DarkCyan),
                        3, h - 8, w - 6, 5);
                });

            // Vestígios
            GerarSimboloSeNaoExiste("Vestigios", "Mancha_Sangue",
                25, 25, (g, w, h) =>
                {
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(3, 5, w - 6, h - 8);
                        path.AddEllipse(w / 3, 2, w / 3, h / 3);
                        using (var brush = new SolidBrush(
                            Color.FromArgb(180, 139, 0, 0)))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                });

            GerarSimboloSeNaoExiste("Vestigios", "Projetil",
                15, 15, (g, w, h) =>
                {
                    g.FillEllipse(Brushes.DarkGoldenrod,
                        2, 2, w - 4, h - 4);
                    g.DrawEllipse(Pens.Black, 2, 2, w - 4, h - 4);
                });

            GerarSimboloSeNaoExiste("Vestigios", "Capsula",
                10, 18, (g, w, h) =>
                {
                    g.FillRectangle(Brushes.Gold, 2, 5, w - 4, h - 7);
                    g.FillEllipse(Brushes.Gold, 1, 1, w - 2, 8);
                    g.DrawRectangle(Pens.DarkGoldenrod, 2, 5, w - 4, h - 7);
                });

            GerarSimboloSeNaoExiste("Vestigios", "Pegada",
                15, 30, (g, w, h) =>
                {
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(2, h / 2, w - 4, h / 2 - 2);
                        path.AddEllipse(3, 2, w - 6, h / 3);
                        g.FillPath(new SolidBrush(
                            Color.FromArgb(150, 80, 60, 40)), path);
                    }
                });

            // Armas
            GerarSimboloSeNaoExiste("Armas", "Revolver",
                35, 25, (g, w, h) =>
                {
                    g.DrawLine(new Pen(Color.DimGray, 3), 5, h / 2,
                        w - 5, h / 2);
                    g.DrawLine(new Pen(Color.DimGray, 2), w / 2, h / 2,
                        w / 2 + 3, h - 3);
                    g.FillEllipse(Brushes.DimGray, w - 10, h / 2 - 5, 10, 10);
                });

            GerarSimboloSeNaoExiste("Armas", "Faca", 10, 40,
                (g, w, h) =>
                {
                    g.FillRectangle(Brushes.Silver, 3, 2, w - 6, h / 2);
                    g.FillRectangle(Brushes.SaddleBrown,
                        2, h / 2, w - 4, h / 2 - 2);
                    g.DrawRectangle(Pens.DimGray, 3, 2, w - 6, h - 4);
                });

            // Sinais
            GerarSimboloSeNaoExiste("Sinais", "Norte", 40, 40,
                (g, w, h) =>
                {
                    // Rosa dos ventos simplificada
                    var center = new PointF(w / 2, h / 2);
                    using (var pen = new Pen(Color.DarkRed, 2))
                    {
                        g.DrawLine(pen, w / 2, 3, w / 2, h - 3);
                        g.DrawLine(pen, 3, h / 2, w - 3, h / 2);
                    }
                    // Seta norte
                    g.FillPolygon(Brushes.Red, new PointF[]
                    {
                    new PointF(w/2, 2),
                    new PointF(w/2 - 5, 12),
                    new PointF(w/2 + 5, 12)
                    });
                    using (var font = new Font("Arial", 7, FontStyle.Bold))
                    {
                        g.DrawString("N", font, Brushes.DarkRed,
                            w / 2 - 4, 0);
                    }
                });

            GerarSimboloSeNaoExiste("Sinais", "Marcador_Evidencia",
                25, 30, (g, w, h) =>
                {
                    // Plaqueta numerada de evidência
                    var points = new PointF[]
                    {
                    new PointF(w/2, 2),
                    new PointF(w - 3, h/2),
                    new PointF(w - 3, h - 3),
                    new PointF(3, h - 3),
                    new PointF(3, h/2)
                    };
                    g.FillPolygon(Brushes.Yellow, points);
                    g.DrawPolygon(Pens.Black, points);
                    using (var font = new Font("Arial", 10, FontStyle.Bold))
                    using (var sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        g.DrawString("#", font, Brushes.Black,
                            new RectangleF(0, h / 3, w, h * 2 / 3), sf);
                    }
                });
        }

        private void GerarSimboloSeNaoExiste(string pasta, string nome,
            int w, int h, Action<Graphics, int, int> desenhar)
        {
            string dir = Path.Combine(_basePath, pasta);
            string file = Path.Combine(dir, nome + ".png");

            if (File.Exists(file)) return;

            Directory.CreateDirectory(dir);

            using (var bmp = new Bitmap(w, h))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                desenhar(g, w, h);
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }

            // Adicionar à categoria
            var cat = Categorias.Find(c =>
                c.Nome == GetNomeCategoria(pasta));
            if (cat != null)
            {
                float alturaPadrao = w > 0
                    ? Math.Max(1f, (float)Math.Round(h * (LarguraInsercaoPng / w)))
                    : LarguraInsercaoPng;

                cat.Itens.Add(new SymbolItem
                {
                    Nome = nome.Replace("_", " "),
                    CaminhoImagem = file,
                    Categoria = cat.Nome,
                    Thumbnail = Image.FromFile(file),
                    LarguraPadrao = LarguraInsercaoPng,
                    AlturaPadrao = alturaPadrao
                });
            }
        }

        private string GetNomeCategoria(string pasta)
        {
            switch (pasta)
            {
                case "Veiculos": return "Veículos";
                case "Moveis": return "Móveis";
                case "Vestigios": return "Vestígios";
                default: return pasta;
            }
        }

        private Image CriarThumbnail(Image original, int w, int h)
        {
            var thumb = new Bitmap(w, h);
            using (var g = Graphics.FromImage(thumb))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent);

                float scale = Math.Min((float)w / original.Width,
                    (float)h / original.Height);
                float sw = original.Width * scale;
                float sh = original.Height * scale;
                g.DrawImage(original, (w - sw) / 2, (h - sh) / 2, sw, sh);
            }
            return thumb;
        }

        /// <summary>
        /// Importar símbolo personalizado
        /// </summary>
        public SymbolItem ImportarSimbolo(string caminhoOrigem,
            string categoria, string nome)
        {
            var cat = Categorias.Find(c => c.Nome == categoria);
            if (cat == null) return null;

            string pasta = categoria.Replace("í", "i").Replace("ó", "o")
                .Replace("ê", "e");
            string dir = Path.Combine(_basePath, pasta);
            Directory.CreateDirectory(dir);

            string destino = Path.Combine(dir,
                Path.GetFileName(caminhoOrigem));
            File.Copy(caminhoOrigem, destino, true);

            var img = Image.FromFile(destino);
            int alturaPadrao = img.Width > 0
                ? Math.Max(1, (int)Math.Round(img.Height * (LarguraInsercaoPng / img.Width)))
                : (int)LarguraInsercaoPng;

            var item = new SymbolItem
            {
                Nome = nome,
                CaminhoImagem = destino,
                Categoria = categoria,
                Thumbnail = CriarThumbnail(img, 48, 48),
                LarguraPadrao = LarguraInsercaoPng,
                AlturaPadrao = alturaPadrao
            };

            cat.Itens.Add(item);
            return item;
        }
    }

    // Extension methods para desenhar retângulos arredondados
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g,
            Brush brush, float x, float y, float w, float h, float r)
        {
            using (var path = CreateRoundedRect(x, y, w, h, r))
                g.FillPath(brush, path);
        }

        public static void DrawRoundedRectangle(this Graphics g,
            Pen pen, float x, float y, float w, float h, float r)
        {
            using (var path = CreateRoundedRect(x, y, w, h, r))
                g.DrawPath(pen, path);
        }

        private static GraphicsPath CreateRoundedRect(
            float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2,
                r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}