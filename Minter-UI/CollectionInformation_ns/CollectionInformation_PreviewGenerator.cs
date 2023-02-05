using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Minter_UI.CollectionInformation_ns
{
    public static partial class CollectionInformation
    {
        private static volatile bool PreviewGenerationRunning = false;
        private static object PreviewGenerationLock = new object();
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async Task GeneratePreviews(CollectionInformation_Object newInfo)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            lock (PreviewGenerationLock)
            {
                if (PreviewGenerationRunning) return;
                PreviewGenerationRunning = true;
            }
            Bitmap bitmap = new Bitmap(1000,1000);
            Bitmap preview = new Bitmap(250,250);
            ImageCodecInfo jpegCodec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 70L);
            foreach (FileInfo info in newInfo.NftFiles.Values)
            {
                if (info.Extension == ".png" || info.Extension == ".jpg" || info.Extension == ".tiff")
                {
                    // get key
                    string name = Path.GetFileNameWithoutExtension(info.Name);
                    string key = GetKeyFromFile(info);
                    FileInfo previewFile = new FileInfo(Path.Combine(Directories.Preview.FullName, name + ".jpg"));
                    if (!previewFile.Exists)
                    {
                        FileInfo previewOrigFile = new FileInfo(Path.Combine(Directories.Preview.FullName, name + ".png"));
                        if (previewOrigFile.Exists)
                        {
                            newInfo.NftPreviewFiles[key] = previewOrigFile;
                            continue;
                        }
                        using (bitmap = new Bitmap(info.FullName))
                        {
                            using (preview = BitmapHelper_Net.BitmapConverter.ResizeLongEdge(ref bitmap, 250))
                            {
                                // save preview to memory to compare size.
                                // If larger than original, use orig as preview
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    preview.Save(memoryStream, ImageFormat.Jpeg);
                                    long previewSize = memoryStream.Length;
                                    long origSize = info.Length;
                                    if (previewSize * 1.2 > origSize)
                                    { // use original image as preview
                                        info.CopyTo(previewOrigFile.FullName);
                                        newInfo.NftPreviewFiles[key] = previewOrigFile;
                                    }
                                    else
                                    { // save preview png
                                        preview.Save(previewFile.FullName, jpegCodec, encoderParams);
                                        newInfo.NftPreviewFiles[key] = previewFile;
                                    }
                                }
                            }
                        }
                    }
                    else {
                        newInfo.NftPreviewFiles[key] = previewFile;
                    }
                }
            }
            preview.Dispose();
            bitmap.Dispose();
            PreviewGenerationRunning = false;
        }
    }
}
