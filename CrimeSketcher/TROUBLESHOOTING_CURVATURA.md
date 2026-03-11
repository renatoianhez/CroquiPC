# 🔍 Troubleshooting - Curvatura não funciona

## Problema Reportado
A curvatura da ferramenta Marca não está funcionando ao arrastar o ponto de controle.

## Checklist de Verificação

### 1. ✅ Compilação
- [x] Código compila sem erros
- [x] Todas as mudanças foram salvas

### 2. 🔍 Passos para Testar

#### Teste Básico:
1. **Execute** a aplicação
2. **Clique** no botão "🔴 Marca" (grupo Vias e Externos)
3. **Configure** a marca (tipo, largura, etc.) e clique OK
4. **Desenhe** uma marca no canvas (clique e arraste)
5. **Pressione V** ou clique em "🖱️ Selecionar"
6. **Clique** na marca para selecioná-la
7. **Vá no PropertyGrid** (coluna direita)
8. **Procure** a categoria "Curvatura"
9. **Marque** "Tem Curva"
10. **Observe** se aparece um diamante azul (ciano)

#### Se o diamante NÃO aparecer:
❌ O método `DesenharPontosControle` não está sendo chamado
- Verificar se `Selecionado` está true
- Verificar se o método `Desenhar` chama `DesenharPontosControle(g)`

#### Se o diamante aparecer MAS não arrastar:
1. **Passe o mouse** sobre o diamante
   - O cursor deve mudar para ⊕ (SizeAll)
   - Se NÃO mudar: problema no `SketchCanvas.AtualizarCursor`
   
2. **Clique no diamante**
   - Verifique se está clicando EXATAMENTE no diamante
   - Área de tolerância: 12 pixels
   - Se clicar fora, move a marca inteira

3. **Arraste**
   - Se nada acontece: problema no `SelectTool.OnMouseMove`
   - Se move tudo: problema na detecção de prioridade

## Possíveis Causas e Soluções

### Causa 1: Propriedade `Posicao` não definida
**Sintoma**: Marca não é selecionável ou se comporta estranho

**Solução aplicada**:
```csharp
// Em MarkTool.OnMouseDown
_marcaPreview = new MarkObject
{
    PontoInicial = worldPos,
    PontoFinal = worldPos,
    Posicao = worldPos, // ✅ ADICIONADO
    // ...
};

// Em MarkObject.Mover
Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy); // ✅ ADICIONADO
```

### Causa 2: SelectTool não detecta MarkObject
**Sintoma**: Cursor não muda, clique move tudo

**Solução aplicada**:
```csharp
// SelectTool.OnMouseDown - agora verifica MarkObject
else if (ObjetoSelecionado is MarkObject mark && mark.TemCurva)
{
    if (mark.ContemPontoCurva(worldPos, 12f))
    {
        _arrastandoPontoCurva = true;
        _objetoComCurva = mark;
        // ...
    }
}
```

### Causa 3: Canvas não atualiza cursor para MarkObject
**Sintoma**: Cursor não muda ao passar sobre o diamante

**Solução aplicada**:
```csharp
// SketchCanvas.AtualizarCursor
else if (objetoSelecionado is Objects.MarkObject mark && mark.TemCurva)
{
    if (mark.ContemPontoCurva(worldPos, 12f))
    {
        this.Cursor = Cursors.SizeAll;
        return;
    }
}
```

## Teste Manual Passo a Passo

### Teste 1: Verificar Seleção
```
1. Desenhe uma marca
2. Clique em Selecionar (V)
3. Clique na marca
4. Vá no PropertyGrid
5. Verifique se aparece "MarkObject" ou informações da marca
```
✅ Se aparecer: Seleção funciona
❌ Se não aparecer: Problema no HitTest ou Posicao

### Teste 2: Verificar Pontos de Controle
```
1. Com a marca selecionada
2. Observe no canvas
3. Deve ter 2 círculos laranjas (início e fim)
```
✅ Se aparecer: DesenharPontosControle está sendo chamado
❌ Se não aparecer: Problema no método Desenhar

### Teste 3: Ativar Curva
```
1. PropertyGrid → Curvatura → "Tem Curva" = true
2. Observe no canvas
3. Deve aparecer:
   - 2 círculos laranjas (início/fim)
   - 1 diamante azul (centro)
   - 2 linhas tracejadas azuis conectando os pontos
```
✅ Se tudo aparecer: Visualização OK
❌ Se diamante não aparecer: Problema no método DesenharPontosControle

### Teste 4: Testar Cursor
```
1. Com curva ativada e diamante visível
2. Passe o mouse SOBRE o diamante
3. Cursor deve mudar para ⊕ (cruz com setas)
```
✅ Se mudar: SketchCanvas.AtualizarCursor OK
❌ Se não mudar: Verificar implementação do AtualizarCursor

### Teste 5: Arrastar Diamante
```
1. Clique EXATAMENTE no diamante (centro)
2. Segure o botão
3. Mova o mouse
4. A marca deve se curvar
```
✅ Se curvar: TUDO FUNCIONANDO!
❌ Se mover tudo: Detecção de prioridade falhou
❌ Se não fazer nada: OnMouseMove não está sendo chamado

## Código de Debug (Opcional)

Para adicionar mensagens de debug, coloque isso nos métodos:

### SelectTool.OnMouseDown
```csharp
// Adicionar após linha 57
else if (ObjetoSelecionado is MarkObject mark && mark.TemCurva)
{
    System.Diagnostics.Debug.WriteLine($"Mark detectada com curva. PontoCurva: {mark.PontoCurva}");
    if (mark.ContemPontoCurva(worldPos, 12f))
    {
        System.Diagnostics.Debug.WriteLine("CLIQUE NO PONTO DE CURVA DETECTADO!");
        // ...
    }
}
```

### SketchCanvas.AtualizarCursor
```csharp
// Adicionar após linha 305
else if (objetoSelecionado is Objects.MarkObject mark && mark.TemCurva)
{
    System.Diagnostics.Debug.WriteLine($"Verificando cursor para Mark. ContemPonto: {mark.ContemPontoCurva(worldPos, 12f)}");
    if (mark.ContemPontoCurva(worldPos, 12f))
    {
        System.Diagnostics.Debug.WriteLine("CURSOR MUDANDO PARA SIZEALL");
        // ...
    }
}
```

## Status das Correções

✅ **Aplicadas**:
- [x] MarkTool define Posicao inicial
- [x] MarkObject.Mover atualiza Posicao
- [x] SelectTool detecta MarkObject com curva
- [x] SelectTool arrasta ponto de curva de MarkObject
- [x] SketchCanvas muda cursor para MarkObject

## Próximos Passos

Se ainda não funcionar após essas correções:

1. **Reconstruir** a solução (Rebuild)
2. **Fechar e reabrir** o Visual Studio
3. **Limpar** bin/obj (Clean Solution)
4. **Rebuild** novamente
5. **Executar** com debug ativo
6. Colocar **breakpoints** em:
   - `SelectTool.OnMouseDown` linha 57
   - `MarkObject.ContemPontoCurva` linha 186
   - `SketchCanvas.AtualizarCursor` linha 305

## Relatar Problema Detalhado

Se continuar não funcionando, informe:
1. **Qual teste** falha (1-5 acima)
2. **O que acontece** exatamente
3. **Mensagens** do Output/Debug (se adicionou código de debug)
4. Se o **cursor muda** ao passar sobre o diamante
5. Se o **diamante aparece** quando marca "Tem Curva"

---

**Última atualização**: Correções aplicadas em MarkTool e MarkObject para definir Posicao corretamente.
