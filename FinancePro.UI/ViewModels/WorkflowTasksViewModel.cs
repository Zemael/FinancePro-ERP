using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Platform.Workflow;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class WorkflowTasksViewModel : ViewModelBase
{
    private readonly IWorkflowService _workflow;
    private readonly int _companyId;
    private readonly int _userId;
    private readonly string _userName;
    private WorkflowTaskItem? _selectedTask;
    private string _comment = string.Empty;
    private string _message = string.Empty;
    private bool _busy;
    private WorkflowSummary _summary = new(0,0,0,0);

    public ObservableCollection<WorkflowTaskItem> Tasks { get; } = new();
    public WorkflowTaskItem? SelectedTask { get => _selectedTask; set => SetProperty(ref _selectedTask, value); }
    public string Comment { get => _comment; set => SetProperty(ref _comment, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }
    public bool Busy { get => _busy; set => SetProperty(ref _busy, value); }
    public int Pending => _summary.Pending;
    public int Overdue => _summary.Overdue;
    public int Urgent => _summary.Urgent;
    public int CompletedToday => _summary.CompletedToday;

    public ICommand RefreshCommand { get; }
    public ICommand ApproveCommand { get; }
    public ICommand RejectCommand { get; }
    public ICommand ReturnCommand { get; }

    public WorkflowTasksViewModel(IWorkflowService workflow, int companyId, int userId, string userName)
    {
        _workflow = workflow; _companyId = companyId; _userId = userId; _userName = userName;
        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync());
        ApproveCommand = new AsyncRelayCommand(_ => DecideAsync(WorkflowTaskStatus.Approved), _ => SelectedTask is not null);
        RejectCommand = new AsyncRelayCommand(_ => DecideAsync(WorkflowTaskStatus.Rejected), _ => SelectedTask is not null);
        ReturnCommand = new AsyncRelayCommand(_ => DecideAsync(WorkflowTaskStatus.Returned), _ => SelectedTask is not null);
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Busy = true; Message = string.Empty;
        try
        {
            var tasks = await _workflow.GetMyTasksAsync(_companyId, _userId);
            _summary = await _workflow.GetSummaryAsync(_companyId, _userId);
            Tasks.Clear(); foreach (var task in tasks) Tasks.Add(task);
            OnPropertyChanged(nameof(Pending)); OnPropertyChanged(nameof(Overdue)); OnPropertyChanged(nameof(Urgent)); OnPropertyChanged(nameof(CompletedToday));
            SelectedTask = Tasks.FirstOrDefault();
        }
        catch (Exception ex) { Message = ex.Message; }
        finally { Busy = false; }
    }

    private async Task DecideAsync(WorkflowTaskStatus decision)
    {
        if (SelectedTask is null) return;
        Message = string.Empty;
        try
        {
            var request = new WorkflowTaskDecision(SelectedTask.Id, _companyId, _userId, _userName, decision, Comment);
            if (decision == WorkflowTaskStatus.Approved) await _workflow.ApproveAsync(request);
            else if (decision == WorkflowTaskStatus.Rejected) await _workflow.RejectAsync(request);
            else await _workflow.ReturnAsync(request);
            Comment = string.Empty;
            Message = "Decisão registada com sucesso.";
            await LoadAsync();
        }
        catch (Exception ex) { Message = ex.Message; }
    }
}
