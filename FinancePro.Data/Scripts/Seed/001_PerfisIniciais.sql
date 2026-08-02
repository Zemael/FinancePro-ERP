-- FinancePro ERP — dados iniciais (perfis de acesso padrão)
-- O utilizador Administrador será criado na Etapa 3, quando a lógica de
-- hash de password (Services/Auth) estiver definida.

INSERT INTO Perfis (Nome, Descricao) VALUES
    ('Administrador', 'Acesso total ao sistema'),
    ('Gestor',         'Acesso aos módulos financeiros e relatórios'),
    ('Operador',       'Acesso limitado a lançamentos do dia a dia');
