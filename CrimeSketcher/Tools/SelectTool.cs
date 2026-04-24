// Tools/SelectTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class SelectTool : ITool
    {
        private enum ArticulacaoCorpoHandle
        {
            Nenhuma = -1,
            Pescoco = 0,
            CotoveloDireito = 1,
            MaoDireita = 2,
            JoelhoDireito = 3,
            PeDireito = 4,
            CotoveloEsquerdo = 5,
            MaoEsquerda = 6,
            JoelhoEsquerdo = 7,
            PeEsquerdo = 8
        }

        public string Nome => "Selecionar";
        public Cursor Cursor => Cursors.Default;

        private readonly SketchDocument _doc;
        private BaseSketchObject _objetoArrastando;
        private bool _arrastando = false;
        private bool _arrastoPendente = false;
        private PointF _pontoInicioArrasto;
        private const float LimiteInicioArrastoPixels = 4f;
        private bool _redimensionando = false;
        private bool _rotacionando = false;
        private int _alcaAtiva = -1;
        private BaseSketchObject _objetoTransformando;
        private PointF _ultimoPontoMouse;
        private RectangleF _selecaoRetangular;
        private bool _selecionandoArea = false;
        private PointF _inicioSelecao;
        private readonly UndoRedoManager _undoRedo;

        private bool _arrastandoVerticeArea = false;
        private AreaObject _areaEditandoVertice = null;
        private int _indiceVerticeArea = -1;

        // Controle de arrasto de ponto de curva
        private bool _arrastandoPontoCurva = false;
        private BaseSketchObject _objetoComCurva = null;
        private PointF _pontoCurvaAnterior;

        // Controle de arraste de articulações do corpo
        private bool _arrastandoArticulacaoCorpo = false;
        private StickFigure _corpoArticulando = null;
        private ArticulacaoCorpoHandle _articulacaoCorpoAtiva = ArticulacaoCorpoHandle.Nenhuma;

        // Controle de handles de abertura de parede
        private bool _arrastandoHandleParede = false;
        private WallObject _paredeComHandle = null;
        private WallHandleType _tipoHandleParede;

        // Múltipla seleção
        private readonly HashSet<BaseSketchObject> _objetosSelecionados = new HashSet<BaseSketchObject>();
        private readonly Dictionary<BaseSketchObject, PointF> _posicoesAnterioresGrupo = new Dictionary<BaseSketchObject, PointF>();

        public BaseSketchObject ObjetoSelecionado { get; private set; }
        public IReadOnlyCollection<BaseSketchObject> ObjetosSelecionados => _objetosSelecionados;

        public event EventHandler<BaseSketchObject> SelectionChanged;
        public event EventHandler<IReadOnlyCollection<BaseSketchObject>> MultiSelectionChanged;

        public float ZoomLevel { get; set; } = 1f;

        public SelectTool(SketchDocument doc, UndoRedoManager undoRedo)
        {
            _doc = doc;
            _undoRedo = undoRedo;
        }

        private void DesselecionarTodos()
        {
            foreach (var obj in _objetosSelecionados)
            {
                obj.Selecionado = false;
            }
            _objetosSelecionados.Clear();
            ObjetoSelecionado = null;
        }

        private void AdicionarSelecionado(BaseSketchObject obj)
        {
            if (!_objetosSelecionados.Contains(obj))
            {
                _objetosSelecionados.Add(obj);
                obj.Selecionado = true;
            }
        }

        private void RemoverSelecionado(BaseSketchObject obj)
        {
            _objetosSelecionados.Remove(obj);
            obj.Selecionado = false;
        }

        private void ToggleSelecionado(BaseSketchObject obj)
        {
            if (_objetosSelecionados.Contains(obj))
            {
                RemoverSelecionado(obj);
            }
            else
            {
                AdicionarSelecionado(obj);
            }
        }

        private float PixelsParaMundo(float pixels)
        {
            float zoom = Math.Max(0.0001f, ZoomLevel);
            return pixels / zoom;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button != MouseButtons.Left) return;

            if (ObjetoSelecionado is AreaObject areaSelecionada && areaSelecionada.EditarVertices && !areaSelecionada.Bloqueado)
            {
                int indiceVertice = areaSelecionada.ObterIndiceVertice(worldPos, PixelsParaMundo(10f));
                if (indiceVertice >= 0)
                {
                    _arrastandoVerticeArea = true;
                    _areaEditandoVertice = areaSelecionada;
                    _indiceVerticeArea = indiceVertice;
                    return;
                }
            }

            // Verificar se está clicando no ponto de controle de curva
            if (ObjetoSelecionado is StreetObject street && street.TemCurva)
            {
                if (street.ContemPontoCurva(worldPos, PixelsParaMundo(12f)))
                {
                    _arrastandoPontoCurva = true;
                    _objetoComCurva = street;
                    _pontoCurvaAnterior = street.PontoCurva.Value;
                    return;
                }
            }
            else if (ObjetoSelecionado is MarkObject mark && mark.TemCurva)
            {
                if (mark.ContemPontoCurva(worldPos, PixelsParaMundo(12f)))
                {
                    _arrastandoPontoCurva = true;
                    _objetoComCurva = mark;
                    _pontoCurvaAnterior = mark.PontoCurva.Value;
                    return;
                }
            }
            else if (ObjetoSelecionado is ArrowObject arrow && arrow.TemCurva)
            {
                if (arrow.ContemPontoCurva(worldPos, PixelsParaMundo(12f)))
                {
                    _arrastandoPontoCurva = true;
                    _objetoComCurva = arrow;
                    _pontoCurvaAnterior = arrow.PontoCurva.Value;
                    return;
                }
            }

            if (ObjetoSelecionado is StickFigure corpoSelecionado && !corpoSelecionado.Bloqueado)
            {
                var articulacao = GetArticulacaoAtPoint(corpoSelecionado, worldPos, PixelsParaMundo(10f));
                if (articulacao != ArticulacaoCorpoHandle.Nenhuma)
                {
                    _arrastandoArticulacaoCorpo = true;
                    _corpoArticulando = corpoSelecionado;
                    _articulacaoCorpoAtiva = articulacao;
                    return;
                }
            }

            if (ObjetoSelecionado is WallObject paredeHandles && !paredeHandles.Bloqueado)
            {
                float tolH = PixelsParaMundo(10f);
                foreach (var kv in paredeHandles.GetHandlesMundo())
                {
                    float ddx = worldPos.X - kv.Value.X;
                    float ddy = worldPos.Y - kv.Value.Y;
                    if ((float)Math.Sqrt(ddx * ddx + ddy * ddy) <= tolH)
                    {
                        _arrastandoHandleParede = true;
                        _paredeComHandle = paredeHandles;
                        _tipoHandleParede = kv.Key;
                        return;
                    }
                }
            }

            if (ObjetoSelecionado != null && !ObjetoSelecionado.Bloqueado)
            {
                int handle = ObjetoSelecionado.GetHandleAtPoint(worldPos, PixelsParaMundo(8f), ZoomLevel);
                if (handle >= 0)
                {
                    _objetoTransformando = ObjetoSelecionado;
                    _alcaAtiva = handle;
                    _ultimoPontoMouse = worldPos;
                    _rotacionando = handle == 8;
                    _redimensionando = handle >= 0 && handle <= 7;
                    return;
                }
            }

            var hit = _doc.HitTest(worldPos, PixelsParaMundo(6f));
            bool ctrlPressed = Control.ModifierKeys.HasFlag(Keys.Control);
            bool shiftPressed = Control.ModifierKeys.HasFlag(Keys.Shift);

            if (hit != null)
            {
                if (ctrlPressed)
                {
                    ToggleSelecionado(hit);
                    ObjetoSelecionado = _objetosSelecionados.Contains(hit)
                        ? hit
                        : _objetosSelecionados.FirstOrDefault();
                }
                else if (shiftPressed && _objetosSelecionados.Count > 0)
                {
                    AdicionarSelecionado(hit);
                    ObjetoSelecionado = hit;
                }
                else
                {
                    DesselecionarTodos();
                    AdicionarSelecionado(hit);
                    ObjetoSelecionado = hit;
                }

                _objetoArrastando = hit;
                _ultimoPontoMouse = worldPos;
                _pontoInicioArrasto = worldPos;

                _posicoesAnterioresGrupo.Clear();
                foreach (var obj in _objetosSelecionados)
                {
                    _posicoesAnterioresGrupo[obj] = obj.Posicao;
                }

                _arrastoPendente = true;
                _arrastando = false;

                SelectionChanged?.Invoke(this, ObjetoSelecionado);
                var lista = _objetosSelecionados.ToList();
                MultiSelectionChanged?.Invoke(this, lista);
            }
            else
            {
                if (!ctrlPressed && !shiftPressed)
                {
                    DesselecionarTodos();
                    SelectionChanged?.Invoke(this, null);
                    var lista = _objetosSelecionados.ToList();
                    MultiSelectionChanged?.Invoke(this, lista);
                }

                _selecionandoArea = true;
                _inicioSelecao = worldPos;
                _selecaoRetangular = RectangleF.Empty;
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            if (_arrastandoVerticeArea && _areaEditandoVertice != null)
            {
                _areaEditandoVertice.MoverVertice(_indiceVerticeArea, worldPos);
            }
            else if (_arrastandoPontoCurva && _objetoComCurva != null)
            {
                bool shiftCircular = Control.ModifierKeys.HasFlag(Keys.Shift);

                if (_objetoComCurva is StreetObject street)
                {
                    street.MoverPontoCurva(worldPos, shiftCircular || street.CurvaCircular);
                }
                else if (_objetoComCurva is MarkObject mark)
                {
                    mark.MoverPontoCurva(worldPos, shiftCircular || mark.CurvaCircular);
                }
                else if (_objetoComCurva is ArrowObject arrow)
                {
                    arrow.MoverPontoCurva(worldPos, shiftCircular || arrow.CurvaCircular);
                }
            }
            else if (_arrastandoArticulacaoCorpo && _corpoArticulando != null)
            {
                AjustarArticulacaoPorArraste(_corpoArticulando, _articulacaoCorpoAtiva, worldPos);
            }
            else if (_arrastandoHandleParede && _paredeComHandle != null)
            {
                _paredeComHandle.AtualizarPorHandle(_tipoHandleParede, worldPos);
            }
            else if (_redimensionando && _objetoTransformando != null)
            {
                AplicarRedimensionamento(_objetoTransformando, worldPos);
                _ultimoPontoMouse = worldPos;
            }
            else if (_rotacionando && _objetoTransformando != null)
            {
                AplicarRotacao(_objetoTransformando, worldPos);
                _ultimoPontoMouse = worldPos;
            }
            else if (_arrastando && _objetoArrastando != null && !_objetoArrastando.Bloqueado)
            {
                float dx = worldPos.X - _ultimoPontoMouse.X;
                float dy = worldPos.Y - _ultimoPontoMouse.Y;
                _ultimoPontoMouse = worldPos;

                foreach (var obj in _objetosSelecionados)
                {
                    if (!obj.Bloqueado)
                    {
                        obj.Mover(dx, dy);
                    }
                }
            }
            else if (_arrastoPendente && _objetoArrastando != null && !_objetoArrastando.Bloqueado)
            {
                float limiteMundo = PixelsParaMundo(LimiteInicioArrastoPixels);
                float dx = worldPos.X - _pontoInicioArrasto.X;
                float dy = worldPos.Y - _pontoInicioArrasto.Y;
                float distancia = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distancia >= limiteMundo)
                {
                    _arrastando = true;
                    _arrastoPendente = false;
                    _ultimoPontoMouse = worldPos;
                }
            }
            else if (_selecionandoArea)
            {
                float x = Math.Min(_inicioSelecao.X, worldPos.X);
                float y = Math.Min(_inicioSelecao.Y, worldPos.Y);
                float w = Math.Abs(worldPos.X - _inicioSelecao.X);
                float h = Math.Abs(worldPos.Y - _inicioSelecao.Y);
                _selecaoRetangular = new RectangleF(x, y, w, h);
            }
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos)
        {
            if (_arrastandoVerticeArea)
            {
                _arrastandoVerticeArea = false;
                _areaEditandoVertice = null;
                _indiceVerticeArea = -1;
            }
            else if (_arrastandoPontoCurva && _objetoComCurva != null)
            {
                if (_objetoComCurva is StreetObject street && street.PontoCurva.HasValue)
                {
                    _ = _pontoCurvaAnterior != street.PontoCurva.Value;
                }
                else if (_objetoComCurva is MarkObject mark && mark.PontoCurva.HasValue)
                {
                    _ = _pontoCurvaAnterior != mark.PontoCurva.Value;
                }
                else if (_objetoComCurva is ArrowObject arrow && arrow.PontoCurva.HasValue)
                {
                    _ = _pontoCurvaAnterior != arrow.PontoCurva.Value;
                }

                _arrastandoPontoCurva = false;
                _objetoComCurva = null;
            }
            else if (_arrastandoArticulacaoCorpo)
            {
                _arrastandoArticulacaoCorpo = false;
                _corpoArticulando = null;
                _articulacaoCorpoAtiva = ArticulacaoCorpoHandle.Nenhuma;
            }
            else if (_arrastandoHandleParede)
            {
                _arrastandoHandleParede = false;
                _paredeComHandle = null;
            }
            else if (_arrastando && _objetoArrastando != null)
            {
                bool algumMoveu = false;
                foreach (var obj in _objetosSelecionados)
                {
                    if (_posicoesAnterioresGrupo.TryGetValue(obj, out var posAnterior) &&
                        posAnterior != obj.Posicao)
                    {
                        algumMoveu = true;
                        break;
                    }
                }

                if (algumMoveu)
                {
                    foreach (var obj in _objetosSelecionados)
                    {
                        if (_posicoesAnterioresGrupo.TryGetValue(obj, out var posAnterior) &&
                            posAnterior != obj.Posicao)
                        {
                            _undoRedo.RegistrarAcao(new MoveObjectAction(
                                obj, posAnterior, obj.Posicao));
                        }
                    }
                }
            }
            else if (_selecionandoArea && _selecaoRetangular != RectangleF.Empty)
            {
                var objetosNaArea = _doc.HitTestArea(_selecaoRetangular);
                bool ctrlPressed = Control.ModifierKeys.HasFlag(Keys.Control);

                if (!ctrlPressed)
                {
                    DesselecionarTodos();
                }

                foreach (var obj in objetosNaArea)
                {
                    AdicionarSelecionado(obj);
                }

                ObjetoSelecionado = _objetosSelecionados.LastOrDefault();
                SelectionChanged?.Invoke(this, ObjetoSelecionado);
                var lista = _objetosSelecionados.ToList();
                MultiSelectionChanged?.Invoke(this, lista);
            }

            _arrastando = false;
            _redimensionando = false;
            _rotacionando = false;
            _alcaAtiva = -1;
            _objetoArrastando = null;
            _objetoTransformando = null;
            _selecionandoArea = false;
            _arrastoPendente = false;
            _posicoesAnterioresGrupo.Clear();
        }

        public bool EstaSobreArticulacaoCorpo(PointF worldPos, float tolerancia = 10f)
        {
            if (ObjetoSelecionado is not StickFigure corpo || corpo.Bloqueado)
                return false;

            return GetArticulacaoAtPoint(corpo, worldPos, PixelsParaMundo(tolerancia)) != ArticulacaoCorpoHandle.Nenhuma;
        }

        private ArticulacaoCorpoHandle GetArticulacaoAtPoint(StickFigure corpo, PointF pontoMundo, float tolerancia)
        {
            var pontos = ObterPontosArticulacoesMundo(corpo);
            ArticulacaoCorpoHandle melhor = ArticulacaoCorpoHandle.Nenhuma;
            float melhorDist = float.MaxValue;

            foreach (var kv in pontos)
            {
                float dx = pontoMundo.X - kv.Value.X;
                float dy = pontoMundo.Y - kv.Value.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                if (dist <= tolerancia && dist < melhorDist)
                {
                    melhorDist = dist;
                    melhor = kv.Key;
                }
            }

            return melhor;
        }

        private bool AjustarArticulacaoPorArraste(StickFigure corpo, ArticulacaoCorpoHandle articulacao, PointF pontoMundo)
        {
            if (articulacao == ArticulacaoCorpoHandle.Nenhuma)
                return false;

            PointF local = WorldToLocal(corpo, pontoMundo);

            PointF pescoco;
            PointF ombroDir;
            PointF ombroEsq;
            PointF quadrilDir;
            PointF quadrilEsq;
            float compBracoSuperiorDir;
            float compBracoSuperiorEsq;
            float compCoxa = corpo.AlturaPerna * 0.55f;

            if (corpo.Pose == PoseCorpo.DeLado)
            {
                float alturaTotal = corpo.AlturaCabeca + corpo.AlturaTronco + corpo.AlturaPerna + corpo.AlturaPe * 0.65f;
                float yTopo = -alturaTotal / 2f;
                float yTroncoTop = yTopo + corpo.AlturaCabeca - 2f;
                float yQuadril = yTroncoTop + corpo.AlturaTronco;

                float larguraTronco = Math.Max(10f, corpo.LarguraCintura * 0.5f);
                float xOmbro = larguraTronco * 0.42f;

                pescoco = new PointF(0f, yTroncoTop - 1f);
                ombroDir = new PointF(xOmbro, yTroncoTop + 9f);
                ombroEsq = new PointF(xOmbro - 1.4f, yTroncoTop + 10f);
                quadrilDir = new PointF(1.5f, yQuadril);
                quadrilEsq = new PointF(-1.5f, yQuadril);

                compBracoSuperiorDir = 16f;
                compBracoSuperiorEsq = 16f * 0.92f;
            }
            else
            {
                float yTroncoTop = -corpo.AlturaTronco / 2f;
                float yQuadril = corpo.AlturaTronco / 2f;

                pescoco = new PointF(0f, yTroncoTop - 2f);
                ombroDir = new PointF(corpo.LarguraOmbros / 2f, yTroncoTop + 8f);
                ombroEsq = new PointF(-corpo.LarguraOmbros / 2f, yTroncoTop + 8f);
                quadrilDir = new PointF(corpo.LarguraQuadril / 4f, yQuadril);
                quadrilEsq = new PointF(-corpo.LarguraQuadril / 4f, yQuadril);

                compBracoSuperiorDir = 18f;
                compBracoSuperiorEsq = 18f;
            }

            var cotoveloDir = Somar(ombroDir, VetorRotacionado(compBracoSuperiorDir, corpo.AnguloBracoDireito));
            var cotoveloEsq = Somar(ombroEsq, VetorRotacionado(compBracoSuperiorEsq, corpo.AnguloBracoEsquerdo));
            var joelhoDir = Somar(quadrilDir, VetorRotacionado(compCoxa, corpo.AnguloPernaDireita));
            var joelhoEsq = Somar(quadrilEsq, VetorRotacionado(compCoxa, corpo.AnguloPernaEsquerda));

            switch (articulacao)
            {
                case ArticulacaoCorpoHandle.Pescoco:
                    corpo.AnguloCabeca = NormalizarAngulo(AnguloDoEixoYPositivo(pescoco, local) - 180f);
                    return true;

                case ArticulacaoCorpoHandle.CotoveloDireito:
                    corpo.AnguloBracoDireito = NormalizarAngulo(AnguloDoEixoYPositivo(ombroDir, local));
                    return true;

                case ArticulacaoCorpoHandle.MaoDireita:
                    {
                        float alvoAbsoluto = AnguloDoEixoYPositivo(cotoveloDir, local);
                        corpo.AnguloCotoveloDireito = NormalizarAngulo(alvoAbsoluto - corpo.AnguloBracoDireito);
                        return true;
                    }

                case ArticulacaoCorpoHandle.CotoveloEsquerdo:
                    corpo.AnguloBracoEsquerdo = NormalizarAngulo(AnguloDoEixoYPositivo(ombroEsq, local));
                    return true;

                case ArticulacaoCorpoHandle.MaoEsquerda:
                    {
                        float alvoAbsoluto = AnguloDoEixoYPositivo(cotoveloEsq, local);
                        corpo.AnguloCotoveloEsquerdo = NormalizarAngulo(alvoAbsoluto - corpo.AnguloBracoEsquerdo);
                        return true;
                    }

                case ArticulacaoCorpoHandle.JoelhoDireito:
                    corpo.AnguloPernaDireita = NormalizarAngulo(AnguloDoEixoYPositivo(quadrilDir, local));
                    return true;

                case ArticulacaoCorpoHandle.PeDireito:
                    {
                        float alvoAbsoluto = AnguloDoEixoYPositivo(joelhoDir, local);
                        corpo.AnguloJoelhoDireito = NormalizarAngulo(alvoAbsoluto - corpo.AnguloPernaDireita);
                        return true;
                    }

                case ArticulacaoCorpoHandle.JoelhoEsquerdo:
                    corpo.AnguloPernaEsquerda = NormalizarAngulo(AnguloDoEixoYPositivo(quadrilEsq, local));
                    return true;

                case ArticulacaoCorpoHandle.PeEsquerdo:
                    {
                        float alvoAbsoluto = AnguloDoEixoYPositivo(joelhoEsq, local);
                        corpo.AnguloJoelhoEsquerdo = NormalizarAngulo(alvoAbsoluto - corpo.AnguloPernaEsquerda);
                        return true;
                    }
            }

            return false;
        }

        private Dictionary<ArticulacaoCorpoHandle, PointF> ObterPontosArticulacoesMundo(StickFigure corpo)
        {
            PointF pescoco;
            PointF ombroDir;
            PointF ombroEsq;
            PointF quadrilDir;
            PointF quadrilEsq;

            float compBracoSuperiorDir;
            float compBracoSuperiorEsq;
            float compAntebracoDir;
            float compAntebracoEsq;
            float compCoxa = corpo.AlturaPerna * 0.55f;
            float compCanela = corpo.AlturaPerna * 0.45f;

            if (corpo.Pose == PoseCorpo.DeLado)
            {
                float alturaTotal = corpo.AlturaCabeca + corpo.AlturaTronco + corpo.AlturaPerna + corpo.AlturaPe * 0.65f;
                float yTopo = -alturaTotal / 2f;
                float yTroncoTop = yTopo + corpo.AlturaCabeca - 2f;
                float yQuadril = yTroncoTop + corpo.AlturaTronco;

                float larguraTronco = Math.Max(10f, corpo.LarguraCintura * 0.5f);
                float xOmbro = larguraTronco * 0.42f;

                pescoco = new PointF(0f, yTroncoTop - 1f);
                ombroDir = new PointF(xOmbro, yTroncoTop + 9f);
                ombroEsq = new PointF(xOmbro - 1.4f, yTroncoTop + 10f);
                quadrilDir = new PointF(1.5f, yQuadril);
                quadrilEsq = new PointF(-1.5f, yQuadril);

                compBracoSuperiorDir = 16f;
                compBracoSuperiorEsq = 16f * 0.92f;
                compAntebracoDir = 14f;
                compAntebracoEsq = 14f * 0.92f;
            }
            else
            {
                float yTroncoTop = -corpo.AlturaTronco / 2f;
                float yQuadril = corpo.AlturaTronco / 2f;

                pescoco = new PointF(0f, yTroncoTop - 2f);
                ombroDir = new PointF(corpo.LarguraOmbros / 2f, yTroncoTop + 8f);
                ombroEsq = new PointF(-corpo.LarguraOmbros / 2f, yTroncoTop + 8f);
                quadrilDir = new PointF(corpo.LarguraQuadril / 4f, yQuadril);
                quadrilEsq = new PointF(-corpo.LarguraQuadril / 4f, yQuadril);

                compBracoSuperiorDir = 18f;
                compBracoSuperiorEsq = 18f;
                compAntebracoDir = 17f;
                compAntebracoEsq = 17f;
            }

            var cotoveloDir = Somar(ombroDir, VetorRotacionado(compBracoSuperiorDir, corpo.AnguloBracoDireito));
            var cotoveloEsq = Somar(ombroEsq, VetorRotacionado(compBracoSuperiorEsq, corpo.AnguloBracoEsquerdo));

            var maoDir = Somar(cotoveloDir, VetorRotacionado(compAntebracoDir, corpo.AnguloBracoDireito + corpo.AnguloCotoveloDireito));
            var maoEsq = Somar(cotoveloEsq, VetorRotacionado(compAntebracoEsq, corpo.AnguloBracoEsquerdo + corpo.AnguloCotoveloEsquerdo));

            var joelhoDir = Somar(quadrilDir, VetorRotacionado(compCoxa, corpo.AnguloPernaDireita));
            var joelhoEsq = Somar(quadrilEsq, VetorRotacionado(compCoxa, corpo.AnguloPernaEsquerda));

            var peDir = Somar(joelhoDir, VetorRotacionado(compCanela, corpo.AnguloPernaDireita + corpo.AnguloJoelhoDireito));
            var peEsq = Somar(joelhoEsq, VetorRotacionado(compCanela, corpo.AnguloPernaEsquerda + corpo.AnguloJoelhoEsquerdo));

            return new Dictionary<ArticulacaoCorpoHandle, PointF>
            {
                [ArticulacaoCorpoHandle.Pescoco] = LocalToWorld(corpo, pescoco),
                [ArticulacaoCorpoHandle.CotoveloDireito] = LocalToWorld(corpo, cotoveloDir),
                [ArticulacaoCorpoHandle.MaoDireita] = LocalToWorld(corpo, maoDir),
                [ArticulacaoCorpoHandle.JoelhoDireito] = LocalToWorld(corpo, joelhoDir),
                [ArticulacaoCorpoHandle.PeDireito] = LocalToWorld(corpo, peDir),
                [ArticulacaoCorpoHandle.CotoveloEsquerdo] = LocalToWorld(corpo, cotoveloEsq),
                [ArticulacaoCorpoHandle.MaoEsquerda] = LocalToWorld(corpo, maoEsq),
                [ArticulacaoCorpoHandle.JoelhoEsquerdo] = LocalToWorld(corpo, joelhoEsq),
                [ArticulacaoCorpoHandle.PeEsquerdo] = LocalToWorld(corpo, peEsq)
            };
        }

        private static PointF LocalToWorld(StickFigure corpo, PointF local)
        {
            float sx = corpo.EscalaCorpo * corpo.EscalaX;
            float sy = corpo.EscalaCorpo * corpo.EscalaY;
            float angulo = corpo.Rotacao + corpo.AnguloCorpo;

            float x = local.X * sx;
            float y = local.Y * sy;

            double rad = angulo * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            return new PointF(
                corpo.Posicao.X + (x * cos - y * sin),
                corpo.Posicao.Y + (x * sin + y * cos));
        }

        private static PointF WorldToLocal(StickFigure corpo, PointF world)
        {
            float sx = corpo.EscalaCorpo * corpo.EscalaX;
            float sy = corpo.EscalaCorpo * corpo.EscalaY;

            if (Math.Abs(sx) < 0.0001f || Math.Abs(sy) < 0.0001f)
                return PointF.Empty;

            float dx = world.X - corpo.Posicao.X;
            float dy = world.Y - corpo.Posicao.Y;

            double rad = -(corpo.Rotacao + corpo.AnguloCorpo) * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float xr = dx * cos - dy * sin;
            float yr = dx * sin + dy * cos;

            return new PointF(xr / sx, yr / sy);
        }

        private static PointF VetorRotacionado(float comprimento, float anguloGraus)
        {
            double rad = anguloGraus * Math.PI / 180.0;
            return new PointF((float)(-Math.Sin(rad) * comprimento), (float)(Math.Cos(rad) * comprimento));
        }

        private static PointF Somar(PointF a, PointF b) => new PointF(a.X + b.X, a.Y + b.Y);

        private static float AnguloDoEixoYPositivo(PointF origem, PointF destino)
        {
            float dx = destino.X - origem.X;
            float dy = destino.Y - origem.Y;
            return (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI - 90.0);
        }

        private static float NormalizarAngulo(float angulo)
        {
            while (angulo > 180f) angulo -= 360f;
            while (angulo < -180f) angulo += 360f;
            return angulo;
        }

        private void AplicarRotacao(BaseSketchObject obj, PointF worldPos)
        {
            var bounds = obj.GetBounds();
            var centro = new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);

            float anguloAnterior = (float)Math.Atan2(_ultimoPontoMouse.Y - centro.Y, _ultimoPontoMouse.X - centro.X);
            float anguloAtual = (float)Math.Atan2(worldPos.Y - centro.Y, worldPos.X - centro.X);
            float deltaGraus = (anguloAtual - anguloAnterior) * 180f / (float)Math.PI;

            if (Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                float passo = 15f;
                float deltaSnapped = (float)(Math.Round(deltaGraus / passo) * passo);
                if (Math.Abs(deltaSnapped) > 0.01f)
                {
                    obj.RotacionarAoRedor(centro, deltaSnapped);
                }
            }
            else
            {
                if (Math.Abs(deltaGraus) > 0.01f)
                {
                    obj.RotacionarAoRedor(centro, deltaGraus);
                }
            }
        }

        private void AplicarRedimensionamento(BaseSketchObject obj, PointF worldPos)
        {
            var bounds = obj.GetBounds();
            if (bounds.Width < 0.001f || bounds.Height < 0.001f)
                return;

            float left = bounds.Left;
            float right = bounds.Right;
            float top = bounds.Top;
            float bottom = bounds.Bottom;

            switch (_alcaAtiva)
            {
                case 0: left = worldPos.X; top = worldPos.Y; break;
                case 1: right = worldPos.X; top = worldPos.Y; break;
                case 2: left = worldPos.X; bottom = worldPos.Y; break;
                case 3: right = worldPos.X; bottom = worldPos.Y; break;
                case 4: top = worldPos.Y; break;
                case 5: bottom = worldPos.Y; break;
                case 6: left = worldPos.X; break;
                case 7: right = worldPos.X; break;
                default: return;
            }

            float larguraAssinada = right - left;
            float alturaAssinada = bottom - top;

            var novo = RectangleF.FromLTRB(
                Math.Min(left, right),
                Math.Min(top, bottom),
                Math.Max(left, right),
                Math.Max(top, bottom));

            float minimoMundo = PixelsParaMundo(5f);
            if (novo.Width < minimoMundo || novo.Height < minimoMundo)
                return;

            float fatorX = (obj is StreetObject ? larguraAssinada : novo.Width) / bounds.Width;
            float fatorY = (obj is StreetObject ? alturaAssinada : novo.Height) / bounds.Height;

            if (_alcaAtiva == 4 || _alcaAtiva == 5) fatorX = 1f;
            if (_alcaAtiva == 6 || _alcaAtiva == 7) fatorY = 1f;

            if (obj is StampObject stamp && stamp.ManterProporcao)
            {
                float fatorUniforme = _alcaAtiva switch
                {
                    4 or 5 => fatorY,
                    6 or 7 => fatorX,
                    _ => Math.Abs(fatorX - 1f) >= Math.Abs(fatorY - 1f) ? fatorX : fatorY
                };

                fatorX = fatorUniforme;
                fatorY = fatorUniforme;
            }

            var centroAntigo = new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
            var centroNovo = new PointF((left + right) / 2f, (top + bottom) / 2f);

            obj.EscalarAoRedor(centroAntigo, fatorX, fatorY);
            obj.Mover(centroNovo.X - centroAntigo.X, centroNovo.Y - centroAntigo.Y);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (_objetosSelecionados.Count == 0) return;

            float step = e.Shift ? 10f : 1f;

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    var aRemover = _objetosSelecionados.ToList();
                    foreach (var obj in aRemover)
                    {
                        _doc.RemoverObjeto(obj);
                    }
                    DesselecionarTodos();
                    SelectionChanged?.Invoke(this, null);
                    var lista = _objetosSelecionados.ToList();
                    MultiSelectionChanged?.Invoke(this, lista);
                    break;

                case Keys.Up:
                    foreach (var obj in _objetosSelecionados)
                        obj.Mover(0, -step);
                    break;

                case Keys.Down:
                    foreach (var obj in _objetosSelecionados)
                        obj.Mover(0, step);
                    break;

                case Keys.Left:
                    foreach (var obj in _objetosSelecionados)
                        obj.Mover(-step, 0);
                    break;

                case Keys.Right:
                    foreach (var obj in _objetosSelecionados)
                        obj.Mover(step, 0);
                    break;
            }
        }

        public void Desenhar(Graphics g)
        {
            if (_selecionandoArea && _selecaoRetangular != RectangleF.Empty)
            {
                using (var pen = new Pen(Color.DodgerBlue, 1f))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(pen,
                        _selecaoRetangular.X, _selecaoRetangular.Y,
                        _selecaoRetangular.Width, _selecaoRetangular.Height);
                }
                using (var brush = new SolidBrush(
                    Color.FromArgb(30, 30, 144, 255)))
                {
                    g.FillRectangle(brush, _selecaoRetangular);
                }
            }

            foreach (var corpo in _objetosSelecionados.OfType<StickFigure>())
            {
                if (!corpo.Bloqueado)
                    DesenharPontosArticulacao(g, corpo);
            }

            foreach (var parede in _objetosSelecionados.OfType<WallObject>())
            {
                if (!parede.Bloqueado)
                    DesenharHandlesParede(g, parede);
            }
        }

        private void DesenharHandlesParede(Graphics g, WallObject parede)
        {
            var handles = parede.GetHandlesMundo();
            if (handles.Count == 0) return;

            var elements = g.Transform.Elements;
            float zoom = Math.Max(0.0001f,
                (float)Math.Sqrt(elements[0] * elements[0] + elements[1] * elements[1]));
            float raio = 4.5f / zoom;
            float penW = 1.5f / zoom;

            foreach (var kv in handles)
            {
                Color cor = kv.Key switch
                {
                    WallHandleType.PosicaoPorta => Color.FromArgb(230, 0, 170, 60),
                    WallHandleType.PosicaoJanela => Color.FromArgb(230, 30, 130, 220),
                    WallHandleType.AnguloPorta => Color.FromArgb(230, 215, 80, 0),
                    _ => Color.Gray
                };
                var p = kv.Value;
                using var brush = new SolidBrush(cor);
                using var pen = new Pen(Color.White, penW);
                g.FillEllipse(brush, p.X - raio, p.Y - raio, raio * 2f, raio * 2f);
                g.DrawEllipse(pen, p.X - raio, p.Y - raio, raio * 2f, raio * 2f);
            }
        }

        public bool EstaSobreHandleParede(PointF worldPos, float toleranciaPixels = 10f)
        {
            if (ObjetoSelecionado is not WallObject parede || parede.Bloqueado)
                return false;

            float tol = PixelsParaMundo(toleranciaPixels);
            foreach (var kv in parede.GetHandlesMundo())
            {
                float ddx = worldPos.X - kv.Value.X;
                float ddy = worldPos.Y - kv.Value.Y;
                if ((float)Math.Sqrt(ddx * ddx + ddy * ddy) <= tol)
                    return true;
            }
            return false;
        }

        private void DesenharPontosArticulacao(Graphics g, StickFigure corpo)
        {
            var pontos = ObterPontosArticulacoesMundo(corpo).Values;

            var elements = g.Transform.Elements;
            float zoomX = (float)Math.Sqrt(elements[0] * elements[0] + elements[1] * elements[1]);
            float zoomY = (float)Math.Sqrt(elements[2] * elements[2] + elements[3] * elements[3]);
            float zoom = Math.Max(0.0001f, (zoomX + zoomY) * 0.5f);

            float raio = 4.5f / zoom;
            using var brush = new SolidBrush(Color.FromArgb(235, 0, 122, 204));
            using var pen = new Pen(Color.White, 1f / zoom);

            foreach (var p in pontos)
            {
                g.FillEllipse(brush, p.X - raio, p.Y - raio, raio * 2f, raio * 2f);
                g.DrawEllipse(pen, p.X - raio, p.Y - raio, raio * 2f, raio * 2f);
            }
        }

        public void Cancelar()
        {
            _arrastando = false;
            _arrastoPendente = false;
            _redimensionando = false;
            _rotacionando = false;
            _alcaAtiva = -1;
            _objetoArrastando = null;
            _objetoTransformando = null;
            _selecionandoArea = false;
            _selecaoRetangular = RectangleF.Empty;
            _arrastandoVerticeArea = false;
            _areaEditandoVertice = null;
            _indiceVerticeArea = -1;
            _arrastandoPontoCurva = false;
            _objetoComCurva = null;
            _arrastandoArticulacaoCorpo = false;
            _corpoArticulando = null;
            _articulacaoCorpoAtiva = ArticulacaoCorpoHandle.Nenhuma;
            _arrastandoHandleParede = false;
            _paredeComHandle = null;
            _posicoesAnterioresGrupo.Clear();
            DesselecionarTodos();
        }

        /// <summary>
        /// Define um único objeto como selecionado (uso público)
        /// </summary>
        public void SelecionarObjeto(BaseSketchObject obj)
        {
            DesselecionarTodos();
            if (obj != null)
            {
                AdicionarSelecionado(obj);
                ObjetoSelecionado = obj;
                SelectionChanged?.Invoke(this, obj);
                var lista = _objetosSelecionados.ToList();
                MultiSelectionChanged?.Invoke(this, lista);
            }
        }

        /// <summary>
        /// Limpa toda a seleção (uso público)
        /// </summary>
        public void LimparSelecao()
        {
            DesselecionarTodos();
            SelectionChanged?.Invoke(this, null);
            var lista = _objetosSelecionados.ToList();
            MultiSelectionChanged?.Invoke(this, lista);
        }
    }
}
