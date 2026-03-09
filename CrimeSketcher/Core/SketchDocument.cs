// Core/SketchDocument.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Core
{
    [Serializable]
    public class SketchDocument
    {
        public string Titulo { get; set; } = "Novo Croqui";
        public string NumeroProcedimento { get; set; } = "";
        public string Perito { get; set; } = "";
        public DateTime DataLevantamento { get; set; } = DateTime.Now;
        public string Endereco { get; set; } = "";
        public string Observacoes { get; set; } = "";

        public float LarguraPapelCm { get; set; } = 42f;
        public float AlturaPapelCm { get; set; } = 29.7f;
        public float EscalaDenominador { get; set; } = 100f;

        public List<BaseSketchObject> Objetos { get; set; } = new List<BaseSketchObject>();

        [JsonIgnore]
        private UndoRedoManager _undoRedo;

        [JsonIgnore]
        public UndoRedoManager UndoRedo
        {
            get
            {
                if (_undoRedo == null)
                    _undoRedo = new UndoRedoManager();
                return _undoRedo;
            }
        }

        public event EventHandler DocumentoAlterado;

        public void AdicionarObjeto(BaseSketchObject obj, bool comUndo = true)
        {
            Objetos.Add(obj);
            if (comUndo)
            {
                UndoRedo.RegistrarAcao(new AddObjectAction(this, obj));
            }
            DocumentoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void RemoverObjeto(BaseSketchObject obj, bool comUndo = true)
        {
            Objetos.Remove(obj);
            if (comUndo)
            {
                UndoRedo.RegistrarAcao(new RemoveObjectAction(this, obj));
            }
            DocumentoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public BaseSketchObject HitTest(PointF ponto, float tolerancia = 5f)
        {
            for (int i = Objetos.Count - 1; i >= 0; i--)
            {
                if (Objetos[i].ContemPonto(ponto, tolerancia))
                    return Objetos[i];
            }
            return null;
        }

        public List<BaseSketchObject> HitTestArea(RectangleF area)
        {
            return Objetos.Where(o => o.IntersectaRetangulo(area)).ToList();
        }

        public void Salvar(string caminho)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new SketchObjectConverter() }
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(caminho, json);
        }

        public static SketchDocument Carregar(string caminho)
        {
            var json = File.ReadAllText(caminho);
            var options = new JsonSerializerOptions
            {
                Converters = { new SketchObjectConverter() }
            };
            return JsonSerializer.Deserialize<SketchDocument>(json, options);
        }

        public void NotificarAlteracao()
        {
            DocumentoAlterado?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Converter para serialização polimórfica de objetos do croqui
    /// </summary>
    public class SketchObjectConverter : JsonConverter<BaseSketchObject>
    {
        public override BaseSketchObject Read(ref Utf8JsonReader reader,
            Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                string tipo = root.GetProperty("Tipo").GetString();

                string json = root.GetRawText();

                return tipo switch
                {
                    "Parede" => JsonSerializer.Deserialize<WallObject>(json, options),
                    "Cômodo" => JsonSerializer.Deserialize<RoomObject>(json, options),
                    "Rua" => JsonSerializer.Deserialize<StreetObject>(json, options),
                    "Cota" => JsonSerializer.Deserialize<DimensionLine>(json, options),
                    "Corpo" => JsonSerializer.Deserialize<StickFigure>(json, options),
                    "Símbolo" => JsonSerializer.Deserialize<StampObject>(json, options),
                    "Texto" => JsonSerializer.Deserialize<TextLabel>(json, options),
                    "Seta" => JsonSerializer.Deserialize<ArrowObject>(json, options),
                    _ => throw new JsonException($"Tipo desconhecido: {tipo}")
                };
            }
        }

        public override void Write(Utf8JsonWriter writer,
            BaseSketchObject value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}