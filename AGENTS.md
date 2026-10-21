# Diretrizes para Agentes de IA - Checkpoint Backend

Este documento define regras operacionais, contexto de domínio e convenções arquiteturais obrigatórias para agentes de IA neste repositório.

---

## 1. Contexto do Produto

O Checkpoint é uma rede social para gamers inspirada no Letterboxd, onde a fogueira representa o ponto para salvar o progresso da jornada gamer.
Este repositório é o backend responsável por regras de negócio, persistência e integração com a Steam Web API, consumido pelo frontend (`Checkpoint-frontend`).

### Escopo Funcional:
- `Catalogo`: Consulta, sincronização e cache de metadados de jogos via Steam Web API.
- `Usuarios`: Autenticação, perfis de jogadores, segurança (JWT), estatísticas e sistema de amigos/seguidores.
- `Diario`: Registro de progresso de jogos (*Jogando*, *Zerado*, *Abandonado*), *Backlog* e *Wishlist*.
- `Reviews`: Avaliações (notas), críticas de jogos e interações da comunidade.
- `Listas`: Criação, ordenação e compartilhamento de listas personalizadas de jogos.

---

## 2. Checklist Obrigatório de Execução

1. Leitura prévia: Antes de propor ou realizar alterações, consulte a documentação aplicável (`README.md`, `AGENTS.md`).
2. Compilação estrita: O código DEVE compilar com 0 erros e 0 warnings (`dotnet build`).
3. Testes obrigatórios: Sempre execute os testes antes de concluir qualquer tarefa (`dotnet test`). 100% de sucesso exigido.

---

## 3. Padrão Arquitetural e Stack

- Plataforma: .NET 10 (C#)
- Arquitetura: Monolito em Camadas com Domain-Driven Design (DDD) e Clean Architecture
- Persistência: Entity Framework Core
- Integrações Externas: Steam Web API
- Comunicação: REST APIs (Minimal APIs)

---

## 4. Git e Convenção de Commits

### Gitflow:
- Padrão geral: Branches dedicadas (`feat/nome-da-feat`, `fix/nome-do-fix`), sem alteração direta em `main`/`develop`, solicitando abertura de Pull Request ao usuário.
- Exceção para fase atual: Durante a etapa de setup inicial e estruturação de arquitetura, commits podem ser feitos diretamente na branch principal/develop conforme orientação do usuário.

### Formato Obrigatório de Commit (Full Body):
Todo commit deve conter título objetivo e corpo descritivo estruturado:
```text
<tipo>: <título objetivo>

<descrição do propósito da alteração>

Principais mudanças:
- <item 1>
- <item 2>
```

---

## 5. Convenções Técnicas

1. Nomenclatura: Pastas e projetos sempre em PascalCase (`Api`, `Application`, `Domain`, `Persistence`, `Test`).
2. Camadas do Monolito:
   - `Api`: Apresentação, endpoints (Minimal APIs), middlewares e injeção de dependência (`Extensions/`). Referencia `Application` e `Persistence`.
   - `Application`: Casos de uso, services, DTOs, handlers, validações e utilitários (`Utils/`). Referencia `Domain`.
   - `Domain`: Entidades (`Model/`), value objects (`ValueObject/`), interfaces de repositórios (`Interfaces/Repository/`), exceções (`Exceptions/` e `Exceptions/Common/`) e regras de negócio puras (sem dependência externa ou de banco).
   - `Persistence`: DbContext (`Context/`), mapeamentos EF Core (`Configuration/`), migrations, implementação de repositórios (`Repository/`) e integração externa (Steam Web API). Referencia `Application` e `Domain`.
   - `Test`: Projeto centralizado de testes automatizados (xUnit) organizado por camada (`Test/Application/`, `Test/Domain/`).
3. Arquivo de Solução:
   - A raiz mantém o arquivo de solução (`.sln` / `.slnx`) unificando todos os projetos do monolito.

---

## 6. Restrições e Proibições

- Proibido violar princípios SOLID, isolamento de camadas ou padrões de código limpo.
- Proibido adicionar pacotes NuGet ou dependências externas sem aprovação prévia e explícita do usuário.
- Proibido expor credenciais, senhas, connection strings, chaves da Steam Web API ou tokens.
- Proibido alterar migrations pré-existentes; crie novas migrations para evolução do banco.
