using System.Windows.Controls;
using FinancePro.UI.Common;
using FinancePro.UI.ViewModels;
using Microsoft.Win32;

namespace FinancePro.UI.Views;

public partial class EmpresasView : UserControl
{
    public EmpresasView() => InitializeComponent();

    private void SelecionarLogotipo_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Selecionar logótipo da empresa",
            Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialogo.ShowDialog() != true || DataContext is not EmpresasViewModel viewModel) return;
        try
        {
            viewModel.DefinirLogotipo(ProcessadorFotoPerfil.CarregarEComprimir(dialogo.FileName));
            viewModel.Mensagem = "Logótipo preparado. Clique em Guardar Empresa para confirmar.";
        }
        catch (Exception ex) { viewModel.Mensagem = ex.Message; }
    }

    private void RemoverLogotipo_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is EmpresasViewModel viewModel) viewModel.RemoverLogotipo();
    }
}
