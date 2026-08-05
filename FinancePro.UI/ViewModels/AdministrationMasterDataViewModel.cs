using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Platform.Administration;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class AdministrationMasterDataViewModel : ViewModelBase
{
    private readonly IAdministrationMasterDataService _service;
    private readonly int _companyId;
    private string _costCode = string.Empty;
    private string _costName = string.Empty;
    private string _taxCode = string.Empty;
    private string _taxName = string.Empty;
    private decimal _taxRate;
    private string _message = string.Empty;
    private bool _busy;

    public ObservableCollection<CostCenter> CostCenters { get; } = new();
    public ObservableCollection<TaxRate> TaxRates { get; } = new();
    public string CostCode { get => _costCode; set => SetProperty(ref _costCode, value); }
    public string CostName { get => _costName; set => SetProperty(ref _costName, value); }
    public string TaxCode { get => _taxCode; set => SetProperty(ref _taxCode, value); }
    public string TaxName { get => _taxName; set => SetProperty(ref _taxName, value); }
    public decimal TaxRate { get => _taxRate; set => SetProperty(ref _taxRate, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }
    public bool Busy { get => _busy; set => SetProperty(ref _busy, value); }
    public int ActiveCostCenters => CostCenters.Count(x => x.Active);
    public int ActiveTaxRates => TaxRates.Count(x => x.Active);

    public ICommand RefreshCommand { get; }
    public ICommand SaveCostCenterCommand { get; }
    public ICommand SaveTaxRateCommand { get; }
    public ICommand ToggleCostCenterCommand { get; }
    public ICommand ToggleTaxRateCommand { get; }

    public AdministrationMasterDataViewModel(IAdministrationMasterDataService service, int companyId)
    {
        _service = service; _companyId = companyId;
        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync());
        SaveCostCenterCommand = new AsyncRelayCommand(_ => SaveCostCenterAsync());
        SaveTaxRateCommand = new AsyncRelayCommand(_ => SaveTaxRateAsync());
        ToggleCostCenterCommand = new AsyncRelayCommand(ToggleCostCenterAsync);
        ToggleTaxRateCommand = new AsyncRelayCommand(ToggleTaxRateAsync);
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Busy = true; Message = string.Empty;
        try
        {
            var costs = await _service.ListCostCentersAsync(_companyId);
            var taxes = await _service.ListTaxRatesAsync(_companyId);
            CostCenters.Clear(); foreach (var item in costs) CostCenters.Add(item);
            TaxRates.Clear(); foreach (var item in taxes) TaxRates.Add(item);
            OnPropertyChanged(nameof(ActiveCostCenters)); OnPropertyChanged(nameof(ActiveTaxRates));
        }
        catch (Exception ex) { Message = ex.Message; }
        finally { Busy = false; }
    }

    private async Task SaveCostCenterAsync()
    {
        try { await _service.SaveCostCenterAsync(new SaveCostCenterRequest(_companyId, null, CostCode, CostName)); CostCode = CostName = string.Empty; Message = "Centro de custo guardado."; await LoadAsync(); }
        catch (Exception ex) { Message = ex.Message; }
    }

    private async Task SaveTaxRateAsync()
    {
        try { await _service.SaveTaxRateAsync(new SaveTaxRateRequest(_companyId, null, TaxCode, TaxName, TaxRate)); TaxCode = TaxName = string.Empty; TaxRate = 0; Message = "Taxa de IVA guardada."; await LoadAsync(); }
        catch (Exception ex) { Message = ex.Message; }
    }

    private async Task ToggleCostCenterAsync(object? parameter)
    {
        if (parameter is not CostCenter item) return;
        await _service.SetCostCenterActiveAsync(_companyId, item.Id, !item.Active); await LoadAsync();
    }

    private async Task ToggleTaxRateAsync(object? parameter)
    {
        if (parameter is not TaxRate item) return;
        await _service.SetTaxRateActiveAsync(_companyId, item.Id, !item.Active); await LoadAsync();
    }
}
