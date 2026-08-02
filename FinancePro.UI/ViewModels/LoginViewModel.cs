using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    private string _email = string.Empty;
    private string _mensagemErro = string.Empty;
    private bool _aAutenticar;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string MensagemErro
    {
        get => _mensagemErro;
        set => SetProperty(ref _mensagemErro, value);
    }

    public bool AAutenticar
    {
        get => _aAutenticar;
        set => SetProperty(ref _aAutenticar, value);
    }

    /// <summary>Disparado quando a autenticação é bem-sucedida — a View trata da navegação.</summary>
    public event Action<LoginResultDto>? LoginBemSucedido;

    public ICommand EntrarCommand { get; }

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        EntrarCommand = new AsyncRelayCommand(ExecutarLoginAsync, _ => !AAutenticar);
    }

    /// <summary>
    /// A PasswordBox do WPF não permite binding direto à password por razões de segurança,
    /// por isso a View lê o valor e passa-o como parâmetro do comando.
    /// </summary>
    private async Task ExecutarLoginAsync(object? passwordParameter)
    {
        var password = passwordParameter as string ?? string.Empty;

        MensagemErro = string.Empty;
        AAutenticar = true;
        try
        {
            var resultado = await _authService.AutenticarAsync(Email, password);
            if (resultado.Sucesso)
            {
                LoginBemSucedido?.Invoke(resultado);
            }
            else
            {
                MensagemErro = resultado.Mensagem ?? "Não foi possível entrar.";
            }
        }
        finally
        {
            AAutenticar = false;
        }
    }
}
