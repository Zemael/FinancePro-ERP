using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FinancePro.Core.DTOs;

namespace FinancePro.UI.Services;

/// <summary>Gera a pré-visualização de faturas/proformas seguindo o modelo A4 aprovado
/// (cabeçalho com marca + estado, dados do cliente, tabela de produtos e serviços,
/// condições de pagamento, resumo financeiro, dados bancários e assinatura/carimbo PAGO).</summary>
public static class FiscalDocumentPrinter
{
    private static readonly Color AzulMarca = Color.FromRgb(15, 76, 129);
    private static readonly Color CinzaTexto = Color.FromRgb(90, 98, 110);
    private static readonly Color CinzaLinha = Color.FromRgb(210, 217, 224);
    private static readonly Color FundoSuave = Color.FromRgb(244, 247, 250);
    private static readonly Color Vermelho = Color.FromRgb(205, 45, 45);

    public static void PrintProforma(EmpresaDto? empresa, ContaReceberListItemDto conta, IEnumerable<DocumentoItemDto> itens) =>
        Print(empresa, "FATURA PROFORMA", conta.NumeroProposta ?? conta.Codigo, conta.DataEmissao, conta, itens, null);

    public static void PrintFiscal(EmpresaDto? empresa, DocumentoFiscalDto documento, ContaReceberListItemDto conta, IEnumerable<DocumentoItemDto> itens) =>
        Print(empresa, NormalizarTipo(documento.Tipo), documento.Numero, documento.DataEmissao, conta, itens, documento);

    private static void Print(EmpresaDto? empresa, string tipo, string numero, DateTime data, ContaReceberListItemDto conta,
        IEnumerable<DocumentoItemDto> itens, DocumentoFiscalDto? fiscal)
    {
        var itensLista = itens.ToList();
        var moeda = string.IsNullOrWhiteSpace(empresa?.Moeda) ? "FCFA" : empresa!.Moeda;

        var subtotalBruto = itensLista.Sum(x => x.Subtotal);
        var baseItens = itensLista.Sum(x => x.Total - x.ValorIva);
        var totalDescontos = (subtotalBruto - baseItens) + conta.DescontoGeral;
        var baseTributavel = fiscal?.BaseTributavel ?? (baseItens - conta.DescontoGeral);
        var totalIva = fiscal?.ValorIva ?? itensLista.Sum(x => x.ValorIva);
        var totalPagar = fiscal?.Total ?? (baseTributavel + totalIva + conta.Frete + conta.OutrasDespesas);
        var paga = conta.SaldoAberto <= 0 && conta.ComercialEstado == "Faturada";

        var doc = new FlowDocument
        {
            PageWidth = 793.7,
            PageHeight = 1122.5,
            PagePadding = new Thickness(45, 30, 45, 35),
            ColumnWidth = double.PositiveInfinity,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 10.5
        };

        // --- barra superior de marca -------------------------------------------------
        doc.Blocks.Add(new Paragraph { Background = new SolidColorBrush(AzulMarca), Margin = new Thickness(0, 0, 0, 22), Padding = new Thickness(0, 6, 0, 6) });

        // --- cabeçalho: emitente | tipo de documento ----------------------------------
        var cabecalho = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 18) };
        cabecalho.Columns.Add(new TableColumn { Width = new GridLength(420) });
        cabecalho.Columns.Add(new TableColumn { Width = new GridLength(238) });
        var cabecalhoGrupo = new TableRowGroup(); cabecalho.RowGroups.Add(cabecalhoGrupo);
        var cabecalhoLinha = new TableRow(); cabecalhoGrupo.Rows.Add(cabecalhoLinha);

        var emitente = new TableCell { Padding = new Thickness(0, 0, 15, 0) };
        var logotipo = CriarImagem(empresa?.Logotipo);
        if (logotipo is not null)
            emitente.Blocks.Add(new BlockUIContainer(new Image
            {
                Source = logotipo, Width = 64, Height = 64, Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Left
            }) { Margin = new Thickness(0, 0, 0, 8) });
        emitente.Blocks.Add(new Paragraph(new Run((empresa?.Nome ?? "FinancePro ERP").ToUpperInvariant()))
            { FontSize = 14, FontWeight = FontWeights.Bold, Margin = new Thickness(0) });
        emitente.Blocks.Add(new Paragraph(new Run(FormatarEmitente(empresa))) { FontSize = 9.5, Foreground = new SolidColorBrush(CinzaTexto), Margin = new Thickness(0, 5, 0, 0), LineHeight = 14 });

        var identificacao = new TableCell();
        identificacao.Blocks.Add(new Paragraph(new Run(tipo))
            { FontSize = 20, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(AzulMarca), TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 0, 0, 8) });
        var numeroBox = new Table { CellSpacing = 0 };
        numeroBox.Columns.Add(new TableColumn { Width = new GridLength(238) });
        var numeroGrupo = new TableRowGroup(); numeroBox.RowGroups.Add(numeroGrupo);
        var numeroLinha = new TableRow(); numeroGrupo.Rows.Add(numeroLinha);
        var numeroCelula = new TableCell { BorderBrush = new SolidColorBrush(CinzaLinha), BorderThickness = new Thickness(1), Padding = new Thickness(10, 8, 10, 8) };
        numeroCelula.Blocks.Add(new Paragraph(new Run(numero)) { FontSize = 13, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right, Margin = new Thickness(0) });
        numeroCelula.Blocks.Add(new Paragraph(new Run("Original")) { FontSize = 9.5, Foreground = new SolidColorBrush(CinzaTexto), TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 3, 0, 3) });
        numeroCelula.Blocks.Add(new Paragraph(new Run($"Estado: {ObterEstado(conta, paga)}")) { FontSize = 10.5, FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Right, Margin = new Thickness(0) });
        numeroLinha.Cells.Add(numeroCelula);
        identificacao.Blocks.Add(numeroBox);

        cabecalhoLinha.Cells.Add(emitente); cabecalhoLinha.Cells.Add(identificacao);
        doc.Blocks.Add(cabecalho);

        // --- datas | faturar a ---------------------------------------------------------
        var infoTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 16) };
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(330) });
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(20) });
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(308) });
        var infoGrupo = new TableRowGroup(); infoTable.RowGroups.Add(infoGrupo);
        var infoLinha = new TableRow(); infoGrupo.Rows.Add(infoLinha);

        var datasCelula = new TableCell();
        datasCelula.Blocks.Add(CriarLinhaLabelValor("DATA DE EMISSÃO", data.ToString("dd/MM/yyyy")));
        datasCelula.Blocks.Add(CriarLinhaLabelValor("VENCIMENTO", conta.DataVencimento.ToString("dd/MM/yyyy")));
        datasCelula.Blocks.Add(CriarLinhaLabelValor("PERÍODO DE FATURAÇÃO", ObterPeriodo(data)));
        infoLinha.Cells.Add(datasCelula);
        infoLinha.Cells.Add(new TableCell());

        var clienteCelula = new TableCell { Padding = new Thickness(12), Background = new SolidColorBrush(FundoSuave) };
        clienteCelula.Blocks.Add(new Paragraph(new Run("FATURAR A")) { FontSize = 9, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(AzulMarca), Margin = new Thickness(0, 0, 0, 6) });
        clienteCelula.Blocks.Add(new Paragraph(new Run(conta.ClienteNome ?? "Consumidor final")) { FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 3) });
        clienteCelula.Blocks.Add(new Paragraph(new Run($"Cliente: CLNT-{conta.ClienteId ?? 0:000} · NIF: {conta.ClienteNIF ?? "N/D"}")) { FontSize = 9.5, Foreground = new SolidColorBrush(CinzaTexto), Margin = new Thickness(0, 0, 0, 2) });
        if (!string.IsNullOrWhiteSpace(conta.ClienteTelefone))
            clienteCelula.Blocks.Add(new Paragraph(new Run($"Contacto: {conta.ClienteTelefone}")) { FontSize = 9.5, Foreground = new SolidColorBrush(CinzaTexto), Margin = new Thickness(0, 0, 0, 2) });
        if (!string.IsNullOrWhiteSpace(conta.ClienteMorada))
            clienteCelula.Blocks.Add(new Paragraph(new Run(conta.ClienteMorada)) { FontSize = 9.5, Foreground = new SolidColorBrush(CinzaTexto), Margin = new Thickness(0) });
        infoLinha.Cells.Add(clienteCelula);
        doc.Blocks.Add(infoTable);

        // --- tabela de produtos e serviços ---------------------------------------------
        doc.Blocks.Add(new Paragraph(new Run("PRODUTOS E SERVIÇOS")) { FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(AzulMarca), Margin = new Thickness(0, 0, 0, 6) });

        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 16) };
        foreach (var largura in new[] { 55d, 205d, 45d, 45d, 75d, 55d, 50d, 90d }) table.Columns.Add(new TableColumn { Width = new GridLength(largura) });
        var group = new TableRowGroup(); table.RowGroups.Add(group);
        AddCabecalhoTabela(group, "ITEM", "DESCRIÇÃO", "QTD.", "UN.", "PREÇO UNIT.", "DESC.", "IVA", "TOTAL");
        foreach (var item in itensLista)
            AddLinhaTabela(group,
                string.IsNullOrWhiteSpace(item.ProdutoCodigo) ? "-" : item.ProdutoCodigo,
                item.ProdutoNome, N(item.Quantidade, 0), item.Unidade,
                N(item.PrecoUnitario), $"{N(item.DescontoPercentual, 1)}%", $"{N(item.IvaPercentual, 1)}%", N(item.Total));
        doc.Blocks.Add(table);

        // --- condições de pagamento | resumo financeiro --------------------------------
        var rodapeTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 16) };
        rodapeTable.Columns.Add(new TableColumn { Width = new GridLength(330) });
        rodapeTable.Columns.Add(new TableColumn { Width = new GridLength(20) });
        rodapeTable.Columns.Add(new TableColumn { Width = new GridLength(308) });
        var rodapeGrupo = new TableRowGroup(); rodapeTable.RowGroups.Add(rodapeGrupo);
        var rodapeLinha = new TableRow(); rodapeGrupo.Rows.Add(rodapeLinha);

        var condicoesCelula = new TableCell();
        condicoesCelula.Blocks.Add(new Paragraph(new Run("CONDIÇÕES DE PAGAMENTO")) { FontSize = 9, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(AzulMarca), Margin = new Thickness(0, 0, 0, 8) });
        foreach (var opcao in new[] { "Pronto pagamento", "Transferência bancária", "Pagamento em duas prestações", "Adiantamento", "Outro" })
        {
            var marcado = !string.IsNullOrWhiteSpace(conta.FormaPagamento) && conta.FormaPagamento.Contains(opcao, StringComparison.OrdinalIgnoreCase);
            condicoesCelula.Blocks.Add(new Paragraph(new Run($"{(marcado ? "☒" : "☐")}  {opcao}")) { FontSize = 10, Margin = new Thickness(0, 0, 0, 4) });
        }
        rodapeLinha.Cells.Add(condicoesCelula);
        rodapeLinha.Cells.Add(new TableCell());

        var resumoCelula = new TableCell();
        resumoCelula.Blocks.Add(new Paragraph(new Run("RESUMO FINANCEIRO")) { FontSize = 9, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(AzulMarca), Margin = new Thickness(0, 0, 0, 8) });
        resumoCelula.Blocks.Add(CriarLinhaResumo("Subtotal", $"{N(subtotalBruto)} {moeda}", false));
        resumoCelula.Blocks.Add(CriarLinhaResumo("Total de descontos", $"-{N(totalDescontos)} {moeda}", false));
        if (conta.Frete != 0) resumoCelula.Blocks.Add(CriarLinhaResumo("Frete", $"{N(conta.Frete)} {moeda}", false));
        if (conta.OutrasDespesas != 0) resumoCelula.Blocks.Add(CriarLinhaResumo("Outras despesas", $"{N(conta.OutrasDespesas)} {moeda}", false));
        resumoCelula.Blocks.Add(CriarLinhaResumo("Base tributável", $"{N(baseTributavel)} {moeda}", false));
        resumoCelula.Blocks.Add(CriarLinhaResumo("Total IVA", $"{N(totalIva)} {moeda}", false));
        var totalPar = CriarLinhaResumo("TOTAL A PAGAR", $"{N(totalPagar)} {moeda}", true);
        totalPar.Padding = new Thickness(0, 8, 0, 0);
        totalPar.BorderBrush = new SolidColorBrush(AzulMarca);
        totalPar.BorderThickness = new Thickness(0, 1, 0, 0);
        resumoCelula.Blocks.Add(totalPar);
        rodapeLinha.Cells.Add(resumoCelula);
        doc.Blocks.Add(rodapeTable);

        // --- dados bancários | observações -----------------------------------------------
        var bancoObsTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 20) };
        bancoObsTable.Columns.Add(new TableColumn { Width = new GridLength(330) });
        bancoObsTable.Columns.Add(new TableColumn { Width = new GridLength(20) });
        bancoObsTable.Columns.Add(new TableColumn { Width = new GridLength(308) });
        var boGrupo = new TableRowGroup(); bancoObsTable.RowGroups.Add(boGrupo);
        var boLinha = new TableRow(); boGrupo.Rows.Add(boLinha);

        var bancoCelula = new TableCell();
        if (!string.IsNullOrWhiteSpace(empresa?.BancoNome))
        {
            bancoCelula.Blocks.Add(new Paragraph(new Run("DADOS BANCÁRIOS")) { FontSize = 9, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(AzulMarca), Margin = new Thickness(0, 0, 0, 8) });
            bancoCelula.Blocks.Add(CriarLinhaLabelValor("Banco", empresa.BancoNome ?? "-"));
            bancoCelula.Blocks.Add(CriarLinhaLabelValor("Conta", empresa.BancoConta ?? "-"));
            bancoCelula.Blocks.Add(CriarLinhaLabelValor("NIB / IBAN", empresa.BancoIban ?? "-"));
            bancoCelula.Blocks.Add(CriarLinhaLabelValor("Titular", empresa.BancoTitular ?? empresa.Nome));
        }
        boLinha.Cells.Add(bancoCelula);
        boLinha.Cells.Add(new TableCell());

        var obsCelula = new TableCell();
        if (!string.IsNullOrWhiteSpace(conta.Observacoes) || !string.IsNullOrWhiteSpace(fiscal?.DocumentoOrigem) || !string.IsNullOrWhiteSpace(fiscal?.Motivo))
        {
            obsCelula.Blocks.Add(new Paragraph(new Run("OBSERVAÇÕES")) { FontSize = 9, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(AzulMarca), Margin = new Thickness(0, 0, 0, 8) });
            if (!string.IsNullOrWhiteSpace(conta.Observacoes))
                obsCelula.Blocks.Add(new Paragraph(new Run(conta.Observacoes)) { FontSize = 9.5, Foreground = new SolidColorBrush(CinzaTexto), Margin = new Thickness(0, 0, 0, 4) });
            if (!string.IsNullOrWhiteSpace(fiscal?.DocumentoOrigem))
                obsCelula.Blocks.Add(new Paragraph(new Run($"Documento de origem: {fiscal.DocumentoOrigem}")) { FontSize = 9.5, Foreground = new SolidColorBrush(CinzaTexto), Margin = new Thickness(0, 0, 0, 4) });
            if (!string.IsNullOrWhiteSpace(fiscal?.Motivo))
                obsCelula.Blocks.Add(new Paragraph(new Run($"Motivo: {fiscal.Motivo}")) { FontSize = 9.5, Foreground = new SolidColorBrush(CinzaTexto), Margin = new Thickness(0) });
        }
        boLinha.Cells.Add(obsCelula);
        doc.Blocks.Add(bancoObsTable);

        // --- assinaturas + carimbo PAGO ---------------------------------------------------
        var assinaturasTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 10, 0, 10) };
        assinaturasTable.Columns.Add(new TableColumn { Width = new GridLength(220) });
        assinaturasTable.Columns.Add(new TableColumn { Width = new GridLength(238) });
        assinaturasTable.Columns.Add(new TableColumn { Width = new GridLength(220) });
        var assGrupo = new TableRowGroup(); assinaturasTable.RowGroups.Add(assGrupo);
        var assLinha = new TableRow(); assGrupo.Rows.Add(assLinha);

        var esquerda = new TableCell { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(0, 0, 0, 3) };
        esquerda.Blocks.Add(new Paragraph(new Run(" ")) { FontSize = 18 });
        assLinha.Cells.Add(esquerda);

        var centro = new TableCell();
        if (paga)
        {
            var pagoBorda = new Border
            {
                BorderBrush = new SolidColorBrush(Vermelho), BorderThickness = new Thickness(2),
                Padding = new Thickness(10, 4, 10, 4), HorizontalAlignment = HorizontalAlignment.Center
            };
            var pagoTexto = new TextBlock { TextAlignment = TextAlignment.Center, Foreground = new SolidColorBrush(Vermelho) };
            pagoTexto.Inlines.Add(new Run("PAGO") { FontSize = 15, FontWeight = FontWeights.Bold });
            pagoTexto.Inlines.Add(new LineBreak());
            pagoTexto.Inlines.Add(new Run((conta.DataRecebimento ?? data).ToString("dd MMM yyyy", new CultureInfo("pt-PT")).ToUpperInvariant()) { FontSize = 9.5 });
            pagoBorda.Child = pagoTexto;
            centro.Blocks.Add(new BlockUIContainer(pagoBorda));
        }
        assLinha.Cells.Add(centro);

        var direita = new TableCell { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(0, 0, 0, 3) };
        direita.Blocks.Add(new Paragraph(new Run(" ")) { FontSize = 18 });
        assLinha.Cells.Add(direita);
        doc.Blocks.Add(assinaturasTable);

        var legendasTable = new Table { CellSpacing = 0 };
        legendasTable.Columns.Add(new TableColumn { Width = new GridLength(220) });
        legendasTable.Columns.Add(new TableColumn { Width = new GridLength(238) });
        legendasTable.Columns.Add(new TableColumn { Width = new GridLength(220) });
        var legGrupo = new TableRowGroup(); legendasTable.RowGroups.Add(legGrupo);
        var legLinha = new TableRow(); legGrupo.Rows.Add(legLinha);
        legLinha.Cells.Add(new TableCell(new Paragraph(new Run("Emitente / Cliente")) { FontSize = 9, Foreground = new SolidColorBrush(CinzaTexto) }));
        legLinha.Cells.Add(new TableCell());
        legLinha.Cells.Add(new TableCell(new Paragraph(new Run("Responsável financeiro")) { FontSize = 9, Foreground = new SolidColorBrush(CinzaTexto) }));
        doc.Blocks.Add(legendasTable);

        // --- rodapé ------------------------------------------------------------------------
        doc.Blocks.Add(new Paragraph(new Run("Documento emitido pelo FinancePro ERP - Demonstração sem valor fiscal"))
            { FontSize = 8.5, Foreground = Brushes.Gray, Margin = new Thickness(0, 24, 0, 0), BorderBrush = new SolidColorBrush(CinzaLinha), BorderThickness = new Thickness(0, 1, 0, 0), Padding = new Thickness(0, 6, 0, 0) });

        var visualizador = new FlowDocumentScrollViewer
        {
            Document = doc,
            Background = new SolidColorBrush(Color.FromRgb(238, 243, 248)),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        var janela = new Window
        {
            Title = $"Pré-visualização — {tipo} {numero}",
            Content = visualizador,
            Width = 1020,
            Height = 780,
            MinWidth = 760,
            MinHeight = 560,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        if (System.Windows.Application.Current?.MainWindow is not null)
            janela.Owner = System.Windows.Application.Current.MainWindow;
        janela.ShowDialog();
    }

    private static Paragraph CriarLinhaLabelValor(string label, string valor)
    {
        var paragrafo = new Paragraph { Margin = new Thickness(0, 0, 0, 6) };
        paragrafo.Inlines.Add(new Run($"{label}  ") { FontSize = 8.5, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(CinzaTexto) });
        paragrafo.Inlines.Add(new Run(valor) { FontSize = 10.5, FontWeight = FontWeights.SemiBold });
        return paragrafo;
    }

    private static Table CriarLinhaResumo(string label, string valor, bool total)
    {
        var linha = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 5) };
        linha.Columns.Add(new TableColumn { Width = new GridLength(180) });
        linha.Columns.Add(new TableColumn { Width = new GridLength(128) });
        var grupo = new TableRowGroup(); linha.RowGroups.Add(grupo);
        var row = new TableRow(); grupo.Rows.Add(row);
        row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontSize = total ? 11 : 10, FontWeight = total ? FontWeights.Bold : FontWeights.Normal, Margin = new Thickness(0) }));
        row.Cells.Add(new TableCell(new Paragraph(new Run(valor)) { FontSize = total ? 11 : 10, FontWeight = total ? FontWeights.Bold : FontWeights.SemiBold, TextAlignment = TextAlignment.Right, Margin = new Thickness(0) }));
        return linha;
    }

    private static void AddCabecalhoTabela(TableRowGroup group, params string[] valores)
    {
        var row = new TableRow();
        foreach (var valor in valores)
            row.Cells.Add(new TableCell(new Paragraph(new Run(valor)) { Margin = new Thickness(4, 6, 4, 6), FontSize = 8.5 })
            {
                BorderBrush = new SolidColorBrush(AzulMarca), BorderThickness = new Thickness(0, 0, 0, 2),
                Foreground = new SolidColorBrush(AzulMarca), FontWeight = FontWeights.Bold
            });
        group.Rows.Add(row);
    }

    private static void AddLinhaTabela(TableRowGroup group, params string[] valores)
    {
        var row = new TableRow();
        foreach (var valor in valores)
            row.Cells.Add(new TableCell(new Paragraph(new Run(valor)) { Margin = new Thickness(4, 5, 4, 5), FontSize = 9.5 })
            { BorderBrush = new SolidColorBrush(CinzaLinha), BorderThickness = new Thickness(0, 0, 0, 1) });
        group.Rows.Add(row);
    }

    private static string ObterEstado(ContaReceberListItemDto conta, bool paga)
    {
        if (paga) return "Paga";
        if (conta.ComercialEstado == "Proposta") return "Proposta";
        if (conta.ComercialEstado == "Aprovada") return "Aprovada";
        if (conta.SaldoAberto > 0 && conta.DiasAtraso > 0) return "Vencida";
        if (conta.ValorLiquidado > 0) return "Parcialmente paga";
        return "Pendente";
    }

    private static string ObterPeriodo(DateTime data)
    {
        var texto = data.ToString("MMMM 'de' yyyy", new CultureInfo("pt-PT"));
        return char.ToUpperInvariant(texto[0]) + texto[1..];
    }

    private static string N(decimal value, int casas = 2) => value.ToString(casas == 0 ? "N0" : casas == 1 ? "N1" : "N2", CultureInfo.CurrentCulture);

    private static BitmapImage? CriarImagem(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 }) return null;
        try
        {
            using var stream = new MemoryStream(bytes, writable: false);
            var imagem = new BitmapImage();
            imagem.BeginInit(); imagem.CacheOption = BitmapCacheOption.OnLoad; imagem.StreamSource = stream; imagem.EndInit();
            imagem.Freeze();
            return imagem;
        }
        catch { return null; }
    }

    private static string FormatarEmitente(EmpresaDto? empresa)
    {
        if (empresa is null) return "Documento emitido pelo FinancePro ERP";
        return string.Join("\n", new[]
        {
            string.IsNullOrWhiteSpace(empresa.NIF) ? null : $"NIF: {empresa.NIF}",
            empresa.Morada,
            string.IsNullOrWhiteSpace(empresa.Telefone) && string.IsNullOrWhiteSpace(empresa.Email) ? null
                : string.Join(" · ", new[] { empresa.Telefone, empresa.Email }.Where(x => !string.IsNullOrWhiteSpace(x)))
        }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string NormalizarTipo(string tipo) => tipo switch
    {
        "Fatura" => "FATURA", "Recibo" => "RECIBO",
        "NotaCredito" => "NOTA DE CRÉDITO", "NotaDebito" => "NOTA DE DÉBITO", _ => tipo.ToUpperInvariant()
    };
}
