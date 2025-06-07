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

### NetworkSetup.cs



## Diagrama de Arquitetura de Redes



--------------------------------------------------------------------------------

## Bibliografia

[Base Unity Documentation](https://docs.unity3d.com/ScriptReference/)
[Unity Netcode Documentation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/api/index.html)
[Playlist das Aulas - Diogo Andrade](https://www.youtube.com/watch?v=Ql9hg1mvBRM&list=PLheBz0T_uVP3JaTA4wMs38MgOKiIpDpLG)
[ChatGPT](https://chatgpt.com/)