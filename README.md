# BncBot

Sistema desktop desenvolvido em **.NET 8 (WPF)** para monitoramento de disputas em tempo real através do navegador Google Chrome utilizando **Microsoft Playwright**.

## Funcionalidades

* Detecção automática de abas abertas do Chrome.
* Integração com Chrome via Remote Debugging.
* Monitoramento simultâneo de múltiplas abas.
* Identificação automática da posição no ranking.
* Indicação visual de liderança:

  * 🟢 Verde = Em primeiro lugar.
  * 🔴 Vermelho = Fora da primeira posição.
* Configuração individual por aba:

  * Valor mínimo.
  * Valor máximo.
  * Incremento.
  * Tipo de disputa (Menor ou Maior).
* Atualização manual das abas abertas.
* Sistema de atualização automática de versões.
* Instalação via Setup (Inno Setup).

## Tecnologias Utilizadas

* C#
* .NET 8
* WPF
* Microsoft Playwright
* GitHub Releases
* Inno Setup

## Requisitos

### Windows

* Windows 10 ou superior.
* Google Chrome instalado.

### Dependências

O Playwright já é distribuído junto da aplicação publicada.

## Instalação

1. Baixe a versão mais recente em:

   Releases do projeto.

2. Execute:

   BncBot_Setup.exe

3. Conclua a instalação normalmente.

## Atualizações

Ao iniciar o sistema, o BncBot verifica automaticamente se existe uma nova versão disponível.

Caso exista:

* O usuário será notificado.
* Será exibida a opção para baixar a nova versão.
* O download será realizado diretamente pelo GitHub Releases.

## Como Utilizar

### 1. Abrir o Chrome

O sistema inicializa automaticamente uma instância compatível do Chrome quando necessário.

### 2. Abrir as páginas desejadas

Abra as páginas que serão monitoradas.

### 3. Atualizar Abas

Clique em:

Atualizar Abas

Todas as páginas compatíveis serão carregadas na grade principal.

### 4. Configurar Monitoramento

Para cada linha:

* Tipo de disputa:

  * Menor
  * Maior

* Valor mínimo

* Valor máximo

* Incremento

### 5. Iniciar

Clique em:

▶ Iniciar

O monitoramento será iniciado para aquela aba.

### 6. Parar

Clique em:

⏹ Parar

O monitoramento será encerrado.

## Estrutura do Projeto

```text
BncBot
│
├── Models
├── Services
├── Workers
├── Views
├── Converters
│
├── App.xaml
├── MainWindow.xaml
└── BncBot.csproj
```

## Serviços

### ChromeService

Responsável por:

* Conectar ao Chrome.
* Obter abas abertas.
* Gerenciar contexto Playwright.

### ChromeLauncherService

Responsável por:

* Verificar se o Chrome Debug está ativo.
* Inicializar o Chrome automaticamente.

### BncService

Responsável por:

* Obter ranking.
* Ler informações do lote.
* Verificar posição.
* Calcular próximos valores.
* Validar limites.
* Preencher campos.
* Confirmar operações.

### UpdateService

Responsável por:

* Consultar version.json.
* Verificar novas versões.
* Direcionar o usuário para download.

## Publicação

O projeto utiliza:

* Visual Studio Publish
* Inno Setup
* GitHub Releases

Fluxo:

```text
Build
 ↓
Publish
 ↓
Inno Setup
 ↓
GitHub Release
 ↓
Atualização automática
```

## Autor

Matheus Henrique

## Licença

Projeto de uso privado.
