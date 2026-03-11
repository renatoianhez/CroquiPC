# Funcionalidade de Curvas em Ruas

## Como Usar

### 1. **Ativar a Curva**

Quando uma rua está selecionada, você pode ativar a curvatura de duas formas:

#### Pelo PropertyGrid (Coluna Propriedades):
- Selecione a rua
- Na categoria **"Curvatura"**
- Marque a opção **"Tem Curva"**
- Um ponto de controle azul (em forma de diamante) aparecerá no meio da rua

#### Por Código:
```csharp
streetObject.TemCurva = true;
// Isso automaticamente cria um ponto de controle no centro
```

### 2. **Ajustar a Curvatura**

Com a rua selecionada e a curva ativada:

1. Você verá **três pontos**:
   - 🟠 **Ponto Inicial** (círculo laranja/verde)
   - 🟠 **Ponto Final** (círculo laranja/verde)
   - 💠 **Ponto de Controle** (diamante ciano) - *este controla a curvatura*

2. **Arraste o ponto de controle (diamante azul)** para ajustar a curva:
   - **Clique e segure** o diamante azul
   - **Arraste** para qualquer direção
   - A rua se curvará automaticamente seguindo o ponto
   - Quanto mais longe do centro, mais pronunciada a curva
   - As linhas tracejadas mostram a influência do ponto

3. **Importante**: 
   - Clique **exatamente** no diamante azul (ponto de controle)
   - Se clicar na rua (fora do diamante), você moverá a rua inteira
   - O diamante tem uma área de clique maior (~12 pixels) para facilitar

### 3. **Visualização**

Quando a rua está selecionada:
- Linhas tracejadas conectam os pontos de controle
- O asfalto, calçadas e faixas seguem a curva automaticamente
- **Meio-fios** seguem perfeitamente a curvatura ✅
- **Faixas de sinalização** (amarelas e brancas) acompanham a curva ✅
- Todos os tipos de faixa funcionam:
  - Tracejada simples
  - Contínua simples
  - Contínua dupla
  - Faixas mistas (contínua + tracejada)
- Faixas laterais (múltiplas pistas) também seguem a curva
- As texturas são aplicadas ao longo da curva

### 4. **Desativar a Curva**

Para voltar a rua para linha reta:
- Desmarque **"Tem Curva"** no PropertyGrid
- O ponto de controle desaparece e a rua volta a ser reta

## Como Funciona (Técnico)

A implementação usa **Curvas Bézier Quadráticas**:

- **P0** = Ponto Inicial
- **P1** = Ponto de Controle (define a curvatura)
- **P2** = Ponto Final

### Fórmula:
```
B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
onde t varia de 0 a 1
```

A rua é desenhada:
1. Calculando pontos ao longo da curva (30 segmentos)
2. Para cada ponto, calculando a perpendicular local
3. Desenhando a largura da rua perpendicular à tangente

## Implementação Concluída! ✅

A funcionalidade de arrastar o ponto de controle de curva **já está implementada** na `SelectTool`.

### Como Funciona Internamente:

1. **Detecção Prioritária**: Ao clicar, a ferramenta primeiro verifica se está clicando no ponto de curva
2. **Arrasto Separado**: O arrasto do ponto de curva é independente do arrasto da rua
3. **Área de Tolerância**: O ponto de curva tem uma área de clique de 12 pixels de raio

### Ordem de Prioridade no Clique:
1. **Ponto de controle de curva** (se existir e estiver próximo)
2. **Objeto sob o cursor** (hit test normal)
3. **Seleção por área** (se não houver objeto)

## Exemplo de Uso Completo

```csharp
// Criar rua reta
var rua = new StreetObject
{
    PontoInicial = new PointF(100, 100),
    PontoFinal = new PointF(500, 100),
    Largura = 80f,
    NomeRua = "Avenida Principal"
};

// Adicionar curva
rua.TemCurva = true;

// Ajustar o ponto de controle para curvar para cima
rua.MoverPontoCurva(new PointF(300, 50));

// A rua agora forma uma curva suave!
```

## Benefícios

✅ Ruas mais realistas
✅ Melhor representação de vias curvas
✅ Faixas e calçadas seguem a curva automaticamente
✅ **Meio-fios curvos** - acompanham perfeitamente a curvatura
✅ **Sinalização horizontal completa** - todas as faixas funcionam em curvas
✅ Fácil de usar pelo PropertyGrid
✅ Visualmente intuitivo com o ponto de controle
✅ **Suporte completo a todos os tipos de faixa** (tracejada, contínua, dupla, mista)
