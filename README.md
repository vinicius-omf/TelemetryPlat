# Telemetry Platform

Plataforma de **telemetria industrial** em .NET 10: cadastro de equipamentos de planta (ex.: termopares de Alto Forno),
ingestão de leituras em tempo real, processamento via mensageria e dashboard ao vivo.

> Projeto de estudo e portfólio, construído com práticas de produção: Clean Architecture, testes automatizados,
> containers, CI/CD e observabilidade.

## Arquitetura

```
Api ──► Infrastructure ──► Application ──► Domain
```

| Projeto | Responsabilidade |
|---|---|
| `TelemetryPlatform.Domain` | Entidades e regras de negócio (sem dependências externas) |
| `TelemetryPlatform.Application` | Casos de uso e contratos (portas) |
| `TelemetryPlatform.Infrastructure` | EF Core + PostgreSQL (futuro: RabbitMQ, Redis) |
| `TelemetryPlatform.Api` | Minimal APIs, ProblemDetails, health checks, OpenAPI |

Decisões de arquitetura ficam registradas em [`docs/adr`](docs/adr).

**Stack atual:** .NET 10 · ASP.NET Core Minimal APIs · EF Core 10 · PostgreSQL 17 · xUnit v3 · Shouldly ·
Testcontainers · Docker Compose · GitHub Actions

## Como rodar

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download) e Docker Desktop (versão recente).

```bash
docker compose up -d                                  # PostgreSQL + RabbitMQ
dotnet run --project src/TelemetryPlatform.Api        # aplica as migrations automaticamente em Development
```

- Documentação interativa da API: http://localhost:5008/scalar
- Health check: http://localhost:5008/health

```bash
dotnet test                                           # unitários + integração (Testcontainers, exige Docker)
```

### Migrations

```bash
dotnet tool restore
dotnet ef migrations add <Nome> --project src/TelemetryPlatform.Infrastructure --startup-project src/TelemetryPlatform.Api --output-dir Persistence/Migrations
```

## Roadmap

### Mês 1 — Fundamentos do .NET moderno
- [x] Solução .NET 10 com Clean Architecture
- [x] Cadastro de equipamentos (Minimal APIs + EF Core + PostgreSQL)
- [x] Tratamento de erros com ProblemDetails + Result pattern
- [x] Testes unitários (xUnit v3, Shouldly, FakeTimeProvider)
- [x] Testes de integração (WebApplicationFactory + Testcontainers)
- [x] Docker Compose e pipeline de CI (GitHub Actions)
- [ ] Atualizar e remover equipamento (`PUT`/`DELETE`) — **faça você mesmo, com testes**
- [ ] Paginação e filtro por área na listagem
- [ ] Validação de entrada com FluentValidation
- [ ] Logs estruturados com Serilog
- [ ] Estudar: DI lifetimes, middleware, `IOptions<T>`, async/await, recursos do C# 12–14

### Mês 2 — Arquitetura e mensageria
- [ ] Entidade `SensorReading` (leituras com timestamp, valor e qualidade)
- [ ] Simulador de equipamentos publicando leituras (Worker Service)
- [ ] RabbitMQ + MassTransit: ingestão assíncrona das leituras
- [ ] CQRS: separar comandos e consultas em handlers
- [ ] Outbox pattern para publicação confiável
- [ ] ADR: por que RabbitMQ e não Kafka

### Mês 3 — Tempo real e front-end
- [ ] Redis: cache da última leitura por equipamento
- [ ] Regras de alarme (limites alto/baixo) com eventos de domínio
- [ ] SignalR: streaming de leituras e alarmes
- [ ] Dashboard Angular (standalone components + signals)
- [ ] Dockerfile da API e do Worker

### Mês 4 — Produção
- [ ] Autenticação JWT / OAuth2 (Keycloak ou Entra ID)
- [ ] Observabilidade com OpenTelemetry (traces, métricas, logs)
- [ ] Resiliência com Polly / `Microsoft.Extensions.Resilience`
- [ ] .NET Aspire para orquestração local
- [ ] Deploy no Azure (Container Apps + PostgreSQL Flexible Server)

### Mês 5+ — Diferenciais
- [ ] Simulador OPC UA real (OPC Foundation .NET Standard)
- [ ] TimescaleDB para séries temporais
- [ ] Testes de carga (k6) e otimização de performance
- [ ] Integração com IA: consulta em linguagem natural sobre os dados (Microsoft.Extensions.AI)
