# FinancePro ERP v0.9 — Administração

## Incluído
- Cadastro de Utilizadores.
- Cadastro de Perfis de Acesso.
- Associação de utilizador a Empresa e Perfil.
- Criação e alteração segura de palavra-passe com BCrypt.
- Pesquisa, edição, ativação e desativação.
- Nova interface visual integrada ao FP Design System.
- Novas opções no menu lateral.

## Teste
1. Abra `FinancePro.sln` no Visual Studio.
2. Restaure os pacotes NuGet.
3. Compile a solução.
4. Entre no sistema com o administrador existente.
5. Abra `Perfis de Acesso` e crie/edite um perfil.
6. Abra `Utilizadores` e crie um utilizador, escolhendo Empresa e Perfil.
7. Termine a sessão e teste o novo utilizador.

## Observação
As tabelas `Perfis` e `Utilizadores` já fazem parte do esquema original do projeto; esta versão não exige nova migration quando essas tabelas já existem.

FinancePro ERP — Copyright © 2026 Zemael Jacinto da Silva. Todos os direitos reservados.
