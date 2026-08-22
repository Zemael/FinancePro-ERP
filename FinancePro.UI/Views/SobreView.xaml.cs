using System.Windows;

namespace FinancePro.UI.Views;

public partial class SobreView : Window
{
    public SobreView()
    {
        InitializeComponent();
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();
}
