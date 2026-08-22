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
using FinancePro.UI.Common.Converters;
using FinancePro.UI.ViewModels;
using FinancePro.UI.Views;
using FinancePro.Platform.Settings;
using FinancePro.Platform.Workflow;
using FinancePro.Platform.Reporting;
using FinancePro.Platform.Documents;
using FinancePro.Platform.Administration;
using FinancePro.Platform.Accounting;
using FinancePro.Platform.Consolidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

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

    public static readonly DependencyProperty IsSidebarExpandidaProperty = DependencyProperty.Register(
        nameof(IsSidebarExpandida), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

    public bool IsSidebarExpandida
    {
        get => (bool)GetValue(IsSidebarExpandidaProperty);
        set => SetValue(IsSidebarExpandidaProperty, value);
    }

    private const double LarguraSidebarExpandida = 238;
    private const double LarguraSidebarColapsada = 72;

    private void Menu_Click(object sender, RoutedEventArgs e)
    {
        IsSidebarExpandida = !IsSidebarExpandida;
        ColunaSidebar.Width = new GridLength(IsSidebarExpandida ? LarguraSidebarExpandida : LarguraSidebarColapsada);
    }

    private readonly LoginResultDto _utilizador;
    private IServiceScope? _scopeAtual;
    private Button? _itemNavAtivo;

    public MainWindow(LoginResultDto utilizador)
    {
        InitializeComponent();
        _utilizador = utilizador;
        UsuarioTexto.Text = $"{utilizador.NomeCompleto} · {utilizador.PerfilNome}";
        InicialUsuarioTexto.Text = string.IsNullOrWhiteSpace(utilizador.NomeCompleto) ? "?" : utilizador.NomeCompleto.Trim()[0].ToString().ToUpperInvariant();
        AtualizarFotoPerfil(utilizador.FotoPerfil);

        AplicarPermissoesMenu();
        MostrarDashboard();

        Closed += (_, _) => _scopeAtual?.Dispose();
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e) => MostrarDashboard();
    private void Notificacoes_Click(object sender, RoutedEventArgs e) => MostrarNotificacoes();
    private void Projetos_Click(object sender, RoutedEventArgs e) => AbrirModuloSeguro("Projetos", MostrarProjetos);
    private void Investimentos_Click(object sender, RoutedEventArgs e) => AbrirModuloSeguro("Investimentos", MostrarInvestimentos);
    private void RecursosHumanos_Click(object sender, RoutedEventArgs e) => AbrirModuloSeguro("RH Financeiro", MostrarRecursosHumanos);

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
    private void Faturacao_Click(object sender, RoutedEventArgs e) => MostrarFaturacao();

    private void Orcamento_Click(object sender, RoutedEventArgs e) => MostrarOrcamento();

    private void Despesas_Click(object sender, RoutedEventArgs e) => MostrarDespesas();

    private void Compras_Click(object sender, RoutedEventArgs e) => MostrarCompras();

    private void Bens_Click(object sender, RoutedEventArgs e) => MostrarBens();

    private void Stocks_Click(object sender, RoutedEventArgs e) => MostrarStocks();

    private void Configuracoes_Click(object sender, RoutedEventArgs e) => MostrarConfiguracoes();

    private void Workflow_Click(object sender, RoutedEventArgs e) => MostrarWorkflow();

    private void Relatorios_Click(object sender, RoutedEventArgs e) => MostrarRelatorios();

    private void Documentos_Click(object sender, RoutedEventArgs e) => MostrarDocumentos();
    private void Auditoria_Click(object sender, RoutedEventArgs e) => MostrarAuditoria();
    private void Administracao_Click(object sender, RoutedEventArgs e) => MostrarAdministracao();

    private void Contabilidade_Click(object sender, RoutedEventArgs e) => MostrarContabilidade();

    private void Consolidacao_Click(object sender, RoutedEventArgs e) => MostrarConsolidacao();

    private void Tema_Click(object sender, RoutedEventArgs e) => GestorTema.Alternar();

    private async void FotoPerfil_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Selecionar a minha fotografia de perfil",
            Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialogo.ShowDialog(this) != true) return;

        try
        {
            var foto = ProcessadorFotoPerfil.CarregarEComprimir(dialogo.FileName);
            using var scope = App.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IUtilizadorService>()
                .AtualizarFotoPerfilAsync(_utilizador.UtilizadorId, foto);
            _utilizador.FotoPerfil = foto;
            SessaoAtual.AtualizarFotoPerfil(foto);
            AtualizarFotoPerfil(foto);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Fotografia de perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

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
        var viewModel = new DashboardViewModel(dashboardService, _utilizador.EmpresaId, _utilizador.UtilizadorId, _utilizador.NomeCompleto);
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
        var viewModel = new UtilizadoresViewModel(service, perfilService, empresaService);
        viewModel.FotoPerfilAtualizada += (_, foto) =>
        {
            _utilizador.FotoPerfil = foto;
            SessaoAtual.AtualizarFotoPerfil(foto);
            AtualizarFotoPerfil(foto);
        };
        ConteudoHost.Content = new UtilizadoresView { DataContext = viewModel };
    }

    private void AtualizarFotoPerfil(byte[]? foto)
    {
        FotoPerfilBrush.ImageSource = new FotoPerfilConverter().Convert(
            foto, typeof(ImageSource), null, System.Globalization.CultureInfo.CurrentCulture) as ImageSource;
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
        BtnParceiros.Visibility = Visibilidade("Parceiros");
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
        BtnStocks.Visibility = Visibilidade("Stocks");
        BtnProjetos.Visibility = Visibilidade("Projetos");
        BtnInvestimentos.Visibility = Visibilidade("Investimentos");
        BtnRecursosHumanos.Visibility = Visibilidade("RecursosHumanos");
        BtnContabilidade.Visibility = Visibilidade("Contabilidade");
        BtnConsolidacao.Visibility = Visibilidade("Consolidacao");
        BtnFaturacao.Visibility = Visibilidade("Faturacao");
        BtnRelatorios.Visibility = Visibilidade("Relatorios");
        BtnDocumentos.Visibility = Visibilidade("Documentos");
        BtnAuditoria.Visibility = Visibilidade("Auditoria");
        BtnWorkflow.Visibility = Visibilidade("Workflow");
        BtnConfiguracoes.Visibility = Visibilidade("Configuracoes");
        BtnAnalitica.Visibility = Visibilidade("ContabilidadeAnalitica");
        BtnAdministracao.Visibility = Visibilidade("Administracao");
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

    private void MostrarFaturacao()
    {
        DestacarItemAtivo(BtnFaturacao);
        TrocarScope();
        var receitasService = _scopeAtual!.ServiceProvider.GetRequiredService<RevenueApplicationService>();
        var viewModel = new ReceitasViewModel(receitasService, _utilizador.EmpresaId, modoFaturacao: true);
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

    private void MostrarStocks()
    {
        DestacarItemAtivo(BtnStocks);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IStockService>();
        ConteudoHost.Content = new StockView { DataContext = new StockViewModel(service, _utilizador.EmpresaId) };
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


    private void MostrarNotificacoes()
    {
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IDashboardService>();
        ConteudoHost.Content = new NotificationsView
        {
            DataContext = new NotificationsViewModel(service, _utilizador.EmpresaId, _utilizador.UtilizadorId)
        };
    }

    private void MostrarAuditoria()
    {
        DestacarItemAtivo(BtnAuditoria);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IAuditoriaService>();
        ConteudoHost.Content = new AuditoriaView { DataContext = new AuditoriaViewModel(service, _utilizador.EmpresaId) };
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

    private void MostrarDocumentos()
    {
        DestacarItemAtivo(BtnDocumentos);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IDocumentService>();
        ConteudoHost.Content = new DocumentsView { DataContext = new DocumentsViewModel(service) };
    }


    private void MostrarContabilidade()
    {
        DestacarItemAtivo(BtnContabilidade);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IAccountingService>();
        var masterData = _scopeAtual.ServiceProvider.GetRequiredService<IAdministrationMasterDataService>();
        var automaticRules = _scopeAtual.ServiceProvider.GetRequiredService<IAutomaticAccountingRulesService>();
        ConteudoHost.Content = new AccountingView
        {
            DataContext = new AccountingViewModel(service, masterData, automaticRules, _utilizador.EmpresaId, _utilizador.UtilizadorId, _utilizador.NomeCompleto)
        };
    }

    private void MostrarConsolidacao()
    {
        DestacarItemAtivo(BtnConsolidacao);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IConsolidationService>();
        ConteudoHost.Content = new ConsolidationView
        {
            DataContext = new ConsolidationViewModel(service, _utilizador.UtilizadorId, _utilizador.NomeCompleto)
        };
    }

    private void MostrarAnalitica()
    {
        DestacarItemAtivo(BtnAnalitica);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IAnalyticAccountingService>();
        ConteudoHost.Content = new AnalyticAccountingView { DataContext = new AnalyticAccountingViewModel(service, _utilizador.EmpresaId) };
    }

    private void MostrarProjetos()
    {
        DestacarItemAtivo(BtnProjetos);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IAnalyticAccountingService>();
        ConteudoHost.Content = new ProjectsView { DataContext = new ProjectsViewModel(service, _utilizador.EmpresaId) };
    }

    private void MostrarInvestimentos()
    {
        DestacarItemAtivo(BtnInvestimentos);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IAnalyticAccountingService>();
        var investmentService = _scopeAtual.ServiceProvider.GetRequiredService<FinancePro.Platform.Investments.IInvestmentService>();
        ConteudoHost.Content = new InvestimentosView { DataContext = new InvestimentosViewModel(service, investmentService, _utilizador.EmpresaId) };
    }

    private void MostrarRecursosHumanos()
    {
        DestacarItemAtivo(BtnRecursosHumanos);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<FinancePro.Platform.HumanResources.IHumanResourcesService>();
        ConteudoHost.Content = new HumanResourcesView { DataContext = new HumanResourcesViewModel(service, _utilizador.EmpresaId) };
    }

    private static void AbrirModuloSeguro(string modulo, Action abrir)
    {
        try { abrir(); }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível abrir {modulo}.\n\n{ex.GetBaseException().Message}\n\nExecute o FinancePro.Bootstrap com --validate e tente novamente.", "FinancePro — Módulo indisponível", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MostrarAdministracao()
    {
        DestacarItemAtivo(BtnAdministracao);
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IAdministrationMasterDataService>();
        ConteudoHost.Content = new AdministrationMasterDataView { DataContext = new AdministrationMasterDataViewModel(service, _utilizador.EmpresaId) };
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
    private void Analitica_Click(object sender, RoutedEventArgs e) => MostrarAnalitica();
}
