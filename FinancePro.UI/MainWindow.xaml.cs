using FinancePro.Application.MasterData.Currencies;
using FinancePro.Application.MasterData.Banking;
using FinancePro.Application.Budget;
using FinancePro.Application.Expenses;
using FinancePro.Application.Revenue;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FinancePro.Core.DTOs;
using FinancePro.Application.Administration.Users;
using FinancePro.Application.Administration.Profiles;
using FinancePro.Application.Administration.Permissions;
using FinancePro.Application.Treasury;
using FinancePro.Application.Purchasing;
using FinancePro.Application.Assets;
using FinancePro.Application.MasterData.Companies;
using FinancePro.Application.MasterData.Partners;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;
using FinancePro.UI.ViewModels;
using FinancePro.UI.Views;
using FinancePro.Platform.Settings;
using FinancePro.Platform.Workflow;
using FinancePro.Platform.Reporting;
using Microsoft.Extensions.DependencyInjection;

namespace FinancePro.UI;

/// <summary>
/// Janela principal (shell) pós-login: barra superior com o utilizador
/// ligado, navegação lateral e uma área de conteúdo que troca entre os
/// módulos. Só Dashboard e Tesouraria estão ativos (Etapa 5); os
/// restantes módulos aparecem como "brevemente" até serem desenvolvidos.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Brush ItemAtivoFundo = new SolidColorBrush(Color.FromRgb(0x15, 0x60, 0x82));
    private static readonly Brush ItemInativoFundo = Brushes.Transparent;

    private readonly LoginResultDto _utilizador;
    private IServiceScope? _scopeAtual;
    private Button? _itemNavAtivo;

    public MainWindow(LoginResultDto utilizador)
    {
        InitializeComponent();
        _utilizador = utilizador;
        UsuarioTexto.Text = $"{utilizador.NomeCompleto} · {utilizador.PerfilNome}";

        AplicarPermissoesMenu();
        MostrarDashboard();

        Closed += (_, _) => _scopeAtual?.Dispose();
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e) => MostrarDashboard();

    private void Empresas_Click(object sender, RoutedEventArgs e) => MostrarEmpresas();

    private void Parceiros_Click(object sender, RoutedEventArgs e) => MostrarParceiros();

    private void Exercicios_Click(object sender, RoutedEventArgs e) => MostrarExercicios();

    private void Moedas_Click(object sender, RoutedEventArgs e) => MostrarMoedas();

    private void Utilizadores_Click(object sender, RoutedEventArgs e) => MostrarUtilizadores();

    private void Perfis_Click(object sender, RoutedEventArgs e) => MostrarPerfis();

    private void Permissoes_Click(object sender, RoutedEventArgs e) => MostrarPermissoes();

    private void Tesouraria_Click(object sender, RoutedEventArgs e) => MostrarTesouraria();

    private void Caixa_Click(object sender, RoutedEventArgs e) => MostrarCaixa();

    private void Bancos_Click(object sender, RoutedEventArgs e) => MostrarBancos();

    private void Receitas_Click(object sender, RoutedEventArgs e) => MostrarReceitas();

    private void Orcamento_Click(object sender, RoutedEventArgs e) => MostrarOrcamento();

    private void Despesas_Click(object sender, RoutedEventArgs e) => MostrarDespesas();

    private void Compras_Click(object sender, RoutedEventArgs e) => MostrarCompras();

    private void Bens_Click(object sender, RoutedEventArgs e) => MostrarBens();

    private void Configuracoes_Click(object sender, RoutedEventArgs e) => MostrarConfiguracoes();

    private void Workflow_Click(object sender, RoutedEventArgs e) => MostrarWorkflow();

    private void Relatorios_Click(object sender, RoutedEventArgs e) => MostrarRelatorios();

    private void Tema_Click(object sender, RoutedEventArgs e) => GestorTema.Alternar();

    /// <summary>Mantém o item do módulo atual sempre destacado a teal na
    /// barra lateral — antes só havia destaque temporário ao passar o rato.</summary>
    private void DestacarItemAtivo(Button item)
    {
        if (_itemNavAtivo is not null)
        {
            _itemNavAtivo.Background = ItemInativoFundo;
        }

        item.Background = ItemAtivoFundo;
        _itemNavAtivo = item;
    }

    private void MostrarDashboard()
    {
        DestacarItemAtivo(BtnDashboard);
        TrocarScope();
        var dashboardService = _scopeAtual!.ServiceProvider.GetRequiredService<IDashboardService>();
        var viewModel = new DashboardViewModel(dashboardService, _utilizador.EmpresaId, _utilizador.NomeCompleto);
        viewModel.NavegarPedido += modulo =>
        {
            switch (modulo)
            {
                case "Tesouraria": MostrarTesouraria(); break;
                case "Receitas": MostrarReceitas(); break;
                case "Caixa": MostrarCaixa(); break;
                case "Configuracoes": MostrarConfiguracoes(); break;
                case "Utilizadores": MostrarUtilizadores(); break;
                case "Despesas": MostrarDespesas(); break;
                case "Orcamento": MostrarOrcamento(); break;
            }
        };
        ConteudoHost.Content = new DashboardView { DataContext = viewModel };
    }

    private void MostrarEmpresas()
    {
        DestacarItemAtivo(BtnEmpresas);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<CompanyApplicationService>();
        ConteudoHost.Content = new EmpresasView { DataContext = new EmpresasViewModel(service) };
    }


    private void MostrarParceiros()
    {
        DestacarItemAtivo(BtnParceiros);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<BusinessPartnerApplicationService>();
        ConteudoHost.Content = new ParceirosView { DataContext = new ParceirosViewModel(service, _utilizador.EmpresaId) };
    }

    private void MostrarExercicios()
    {
        DestacarItemAtivo(BtnExercicios);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IExercicioFinanceiroService>();
        var empresaService = _scopeAtual.ServiceProvider.GetRequiredService<IEmpresaService>();
        ConteudoHost.Content = new ExerciciosFinanceirosView { DataContext = new ExerciciosFinanceirosViewModel(service, empresaService) };
    }

    private void MostrarMoedas()
    {
        DestacarItemAtivo(BtnMoedas);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<CurrencyMasterDataService>();
        ConteudoHost.Content = new MoedasView { DataContext = new MoedasViewModel(service) };
    }

    private void MostrarUtilizadores()
    {
        DestacarItemAtivo(BtnUtilizadores);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<UserAdministrationService>();
        var perfilService = _scopeAtual.ServiceProvider.GetRequiredService<IPerfilService>();
        var empresaService = _scopeAtual.ServiceProvider.GetRequiredService<IEmpresaService>();
        ConteudoHost.Content = new UtilizadoresView { DataContext = new UtilizadoresViewModel(service, perfilService, empresaService) };
    }

    private void MostrarPerfis()
    {
        DestacarItemAtivo(BtnPerfis);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<ProfileAdministrationService>();
        ConteudoHost.Content = new PerfisView { DataContext = new PerfisViewModel(service) };
    }

    private void MostrarPermissoes()
    {
        if (!SessaoAtual.TemPermissao("Permissoes")) return;
        DestacarItemAtivo(BtnPermissoes);
        TrocarScope();
        var permissaoService = _scopeAtual!.ServiceProvider.GetRequiredService<PermissionAdministrationService>();
        var perfilService = _scopeAtual.ServiceProvider.GetRequiredService<IPerfilService>();
        ConteudoHost.Content = new PermissoesView { DataContext = new PermissoesViewModel(permissaoService, perfilService) };
    }

    private void AplicarPermissoesMenu()
    {
        BtnDashboard.Visibility = Visibilidade("Dashboard");
        BtnEmpresas.Visibility = Visibilidade("Empresas");
        BtnParceiros.Visibility = Visibility.Visible;
        BtnExercicios.Visibility = Visibilidade("Exercicios");
        BtnMoedas.Visibility = Visibilidade("Moedas");
        BtnUtilizadores.Visibility = Visibilidade("Utilizadores");
        BtnPerfis.Visibility = Visibilidade("Perfis");
        BtnPermissoes.Visibility = Visibilidade("Permissoes");
        BtnCaixa.Visibility = Visibilidade("Caixa");
        BtnBancos.Visibility = Visibilidade("Bancos");
        BtnOrcamento.Visibility = Visibilidade("Orcamento");
        BtnTesouraria.Visibility = Visibilidade("Tesouraria");
        BtnReceitas.Visibility = Visibilidade("Receitas");
        BtnDespesas.Visibility = Visibilidade("Despesas");
        BtnCompras.Visibility = Visibilidade("Compras");
        BtnBens.Visibility = Visibilidade("Patrimonio");
        BtnConfiguracoes.Visibility = Visibilidade("Configuracoes");
        BtnWorkflow.Visibility = Visibility.Visible;
        BtnRelatorios.Visibility = Visibility.Visible;
    }

    private static Visibility Visibilidade(string modulo) =>
        SessaoAtual.TemPermissao(modulo) ? Visibility.Visible : Visibility.Collapsed;

    private void MostrarTesouraria()
    {
        DestacarItemAtivo(BtnTesouraria);
        TrocarScope();
        var tesourariaService = _scopeAtual!.ServiceProvider.GetRequiredService<TreasuryApplicationService>();
        var viewModel = new TesourariaViewModel(tesourariaService, _utilizador.EmpresaId);
        ConteudoHost.Content = new TesourariaView { DataContext = viewModel };
    }

    private void MostrarCaixa()
    {
        DestacarItemAtivo(BtnCaixa);
        TrocarScope();
        var caixaService = _scopeAtual!.ServiceProvider.GetRequiredService<ICaixaService>();
        var viewModel = new CaixaViewModel(caixaService, _utilizador.EmpresaId);
        ConteudoHost.Content = new CaixaView { DataContext = viewModel };
    }

    private void MostrarBancos()
    {
        DestacarItemAtivo(BtnBancos);
        TrocarScope();
        var bancoService = _scopeAtual!.ServiceProvider.GetRequiredService<BankingMasterDataService>();
        var viewModel = new BancosViewModel(bancoService, _utilizador.EmpresaId);
        ConteudoHost.Content = new BancosView { DataContext = viewModel };
    }

    private void MostrarReceitas()
    {
        DestacarItemAtivo(BtnReceitas);
        TrocarScope();
        var receitasService = _scopeAtual!.ServiceProvider.GetRequiredService<RevenueApplicationService>();
        var viewModel = new ReceitasViewModel(receitasService, _utilizador.EmpresaId);
        ConteudoHost.Content = new ReceitasView { DataContext = viewModel };
    }

    private void MostrarOrcamento()
    {
        DestacarItemAtivo(BtnOrcamento);
        TrocarScope();
        var orcamentoService = _scopeAtual!.ServiceProvider.GetRequiredService<BudgetApplicationService>();
        var viewModel = new OrcamentoViewModel(orcamentoService, _utilizador.EmpresaId);
        ConteudoHost.Content = new OrcamentoView { DataContext = viewModel };
    }

    private void MostrarDespesas()
    {
        DestacarItemAtivo(BtnDespesas);
        TrocarScope();
        var despesasService = _scopeAtual!.ServiceProvider.GetRequiredService<ExpenseApplicationService>();
        var viewModel = new DespesasViewModel(despesasService, _utilizador.EmpresaId);
        ConteudoHost.Content = new DespesasView { DataContext = viewModel };
    }

    private void MostrarCompras()
    {
        DestacarItemAtivo(BtnCompras);
        TrocarScope();
        var compraService = _scopeAtual!.ServiceProvider.GetRequiredService<PurchasingApplicationService>();
        var viewModel = new CompraViewModel(compraService, _utilizador.EmpresaId);
        ConteudoHost.Content = new CompraView { DataContext = viewModel };
    }

    private void MostrarBens()
    {
        DestacarItemAtivo(BtnBens);
        TrocarScope();
        var bemService = _scopeAtual!.ServiceProvider.GetRequiredService<AssetApplicationService>();
        var auditoriaService = _scopeAtual.ServiceProvider.GetRequiredService<IAuditoriaService>();
        var viewModel = new BemViewModel(bemService, auditoriaService, _utilizador.EmpresaId);
        ConteudoHost.Content = new BemView { DataContext = viewModel };
    }


    private void MostrarWorkflow()
    {
        DestacarItemAtivo(BtnWorkflow);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IWorkflowService>();
        var viewModel = new WorkflowTasksViewModel(service, _utilizador.EmpresaId, _utilizador.UtilizadorId, _utilizador.NomeCompleto);
        ConteudoHost.Content = new WorkflowTasksView { DataContext = viewModel };
    }

    private void MostrarRelatorios()
    {
        DestacarItemAtivo(BtnRelatorios);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IReportingService>();
        ConteudoHost.Content = new ReportingView { DataContext = new ReportingViewModel(service, _utilizador.EmpresaId) };
    }

    private void MostrarConfiguracoes()
    {
        DestacarItemAtivo(BtnConfiguracoes);
        TrocarScope();
        var configService = _scopeAtual!.ServiceProvider.GetRequiredService<IConfiguracoesService>();
        var settingsService = _scopeAtual.ServiceProvider.GetRequiredService<ISettingsService>();
        var viewModel = new ConfiguracoesViewModel(configService, settingsService, _utilizador.EmpresaId);
        ConteudoHost.Content = new ConfiguracoesView { DataContext = viewModel };
    }

    /// <summary>Cada módulo recebe o seu próprio scope de DI (e portanto o seu
    /// próprio DbContext), fechado assim que se troca de módulo.</summary>
    private void TrocarScope()
    {
        _scopeAtual?.Dispose();
        _scopeAtual = App.Services.CreateScope();
    }
}

