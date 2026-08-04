using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Application.Accounting;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class ContabilidadeViewModel : ViewModelBase
{
    private readonly AccountingApplicationService _service; private readonly int _empresaId;
    private string _pesquisa=string.Empty, _codigo=string.Empty, _nome=string.Empty; private string? _mensagem; private PlanoContaDto? _selecionada; private TipoConta _tipo=TipoConta.Ativo; private NaturezaContabil _natureza=NaturezaContabil.Devedora; private bool _aceitaLancamentos=true; private string _ultimaAtualizacao="--";
    public ContabilidadeViewModel(AccountingApplicationService service,int empresaId){_service=service;_empresaId=empresaId;AtualizarCommand=new AsyncRelayCommand(_=>LoadAsync());GuardarCommand=new AsyncRelayCommand(_=>SaveAsync());NovoCommand=new RelayCommand(_=>Clear());EditarCommand=new RelayCommand(Edit); _=LoadAsync();}
    public ObservableCollection<PlanoContaDto> Contas {get;}=new(); public ObservableCollection<LancamentoContabilDto> Lancamentos {get;}=new();
    public Array Tipos=>Enum.GetValues<TipoConta>(); public Array Naturezas=>Enum.GetValues<NaturezaContabil>();
    public string Pesquisa{get=>_pesquisa;set=>SetProperty(ref _pesquisa,value);} public string Codigo{get=>_codigo;set=>SetProperty(ref _codigo,value);} public string Nome{get=>_nome;set=>SetProperty(ref _nome,value);} public TipoConta Tipo{get=>_tipo;set=>SetProperty(ref _tipo,value);} public NaturezaContabil Natureza{get=>_natureza;set=>SetProperty(ref _natureza,value);} public bool AceitaLancamentos{get=>_aceitaLancamentos;set=>SetProperty(ref _aceitaLancamentos,value);} public string? Mensagem{get=>_mensagem;set=>SetProperty(ref _mensagem,value);} public PlanoContaDto? Selecionada{get=>_selecionada;set=>SetProperty(ref _selecionada,value);} public string UltimaAtualizacao{get=>_ultimaAtualizacao;private set=>SetProperty(ref _ultimaAtualizacao,value);}
    public int TotalContas => Contas.Count; public int ContasAtivas => Contas.Count(x=>x.Ativo); public int ContasAnaliticas => Contas.Count(x=>x.AceitaLancamentos); public int TotalLancamentos => Lancamentos.Count;
    public ICommand AtualizarCommand{get;} public ICommand GuardarCommand{get;} public ICommand NovoCommand{get;} public ICommand EditarCommand{get;}
    private async Task LoadAsync(){var a=await _service.ListAccountsAsync(_empresaId,Pesquisa);Contas.Clear();if(a.IsSuccess&&a.Value!=null)foreach(var x in a.Value)Contas.Add(x);var l=await _service.ListEntriesAsync(_empresaId);Lancamentos.Clear();if(l.IsSuccess&&l.Value!=null)foreach(var x in l.Value)Lancamentos.Add(x);Mensagem=a.IsSuccess?null:a.Message??string.Join(Environment.NewLine,a.Errors);UltimaAtualizacao=DateTime.Now.ToString("dd/MM/yyyy HH:mm");OnPropertyChanged(nameof(TotalContas));OnPropertyChanged(nameof(ContasAtivas));OnPropertyChanged(nameof(ContasAnaliticas));OnPropertyChanged(nameof(TotalLancamentos));}
    private async Task SaveAsync(){var r=await _service.SaveAccountAsync(new AccountSaveRequest(Selecionada?.Id??0,_empresaId,Codigo,Nome,Tipo,Natureza,AceitaLancamentos,false,Selecionada?.ContaPaiId,Selecionada?.Ativo??true));Mensagem=r.IsSuccess?r.Message:r.Message??string.Join(Environment.NewLine,r.Errors);if(r.IsSuccess){Clear();await LoadAsync();}}
    private void Edit(object? p){if(p is PlanoContaDto x)Selecionada=x;if(Selecionada==null)return;Codigo=Selecionada.Codigo;Nome=Selecionada.Nome;Tipo=Selecionada.Tipo;Natureza=Selecionada.Natureza;AceitaLancamentos=Selecionada.AceitaLancamentos;}
    private void Clear(){Selecionada=null;Codigo=string.Empty;Nome=string.Empty;Tipo=TipoConta.Ativo;Natureza=NaturezaContabil.Devedora;AceitaLancamentos=true;Mensagem=null;}
}
