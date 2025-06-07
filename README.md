o Descrição de mensagens de rede
o Análise de largura de banda/custo/etc
o Diagrama de arquitectura de redes
o Diagrama de protocolo

# Sistemas de Redes para Jogos - Projecto Final

## Autoria

- Ricardo Louro (21807601)
  

## Descrição Técnica do Projeto

### CameraController.cs
Este script faz uso de apenas os componentes manualmente selecionados serem sincronizados através da rede para utilizar a mesma câmera para cada jogador de forma local.
Assim sendo, este script possui uma referência a um PlayerMovement no qual o jogador local se regista. Após isto, a câmera move-se para a posição do jogador, coloca-se à altura dos olhos.
Esta pode ser rodada através de movimento do rato e atualiza a rotação do jogador.

### GroundCheck.cs
Este script informa o jogador controlado pelo cliente local (e apenas esse jogador) se está em contact com o chão ou não através do valor de um bool contido no PlayerMovement.

### NetworkSetup.cs



## Diagrama de Arquitetura de Redes



--------------------------------------------------------------------------------

## Bibliografia

[Base Unity Documentation](https://docs.unity3d.com/ScriptReference/)
[Unity Netcode Documentation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/api/index.html)
[Playlist das Aulas - Diogo Andrade](https://www.youtube.com/watch?v=Ql9hg1mvBRM&list=PLheBz0T_uVP3JaTA4wMs38MgOKiIpDpLG)
[ChatGPT](https://chatgpt.com/)