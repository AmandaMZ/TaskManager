# TaskManager

Muito conhecido, o gerenciador de tarefas do Sistema Operacional Windows é bastante utilizado para iniciar, encerrar ou gerenciar um processo do computador. Com base nisso, este projeto simula um gerenciador de tarefas capaz de listar todos os processos que estão rodando no computador.

• Pré-requisitos: Git, Visual Studio 2022 e .NET.

Para fazer a execução do projeto, primeiramente, é necessário ter o Visual Studio (2022) instalado, de modo que esteja configurado para permitir APS.NET e desenvolvimento Web, e desenvolvimento para desktop com .NET. Além disso, todas os detalhes opcionais devem ser marcados na instalação.

Já tendo o .NET SDK instalado, em sua versão 6.0.301, basta que, para acessar o projeto, seja feito o clone do repositório do TaskManager, que se localiza no sistema de versionamento GitHub. Para tal, é necessário ter o Git instalado e, então, seguir os seguintes passos:

•	Abrir o local desejado e clonar este repositório por meio da inserção do comando abaixo, que é executado no Git Bash:

git clone https://github.com/AmandaMZ/TaskManager.git 

•	Abrir o Visual Studio como administrador, em virtude da chamada de métodos que precisam de autorização para serem acessados. Na própria IDE, basta selecionar Abrir um projeto ou uma solução, selecionar a pasta do Task Manager e abrir o arquivo Task.csproj.

•	Com o projeto já importado, selecionar a aba Ferramentas > Gerenciador de Pacotes do NuGet > Gerenciar Pacotes do NuGet para a Solução...

•	Caso não hajam dependências instaladas, é necessário instalar os NuGets System.Diagnostics.PerformanceCounter e System.Management. Após instalados, o projeto precisa ser compilado e executado. Para isso, deve-se apertar a tecla F5 do teclado.

Observação: Caso apareçam mensagens na tela referente ao Certificado SSL do ASP.NET Core, selecione sempre a opção Sim.
