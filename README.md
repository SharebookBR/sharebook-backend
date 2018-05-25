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

Estamos usando a biblioteca xUnit[https://github.com/xunit/xunit]. Todos os pacotes foram instalados através do NuGet. Caso necessário, efetue o comando Restore para baixar os pacotes para a sua máquina local.

Para executar os testes, faça um build da solução e depois acesse o Test Explorer acessando Test < Windows < Test Explorer no menu superior do Visual Studio.

Lista de bibliotecas instaladas para execução dos testes:
- Microsoft.NET.Test.Sdk
- Microsoft.AspNetCore.App [Microsoft.NET.Test.Sdk dependency]
- xunit
- xunit.runner.visualstudio
