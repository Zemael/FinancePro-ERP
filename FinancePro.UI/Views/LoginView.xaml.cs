using System.Windows;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.UI.ViewModels;

namespace FinancePro.UI.Views;

public partial class LoginView : Window
{
    public event Action<LoginResultDto>? LoginBemSucedido;

    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.LoginBemSucedido += resultado => LoginBemSucedido?.Invoke(resultado);

        Loaded += (_, _) => EmailBox.Focus();
    }

    private void SenhaBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TentarEntrar();
        }
    }

    private void Entrar_Click(object sender, RoutedEventArgs e) => TentarEntrar();

    private void TentarEntrar()
    {
        if (DataContext is LoginViewModel vm && vm.EntrarCommand.CanExecute(SenhaBox.Password))
        {
            vm.EntrarCommand.Execute(SenhaBox.Password);
        }
    }

    private void BarraTitulo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            AlternarMaximizacao();
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Minimizar_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximizar_Click(object sender, RoutedEventArgs e) => AlternarMaximizacao();

    private void AlternarMaximizacao()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}
