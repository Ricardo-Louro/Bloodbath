# Sistemas de Redes para Jogos - Projecto Final

## Autoria
- Ricardo Louro (21807601)
  

## Descrição Técnica do Projeto

De forma a descrever o funcionamento do projeto, inclue abaixo uma descrição de todos os scripts do projeto, o seu proposito e o seu método de utilização.

Para informações mais detalhadas, incluí comentário extrememamente detalhado em todos os scripts.

### CameraController.cs
Este script faz uso de apenas os componentes manualmente selecionados serem sincronizados através da rede para utilizar a mesma câmera para cada jogador de forma local.

Assim sendo, este script possui uma referência a um PlayerMovement no qual o jogador local se regista. Após isto, a câmera move-se para a posição do jogador, coloca-se à altura dos olhos.

Esta pode ser rodada através de movimento do rato e atualiza a rotação do jogador.

--------------------------------------------------------------------------------

### GroundCheck.cs
Este script informa o jogador controlado pelo cliente local (e apenas esse jogador) se está em contact com o chão ou não através do valor de um bool contido no PlayerMovement.

--------------------------------------------------------------------------------

### HealthPickup.cs - UNUSED
Este script herda da classe Pickable e seria utilizado na criação de um item que recupera vida ao jogador.

O script é funcional (precisaria apenas de verificar se o jogador falta vida suficiente para o utilizar) e existe esse item nos Prefabs no jogo mas devido ao game design, não foi utilizado.

--------------------------------------------------------------------------------

### HealthSystem.cs
Este script controla os pontos de vida do personagem, a manipulação destes e o processo de vida e morte.

Quando os pontos de vida do jogador mudam, este verifica se eles são menores ou iguais a 0. Caso esse seja o caso, muda o estado atual do jogo para morto (alive = false)

Quando o estado de vida do jogador muda, este dá toggle dos componentes visuais quer dos clientes dos outros (componentes que estão desativados sempre na visão em primeira pessoa) quer do cliente do jogador local (a UI Weapon que encontra-se ligada à camera que é utilizada localmente por cada jogador no seu cliente).

Adicionalmente, em caso de morte, também chama o Respawn do jogador que após algum tempo comunica com a classe SpawnSystem para mover o jogador morto para um spawn point e, de seguida, reviver o jogador ao mudar a sua vida de 0 para 75 e mudar o estado de alive para true o que levará ao toggle dos componentes visuais.

--------------------------------------------------------------------------------

### MainMenuHandler.cs
Este script gere a funcionalidade dos elementos de UI no Menu Principal.

Contém os métodos que alteram o estado dos GameObjects neste de forma a o jogador conseguir navegar pelas opções necessárias para inicializar o jogo.

--------------------------------------------------------------------------------

### MatchManager.cs - UNUSED
Este script controla o timer da match e o score de cada jogador.

Quando o timer chega ao fim, calcula qual dos jogadores ganhou (ou multiplos) e devolve uma lista com os vencedores.

De seguida, chama um método que iria mostrar os components de UI necessários para informar os jogadores dos vencedores e dos scores.

Por motivos de restrições de tempo e scope, este script não foi completo e implementado.

--------------------------------------------------------------------------------

### NetworkSetup.cs
Este script trata-se do componente mais importante do jogo que trata das ligações entre os jogadores e trata dos requisitos necessários para o começo do jogo.

Caso o jogador seja o Host e use o Relay Server do Unity, o script irá criar a allocation e obter toda a informação necessária sobre esta e o Relay Server correspondente. Utilizando essa informação, o script é capaz de gerar um código que os Clientes podem introduzir para juntar-se à sessão.

Caso o jogador seja apenas um Cliente e use o Relay Server do Unity, ele pode introduzir o código e juntar-se à allocation criada pelo Host com o código correspondente.

Quando o número desejado de jogadores estiver connectado, o Host irá começar o jogo ao transicionar para a cena de Gameplay, instanciar um Player prefab para cada cliente e dar a ownership destes ao cliente correspondente.

Foi o script mais desafiante do projeto e exigiu bastantes horas de debug e testes com o auxilio do ChatGPT e dos vídeos do professor, nomeadamente bastante baseado no script com o mesmo nome feito para o projeto WyzardsMP.

--------------------------------------------------------------------------------

### OverhealthPickup.cs - UNUSED

Este script, tal como o HealthPickup.cs, herda da classe Pickable e seria utilizado na criação de um item que recupera vida ao jogador acima do limite máximo de vida.

O script é funcional e pronto para ser implementado (precisaria apenas de verificar se o jogador falta vida suficiente para o utilizar) mas por motivos de game design, não foi utilizado.

--------------------------------------------------------------------------------

### Pickable.cs - UNUSED

Este script abstract serve como a base para todos os potenciais Pickups do jogo.

Este pode apenas ser apanhado no Host (para evitar multiplos triggers dos seus efeitos) e quando entra em contact com um jogador, aplica o seu efeito Pickup (para ser definido na classe especifica que herdar desta) e desativa quer o seu modelo quer o seu collider.

O objeto tem em conta o tempo que passou desde que aplicou o seu efeito e mantém-se desativado até o seu cooldown (definido via SerializeField) acabar. Após isto, volta a ser ativo e poderá ser utilizado novamente.

--------------------------------------------------------------------------------

### PlayerMovement.cs

Este script serve para calcular o movimento do jogador local assim como desativar os componentes necessários para que a visão em primeira pessoa deste funcione corretamente.

O script utiliza o facto de todos os aspetos do jogo não estarem sincronizados para simplesmente desativar os componentes visuais do jogador local de forma a que a visão em primeira pessoa funcione corretamente enquanto que para todos os outros potenciais clientes connectados, o modelo está totalmente visivel.

O script informa a câmera de qual é o prefab correspondente ao jogador local e de seguida calcula o seu movimento através das teclas premidas pelo jogador. Calcula também o seu movimento vertical se for premido o espaço enquanto GroundChecker informar que o jogador encontra-se grounded.

--------------------------------------------------------------------------------

### SpawnSystem.cs

Este script escolhe informa os clientes (o cliente tem autoridade sobre o transform do player a que corresponde) para escolherem um spawn point aleatório e enviarem o jogador em questão para esta localização. Apenas o cliente a que esse jogador corresponde irá ser ouvido e a operação irá ocorrer apenas uma vez.

--------------------------------------------------------------------------------

### UIWeapon.cs

Este script é bastante simples, contendo apenas informações importantes para outros scripts, nomeadamente o Transform de onde o line renderer terá de originar no tiro em primeira pessoa (utilizado na classe Weapon) e o GameObject do modelo da arma que é desativado durante a morte e re-ativado após o respawn (utilizado na class HealthSystem).

--------------------------------------------------------------------------------

### Weapon.cs

Este script é um dos mais complexos e importantes do jogo. Ele calcula o tiro e a direção deste e informa os clientes se algo foi atingido e, se sim, o quê. Adicionalmente ativa os HealthSystems para removerem vida igual ao dano da arma caso sejam atingidos.

A maior complexidade deste script vem do facto de ter de gerir o desenho do tiro em primeira pessoa (da arma de UI para o ponto do tiro) e em terceira pessoa (da arma do modelo do jogador para o ponto do tiro) e adicionalmente, gerir este tiro caso venha do servidor ou do cliente.

## Diagrama de Arquitetura de Redes

<img src="/ReadmeImages/AND.png" width="500">


## Bibliografia

- [Base Unity Documentation](https://docs.unity3d.com/ScriptReference/)
- [Unity Netcode Documentation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/api/index.html)
- [Playlist das Aulas - Diogo Andrade](https://www.youtube.com/watch?v=Ql9hg1mvBRM&list=PLheBz0T_uVP3JaTA4wMs38MgOKiIpDpLG)
- [ChatGPT](https://chatgpt.com/)