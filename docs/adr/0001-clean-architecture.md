# ADR 0001 — Clean Architecture em 4 projetos

- **Status:** Aceito
- **Data:** 2026-10-02

## Contexto

A plataforma vai crescer com ingestão de dados (OPC UA simulado), mensageria, cache e dashboard em tempo real.
Precisamos que a regra de negócio seja testável sem banco, broker ou HTTP, e que trocar uma tecnologia
de infraestrutura (ex.: PostgreSQL → TimescaleDB) não afete o domínio.

## Decisão

Separar a solução em quatro projetos, com dependências apontando sempre para dentro:

```
Api ──► Infrastructure ──► Application ──► Domain
```

- **Domain:** entidades e invariantes. Sem dependências externas.
- **Application:** casos de uso e contratos (ex.: `IEquipmentRepository`). Depende só do Domain.
- **Infrastructure:** EF Core, PostgreSQL e, no futuro, RabbitMQ e Redis. Implementa os contratos da Application.
- **Api:** endpoints HTTP, composição de dependências e tratamento de erros.

Falhas de negócio esperadas (não encontrado, duplicado) usam o *Result pattern*; violações de invariante
do domínio lançam `DomainException`, convertida em HTTP 400.

## Consequências

- ✅ Domínio e casos de uso testáveis com testes unitários rápidos (fakes em memória).
- ✅ Infraestrutura substituível.
- ⚠️ Mais projetos e mais cerimônia do que um projeto único — aceitável pelo objetivo de evolução e estudo.
- 🔁 Reavaliar no Mês 2: CQRS com handlers por caso de uso no lugar de `EquipmentService`.
