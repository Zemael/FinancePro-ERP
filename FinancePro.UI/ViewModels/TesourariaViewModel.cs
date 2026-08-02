using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Application.Treasury;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class TesourariaViewModel : ViewModelBase
{
    private readonly TreasuryApplicationService _service;
    private readonly int _empresaId;

    private DateTime _data = DateTime.Today;
    private string _descricao = string.Empty;
    private string _valorTexto = string.Empty;
    private TipoOperacao _tipoOperacao = TipoOperacao.Entrada;
    private OpcaoOrigemDto? _origemSelecionada;
    private OpcaoOrigemDto? _destinoSelecionado;
    private CategoriaOpcaoDto? _categoriaSelecionada;
    private string _formaPagamento = string.Empty;
    private string _centroCusto = string.Empty;
    private string _mensagemErro = string.Empty;
    private bool _aGuardar;

    public DateTime Data { get => _data; set => SetProperty(ref _data, value); }
    public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
    public string ValorTexto { get => _valorTexto; set => SetProperty(ref _valorTexto, value); }

    public TipoOperacao TipoOperacaoSelecionado
    {
        get => _tipoOperacao;
        set
        {
            if (SetProperty(ref _tipoOperacao, value))
            {
                OnPropertyChanged(nameof(EhTransferencia));
                OnPropertyChanged(nameof(TipoCategoriaImplicito));
                _ = CarregarCategoriasAsync();
            }
        }
    }

    /// <summary>Transferência/Sangria/Reforço precisam de origem E destino;
    /// os outros tipos só de uma origem.</summary>
    public bool EhTransferencia => TipoOperacaoSelecionado is TipoOperacao.Transferencia or TipoOperacao.Sangria or TipoOperacao.Reforco;

    public string TipoCategoriaImplicito => TipoOperacaoSelecionado switch
    {
        TipoOperacao.Saida => "Despesa",
        TipoOperacao.Entrada => "Receita",
        _ => "Receita ou Despesa, consoante o sinal do valor"
    };

    public IReadOnlyList<TipoOperacao> TiposOperacaoDisponiveis { get; } =
        Enum.GetValues<TipoOperacao>().ToList();

    public ObservableCollection<OpcaoOrigemDto> Origens { get; } = new();

    public OpcaoOrigemDto? OrigemSelecionada { get => _origemSelecionada; set => SetProperty(ref _origemSelecionada, value); }
    public OpcaoOrigemDto? DestinoSelecionado { get => _destinoSelecionado; set => SetProperty(ref _destinoSelecionado, value); }

    public ObservableCollection<CategoriaOpcaoDto> Categorias { get; } = new();
    public CategoriaOpcaoDto? CategoriaSelecionada { get => _categoriaSelecionada; set => SetProperty(ref _categoriaSelecionada, value); }

    public string FormaPagamento { get => _formaPagamento; set => SetProperty(ref _formaPagamento, value); }
    public string CentroCusto { get => _centroCusto; set => SetProperty(ref _centroCusto, value); }

    public IReadOnlyList<string> FormasPagamentoSugeridas { get; } =
        new[] { "Numerário", "Transferência Bancária", "Cheque", "Cartão", "Outro" };

    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }

    public ObservableCollection<MovimentoListItemDto> Movimentos { get; } = new();

    public ICommand RegistarCommand { get; }
    public ICommand AlternarConciliadoCommand { get; }

    public TesourariaViewModel(TreasuryApplicationService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;
        RegistarCommand = new AsyncRelayCommand(_ => RegistarAsync(), _ => !AGuardar);
        AlternarConciliadoCommand = new AsyncRelayCommand(AlternarConciliadoAsync);
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var resultadoOrigens = await _service.ListarOrigensAsync(_empresaId);
        if (resultadoOrigens.IsFailure) { MensagemErro = string.Join(" ", resultadoOrigens.Errors); return; }
        var origens = resultadoOrigens.Value ?? Array.Empty<OpcaoOrigemDto>();
        Origens.Clear();
        foreach (var origem in origens)
        {
            Origens.Add(origem);
        }
        OrigemSelecionada = Origens.FirstOrDefault();
        DestinoSelecionado = Origens.Skip(1).FirstOrDefault() ?? Origens.FirstOrDefault();

        await CarregarCategoriasAsync();
        await CarregarMovimentosAsync();
    }

    private async Task CarregarCategoriasAsync()
    {
        var tipo = TipoOperacaoSelecionado == TipoOperacao.Saida ? TipoCategoria.Despesa : TipoCategoria.Receita;
        var resultadoCategorias = await _service.ListarCategoriasAsync(_empresaId, tipo);
        if (resultadoCategorias.IsFailure) { MensagemErro = string.Join(" ", resultadoCategorias.Errors); return; }
        var categorias = resultadoCategorias.Value ?? Array.Empty<CategoriaOpcaoDto>();
        Categorias.Clear();
        foreach (var categoria in categorias)
        {
            Categorias.Add(categoria);
        }
        CategoriaSelecionada = Categorias.FirstOrDefault();
    }

    private async Task CarregarMovimentosAsync()
    {
        var resultadoMovimentos = await _service.ListarMovimentosAsync(_empresaId);
        if (resultadoMovimentos.IsFailure) { MensagemErro = string.Join(" ", resultadoMovimentos.Errors); return; }
        var movimentos = resultadoMovimentos.Value ?? Array.Empty<MovimentoListItemDto>();
        Movimentos.Clear();
        foreach (var movimento in movimentos)
        {
            Movimentos.Add(movimento);
        }
    }

    private async Task RegistarAsync()
    {
        MensagemErro = string.Empty;

        if (string.IsNullOrWhiteSpace(Descricao))
        {
            MensagemErro = "Indique uma descrição.";
            return;
        }

        if (!decimal.TryParse(ValorTexto, out var valor) || valor == 0)
        {
            MensagemErro = TipoOperacaoSelecionado == TipoOperacao.Ajuste
                ? "Indique um valor válido (pode ser negativo, para um ajuste a subtrair)."
                : "Indique um valor válido.";
            return;
        }

        AGuardar = true;
        try
        {
            if (EhTransferencia)
            {
                if (OrigemSelecionada is null || DestinoSelecionado is null)
                {
                    MensagemErro = "Selecione a origem e o destino.";
                    return;
                }

                var resultado = await _service.RegistarTransferenciaAsync(new NovaTransferenciaDto
                {
                    Data = Data,
                    Descricao = Descricao,
                    Valor = Math.Abs(valor),
                    TipoOperacao = TipoOperacaoSelecionado,
                    FormaPagamento = FormaPagamento,
                    CentroCusto = CentroCusto,
                    OrigemTipo = OrigemSelecionada.Tipo,
                    OrigemId = OrigemSelecionada.Id,
                    DestinoTipo = DestinoSelecionado.Tipo,
                    DestinoId = DestinoSelecionado.Id,
                    EmpresaId = _empresaId
                });
                if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
            }
            else
            {
                if (OrigemSelecionada is null)
                {
                    MensagemErro = "Selecione a origem (caixa ou conta bancária).";
                    return;
                }

                var resultado = await _service.RegistarMovimentoAsync(new NovoMovimentoDto
                {
                    Data = Data,
                    Descricao = Descricao,
                    Valor = valor,
                    TipoOperacao = TipoOperacaoSelecionado,
                    FormaPagamento = FormaPagamento,
                    CentroCusto = CentroCusto,
                    CategoriaId = CategoriaSelecionada?.Id,
                    EmpresaId = _empresaId,
                    CaixaId = OrigemSelecionada.Tipo == "Caixa" ? OrigemSelecionada.Id : null,
                    ContaBancariaId = OrigemSelecionada.Tipo == "ContaBancaria" ? OrigemSelecionada.Id : null
                });
                if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
            }

            Descricao = string.Empty;
            ValorTexto = string.Empty;

            await CarregarMovimentosAsync();
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

    private async Task AlternarConciliadoAsync(object? parametro)
    {
        if (parametro is not MovimentoListItemDto movimento)
        {
            return;
        }

        var resultado = await _service.MarcarConciliadoAsync(movimento.Id, !movimento.Conciliado);
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        await CarregarMovimentosAsync();
    }
}
