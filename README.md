# PRIMEIROS PASSOS DEVELOPER BACKEND

## **1 - CONHEÇA O PROJETO SHAREBOOK**

https://www.linkedin.com/pulse/projeto-sharebook-raffaello-damgaard/

## **2 - ENTRE NO SLACK**

https://join.slack.com/t/sharebookworkspace/shared_invite/zt-4fb3uu8m-VPrkhzdI9u3lsOlS1OkVvg

### 2.2 - LÁ NO SLACK, ENTRE NO CANAL #BACKEND

- Se apresente. Nome, cidade, profissão, e principais habilidades.
- Pergunte sobre as tarefas em aberto.
- Troque uma ideia com o time técnico. Comente como planeja solucionar. Ouça os conselhos dos devs mais experientes. Esse alinhamento é super importante pra aumentar significativamente as chances do seu PULL REQUEST ser aprovado depois.

## **3 - FAÇA PARTE DA EQUIPE NO TRELLO**

https://trello.com/invite/sharebook6/928f21ef82592b5edafde06f171d338b

</br>

### 3.2 - PEGUE UMA TAREFA NO TRELLO.

- https://trello.com/b/QTdWPYhl/sharebook
- Coloque no seu nome e mova para DOING.

</br>

## **4 - GITHUB**

- **4.1 FAÇA UM FORK DO REPOSITÓRIO:** https://github.com/SharebookBR/sharebook-backend

- **4.2 Crie uma branch baseada na branch original `develop`**

  (experimente: `git checkout develop && git checkout -b nomeDaSuaNovaBranch`)

- **4.3 Escreva seu código** ❤️

  **Nota:** Não esqueça de garantir que todos os testes unitários continuem sendo executados corretamente.

- **4.4 Crie sua PR apontando para a branch base `develop`** (conforme imagem abaixo)

![image](https://user-images.githubusercontent.com/51380783/145312556-54b67a73-e62d-48c0-9a6f-1932901f8409.png)

- **4.5 Aguarde/acompanhe o status do seu PR**

- **4.6 Compartilhe/convide um amigo para contribuir com o ShareBook**

</br>

## **---> ❤️ MISSÃO CUMPRIDA. VOCÊ AJUDOU O PROJETO. ❤️ <---**

</br>

## **6 - Rodar o app pela primeira vez?**

- Instalar o .NET SDK 8 (stable)
  https://github.com/SharebookBR/sharebook-backend/wiki/Como-rodar-o-projeto%3F

## **7 - Dicas Visual Studio Code**

Nosso projeto já está configurado para você debugar e testar pelo vs code. Só precisa instalar alguns plugins abaixo:

- [C# Extensions](https://marketplace.visualstudio.com/items?itemName=jchannon.csharpextensions)
- [Netcore Extension Pack](https://marketplace.visualstudio.com/items?itemName=doggy8088.netcore-extension-pack)
- [GitLens](https://marketplace.visualstudio.com/items?itemName=eamodio.gitlens)
- [.NET Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)

Caso prefira linha de comando, segue uma colinha...

```bash
# restaurar dependências
dotnet restore ./ShareBook/ShareBook.sln

# build
dotnet build ./ShareBook/ShareBook.Api/ShareBook.Api.csproj --verbosity minimal

# Spinning up database (needs docker)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=weWkh]6qA3jk" -p 1433:1433 --name=sql-server --hostname=sql-server -d mcr.microsoft.com/mssql/server:2022-latest

# rodar o app com hot reload
dotnet watch --project ./ShareBook/ShareBook.Api/ShareBook.Api.csproj

# rodar os testes
dotnet test ./ShareBook/ShareBook.Test.Unit/ShareBook.Test.Unit.csproj

# clean
dotnet clean ./ShareBook/ShareBook.Api/ShareBook.Api.csproj --verbosity quiet
```

## **[WIP] 8 - Como testar a aplicação usando postman**

Atenção! Este passo está em construção e ainda exige alguns passos manuais. Em breve será automatizado.

Consiste em usar uma collection do postman (v2.1) para testar os resultados das requisições. No momento a collection está pronta para usa no ambiente de dev.

1. Obter o arquivo [ShareBook API - Tests.postman_collection.json](./ShareBook%20API%20-%20Tests.postman_collection.json) do repositório
2. Usando a ferramenta postman, clique em importar e selecione o arquivo
3. Com o botão direito na collection `ShareBook API - Tests`, clique em `Run collection`
4. Na nova janela clique em executar. Após executar verifique a quantidade de erros.

# Build e Run com Docker!
```bash
# Build da imagem
docker build -t sharebook-api -f devops/Dockerfile .

# Run com environment Development
docker run -d -p 8000:8080 -e ASPNETCORE_ENVIRONMENT=Development --name sharebook-container sharebook-api
```
