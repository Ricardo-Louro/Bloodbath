o Descri��o de mensagens de rede
o An�lise de largura de banda/custo/etc
o Diagrama de arquitectura de redes
o Diagrama de protocolo

# Sistemas de Redes para Jogos - Projecto Final

## Autoria

- Ricardo Louro (21807601)
  

## Descri��o T�cnica do Projeto

### CameraController.cs
Este script faz uso de apenas os componentes manualmente selecionados serem sincronizados atrav�s da rede para utilizar a mesma c�mera para cada jogador de forma local.

Assim sendo, este script possui uma refer�ncia a um PlayerMovement no qual o jogador local se regista. Ap�s isto, a c�mera move-se para a posi��o do jogador, coloca-se � altura dos olhos.

Esta pode ser rodada atrav�s de movimento do rato e atualiza a rota��o do jogador.


### GroundCheck.cs
Este script informa o jogador controlado pelo cliente local (e apenas esse jogador) se est� em contact com o ch�o ou n�o atrav�s do valor de um bool contido no PlayerMovement.


### HealthPickup.cs - UNUSED
Este script herda da classe Pickable e seria utilizado na cria��o de um item que recupera vida ao jogador.

O script � funcional e existe esse item nos Prefabs no jogo mas devido ao game design, n�o foi utilizado.


### HealthSystem.cs
Este script controla os pontos de vida do personagem, a manipula��o destes e o processo de vida e morte.

Quando os pontos de vida do jogador mudam, este verifica se eles s�o menores ou iguais a 0. Caso esse seja o caso, muda o estado atual do jogo para morto (alive = false)

Quando o estado de vida do jogador muda, este d� toggle dos componentes visuais quer dos clientes dos outros (componentes que est�o desativados sempre na vis�o em primeira pessoa) quer do cliente do jogador local (a UI Weapon que encontra-se ligada � camera que � utilizada localmente por cada jogador no seu cliente).

Adicionalmente, em caso de morte, tamb�m chama o Respawn do jogador que ap�s algum tempo comunica com a classe SpawnSystem para mover o jogador morto para um spawn point e, de seguida, reviver o jogador ao mudar a sua vida de 0 para 75 e mudar o estado de alive para true o que levar� ao toggle dos componentes visuais.

### NetworkSetup.cs



## Diagrama de Arquitetura de Redes



--------------------------------------------------------------------------------

## Bibliografia

- [Base Unity Documentation](https://docs.unity3d.com/ScriptReference/)
- [Unity Netcode Documentation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/api/index.html)
- [Playlist das Aulas - Diogo Andrade](https://www.youtube.com/watch?v=Ql9hg1mvBRM&list=PLheBz0T_uVP3JaTA4wMs38MgOKiIpDpLG)
- [ChatGPT](https://chatgpt.com/)