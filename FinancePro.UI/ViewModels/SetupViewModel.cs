using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class SetupViewModel : ViewModelBase
{
    private readonly IConfiguracoesService _service;

    private string _empresaNome = string.Empty;
    private string _moeda = "FCFA";
    private string _adminNome = string.Empty;
    private string _adminEmail = string.Empty;
    private string _mensagemErro = string.Empty;
    private bool _aGuardar;

    public string EmpresaNome { get => _empresaNome; set => SetProperty(ref _empresaNome, value); }
    public string Moeda { get => _moeda; set => SetProperty(ref _moeda, value); }
    public string AdminNome { get => _adminNome; set => SetProperty(ref _adminNome, value); }
    public string AdminEmail { get => _adminEmail; set => SetProperty(ref _adminEmail, value); }
    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }

    /// <summary>Disparado quando a configuração inicial é concluída com sucesso.</summary>
    public event Action? ConfiguracaoConcluida;

    public ICommand ConcluirCommand { get; }

    public SetupViewModel(IConfiguracoesService service)
    {
        _service = service;
        ConcluirCommand = new AsyncRelayCommand(ConcluirAsync, _ => !AGuardar);
    }

    /// <summary>A PasswordBox lê o valor no code-behind e passa-o como parâmetro,
    /// pela mesma razão descrita em LoginViewModel.</summary>
    private async Task ConcluirAsync(object? passwordParameter)
    {
        var password = passwordParameter as string ?? string.Empty;

        MensagemErro = string.Empty;
        AGuardar = true;
        try
        {
            await _service.ConfigurarInicialAsync(new ConfiguracaoInicialDto
            {
                EmpresaNome = EmpresaNome,
                Moeda = Moeda,
                AdminNome = AdminNome,
                AdminEmail = AdminEmail,
                AdminPassword = password
            });

            ConfiguracaoConcluida?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            AGuardar = false;
        }
    }
}
