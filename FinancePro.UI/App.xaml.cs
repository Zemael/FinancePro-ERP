using FinancePro.Application.Platform;
using FinancePro.Data.Platform;
using FinancePro.Application.Treasury;
using FinancePro.Data.Treasury;
using FinancePro.Application.Payables;
using FinancePro.Data.Payables;
using FinancePro.Application.Receivables;
using FinancePro.Data.Receivables;
using FinancePro.Data.Accounting;
using FinancePro.Application.Accounting;
using FinancePro.Application.Customers;
using FinancePro.Application.Suppliers;
using FinancePro.Data.Customers;
using FinancePro.Data.Suppliers;
using FinancePro.Data.Administration;
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
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        var connectionString = configuration.GetConnectionString("FinanceProDb");
        services.AddDbContext<FinanceProDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IExercicioFinanceiroService, ExercicioFinanceiroService>();
        services.AddScoped<IMoedaService, MoedaService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IPermissaoService, PermissaoService>();
        services.AddScoped<IUtilizadorService, UtilizadorService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITesourariaService, TesourariaService>();
        services.AddScoped<IAdvancedTreasuryGateway, AdvancedTreasuryGateway>();
        services.AddScoped<AdvancedTreasuryApplicationService>();
        services.AddScoped<ICaixaService, CaixaService>();
        services.AddScoped<IBancoService, BancoService>();
        services.AddScoped<IConfiguracoesService, ConfiguracoesService>();
        services.AddScoped<IReceitasService, ReceitasService>();
        services.AddScoped<IReceivablesGateway, ReceivablesGateway>();
        services.AddScoped<ReceivablesApplicationService>();
        services.AddScoped<IDespesasService, DespesasService>();
        services.AddScoped<IPayablesGateway, PayablesGateway>();
        services.AddScoped<PayablesApplicationService>();
        services.AddScoped<IOrcamentoService, OrcamentoService>();
        services.AddScoped<ICompraService, CompraService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IBemService, BemService>();
        services.AddScoped<ICustomerGateway, CustomerGateway>();
        services.AddScoped<CustomerApplicationService>();
        services.AddScoped<ISupplierGateway, SupplierGateway>();
        services.AddScoped<SupplierApplicationService>();
        services.AddScoped<IAccountingGateway, AccountingGateway>();
        services.AddScoped<AccountingApplicationService>();
        services.AddScoped<INumberingGateway, NumberingGateway>();
        services.AddScoped<INumberingService, NumberingService>();
        services.AddScoped<IAuditTrailGateway, AuditTrailGateway>();
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IFinancialEngine, FinancialEngineService>();
        services.AddTransient<LoginViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        bool existeUtilizador;
        using (var scopeVerificacao = Services.CreateScope())
        {
            var configService = scopeVerificacao.ServiceProvider.GetRequiredService<IConfiguracoesService>();
            existeUtilizador = await configService.ExisteAlgumUtilizadorAsync();
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
            SessaoAtual.Definir(resultado.UtilizadorId, resultado.NomeCompleto, resultado.PerfilNome, resultado.EmpresaId, resultado.Permissoes);

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
