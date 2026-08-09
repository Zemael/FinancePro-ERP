using System.Windows;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.UI.ViewModels;

namespace FinancePro.UI.Views;

public partial class LoginView : Window
{
    private bool _senhaVisivel;
    private bool _sincronizandoSenha;

    public event Action<LoginResultDto>? LoginBemSucedido;

    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.LoginBemSucedido += resultado => LoginBemSucedido?.Invoke(resultado);

        Loaded += (_, _) => EmailBox.Focus();
    }


    private void MostrarSenha_Click(object sender, RoutedEventArgs e)
    {
        _senhaVisivel = !_senhaVisivel;
        _sincronizandoSenha = true;

        if (_senhaVisivel)
        {
            SenhaVisivelBox.Text = SenhaBox.Password;
            SenhaBox.Visibility = Visibility.Collapsed;
            SenhaVisivelBox.Visibility = Visibility.Visible;
            EyeOpenIcon.Visibility = Visibility.Collapsed;
            EyeOpenPupil.Visibility = Visibility.Collapsed;
            EyeClosedIcon.Visibility = Visibility.Visible;
            MostrarSenhaButton.ToolTip = "Ocultar palavra-passe";
            SenhaVisivelBox.Focus();
            SenhaVisivelBox.CaretIndex = SenhaVisivelBox.Text.Length;
        }
        else
        {
            SenhaBox.Password = SenhaVisivelBox.Text;
            SenhaVisivelBox.Visibility = Visibility.Collapsed;
            SenhaBox.Visibility = Visibility.Visible;
            EyeClosedIcon.Visibility = Visibility.Collapsed;
            EyeOpenIcon.Visibility = Visibility.Visible;
            EyeOpenPupil.Visibility = Visibility.Visible;
            MostrarSenhaButton.ToolTip = "Mostrar palavra-passe";
            SenhaBox.Focus();
        }

        _sincronizandoSenha = false;
    }

    private void SenhaBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_sincronizandoSenha)
            return;

        _sincronizandoSenha = true;
        SenhaVisivelBox.Text = SenhaBox.Password;
        _sincronizandoSenha = false;
    }

    private void SenhaVisivelBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_sincronizandoSenha)
            return;

        _sincronizandoSenha = true;
        SenhaBox.Password = SenhaVisivelBox.Text;
        _sincronizandoSenha = false;
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

    private void Fechar_Click(object sender, RoutedEventArgs e) => System.Windows.Application.Current.Shutdown();
}
