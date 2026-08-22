using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FinancePro.UI.Common;

public static class ProcessadorFotoPerfil
{
    private const int DimensaoMaxima = 512;
    private const int TamanhoMaximoBytes = 1024 * 1024;

    public static byte[] CarregarEComprimir(string caminho)
    {
        using var origem = File.OpenRead(caminho);
        var decoder = BitmapDecoder.Create(origem, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var frame = decoder.Frames.FirstOrDefault() ?? throw new InvalidOperationException("A imagem selecionada não é válida.");
        BitmapSource imagem = frame;
        var maiorDimensao = Math.Max(frame.PixelWidth, frame.PixelHeight);
        if (maiorDimensao > DimensaoMaxima)
        {
            var escala = (double)DimensaoMaxima / maiorDimensao;
            var transformada = new TransformedBitmap(frame, new ScaleTransform(escala, escala));
            transformada.Freeze();
            imagem = transformada;
        }

        var encoder = new JpegBitmapEncoder { QualityLevel = 88 };
        encoder.Frames.Add(BitmapFrame.Create(imagem));
        using var destino = new MemoryStream();
        encoder.Save(destino);
        if (destino.Length > TamanhoMaximoBytes)
            throw new InvalidOperationException("A fotografia continua demasiado grande. Selecione outra imagem.");
        return destino.ToArray();
    }
}
