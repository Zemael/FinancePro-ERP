using System.Collections.ObjectModel;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class AuditoriaViewModel : ViewModelBase
{
    private readonly IAuditoriaService _service; private readonly int _empresaId;
    private DateTime? _de = DateTime.Today.AddDays(-30), _ate = DateTime.Today; private string _termo = "", _mensagem = "";
    public ObservableCollection<AuditoriaConsultaDto> Registos { get; } = new();
    public DateTime? De { get=>_de; set=>SetProperty(ref _de,value); }
    public DateTime? Ate { get=>_ate; set=>SetProperty(ref _ate,value); }
    public string Termo { get=>_termo; set=>SetProperty(ref _termo,value); }
    public string Mensagem { get=>_mensagem; set=>SetProperty(ref _mensagem,value); }
    public AsyncRelayCommand AtualizarCommand { get; }
    public AuditoriaViewModel(IAuditoriaService service,int empresaId){_service=service;_empresaId=empresaId;AtualizarCommand=new AsyncRelayCommand(_ => CarregarAsync());_=CarregarAsync();}
    private async Task CarregarAsync(){var rows=await _service.ListarAsync(_empresaId,De,Ate,Termo);Registos.Clear();foreach(var x in rows)Registos.Add(x);Mensagem=$"{Registos.Count} evento(s) de auditoria.";}
}
