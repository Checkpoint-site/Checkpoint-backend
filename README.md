# 🔥 Checkpoint — Back-end

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-FF6B35?style=for-the-badge)
![Design](https://img.shields.io/badge/Design-DDD%20%2B%20Layered-0A66C2?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-F59E0B?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

> **Salve seu progresso.** O **Checkpoint** é uma rede social para gamers inspirada no *Letterboxd*. Assim como a fogueira nos games representa um ponto seguro para registrar sua jornada, o Checkpoint é o lugar onde jogadores registram o que jogaram, avaliam títulos, escrevem reviews, organizam seu backlog e acompanham a jornada de outros jogadores.

Este repositório concentra a **Web API e as regras de negócio** da plataforma Checkpoint, desenvolvida em **C# com .NET 10** seguindo os princípios de **Monólito Modular**, **Arquitetura em Camadas** e **Domain-Driven Design (DDD)**.

---

## 🎯 Funcionalidades Planejadas

> *Nota: O projeto encontra-se em fase inicial de desenvolvimento; o escopo abaixo representa a visão inicial do produto e pode evoluir.*

- 🎮 **Catálogo de Jogos (Steam Web API):** Consulta e sincronização de dados de jogos diretamente do catálogo da Steam, evitando o cadastro manual.
- ⭐ **Avaliações e Reviews:** Sistema de notas e críticas de jogos.
- 📓 **Diário de Jogos:** Registro do histórico e status de progresso do jogador (*Jogando*, *Zerado*, *Abandonado*).
- 📌 **Backlog & Wishlist:** Organização dos jogos que o usuário pretende jogar futuramente (Backlog) ou deseja adquirir (Wishlist).
- 👥 **Perfis e Seguidores:** Perfis de jogadores e sistema de amigos/seguidores.
- 📋 **Listas Personalizadas:** Criação e compartilhamento de listas de jogos pela comunidade.

---

## 🏛️ Arquitetura e Design de Software

O back-end é construído como um **Monólito Modular** em **.NET 10** com **Minimal APIs**, utilizando **Classic DDD** e **Arquitetura em Camadas** de forma direta e enxuta.

### 1. Classic DDD & Arquitetura em Camadas

O projeto é organizado diretamente em **4 camadas principais** (`Api`, `Application`, `Domain` e `Persistence`):

1. **`Api`:**
   - Endpoints HTTP construídos com **Minimal APIs**, middlewares, configuração de injeção de dependência e `Program.cs`.
2. **`Application`:**
   - Casos de uso, Services/Handlers, DTOs e contratos da aplicação. Orquestra os fluxos chamando o domínio e a persistência.
3. **`Domain`:**
   - **Coração do negócio**, isolado de banco de dados e frameworks. Contém Entidades ricas, *Value Objects*, *Aggregates*, regras de negócio e interfaces de repositórios.
4. **`Persistence`:**
   - Camada de dados e integrações: configuração do banco de dados (EF Core, `DbContext`, mapeamentos, Migrations), implementação concreta dos repositórios e integração com a **Steam Web API**.

---

### 2. Organização Modular por Domínio

Dentro das camadas, as funcionalidades são separadas de forma modular pelos contextos do sistema:

| Contexto / Módulo | Responsabilidade Principal |
| :--- | :--- |
| **`Catalog`** | Integração com a **Steam Web API** e catálogo de jogos. |
| **`Users`** | Contas, perfis de jogadores e sistema de amigos/seguidores. |
| **`Diary`** | Diário de jogos (*Jogando*, *Zerado*, *Abandonado*), *Backlog* e *Wishlist*. |
| **`Reviews`** | Avaliações (notas), críticas e interações da comunidade. |
| **`Lists`** | Criação e compartilhamento de listas personalizadas de jogos. |

---

## 📂 Estrutura de Pastas Planejada

```text
Checkpoint-backend/
├── Api/                    # Minimal APIs (Endpoints, Middlewares, Extensions, Program.cs)
├── Application/            # Services, Use Cases, DTOs e Utils
├── Domain/                 # Model, ValueObject, Interfaces/Repository e Exceptions
├── Persistence/            # Context, Configuration, Migrations, Repository e Steam API Client
├── Test/                   # Testes automatizados (xUnit) organizados por camada
├── .editorconfig           # Padronização de estilo e formatação de código
├── AGENTS.md               # Diretrizes arquiteturais e operacionais para agentes de IA
├── Checkpoint.sln          # Solução unificada na raiz
├── Directory.Build.props   # Configurações globais de compilação (.NET 10)
├── .gitignore
├── LICENSE
└── README.md
```

---

## 🛠️ Stack e Repositórios

### Stack do Back-end
- **Linguagem:** C#
- **Framework:** .NET 10 (Minimal APIs)
- **Dados de Jogos:** [Steam Web API](https://steamcommunity.com/dev)
- **Arquitetura:** Monólito Modular + Classic DDD (`Api`, `Application`, `Domain`, `Persistence`)

### Organização no GitHub ([Checkpoint-site](https://github.com/Checkpoint-site))
- 🔙 **[Checkpoint-backend](https://github.com/Checkpoint-site/Checkpoint-backend):** API e regras de negócio em C# / .NET 10 *(este repositório)*.
- 🖥️ **[Checkpoint-frontend](https://github.com/Checkpoint-site/Checkpoint-frontend):** Aplicação web em React + Vite.
- 🏢 **[.github](https://github.com/Checkpoint-site/.github):** README e configurações do perfil da organização.

---

## 🚀 Como Executar Localmente

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Chave da [Steam Web API](https://steamcommunity.com/dev/apikey)

### Comandos

```bash
# 1. Clonar o repositório
git clone git@github.com:Checkpoint-site/Checkpoint-backend.git
cd Checkpoint-backend

# 2. Configurar a chave da Steam Web API via User Secrets
dotnet user-secrets set "SteamApi:ApiKey" "SUA_STEAM_API_KEY"

# 3. Restaurar pacotes e rodar a API
dotnet restore
dotnet run
```

---

## 📌 Status do Projeto

🚧 **Fase inicial de desenvolvimento.**

---

## 📄 Licença

Distribuído sob a licença **MIT**. Veja o arquivo [LICENSE](./LICENSE) para mais detalhes.
