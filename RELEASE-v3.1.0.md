# FinancePro v3.1.0 — Master Data: Empresas

## Incluído

- camada Application para o cadastro de Empresas;
- validação centralizada de nome, email e moeda;
- normalização de dados antes da gravação;
- gateway desacoplado da interface WPF;
- `EmpresasViewModel` integrado ao padrão `Result`;
- tratamento uniforme de erros e estados de carregamento;
- três testes unitários para o caso de uso de Empresas;
- nenhuma alteração no esquema da base de dados.

## Validação local

```powershell
dotnet clean .\FinancePro.sln
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
