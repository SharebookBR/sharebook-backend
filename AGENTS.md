# AGENTS.md — Guia operacional para sessões futuras

Este arquivo cobre todo o repositório.

## Objetivo
Ajudar o agente a ganhar tração rápida no projeto ShareBook (backend .NET), com build/test/run previsíveis e mínima fricção.

## Stack e contexto rápido
- Solução principal: `./ShareBook/ShareBook.sln`
- API: `./ShareBook/ShareBook.Api/ShareBook.Api.csproj`
- Testes unitários: `./ShareBook/ShareBook.Test.Unit/ShareBook.Test.Unit.csproj`
- Target framework atual: `net10.0`

## Pré-requisito principal (Codex Web): instalar .NET 10 se não existir
Se `dotnet --info` falhar com `command not found`, o agente DEVE instalar o SDK antes de qualquer outra ação.

### Ubuntu 24.04 (padrão deste ambiente)
```bash
apt-get update -y
apt-get install -y wget gpg apt-transport-https ca-certificates
wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
dpkg -i /tmp/packages-microsoft-prod.deb
apt-get update -y
apt-get install -y dotnet-sdk-10.0
```

### Validação obrigatória pós-instalação
```bash
dotnet --info
```
Esperado: SDK `10.x` disponível.

## Preflight (sempre rodar no começo)
1. Validar ferramentas:
   - `dotnet --info`
   - `git --version`
2. Confirmar diretório:
   - `pwd` (esperado: raiz deste repositório)
3. Restaurar dependências:
   - `dotnet restore ./ShareBook/ShareBook.sln`

## Build e testes (fluxo padrão)
- Build API:
  - `dotnet build ./ShareBook/ShareBook.Api/ShareBook.Api.csproj --verbosity minimal`
- Testes unitários:
  - `dotnet test ./ShareBook/ShareBook.Test.Unit/ShareBook.Test.Unit.csproj --verbosity minimal`

## Run local recomendado (sem dependência de SQL Server)
Por padrão o `appsettings.json` vem com `DatabaseProvider=sqlserver` e `SecretJwtKey` vazio.
Para subir local sem banco externo, usar variáveis de ambiente:

```bash
ASPNETCORE_URLS=http://127.0.0.1:5099 \
DatabaseProvider=sqlite \
TokenConfigurations__SecretJwtKey='dev-secret-key-sharebook-123456789' \
dotnet run --no-launch-profile --project ./ShareBook/ShareBook.Api/ShareBook.Api.csproj
```

Validação rápida:
- `curl -sf http://127.0.0.1:5099/health` (esperado: `Healthy`)

## Diagnóstico rápido de falhas comuns
- Erro `IDX10703 ... key length is zero`:
  - Causa: `TokenConfigurations__SecretJwtKey` não definido.
- Falha de conexão em SQL Server:
  - Defina `DatabaseProvider=sqlite` para desenvolvimento local rápido.
- Warnings de vulnerabilidade em restore/test:
  - Registrar no resumo da sessão; não bloquear fluxo sem decisão explícita do usuário.

## Regras de execução para mudanças de código
- Sempre rodar pelo menos:
  - restore + build do projeto impactado
  - testes unitários se houver impacto de domínio/serviço/API
- Não adicionar segredos reais em arquivos versionados.
- Preferir variáveis de ambiente para segredos e configuração local.

## Entrega (higiene de PR)
- Commit único e objetivo quando possível.
- Mensagem de commit clara, em inglês técnico curto.
- No resumo final: listar comandos executados e status (pass/warn/fail).

## MCP/tools
- Se ferramentas MCP de browser estiverem disponíveis, usar para evidências visuais em mudanças de frontend.
- Para este backend, priorizar validações de CLI (`dotnet restore/build/test/run + curl /health`).
