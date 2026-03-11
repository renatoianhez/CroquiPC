# 🔴 Ferramenta "Marca" - Documentação

## Visão Geral

A ferramenta **Marca** foi criada para representar evidências físicas em locais de crime, como marcas de frenagem, derrapagem, sulcos e outros tipos de marcas deixadas por veículos, objetos ou outras ações.

## Características Principais

### ✨ Tipos de Marca

1. **Frenagem** 🚗
   - Linhas paralelas escuras
   - Típicas de freadas bruscas
   - 3 linhas paralelas simulando pneus

2. **Derrapagem** 🔄
   - Faixa larga contínua  
   - Marca lateral de perda de controle
   - Com textura irregular

3. **Sulco** ⚠️
   - Marca profunda no solo
   - Bordas definidas
   - Indica impacto ou arrasto pesado

4. **Arranhão** ✏️
   - Linha fina e irregular
   - Pequenos riscos aleatórios
   - Indica raspagem superficial

5. **Rastro** 👣
   - Marca de arrasto
   - Textura hachurada diagonal
   - Útil para indicar movimentação

6. **Impacto** 💥
   - Padrão irregular e disperso
   - Múltiplas marcas pequenas ao redor
   - Indica queda ou colisão

7. **Personalizada** ⚙️
   - Marca básica editável
   - Para casos específicos

### 🎛️ Propriedades Configuráveis

#### No PropertyGrid (após desenhar):
- **Tipo de Marca**: Escolha entre os 7 tipos
- **Largura**: 3 a 50 pixels
- **Intensidade**: Leve, Média, Forte, Muito Forte
- **Cor**: Personalizável (padrão: cinza escuro)
- **Descrição**: Texto descritivo
- **Mostrar Descrição**: Exibe texto acima da marca
- **Tem Curva**: Ativa curvatura Bézier
- **Comprimento**: Calculado automaticamente (somente leitura)

#### Curvatura Bézier:
- ✅ Suporte completo a curvas (igual às ruas)
- Ponto de controle arrastável (diamante azul)
- Todas as texturas seguem a curva

## Como Usar

### 1. Ativar a Ferramenta

**Opção 1: Pelo Painel**
- Clique em "🔴 Marca" no painel de ferramentas
- Localizado em "Vias e Externos"

**Opção 2: Pelo Atalho**
- Pressione `M` (quando disponível)

**Opção 3: Configurar Antes**
- Ao clicar no botão "Marca", abre o formulário de configuração
- Defina tipo, largura, intensidade e cor
- Clique "OK" para iniciar o desenho

### 2. Desenhar a Marca

1. **Clique** no ponto inicial da marca
2. **Arraste** até o ponto final
3. **Solte** o mouse para finalizar
4. A marca aparece com as configurações definidas

### 3. Adicionar Curvatura

1. **Selecione** a marca desenhada (ferramenta Selecionar - V)
2. **PropertyGrid** → **Curvatura** → Marque **"Tem Curva"**
3. Aparece um **diamante azul (ciano)** no centro
4. **Arraste o diamante** para ajustar a curva
   - O cursor muda para ⊕ quando sobre o diamante
   - Clique e arraste o diamante (não a marca)
   - Todas as texturas seguem a curvatura automaticamente

**Importante**: 
- Clique **exatamente no diamante azul** para ajustar a curva
- Se clicar na marca (fora do diamante), você moverá a marca inteira
- O diamante tem área de clique de ~12 pixels para facilitar

### 4. Editar Propriedades

Após desenhar, selecione a marca e edite no PropertyGrid:
- Mudar tipo de marca
- Ajustar largura
- Modificar intensidade
- Alterar cor
- Adicionar/editar descrição

## Exemplos de Uso

### Exemplo 1: Marca de Frenagem
```csharp
// Criar marca de frenagem de 10 metros
var marca = new MarkObject
{
    PontoInicial = new PointF(100, 200),
    PontoFinal = new PointF(300, 200),
    TipoMarca = TipoMarca.Frenagem,
    Largura = 15f,
    Intensidade = IntensidadeMarca.Forte,
    Descricao = "Marca de frenagem - Veículo 1",
    MostrarDescricao = true
};
```

### Exemplo 2: Marca Curva de Derrapagem
```csharp
var marca = new MarkObject
{
    PontoInicial = new PointF(100, 100),
    PontoFinal = new PointF(400, 200),
    TipoMarca = TipoMarca.Derrapagem,
    Largura = 25f,
    Intensidade = IntensidadeMarca.Media,
    TemCurva = true
};

// Ajustar ponto de curva
marca.MoverPontoCurva(new PointF(200, 50));
```

### Exemplo 3: Sulco Profundo
```csharp
var sulco = new MarkObject
{
    PontoInicial = new PointF(150, 300),
    PontoFinal = new PointF(350, 300),
    TipoMarca = TipoMarca.Sulco,
    Largura = 20f,
    Intensidade = IntensidadeMarca.MuitoForte,
    CorMarca = Color.FromArgb(20, 20, 20), // Quase preto
    Descricao = "Sulco de impacto"
};
```

## Detalhes Técnicos

### Renderização

Cada tipo de marca tem um método específico de desenho:
- **Frenagem**: 3 linhas paralelas
- **Derrapagem**: Polígono preenchido com textura tracejada
- **Sulco**: Linha grossa com bordas
- **Arranhão**: Linha fina com riscos irregulares
- **Rastro**: Preenchimento com HatchBrush diagonal
- **Impacto**: Múltiplas elipses aleatórias
- **Personalizada**: Linha simples

### Opacidade por Intensidade

```csharp
Leve: 40% de opacidade
Média: 70% de opacidade
Forte: 90% de opacidade
Muito Forte: 100% de opacidade
```

### Curvas Bézier

Utiliza a mesma implementação das ruas:
- Curva Bézier quadrática (3 pontos)
- 30 segmentos para suavidade
- Texturas seguem a curvatura
- Ponto de controle visual (diamante azul)

## Integração com o Sistema

### Arquivos Criados:
1. **MarkEnums.cs** - Enumerações (TipoMarca, IntensidadeMarca)
2. **MarkObject.cs** - Classe do objeto Marca
3. **MarkTool.cs** - Ferramenta de desenho
4. **FormConfiguracaoMarca.cs** - Diálogo de configuração

### Integrado em:
- **FormPrincipal.cs** - Adicionado botão e lógica
- Grupo "Vias e Externos"
- Atalho 'M' (configurável)

## Dicas de Uso

### ✅ Boas Práticas:
- Use **Frenagem** para marcas de freio de veículos
- Use **Derrapagem** para perda de controle lateral
- Use **Sulco** para marcas profundas ou de arrasto pesado
- Use **Rastro** para movimentação de objetos/corpos
- Sempre adicione **Descrição** para identificação clara
- Use **Intensidade Forte** para marcas muito visíveis
- Ajuste a **Cor** se necessário (marcas em superfícies claras)

### ⚠️ Observações:
- Marcas muito curtas (< 5 pixels) não são criadas
- A descrição aparece acima da marca quando visível
- Marcas curvadas requerem mais processamento
- Use **Intensidade Leve** para marcas antigas/desbotadas

## Atalhos de Teclado

- `M` - Ativar ferramenta Marca (após clicar, abre configuração)
- `Esc` - Cancelar desenho em andamento
- `Delete` - Deletar marca selecionada

## Compatibilidade

✅ Totalmente integrado com:
- Sistema de Undo/Redo
- Serialização (salvar/carregar documentos)
- PropertyGrid (edição de propriedades)
- SelectTool (seleção e edição)
- Sistema de curvas Bézier

## Casos de Uso Típicos

1. **Acidente de Trânsito**
   - Marcar frenagens de veículos envolvidos
   - Indicar derrapagens e pontos de impacto
   - Documentar trajeto com curvas

2. **Local de Crime**
   - Marcar rastros de arrasto
   - Indicar sulcos de objetos pesados
   - Documentar arranhões em superfícies

3. **Perícia de Veículos**
   - Registrar marcas em solo
   - Documentar trajetórias curvas
   - Marcar pontos de impacto

## Atualizações Futuras (Sugestões)

- [ ] Marca com largura variável ao longo do comprimento
- [ ] Padrões de pneus específicos
- [ ] Biblioteca de marcas pré-definidas
- [ ] Exportar legenda de marcas automaticamente
- [ ] Medição automática de comprimento de frenagem
- [ ] Cálculo de velocidade estimada (baseado em comprimento)

---

**Desenvolvido para uso pericial em documentação de locais de crime**
