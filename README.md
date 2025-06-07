# Sistemas de Redes para Jogos - Projecto Final

## Autoria
- Ricardo Louro (21807601)
  

## Descri��o T�cnica do Projeto

### CameraController.cs
Este script faz uso de apenas os componentes manualmente selecionados serem sincronizados atrav�s da rede para utilizar a mesma c�mera para cada jogador de forma local.

Assim sendo, este script possui uma refer�ncia a um PlayerMovement no qual o jogador local se regista. Ap�s isto, a c�mera move-se para a posi��o do jogador, coloca-se � altura dos olhos.

Esta pode ser rodada atrav�s de movimento do rato e atualiza a rota��o do jogador.

--------------------------------------------------------------------------------

### GroundCheck.cs
Este script informa o jogador controlado pelo cliente local (e apenas esse jogador) se est� em contact com o ch�o ou n�o atrav�s do valor de um bool contido no PlayerMovement.

--------------------------------------------------------------------------------

### HealthPickup.cs - UNUSED
Este script herda da classe Pickable e seria utilizado na cria��o de um item que recupera vida ao jogador.

O script � funcional (precisaria apenas de verificar se o jogador falta vida suficiente para o utilizar) e existe esse item nos Prefabs no jogo mas devido ao game design, n�o foi utilizado.

--------------------------------------------------------------------------------

### HealthSystem.cs
Este script controla os pontos de vida do personagem, a manipula��o destes e o processo de vida e morte.

Quando os pontos de vida do jogador mudam, este verifica se eles s�o menores ou iguais a 0. Caso esse seja o caso, muda o estado atual do jogo para morto (alive = false)

Quando o estado de vida do jogador muda, este d� toggle dos componentes visuais quer dos clientes dos outros (componentes que est�o desativados sempre na vis�o em primeira pessoa) quer do cliente do jogador local (a UI Weapon que encontra-se ligada � camera que � utilizada localmente por cada jogador no seu cliente).

Adicionalmente, em caso de morte, tamb�m chama o Respawn do jogador que ap�s algum tempo comunica com a classe SpawnSystem para mover o jogador morto para um spawn point e, de seguida, reviver o jogador ao mudar a sua vida de 0 para 75 e mudar o estado de alive para true o que levar� ao toggle dos componentes visuais.

--------------------------------------------------------------------------------

### MainMenuHandler.cs
Este script gere a funcionalidade dos elementos de UI no Menu Principal.

Cont�m os m�todos que alteram o estado dos GameObjects neste de forma a o jogador conseguir navegar pelas op��es necess�rias para inicializar o jogo.

--------------------------------------------------------------------------------

### MatchManager.cs - UNUSED
Este script controla o timer da match e o score de cada jogador.

Quando o timer chega ao fim, calcula qual dos jogadores ganhou (ou multiplos) e devolve uma lista com os vencedores.

De seguida, chama um m�todo que iria mostrar os components de UI necess�rios para informar os jogadores dos vencedores e dos scores.

Por motivos de restri��es de tempo e scope, este script n�o foi completo e implementado.

--------------------------------------------------------------------------------

### NetworkSetup.cs
Este script trata-se do componente mais importante do jogo que trata das liga��es entre os jogadores e trata dos requisitos necess�rios para o come�o do jogo.

Caso o jogador seja o Host e use o Relay Server do Unity, o script ir� criar a allocation e obter toda a informa��o necess�ria sobre esta e o Relay Server correspondente. Utilizando essa informa��o, o script � capaz de gerar um c�digo que os Clientes podem introduzir para juntar-se � sess�o.

Caso o jogador seja apenas um Cliente e use o Relay Server do Unity, ele pode introduzir o c�digo e juntar-se � allocation criada pelo Host com o c�digo correspondente.

Quando o n�mero desejado de jogadores estiver connectado, o Host ir� come�ar o jogo ao transicionar para a cena de Gameplay, instanciar um Player prefab para cada cliente e dar a ownership destes ao cliente correspondente.

Foi o script mais desafiante do projeto e exigiu bastantes horas de debug e testes com o auxilio do ChatGPT e dos v�deos do professor, nomeadamente bastante baseado no script com o mesmo nome feito para o projeto WyzardsMP.

--------------------------------------------------------------------------------

### OverhealthPickup.cs - UNUSED

Este script, tal como o HealthPickup.cs, herda da classe Pickable e seria utilizado na cria��o de um item que recupera vida ao jogador acima do limite m�ximo de vida.

O script � funcional e pronto para ser implementado (precisaria apenas de verificar se o jogador falta vida suficiente para o utilizar) mas por motivos de game design, n�o foi utilizado.

--------------------------------------------------------------------------------


## Diagrama de Arquitetura de Redes

## Bibliografia

- [Base Unity Documentation](https://docs.unity3d.com/ScriptReference/)
- [Unity Netcode Documentation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/api/index.html)
- [Playlist das Aulas - Diogo Andrade](https://www.youtube.com/watch?v=Ql9hg1mvBRM&list=PLheBz0T_uVP3JaTA4wMs38MgOKiIpDpLG)
- [ChatGPT](https://chatgpt.com/)