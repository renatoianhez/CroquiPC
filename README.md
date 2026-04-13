Tutorial – CroquiPC 2.0.2
	O CroquiPC nasceu da necessidade diária dos Peritos Criminais de apresentar um Laudo com croquis esquemáticos de locais de crimes e da inexistência de softwares dedicados que fossem de baixo ou nenhum custo às seções de Perícia.
Sua área de trabalho é constituída de uma barra de ferramentas básicas e menu na porção superior, uma coluna de ferramentas de desenho e biblioteca de símbolos na porção esquerda e uma coluna de propriedades na porção direita. Ao centro está a tela de desenho (canvas).
 
	A Barra de Menus possui as funções básicas de manejo de arquivos, exportação, cópia, inversão, inserção de legenda de Laudo, inserção de imagem externa, zoom etc. Algumas funções estão repetidas na Barra de Ferramentas.
     

A Barra de Ferramentas Básicas agrupa funções rápidas de arquivo e manipulação dos objetos de desenho, além de opções de grade e lista de objetos.
 
 
	Exemplo de Legenda e Lista de Objetos:
 

A Coluna de Ferramentas de Desenho agrupa os desenhos mais usados na confecção de Croquis. Tais ferramentas funcionam com um clique inicial (liga a ferramenta), o arraste do mouse (o desenho é feito) e um segundo clique (encerra o desenho). A ferramenta fica ativa até que seja selecionada outra ferramenta ou o botão direito do mouse é apertado fora do desenho (ou tecla ESC), quando a ferramenta de Seleção fica ativa.
	Enquanto uma ferramenta de desenho está ativa, a seta do mouse fica na forma de uma cruz (✛). Na função de seleção, aparece a seta normal do Windows.
	As ferramentas desenham de acordo com o alinhamento da grade, quando o “snap” à grade está ativado, e sempre em ângulos a cada 5º. Se, durante o desenho, a tecla SHIFT está pressionada, os ângulos saltam a cada 15º. Nos símbolos, o ângulo 0º é sempre de baixo para cima, na direção vertical e aumentam no sentido horário. 
	A Biblioteca de Símbolos possui uma série de figuras prontas categorizadas em abas. Para inserir um símbolo no Croqui, deve-se clicar duas vezes sobre ele. Os símbolos estão na pasta “Symbols” do programa, onde estão agrupados em pastas de acordo com a categoria. Símbolos extras podem ser copiados diretamente nas pastas ou inseridos pelo botão “Importar Símbolo...”. Os símbolos devem ser arquivos de figura do tipo PNG com fundo transparente.
 
	Qualquer desenho ou símbolo pode ser alterado nas suas propriedades, que são listadas na Coluna de Propriedades quando o objeto de desenho está selecionado. Várias opções que estão presentes na Coluna de Propriedades possuem ajuste fino que pode ser difícil de regular via mouse, como rotação e posição.
•	Desenho de planta baixa: o desenho de paredes de planta baixa, inicialmente, é feito somente das paredes simples. Na Coluna de Propriedades há a possibilidade de inserir e apagar a porta ou janela e alterar seus tamanhos e posições. No caso de possuir mais de uma porta ou janela em uma parede, é necessário fazer a parede em duas porções, pois para cada segmento de parede só é possível colocar uma porta e uma janela. A posição da porta ou janela é um fator de 0 a 1, onde 0,5 é o meio da parede desenhada coincidindo com o meio do desenho da porta/janela. Há a opção de mostrar a jamba da porta ou não, para o caso vãos livres, de posição da jamba (dentro, fora, direita ou esquerda) e de alterar cores. 
 
•	Desenho de área ou polígono: é possível inserir um polígono simples para mostrar uma área. Essa ferramenta é útil para o desenho livre de figuras geométricas, onde é possível customizar cores, textura e transparência. O controle de cores e transparência é o mesmo para todos os objetos de desenho e pode ser feito manualmente (são 4 valores de 0 a 255, separados por ponto e vírgula que representam as intensidades de: opacidade; vermelho; verde; e azul) ou por paletas de cores prontas (paletas: Personalizado, Web ou Sistema);
 
•	Desenho de ruas, avenidas e estradas: o desenho básico da rua aparece como uma via com duas pistas, com calçada, meio-fio e sinalização horizontal de linha tracejada laranja. Novas faixas (pista extra, ciclofaixa, faixa de estacionamento) e canteiro central podem ser colocados ou retirados na coluna de propriedades:
o	Rua simples: basta desenhá-la;
o	Rua com uma pista: altere o número de pistas para 1;
o	Rua com faixas de estacionamento e ciclofaixa: há opção de inclusão dessas faixas separadamente. A ciclofaixa fica entre a faixa de estacionamento e a rua. No caso de haver canteiro central, a ciclofaixa fica junto ao canteiro;
o	Estrada: a estrada é uma rua sem calçadas, com a faixa de estacionamento transformada em acostamento.
•	Curvas: as ferramentas de Rua, Marca e Seta possuem a possibilidade de serem curvadas, bastando alterar a propriedade “Tem curva” para “Sim”. Um losango azul (alça de curvatura) aparecerá no objeto selecionado para que a curvatura seja feita via arraste do mouse. A curvatura segue o algoritmo de curva de Bézier. Porém, se durante o arraste do mouse na alça de curvatura a tecla SHIFT é pressionada, o desenho seguirá uma curvatura circular.

•	Cruzamentos de ruas: quando se cruzam duas ruas, um objeto de cruzamento é formado e, ao ser selecionado, há opções de faixa de pedestres e marcação de PARE na Coluna de Propriedades. É importante definir as faixas extras existentes na rua que receberá o cruzamento (Rua 1) antes de fazer o cruzamento da outra rua (Rua 2).
 
•	Rotatória: a ferramenta Rotatória insere uma rotatória com 4 alças de saída. Nas propriedades é possível alterar o número de alças.
•	Trevos: uma série de modelos (“Templates”) de cruzamentos e derivações viários estão já prontos e podem ser selecionados e visualizados por esta ferramenta. Há a possibilidade de utilizar desenhos de outros croquis do próprio usuário, bastando para isso copiar o arquivo de croqui para a pasta “Templates” do programa;

•	Marcas: as marcas de frenagem, derrapagem etc, são inseridas com esta ferramenta, onde há a abertura de um formulário com os diversos tipos de marcas customizáveis. Nas Propriedades é possível alterar todas as características e incluir curvatura, assim como em Rua/Estrada;
•	Cota/Medida: ferramenta para mostrar distâncias entre um objeto e outro no desenho. É possível colocar um valor ou texto customizado nas Propriedades; 
•	Fator de escala para elementos de trânsito: há um CheckBox no grupo de elementos de trânsito para que as medidas sejam corrigidas à escala de trânsito. Visualmente, somente os valores serão alterados (os tamanhos das figuras continuam os mesmos). Afeta as ferramentas de Rua/Estrada, Marca e Cota/Medida. O valor padrão é 3,3, e corresponde à correção para a largura de uma rua/rodovia real;
•	Corpos: há a opção de colocar um corpo articulado, masculino ou feminino, com possibilidade de alterar os ângulos dos membros e da cabeça e alterar a cor de todas as partes do corpo. Todas as dimensões e ângulos são customizáveis via Propriedades.
•	Menus de Contexto: ferramentas básicas e giro, inversão, agrupamento, cópia, ordem etc, podem ser acessadas via menu de contexto, bastando clicar com o botão direito sobre o um objeto de desenho ou um grupo de objetos selecionados.






Renato Ianhez – Perito Criminal II – STRC Patos de Minas
renatoia@terra.com.br
WhatsApp: (34)999269369
