using FinancePro.Application.Treasury;
using FinancePro.Application.Payables;
using FinancePro.Application.Receivables;
using FinancePro.Application.Accounting;
using FinancePro.Application.Customers;
using FinancePro.Application.Suppliers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;
using FinancePro.UI.ViewModels;
using FinancePro.UI.Views;
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
    private bool _sidebarCollapsed;

    public MainWindow(LoginResultDto utilizador)
    {
        InitializeComponent();
        _utilizador = utilizador;
        UsuarioTexto.Text = $"{utilizador.NomeCompleto} · {utilizador.PerfilNome}";

        AplicarPermissoesMenu();
        MostrarDashboard();

        Closed += (_, _) => _scopeAtual?.Dispose();
    }


    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _sidebarCollapsed = !_sidebarCollapsed;
        SidebarColumn.Width = new GridLength(_sidebarCollapsed ? 76 : 238);
        BrandTextPanel.Visibility = _sidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        CompanyCard.Visibility = _sidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;

        foreach (var text in FindVisualChildren<TextBlock>(SidebarRoot))
        {
            if (text.Style == FindResource("NavText"))
                text.Visibility = _sidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void Notifications_Click(object sender, RoutedEventArgs e) =>
        NotificationsPopup.IsOpen = !NotificationsPopup.IsOpen;

    private void GlobalSearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        var query = GlobalSearchBox.Text.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(query)) return;

        if (query.Contains("dashboard")) MostrarDashboard();
        else if (query.Contains("cliente")) MostrarClientes();
        else if (query.Contains("fornecedor")) MostrarFornecedores();
        else if (query.Contains("empresa")) MostrarEmpresas();
        else if (query.Contains("banco")) MostrarBancos();
        else if (query.Contains("caixa")) MostrarCaixa();
        else if (query.Contains("tesour")) MostrarTesouraria();
        else if (query.Contains("receita") || query.Contains("receber")) MostrarReceitas();
        else if (query.Contains("despesa") || query.Contains("pagar")) MostrarDespesas();
        else if (query.Contains("compra")) MostrarCompras();
        else if (query.Contains("patrim") || query.Contains("bem")) MostrarBens();
        else if (query.Contains("orçamento") || query.Contains("orcamento")) MostrarOrcamento();
        else if (query.Contains("contab")) MostrarContabilidade();
        else if (query.Contains("utilizador")) MostrarUtilizadores();
        else if (query.Contains("perfil")) MostrarPerfis();
        else if (query.Contains("permiss")) MostrarPermissoes();
        else if (query.Contains("config")) MostrarConfiguracoes();

        GlobalSearchBox.SelectAll();
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typed) yield return typed;
            foreach (var nested in FindVisualChildren<T>(child)) yield return nested;
        }
    }

    private void AtualizarBreadcrumb(string titulo) => BreadcrumbText.Text = titulo;

    private void Dashboard_Click(object sender, RoutedEventArgs e) => MostrarDashboard();

    private void Empresas_Click(object sender, RoutedEventArgs e) => MostrarEmpresas();

    private void Clientes_Click(object sender, RoutedEventArgs e) => MostrarClientes();

    private void Fornecedores_Click(object sender, RoutedEventArgs e) => MostrarFornecedores();

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

    private void Contabilidade_Click(object sender, RoutedEventArgs e) => MostrarContabilidade();

    private void Configuracoes_Click(object sender, RoutedEventArgs e) => MostrarConfiguracoes();

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
        AtualizarBreadcrumb("Dashboard");
        TrocarScope();
        var dashboardService = _scopeAtual!.ServiceProvider.GetRequiredService<IDashboardService>();
        var viewModel = new DashboardViewModel(dashboardService, _utilizador.EmpresaId);
        viewModel.NavegarPedido += modulo =>
        {
            switch (modulo)
            {
                case "Tesouraria": MostrarTesouraria(); break;
                case "Receitas": MostrarReceitas(); break;
                case "Caixa": MostrarCaixa(); break;
                case "Configuracoes": MostrarConfiguracoes(); break;
            }
        };
        ConteudoHost.Content = new DashboardView { DataContext = viewModel };
    }

    private void MostrarEmpresas()
    {
        DestacarItemAtivo(BtnEmpresas);
        AtualizarBreadcrumb("Empresas");
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IEmpresaService>();
        ConteudoHost.Content = new EmpresasView { DataContext = new EmpresasViewModel(service) };
    }

    private void MostrarClientes()
    {
        DestacarItemAtivo(BtnClientes);
        AtualizarBreadcrumb("Clientes");
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<CustomerApplicationService>();
        ConteudoHost.Content = new ClientesView { DataContext = new ClientesViewModel(service, _utilizador.EmpresaId) };
    }

    private void MostrarFornecedores()
    {
        DestacarItemAtivo(BtnFornecedores);
        AtualizarBreadcrumb("Fornecedores");
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<SupplierApplicationService>();
        ConteudoHost.Content = new FornecedoresView { DataContext = new FornecedoresViewModel(service, _utilizador.EmpresaId) };
    }

    private void MostrarExercicios()
    {
        DestacarItemAtivo(BtnExercicios);
        AtualizarBreadcrumb("Exercícios Financeiros");
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IExercicioFinanceiroService>();
        var empresaService = _scopeAtual.ServiceProvider.GetRequiredService<IEmpresaService>();
        ConteudoHost.Content = new ExerciciosFinanceirosView { DataContext = new ExerciciosFinanceirosViewModel(service, empresaService) };
    }

    private void MostrarMoedas()
    {
        DestacarItemAtivo(BtnMoedas);
        AtualizarBreadcrumb("Moedas");
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IMoedaService>();
        ConteudoHost.Content = new MoedasView { DataContext = new MoedasViewModel(service) };
    }

    private void MostrarUtilizadores()
    {
        DestacarItemAtivo(BtnUtilizadores);
        AtualizarBreadcrumb("Utilizadores");
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IUtilizadorService>();
        var perfilService = _scopeAtual.ServiceProvider.GetRequiredService<IPerfilService>();
        var empresaService = _scopeAtual.ServiceProvider.GetRequiredService<IEmpresaService>();
        ConteudoHost.Content = new UtilizadoresView { DataContext = new UtilizadoresViewModel(service, perfilService, empresaService) };
    }

    private void MostrarPerfis()
    {
        DestacarItemAtivo(BtnPerfis);
        AtualizarBreadcrumb("Perfis de Acesso");
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<IPerfilService>();
        ConteudoHost.Content = new PerfisView { DataContext = new PerfisViewModel(service) };
    }

    private void MostrarPermissoes()
    {
        if (!SessaoAtual.TemPermissao("Permissoes")) return;
        DestacarItemAtivo(BtnPermissoes);
        AtualizarBreadcrumb("Permissões");
        TrocarScope();
        var permissaoService = _scopeAtual!.ServiceProvider.GetRequiredService<IPermissaoService>();
        var perfilService = _scopeAtual.ServiceProvider.GetRequiredService<IPerfilService>();
        ConteudoHost.Content = new PermissoesView { DataContext = new PermissoesViewModel(permissaoService, perfilService) };
    }

    private void AplicarPermissoesMenu()
    {
        BtnDashboard.Visibility = Visibilidade("Dashboard");
        BtnEmpresas.Visibility = Visibilidade("Empresas");
        BtnClientes.Visibility = Visibilidade("Clientes");
        BtnFornecedores.Visibility = Visibilidade("Fornecedores");
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
        BtnContabilidade.Visibility = Visibilidade("Contabilidade");
        BtnConfiguracoes.Visibility = Visibilidade("Configuracoes");
    }

    private static Visibility Visibilidade(string modulo) =>
        SessaoAtual.TemPermissao(modulo) ? Visibility.Visible : Visibility.Collapsed;

    private void MostrarTesouraria()
    {
        DestacarItemAtivo(BtnTesouraria);
        AtualizarBreadcrumb("Tesouraria");
        TrocarScope();
        var tesourariaService = _scopeAtual!.ServiceProvider.GetRequiredService<AdvancedTreasuryApplicationService>();
        var viewModel = new TesourariaViewModel(tesourariaService, _utilizador.EmpresaId);
        ConteudoHost.Content = new TesourariaView { DataContext = viewModel };
    }

    private void MostrarCaixa()
    {
        DestacarItemAtivo(BtnCaixa);
        AtualizarBreadcrumb("Caixa");
        TrocarScope();
        var caixaService = _scopeAtual!.ServiceProvider.GetRequiredService<ICaixaService>();
        var viewModel = new CaixaViewModel(caixaService, _utilizador.EmpresaId);
        ConteudoHost.Content = new CaixaView { DataContext = viewModel };
    }

    private void MostrarBancos()
    {
        DestacarItemAtivo(BtnBancos);
        AtualizarBreadcrumb("Bancos e Contas Bancárias");
        TrocarScope();
        var bancoService = _scopeAtual!.ServiceProvider.GetRequiredService<IBancoService>();
        var viewModel = new BancosViewModel(bancoService, _utilizador.EmpresaId);
        ConteudoHost.Content = new BancosView { DataContext = viewModel };
    }

    private void MostrarReceitas()
    {
        DestacarItemAtivo(BtnReceitas);
        AtualizarBreadcrumb("Receitas e Contas a Receber");
        TrocarScope();
        var receitasService = _scopeAtual!.ServiceProvider.GetRequiredService<ReceivablesApplicationService>();
        var viewModel = new ReceitasViewModel(receitasService, _utilizador.EmpresaId);
        ConteudoHost.Content = new ReceitasView { DataContext = viewModel };
    }

    private void MostrarOrcamento()
    {
        DestacarItemAtivo(BtnOrcamento);
        AtualizarBreadcrumb("Gestão Orçamental");
        TrocarScope();
        var orcamentoService = _scopeAtual!.ServiceProvider.GetRequiredService<IOrcamentoService>();
        var viewModel = new OrcamentoViewModel(orcamentoService, _utilizador.EmpresaId);
        ConteudoHost.Content = new OrcamentoView { DataContext = viewModel };
    }

    private void MostrarDespesas()
    {
        DestacarItemAtivo(BtnDespesas);
        AtualizarBreadcrumb("Despesas e Contas a Pagar");
        TrocarScope();
        var despesasService = _scopeAtual!.ServiceProvider.GetRequiredService<PayablesApplicationService>();
        var viewModel = new DespesasViewModel(despesasService, _utilizador.EmpresaId);
        ConteudoHost.Content = new DespesasView { DataContext = viewModel };
    }

    private void MostrarCompras()
    {
        DestacarItemAtivo(BtnCompras);
        AtualizarBreadcrumb("Compras");
        TrocarScope();
        var compraService = _scopeAtual!.ServiceProvider.GetRequiredService<ICompraService>();
        var viewModel = new CompraViewModel(compraService, _utilizador.EmpresaId);
        ConteudoHost.Content = new CompraView { DataContext = viewModel };
    }

    private void MostrarBens()
    {
        DestacarItemAtivo(BtnBens);
        AtualizarBreadcrumb("Gestão Patrimonial");
        TrocarScope();
        var bemService = _scopeAtual!.ServiceProvider.GetRequiredService<IBemService>();
        var auditoriaService = _scopeAtual!.ServiceProvider.GetRequiredService<IAuditoriaService>();
        var viewModel = new BemViewModel(bemService, auditoriaService, _utilizador.EmpresaId);
        ConteudoHost.Content = new BemView { DataContext = viewModel };
    }


    private void MostrarContabilidade()
    {
        DestacarItemAtivo(BtnContabilidade);
        AtualizarBreadcrumb("Contabilidade");
        TrocarScope();
        var service = _scopeAtual!.ServiceProvider.GetRequiredService<AccountingApplicationService>();
        ConteudoHost.Content = new ContabilidadeView { DataContext = new ContabilidadeViewModel(service, _utilizador.EmpresaId) };
    }

    private void MostrarConfiguracoes()
    {
        DestacarItemAtivo(BtnConfiguracoes);
        AtualizarBreadcrumb("Configurações");
        TrocarScope();
        var configService = _scopeAtual!.ServiceProvider.GetRequiredService<IConfiguracoesService>();
        var viewModel = new ConfiguracoesViewModel(configService, _utilizador.EmpresaId);
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

