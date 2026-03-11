# 🔗 Seleção Múltipla e Agrupamento de Objetos

## Visão Geral

O CrimeSketcher agora suporta seleção de múltiplos objetos e agrupamento/desagrupamento de objetos para facilitar a organização e manipulação de elementos no croqui.

## Seleção Múltipla

### Combinações de Teclado

- **Click simples**: Seleciona apenas o objeto clicado
- **Ctrl + Click**: Adiciona/remove o objeto à seleção (toggle)
- **Shift + Click**: Adiciona o objeto à seleção atual
- **Drag (Retângulo)**: Seleciona todos os objetos dentro do retângulo
- **Ctrl + Drag**: Adiciona objetos dentro do retângulo à seleção existente

### Manipulação de Múltiplos Objetos

Quando há múltiplos objetos selecionados, você pode:

- **Mover**: Arraste qualquer objeto selecionado para mover todos os selecionados juntos
- **Teclas de Seta**: Use as setas do teclado para mover 1 pixel (ou 10 com Shift)
  - `↑` / `↓` / `←` / `→` : Move na direção
- **Delete**: Deleta todos os objetos selecionados

### Propriedades

O PropertyGrid (painel direito) sempre mostra as propriedades do último objeto selecionado (objeto ativo).

## Agrupamento

### Agrupar Objetos

1. **Selecione múltiplos objetos** usando as técnicas acima (mínimo 2 objetos)
2. **Pressione `Ctrl+G`** ou clique no botão 🔗 de Agrupar na toolbar
3. Os objetos serão agrupados em um novo **GroupObject**
4. O grupo aparecerá visualmente com seus objetos membros
5. O grupo também aparecerá na lista de objetos do documento

### Desagrupar Objetos

1. **Selecione o grupo** desejado
2. **Pressione `Ctrl+Shift+G`** ou clique no botão ⛓️‍💥 de Desagrupar na toolbar
3. O grupo será desmembrado e seus objetos componentes reaparecerão como objetos individuais

## Comportamento de Grupos

### Características

- Um **GroupObject** funciona como um contenedor invisível para múltiplos objetos
- O grupo **se comporta como um objeto único** para movimento e seleção
- O grupo **desenha todos os seus membros** - não é invisível!
- Quando você arrasta um grupo, **todos os seus membros se movem juntos**
- As posições relativas dos objetos dentro do grupo são preservadas
- Grupos podem ser **salvos e carregados** junto com o documento

### Renderização

- Os membros do grupo são desenhados quando o grupo é desenhado
- A seleção do grupo mostra um retângulo envolvendo todos os membros
- Os objetos membros mantêm suas aparências individuais

### Undo/Redo

- Operações de agrupar e desagrupar suportam **desfazer (Ctrl+Z)** e **refazer (Ctrl+Y)**
- Movimentos de objetos em grupo também são registrados

## Atalhos de Teclado

| Atalho | Ação |
|--------|------|
| `Ctrl+G` | Agrupar objetos selecionados |
| `Ctrl+Shift+G` | Desagrupar grupo selecionado |
| `Ctrl+Click` | Toggle seleção |
| `Shift+Click` | Adicionar à seleção |
| `↑` `↓` `←` `→` | Mover 1 pixel |
| `Shift + Arrow` | Mover 10 pixels |
| `Delete` | Excluir selecionados |

## Exemplos de Uso

### Exemplo 1: Agrupar Móveis de um Cômodo

1. Selecione todos os objetos de móveis do cômodo (camas, mesas, etc.)
2. Pressione `Ctrl+G` para agrupar
3. Agora você pode mover todos os móveis juntos se precisar reorganizar o cômodo
4. Os móveis permanecerão visíveis no croqui como um grupo

### Exemplo 2: Trabalhar com Múltiplos Grupos

1. Crie um grupo de móveis (como acima)
2. Crie um grupo de pontos de evidência
3. Selecione ambos os grupos e mova-os juntos
4. Pode desagrupar qualquer um deles depois

### Exemplo 3: Seleção por Área

1. Clique e arraste para desenhar um retângulo ao redor dos objetos
2. Solte o botão do mouse para selecionar todos os objetos no retângulo
3. Pressione `Ctrl+G` para agrupar

## Limitações e Observações

- **Não há grupos aninhados**: Você não pode agrupar um grupo dentro de outro grupo
- **PropertyGrid**: Mostra propriedades do objeto ativo (último selecionado), não do grupo como um todo
- **Detalhes técnicos**: Groups internamente mantêm referências aos objetos membros e desenham todos eles

## Integração com Outras Ferramentas

- **Ordenação de Camadas**: Grupos não afetam a ordem de camadas dos membros (o grupo aparece como um objeto na lista)
- **Alinhamento**: Você ainda pode alinhar objetos individuais selecionando-os separadamente
- **Snap**: O snap continua funcionando normalmente para todos os objetos

## Troubleshooting

### "Selecione pelo menos 2 objetos para agrupar"
- Você tentou agrupar menos de 2 objetos. Selecione pelo menos 2 antes de agrupar.

### "Selecione um grupo para desagrupar"
- O objeto selecionado não é um grupo. Grupos têm tipo "Grupo" no PropertyGrid.

### Grupo aparece invisível ou "desapareceu"
- **RESOLVIDO**: Este problema foi corrigido na versão atual. Os grupos agora desenham corretamente seus membros.
- Se ainda tiver problemas, verifique:
  - A propriedade `Visível` do grupo está true?
  - Os membros do grupo têm a propriedade `Visível` como true?
  - O grupo está em uma camada visível?

### Os objetos dentro do grupo não se movem juntos
- Certifique-se de que está arrastando o grupo (ou um objeto dentro do grupo)
- Se estiver arrastando um objeto individual que não faz parte de um grupo, apenas esse objeto se moverá

