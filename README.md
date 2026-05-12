# 🚜 Controle de Pátio API

Uma Minimal API RESTful desenvolvida em **.NET 8** para o gerenciamento dinâmico de veículos e vagas em um pátio. 

Este projeto foi construído com foco na separação de responsabilidades (Normalização), divorciando as entidades de Veículos e Vagas para permitir uma alocação flexível. O sistema controla onde cada veículo está estacionado e mantém um registro completo do histórico de movimentações.

## 🚀 Tecnologias Utilizadas
* **C# & .NET 8** (Minimal APIs)
* **Entity Framework Core** (ORM)
* **SQLite** (Banco de dados leve e integrado)
* **Swagger/OpenAPI** (Documentação e interface de testes)

## ⚙️ Funcionalidades Principais
* **Gestão do Pátio (Vagas):** Criação, listagem e demolição de vagas físicas.
* **Gestão da Frota (Veículos):** Cadastro de novos tratores/veículos e remoção do sistema.
* **Controle de Movimentações:** 
  * **Saída:** Registra a saída do veículo com o nome do condutor e libera a vaga instantaneamente.
  * **Retorno:** Registra a volta do veículo e tranca a nova vaga escolhida.
* **Histórico:** Consulta completa da "fita da catraca", exibindo todas as viagens da mais recente para a mais antiga.

## 🛠️ Como executar o projeto localmente

Por padrão e segurança, o arquivo do banco de dados (`patio.db`) é ignorado pelo Git. Para rodar o projeto na sua máquina, siga os passos para recriá-lo:

1. Clone este repositório para a sua máquina local.
2. Abra a solução no **Visual Studio 2022**.
3. No menu superior, vá em **Ferramentas** > **Gerenciador de Pacotes do NuGet** > **Console do Gerenciador de Pacotes**.
4. No console que abrir na parte inferior, digite o comando abaixo e pressione Enter para gerar o banco de dados com base nas *Migrations* existentes:
   ```powershell
   Update-Database
6. Pressione F5 para iniciar a aplicação.
7. O navegador abrirá automaticamente na interface do Swagger, onde todos os endpoints podem ser testados de forma interativa.
