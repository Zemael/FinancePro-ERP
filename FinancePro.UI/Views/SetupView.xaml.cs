using System.Windows;
using FinancePro.UI.ViewModels;

namespace FinancePro.UI.Views;

public partial class SetupView : Window
{
    public event Action? ConfiguracaoConcluida;

    public SetupView(SetupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.ConfiguracaoConcluida += () => ConfiguracaoConcluida?.Invoke();
        MouseLeftButtonDown += (_, _) => DragMove();
    }

    private void Concluir_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SetupViewModel vm && vm.ConcluirCommand.CanExecute(SenhaBox.Password))
        {
            vm.ConcluirCommand.Execute(SenhaBox.Password);
        }
    }
}
