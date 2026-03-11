# Changelog - Funcionalidade de Curvas em Ruas

## ✅ Implementações Realizadas

### 1. **StreetObject.cs** - Suporte a Curvas Bézier

#### Novas Propriedades:
- `PointF? PontoCurva` - Ponto de controle opcional da curva
- `bool TemCurva` - Ativa/desativa a curvatura (visível no PropertyGrid)

#### Novos Métodos:
- `GetPontoNaCurva(float t)` - Calcula pontos na curva Bézier quadrática
- `GetTangenteNaCurva(float t)` - Calcula a direção tangente em cada ponto
- `GetPerpendicularNaCurva(float t)` - Retorna perpendicular para largura da rua
- `GetCurvaPath()` - Cria GraphicsPath da curva
- `ContemPontoCurva(PointF, float)` - Verifica clique no ponto de controle
- `MoverPontoCurva(PointF)` - Move o ponto de controle
- `DesenharLinhaTracejadaCurva()` - Desenha faixa tracejada seguindo a curva
- `DesenharLinhaDuplaCurva()` - Desenha faixa dupla seguindo a curva

#### Métodos Modificados:
- `DesenharCalcadas()` - Agora desenha calçadas seguindo a curva ✅
- `DesenharAsfalto()` - Asfalto adaptado para curvas ✅
- `DesenharMeioFio()` - **NOVO: Meio-fio agora segue a curvatura** ✅
- `DesenharFaixaCentral()` - **NOVO: Faixa central segue a curvatura** ✅
- `DesenharFaixasLaterais()` - **NOVO: Faixas laterais seguem a curvatura** ✅
- `DesenharPontosConexao()` - Visualiza o ponto de controle (diamante ciano)
- `GetBounds()` - Calcula bounds considerando a curvatura
- `Mover()` - Move também o ponto de curva

---

### 2. **SelectTool.cs** - Edição Interativa de Curvas

#### Novos Campos:
- `bool _arrastandoPontoCurva` - Flag de arrasto do ponto de curva
- `StreetObject _ruaComCurva` - Referência à rua sendo editada
- `PointF _pontoCurvaAnterior` - Posição anterior para undo

#### Métodos Modificados:
- `OnMouseDown()` - Detecta clique prioritário no ponto de controle
- `OnMouseMove()` - Arrasta o ponto de curva independentemente
- `OnMouseUp()` - Finaliza arrasto do ponto de curva
- `Cancelar()` - Limpa flags de arrasto de curva

#### Comportamento:
1. **Prioridade de Clique:**
   - Ponto de controle de curva (se existir)
   - Objeto sob o cursor
   - Seleção por área

2. **Tolerância de Clique:** 12 pixels de raio no ponto de controle

---

### 3. **SketchCanvas.cs** - Feedback Visual de Cursor

#### Novo Método:
- `AtualizarCursor(PointF)` - Muda cursor quando sobre ponto de controle

#### Comportamento:
- **Cursor SizeAll** (⊕) quando sobre o ponto de controle
- **Cursor padrão da ferramenta** em outras situações

---

## 🎨 Experiência do Usuário

### Como Usar:

1. **Criar/Selecionar Rua**
2. **PropertyGrid → Curvatura → Marcar "Tem Curva"**
3. **Visualizar** diamante azul (ponto de controle)
4. **Arrastar** o diamante para ajustar a curvatura
5. **Cursor muda** automaticamente ao passar sobre o ponto

### Indicadores Visuais:

- 🔷 **Diamante Ciano** = Ponto de controle de curva
- 🔵 **Linhas Tracejadas** = Influência do ponto de controle
- ⊕ **Cursor SizeAll** = Indica que pode arrastar o ponto
- 🟠 **Círculos Laranja/Verde** = Pontos inicial/final da rua

---

## 📐 Detalhes Técnicos

### Curva Bézier Quadrática:

```
B(t) = (1-t)²·P₀ + 2(1-t)t·P₁ + t²·P₂

Onde:
- P₀ = Ponto Inicial
- P₁ = Ponto de Controle (define curvatura)
- P₂ = Ponto Final
- t ∈ [0, 1]
```

### Renderização:
- **30 segmentos** para aproximação suave da curva
- Largura perpendicular calculada em cada ponto
- Textura aplicada ao longo da curva

---

## 🚀 Benefícios

✅ **Ruas Realistas** - Representa melhor vias curvas reais  
✅ **Fácil de Usar** - Ativa/desativa pelo PropertyGrid  
✅ **Edição Intuitiva** - Arraste visual do ponto de controle  
✅ **Feedback Imediato** - Cursor muda ao passar sobre o ponto  
✅ **Preserva Qualidade** - Calçadas, faixas e texturas seguem a curva  
✅ **Compatível** - Funciona com serialização (salvar/carregar)

---

## 📝 Notas

- O ponto de controle é **salvo/carregado** automaticamente
- Marcar "Tem Curva" cria o ponto no **centro** da rua
- Desmarcar "Tem Curva" **remove** o ponto e volta para linha reta
- O arrasto do ponto de curva é **independente** do arrasto da rua
- Sistema pronto para **undo/redo** (TODO: implementar action específica)

---

## 🔜 Melhorias Futuras (Opcional)

- [ ] Ação de Undo/Redo específica para movimento do ponto de curva
- [ ] Suporte a múltiplos pontos de controle (curvas complexas)
- [ ] Ajuste de curvatura por slider no PropertyGrid
- [ ] Mostrar raio de curvatura estimado
- [ ] Snap do ponto de controle à grade
