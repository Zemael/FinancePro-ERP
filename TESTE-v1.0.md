# FinancePro ERP v1.0 — Permissões por Perfil

1. Atualize a base de dados com a migration EF Core ou execute `FinancePro.Data/Scripts/Schema/010_PermissoesPerfis.sql`.
2. Entre com um utilizador do perfil `Administrador`.
3. Abra **Permissões** no menu lateral.
4. Selecione um perfil não administrador, marque as ações permitidas e clique em **Guardar permissões**.
5. Associe um utilizador ao perfil configurado e termine a sessão.
6. Entre com esse utilizador: os módulos sem `Consultar` devem ficar ocultos.
7. Alterações de permissões produzem efeito no próximo login.

Observação: o perfil `Administrador` possui acesso total por definição.
