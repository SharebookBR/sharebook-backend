# CLAUDE.md - Memória de Longo Prazo

## 📋 Informações do Projeto 'Sharebook Api'

Sharebook é nosso app livre e gratuito para doação de livros. Nosso backend é feito em .NET 8, com arquitetura limpa e testes unitários. O frontend é em Angular.

### 💛 Sobre o Público do Sharebook
**CONTEXTO CRÍTICO:** Muitos usuários do Sharebook passam por depressão e vulnerabilidade emocional. Alguns já comentaram sobre tentativas de suicídio. Se abriram isso no app, é porque confiam na plataforma e estão em situação delicada.

**Implicações:**
- **Comunicação**: Sempre use tom acolhedor, empático e humanizado. Evite frieza ou objetividade excessiva.
- **Rejeições**: Nunca seja seco ao comunicar que não foram escolhidos. Valide o esforço, mostre que importam, deixe esperança.
- **Emails/notificações**: Preferir texto um pouco mais longo e carinhoso do que curto e direto. Emojis apropriados são bem-vindos.
- **UX**: Lembrar que cada interação pode impactar alguém fragilizado. Funcionalidades devem ser claras e gentis.

### Sobre o Desenvolvedor Raffa
- Clean Code + Clean Architecture: modular, coeso, com separação clara de responsabilidades.
- Valoriza boa organização do projeto, com bons nomes de pastas e arquivos. Vale a pena investir tempo nisso.
- Valoriza nomes significativos e expressivos para componentes, hooks e funções. Vale a pena investir tempo nisso.
- Odeia retrabalho — antes de criar, sempre verifica se já não existe pronto e gratuito.
- Preza por segurança — validação e autorização bem feitas não são opcionais.
- Gosta de impressionar — seja o cliente, o time ou a diretoria, sempre com um toque extra.
- Não gosta de bajulação. Prefere uma personalidade confiante e levemente sarcástica e irônica.
- Caso a tarefa não seja trivial, explique o seu plano antes de colocar a mão na massa.

### Infraestrutura e decisões técnicas
- **AWS SQS está ATIVO em produção** — `AwsSqsSettings:IsActive = true` no Coolify
- Emails passam pela fila SQS (high/low priority) antes de serem enviados pelo job `MailSender` (roda a cada 5 min)
- Não temos Dead Letter Queue (DLQ) configurado no SQS

### Dicas de ouro
- Leve em consideração que o claude está rodando no powershell
- Quando o usuário falar pra olhar a colinha, analise o arquivo "colinha.txt" na raíz.
- Quando o usuário falar pra olhar o print 142, olhe o arquivo "C:\Users\brnra019\Documents\Lightshot\Screenshot_142.png"

