CroquiPC 1.7.3
	O CroquiPC nasceu da necessidade diária dos Peritos Criminais de apresentar um Laudo com croquis esquemáticos de locais de crimes.
Sua área de trabalho é constituída de uma barra de ferramentas básicas e menu na porção superior, uma coluna de ferramentas de desenho e biblioteca de símbolos na porção esquerda e uma coluna de propriedades na porção direita. Ao centro está a tela de desenho (canvas).
<img width="890" height="480" alt="image" src="https://github.com/user-attachments/assets/dde4c433-ec17-4189-8119-e32e7ac0d20f" />

	A Barra de Menus possui as funções básicas de manejo de arquivos, exportação, cópia, inversão, inserção de legenda de Laudo, inserção de imagem externa, zoom etc. Algumas funções estão repetidas na Barra de Ferramentas.
 <img width="757" height="425" alt="image" src="https://github.com/user-attachments/assets/6de62274-4337-49a4-86d7-eceb353c8896" />
 
 A Barra de Ferramentas Básicas agrupa funções rápidas de arquivo e manipulação dos objetos de desenho, além de opções de grade e lista de objetos.
 <img width="843" height="146" alt="image" src="https://github.com/user-attachments/assets/154bf1f8-ad9f-48dc-885e-f18b71af2463" />
 
	Exemplo de Legenda e Lista de Objetos:
 <img width="890" height="573" alt="image" src="https://github.com/user-attachments/assets/279fd7a3-f59d-4f0b-a8d7-ce314832c945" />

A Coluna de Ferramentas de Desenho agrupa os desenhos mais usados na confecção de Croquis. Tais ferramentas funcionam com um clique inicial (liga a ferramenta), o arraste do mouse (o desenho é feito) e um segundo clique (encerra o desenho). A ferramenta fica ativa até que seja selecionada outra ferramenta ou o botão direito do mouse é apertado (ou tecla ESC), quando a ferramenta de Seleção fica ativa.
	Enquanto uma ferramenta de desenho está ativa, a seta do mouse fica na forma de uma cruz (✛). Na função de seleção, aparece a seta normal do Windows.
	As ferramentas desenham de acordo com o alinhamento da grade, quando o “snap” à grade está ativado, e sempre em ângulos a cada 5º. Se, durante o desenho, a tecla SHIFT está pressionada, os ângulos saltam a cada 15º. Nos símbolos, o ângulo 0º é sempre de baixo para cima, na direção vertical e aumenta no sentido horário.
<img width="126" height="100" alt="image" src="https://github.com/user-attachments/assets/fdfbeffb-b5a0-44a9-83b1-aad8ab0dbf46" />

	A Biblioteca de Símbolos possui uma série de figuras prontas categorizadas em abas. Para inserir um símbolo, deve-se clicar duas vezes sobre ele. Os símbolos estão na pasta “Symbols” do programa, onde estão agrupados em pastas de acordo com a categoria. Símbolos extras podem ser copiados diretamente nas pastas ou inseridos pelo botão “Importar Símbolo...”. Os símbolos devem ser arquivos de figura do tipo PNG com fundo transparente.
<img width="890" height="361" alt="image" src="https://github.com/user-attachments/assets/262b00fb-5eb6-4a70-864b-064ff7b4ed30" />

	Qualquer desenho ou símbolo pode ser alterado nas suas propriedades, que são listadas na Coluna de Propriedades. Várias opções que estão presentes ali possuem ajuste fino que pode ser difícil de regular via mouse, como rotação e posição.
 
•	Desenho de planta baixa: o desenho das paredes pode ser da parede apenas, parede com porta ou parede com porta e janela. Na Coluna de Propriedades há a possibilidade de inserir e apagar a porta ou janela e alterar seus tamanhos e posições. A posição é um fator de 0 a 1, onde 0,5 é o meio da parede desenhada coincidindo com o meio do desenho da porta/janela. Há a opção de mostrar a jamba da porta ou não, para o caso vãos livres, de posição da jamba (dentro, fora, direita ou esquerda) e de alterar cores.
<img width="890" height="662" alt="image" src="https://github.com/user-attachments/assets/6cff6ae8-14b4-4ced-a8e0-f80a21b7078f" />

•	Desenho de área ou polígono: é possível inserir um polígono simples para mostrar uma área, onde pode ser customizados a linha e o preenchimento.
•	Desenho de ruas, avenidas e estradas: o desenho básico da rua/estrada aparece como rua com duas pistas, com calçada e sinalização horizontal de linha tracejada laranja. Novas faixas (pista extra, ciclofaixa, faixa de estacionamento) e canteiro central podem ser colocados ou retirados na coluna de propriedades:
o	Rua simples: basta desenhá-la;
o	Rua com uma pista: altere o número de pistas para 1;
o	Rua com faixas de estacionamento e ciclofaixa: há opção de inclusão dessas faixas separadamente. A ciclofaixa fica entre a faixa de estacionamento e a rua. No caso de haver canteiro central, a ciclofaixa fica junto ao canteiro;
o	Estrada: retire as calçadas, insira faixa de estacionamento, altere a cor da faixa e o tipo de sinalização horizontal (linha do estacionamento).
<img width="463" height="915" alt="image" src="https://github.com/user-attachments/assets/1be72052-2ed6-4d25-97c7-63cd04056e08" />

•	Curvas: as ferramentas de Rua, Marca e Seta possuem a possibilidade de serem curvadas, bastando alterar a propriedade “Tem curva” para “Sim”. Um losango azul aparecerá no objeto selecionado para que a curvatura seja feita via arraste do mouse.
<img width="375" height="148" alt="image" src="https://github.com/user-attachments/assets/73ea69e1-df7c-47f7-9ab1-bbcf246c81d2" />

•	Cruzamentos de ruas: quando se cruzam duas ruas, um objeto de cruzamento é formado e, ao ser selecionado, há opções de faixa de pedestres e marcação de PARE. É importante definir as faixas existentes na rua que receberá o cruzamento (Rua 1) antes de fazer o cruzamento da outra rua (Rua 2).
 <img width="890" height="697" alt="image" src="https://github.com/user-attachments/assets/c3e49511-066a-4dfc-b975-ba687c742ead" />

•	Rotatória: A ferramenta Rotatória insere uma rotatória com 4 alças de saída. Nas propriedades é possível alterar o número de alças. 
•	Marcas: as marcas de frenagem, derrapagem etc, são inseridas com esta ferramenta, onde há a abertura de um formulário com os diversos tipos de marcas customizáveis.
<img width="445" height="219" alt="image" src="https://github.com/user-attachments/assets/8900b17e-6194-4263-868c-3871e92d94b4" />

•	Cota/Medida: ferramenta para mostrar distâncias entre um objeto e outro no desenho. É possível colocar um valor ou texto customizado nas Propriedades. 
•	Corpos: há a opção de colocar um corpo articulado, masculino ou feminino, com possibilidade de alterar os ângulos dos membros e da cabeça e alterar a cor de todas as partes do corpo. Todas as dimensões e ângulos são customizáveis via Propriedades.
<img width="243" height="325" alt="image" src="https://github.com/user-attachments/assets/b10615a4-59a9-4ed0-996c-a3f470932a00" />



Renato Ianhez – Perito Criminal II – STRC Patos de Minas
renatoia@terra.com.br

