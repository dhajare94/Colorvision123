using QRCoder;

namespace QRRewardPlatform.Services
{
    public class QRCodeGeneratorService
    {
        public byte[] GenerateQRCode(string url)
        {
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }
    }
}
