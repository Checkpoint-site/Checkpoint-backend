# 🔥 Checkpoint — Back-end

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-FF6B35?style=for-the-badge)
![Design](https://img.shields.io/badge/Design-DDD%20%2B%20Layered-0A66C2?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-F59E0B?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

> **Salve seu progresso.** O **Checkpoint** é uma rede social para gamers inspirada no *Letterboxd*. Assim como a fogueira nos games representa um ponto seguro para registrar sua jornada, o Checkpoint é o lugar onde jogadores registram o que jogaram, avaliam títulos com **fogueiras**, escrevem relatos, organizam suas estantes e acompanham a jornada de outros jogadores.

Este repositório concentra a **Web API e as regras de negócio** da plataforma Checkpoint, desenvolvida em **C# com .NET 10** seguindo os princípios de **Monólito em Camadas**, **Domain-Driven Design (DDD)** e **Programação Orientada a Objetos (POO)**.

---

## 🎯 Funcionalidades e Arquitetura de Catálogo & Engajamento

- 🎮 **Catálogo de Jogos (*Keyless* Steam Storefront API):** Consulta em `pt-BR`, cache distribuído em **Redis** e sincronização sob demanda (*Lazy Sync* com *fallback* resiliente) no **SQL Server** via `store.steampowered.com/api`, sem necessidade de API Key privada.
- 🔎 **Busca Polimórfica (`Application/Strategies/Busca`):** Estratégias especializadas para busca por **Nome**, **Ano de Lançamento** e **Autor** (*Desenvolvedora* / *Publicadora*).
- 📊 **Trinca de Loggers de Engajamento (`EngajamentoLogger`):**
  - `GeneroLogger`: termômetro global dos 12 gêneros da comunidade (presença em posts, comentários, favoritos, lista de desejos e curtidas).
  - `JogoLogger`: engajamento próprio de cada título (favoritos, lista de desejos, posts e soma de fogueiras), evitando que grandes jogos de gêneros nichados fiquem escondidos.
  - `UsuarioLogger`: relevância dos membros da comunidade para destacar criadores populares e seus relatos.
- 🧠 **Algoritmos Polimórficos de Curadoria (`Application/Strategies/Algoritmos`):** Ordenação dinâmica por **Popularidade do Jogo**, **Tendência de Gênero** e **Custo-Benefício**.
- 📚 **Estantes Polimórficas (`Catalogo` → `Favoritos` e `PretendeJogar`):** Validações específicas por tipo de estante e atualização coordenada dos três loggers.

---

## 🏛️ Arquitetura em Camadas e Isolamento DDD

1. **`Api`:** Endpoints HTTP construídos com **Minimal APIs**, middlewares globais, autenticação JWT no Swagger e injeção de dependência (`Extension/`).
2. **`Application`:** Casos de uso (`Services/`), estratégias polimórficas (`Strategies/`), DTOs, mapeamentos (`Riok.Mapperly`) e utilitários.
3. **`Domain`:** Entidades enxutas de estado e herança com atributos específicos (`Model/`), enums (`Enum/`), contratos (`Interfaces/`) e exceções de domínio (`Exceptions/`).
4. **`Persistence`:** `ApplicationDbContext`, configurações explícitas por entidade (`Configuration/`), `Migrations`, repositórios EF Core (`Repository/`), cache Redis e cliente `SteamStoreService`.
5. **`Test`:** Suíte automatizada com **xUnit** validando polimorfismo, serviços, mapeamentos, extensões de API e integração com a Steam.

---

## 🚀 Como Executar Localmente

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker & Docker Compose (para SQL Server e Redis)

### Comandos

```bash
# 1. Clonar o repositório e configurar variáveis de ambiente
git clone git@github.com:Checkpoint-site/Checkpoint-backend.git
cd Checkpoint-backend
cp .env.example .env

# 2. Subir banco SQL Server e Redis
docker compose up -d

# 3. Aplicar as migrations e iniciar a API (nenhuma Steam API Key é necessária!)
dotnet ef database update --project Persistence --startup-project Api
dotnet run --project Api
```

---

## 📄 Licença

Distribuído sob a licença **MIT**. Veja o arquivo [LICENSE](./LICENSE) para mais detalhes.
