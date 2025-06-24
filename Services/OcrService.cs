using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Tesseract;

namespace Textract.Services
{
    public class OcrService:IDisposable
    {
        private readonly TesseractEngine _engine;
        private string language;

        public OcrService(string tessDataPath, string language = "jpn")
        {
            this.language = language;
            _engine = new TesseractEngine(tessDataPath, language, EngineMode.Default);
        }

        public string OCRProcess(BitmapSource bitmap)
        {
            try
            {
                // 이진화 
                Mat mat = BitmapSourceConverter.ToMat(bitmap);
                Mat gray = new Mat();
                Mat binary = new Mat();

                Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.Binary);

                // 확대 (옵션)
                Mat resized = new Mat();
                Cv2.Resize(binary, resized, new OpenCvSharp.Size(), 2, 2);

                BitmapSource binaryBitmap = BitmapSourceConverter.ToBitmapSource(resized);

                using var ms = new MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(binaryBitmap));
                encoder.Save(ms);
                ms.Position = 0;
                using var img = Pix.LoadFromMemory(ms.ToArray());

                using var page = _engine.Process(img, PageSegMode.SingleBlock);

                string text = page.GetText().Trim();

                if (language.Contains("jpn"))
                {
                    text = text.Replace(" ", "");
                }

                return string.IsNullOrWhiteSpace(text) ? "인식된 텍스트 없음" : text;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
