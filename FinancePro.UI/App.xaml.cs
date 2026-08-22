using FinancePro.Application.MasterData.Currencies;
using FinancePro.Application.MasterData.Banking;
using FinancePro.Data.Administration;
using FinancePro.Application.MasterData.Companies;
using FinancePro.Application.MasterData.Partners;
using FinancePro.Data.MasterData;
using FinancePro.Data.Treasury;
using FinancePro.Data.Platform;
using FinancePro.Platform.Settings;
using FinancePro.Platform.Workflow;
using FinancePro.Data.Workflow;
using FinancePro.Data.Reporting;
using FinancePro.Platform.Reporting;
using FinancePro.Platform.Documents;
using FinancePro.Platform.Administration;
using FinancePro.Platform.Accounting;
using FinancePro.Platform.Consolidation;
using FinancePro.Data.Accounting;
using FinancePro.Data.Consolidation;
using FinancePro.Data.AdministrationMasterData;
using FinancePro.Application.Treasury;
using FinancePro.Data.Revenue;
using FinancePro.Application.Revenue;
using FinancePro.Data.Expenses;
using FinancePro.Data.Budget;
using FinancePro.Data.Purchasing;
using FinancePro.Application.Expenses;
using FinancePro.Application.Budget;
using FinancePro.Application.Purchasing;
using FinancePro.Application.Assets;
using FinancePro.Data.Assets;
using FinancePro.Application.Administration.Users;
using FinancePro.Application.Administration.Profiles;
using FinancePro.Application.Administration.Permissions;
using System.Windows;
using FinancePro.Application.Common.Interfaces;
using FinancePro.Data.Context;
using FinancePro.Services.Implementations;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;
using FinancePro.UI.ViewModels;
using FinancePro.UI.Views;
using FinancePro.UI.Services.Foundation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancePro.UI;

/// <summary>
/// Ponto de entrada da aplicação e composition root: monta o contentor de
/// Injeção de Dependências e decide o primeiro ecrã — se ainda não existir
/// nenhum utilizador, mostra o assistente de Configuração Inicial (cria a
/// primeira Empresa + Administrador); caso contrário, vai direto ao Login.
/// </summary>
public partial class App : System.Windows.Application
{
    private IServiceProvider? _serviceProvider;

    public static IServiceProvider Services =>
        ((App)System.Windows.Application.Current)._serviceProvider
        ?? throw new InvalidOperationException("O contentor de DI ainda não foi inicializado.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Evita que a app feche antes de mostrarmos a primeira janela,
        // já que a decisão (Setup vs Login) depende de uma consulta assíncrona.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddScoped<AdministrationGateway>();
        services.AddScoped<IUserAdministrationGateway>(sp => sp.GetRequiredService<AdministrationGateway>());
        services.AddScoped<IProfileAdministrationGateway>(sp => sp.GetRequiredService<AdministrationGateway>());
        services.AddScoped<IPermissionAdministrationGateway>(sp => sp.GetRequiredService<AdministrationGateway>());
        services.AddScoped<UserAdministrationService>();
        services.AddScoped<ProfileAdministrationService>();
        services.AddScoped<PermissionAdministrationService>();
        services.AddScoped<ICompanyGateway, CompanyGateway>();
        services.AddScoped<CompanyApplicationService>();
        services.AddScoped<IBusinessPartnerGateway, BusinessPartnerGateway>();
        services.AddScoped<BusinessPartnerApplicationService>();
        services.AddScoped<IBankingMasterDataGateway, BankingMasterDataGateway>();
        services.AddScoped<BankingMasterDataService>();
        services.AddScoped<ICurrencyMasterDataGateway, CurrencyMasterDataGateway>();
        services.AddScoped<CurrencyMasterDataService>();
        services.AddScoped<TreasuryGateway>();
        services.AddScoped<ITreasuryGateway>(sp => sp.GetRequiredService<TreasuryGateway>());
        services.AddScoped<TreasuryApplicationService>();
        services.AddScoped<RevenueGateway>();
        services.AddScoped<IRevenueGateway>(sp => sp.GetRequiredService<RevenueGateway>());
        services.AddScoped<RevenueApplicationService>();
        services.AddScoped<ExpenseGateway>();
        services.AddScoped<IExpenseGateway>(sp => sp.GetRequiredService<ExpenseGateway>());
        services.AddScoped<ExpenseApplicationService>();

        services.AddScoped<IBudgetGateway, BudgetGateway>();
        services.AddScoped<BudgetApplicationService>();
        services.AddScoped<IPurchasingGateway, PurchasingGateway>();
        services.AddScoped<PurchasingApplicationService>();
        services.AddScoped<IAssetGateway, AssetGateway>();
        services.AddScoped<AssetApplicationService>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<ISettingsStore, SqlSettingsStore>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IWorkflowStore, SqlWorkflowStore>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IReportDataProvider, EfReportDataProvider>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IAdministrationMasterDataStore, SqlAdministrationMasterDataStore>();
        services.AddScoped<IAdministrationMasterDataService, AdministrationMasterDataService>();
        services.AddScoped<IAccountingStore, SqlAccountingStore>();
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<IAutomaticAccountingRulesService, AutomaticAccountingRulesService>();
        services.AddScoped<IAnalyticAccountingService, SqlAnalyticAccountingService>();
        services.AddScoped<FinancePro.Platform.Investments.IInvestmentService, FinancePro.Data.Investments.SqlInvestmentService>();
        services.AddScoped<FinancePro.Platform.HumanResources.IHumanResourcesService, FinancePro.Data.HumanResources.SqlHumanResourcesService>();
        services.AddScoped<IConsolidationStore, SqlConsolidationStore>();
        services.AddScoped<IConsolidationService, ConsolidationService>();

        var connectionString = configuration.GetConnectionString("FinanceProDb")
            ?? throw new InvalidOperationException("A connection string 'FinanceProDb' não foi configurada.");

        services.AddDbContext<FinanceProDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(60);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            }));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IExercicioFinanceiroService, ExercicioFinanceiroService>();
        services.AddScoped<IMoedaService, MoedaService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IPermissaoService, PermissaoService>();
        services.AddScoped<IUtilizadorService, UtilizadorService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITesourariaService, TesourariaService>();
        services.AddScoped<ICaixaService, CaixaService>();
        services.AddScoped<IBancoService, BancoService>();
        services.AddScoped<IConfiguracoesService, ConfiguracoesService>();
        services.AddScoped<IReceitasService, ReceitasService>();
        services.AddScoped<IDespesasService, DespesasService>();
        services.AddScoped<IOrcamentoService, OrcamentoService>();
        services.AddScoped<ICompraService, CompraService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IBemService, BemService>();
        services.AddTransient<LoginViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        bool existeUtilizador;
        try
        {
            existeUtilizador = await VerificarConfiguracaoInicialComRecuperacaoAsync();
        }
        catch (OperationCanceledException)
        {
            Shutdown();
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível iniciar o FinancePro.\n\n{ObterMensagemAmigavel(ex)}",
                "FinancePro — Erro de ligação",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        ShutdownMode = ShutdownMode.OnMainWindowClose;

        if (existeUtilizador)
        {
            MostrarLogin();
        }
        else
        {
            MostrarConfiguracaoInicial();
        }
    }


    private async Task<bool> VerificarConfiguracaoInicialComRecuperacaoAsync()
    {
        while (true)
        {
            try
            {
                // Elimina ligações antigas ou inválidas do pool antes da primeira consulta.
                SqlConnection.ClearAllPools();

                using var scope = Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FinanceProDbContext>();

                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(75));
                if (!await dbContext.Database.CanConnectAsync(timeout.Token))
                {
                    throw new InvalidOperationException(
                        "O SQL Server respondeu, mas a base de dados FinanceProDb não está acessível.");
                }

                var configService = scope.ServiceProvider.GetRequiredService<IConfiguracoesService>();
                return await configService.ExisteAlgumUtilizadorAsync(timeout.Token);
            }
            catch (Exception ex) when (ex is SqlException or TimeoutException or InvalidOperationException)
            {
                var resposta = MessageBox.Show(
                    $"Não foi possível ligar à base de dados.\n\n{ObterMensagemAmigavel(ex)}\n\n" +
                    "Confirme se o serviço SQL Server (SQLEXPRESS) está em execução.\n\n" +
                    "Deseja tentar novamente?",
                    "FinancePro — Base de dados indisponível",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resposta != MessageBoxResult.Yes)
                {
                    throw new OperationCanceledException();
                }

                await Task.Delay(1500);
            }
        }
    }

    private static string ObterMensagemAmigavel(Exception ex)
    {
        var erro = ex;
        while (erro.InnerException is not null)
        {
            erro = erro.InnerException;
        }

        return erro switch
        {
            SqlException sqlEx when sqlEx.Number == -2 =>
                "A ligação ao SQL Server excedeu o tempo limite.",
            SqlException sqlEx =>
                $"Erro SQL {sqlEx.Number}: {sqlEx.Message}",
            OperationCanceledException =>
                "A tentativa de ligação excedeu o tempo limite configurado.",
            _ => erro.Message
        };
    }

    private void MostrarConfiguracaoInicial()
    {
        var scope = Services.CreateScope();
        var configService = scope.ServiceProvider.GetRequiredService<IConfiguracoesService>();
        var setupViewModel = new SetupViewModel(configService);
        var setupView = new SetupView(setupViewModel);

        setupView.ConfiguracaoConcluida += () =>
        {
            setupView.Close();
            scope.Dispose();
            MostrarLogin();
        };

        MainWindow = setupView;
        setupView.Show();
    }

    private void MostrarLogin()
    {
        var scope = Services.CreateScope();
        var loginViewModel = scope.ServiceProvider.GetRequiredService<LoginViewModel>();
        var loginView = new LoginView(loginViewModel);

        loginView.LoginBemSucedido += resultado =>
        {
            SessaoAtual.Definir(resultado.UtilizadorId, resultado.NomeCompleto, resultado.PerfilNome, resultado.EmpresaId, resultado.Permissoes, resultado.FotoPerfil);

            var mainWindow = new MainWindow(resultado);
            MainWindow = mainWindow;
            mainWindow.Show();
            loginView.Close();
            scope.Dispose();
        };

        MainWindow = loginView;
        loginView.Show();
    }
}
