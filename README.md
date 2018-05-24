### Projeto SHAREBOOK backend.
#### Tecnologias usadas no projeto

- Visual Studio 2017.
- ASP.NET CORE 2.0.
- C#.
- SQL SERVER.
- SWAGGER.

#### Como rodar o projeto ?

- Não se esquecer de mudar a connection string para a sua base de dados no arquivo appsettings.json que se encontra na camada da API do projeto.

- Sempre que criar as classes de model e map  rodar o comando:
"Add-Migration nome-do-migration" na camada BASE do projeto  para criar o novo migration para o  banco de dados.

#### Como rodar os testes ?

- É necessário instalar via Nuget dois pacotes:

1. Adicionar o pacote Microsoft.TestPlatform.TestHost
2. Adicionar o pacote Microsoft.NET.Test.Sdk

Instalar também o pacote do xunit se não tiver baixado ainda.

1. xunit (2.3.0-beta1-build3642)
2. xunit.runner.visualstudio

Depois de instalado os pacotes, faça um build da solução e depois acesse Test Explorer acessando Test < Windows < Test Explorer no menu superior do visual studio.
