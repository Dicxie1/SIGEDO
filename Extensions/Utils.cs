using QRCoder;
namespace Asistencia.Extensions;
public class Utils
{

    public byte[] GenerarCodigoQR(string texto)
{
    using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
    {
        // Crear la data del QR
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
        
        // Generar el gráfico en formato PNG (Byte Array)
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        
        // El número 20 indica los píxeles por módulo (calidad)
        return qrCode.GetGraphic(20);
    }
}
    static public string GetInitials(string name, string lastName)
    {
        char first = !string.IsNullOrEmpty(name) ? name[0] : '?';
        char second = !string.IsNullOrEmpty(lastName) ? lastName[0] : '?';
        return $"{first}{second}".ToUpper();
    }
}