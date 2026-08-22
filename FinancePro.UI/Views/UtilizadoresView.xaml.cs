using System.Windows.Controls;
using Microsoft.Win32;
using FinancePro.UI.Common;
using FinancePro.UI.ViewModels;

namespace FinancePro.UI.Views;

public partial class UtilizadoresView : UserControl
{
    public UtilizadoresView() => InitializeComponent();

    private void SelecionarFoto_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Selecionar fotografia de perfil",
            Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialogo.ShowDialog() != true || DataContext is not UtilizadoresViewModel viewModel) return;
        try
        {
            viewModel.DefinirFotoPerfil(ProcessadorFotoPerfil.CarregarEComprimir(dialogo.FileName));
            viewModel.Mensagem = "Fotografia preparada. Clique em Guardar Utilizador para confirmar.";
        }
        catch (Exception ex) { viewModel.Mensagem = ex.Message; }
    }
}
