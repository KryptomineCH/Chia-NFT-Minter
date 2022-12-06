using System.Drawing;
using System.Drawing.Imaging;

namespace Chia_NFT_Minter.CollectionInformation_ns
{
    public static partial class CollectionInformation
    {
        private static void GeneratePreviews(bool caseSensitive, CollectionInformation_Object newInfo)
        {
            foreach(FileInfo info in newInfo.NftFileInfos)
            {
                if (info.Extension == ".png" || info.Extension == ".jpg" || info.Extension == ".tiff")
                {
                    // get key
                    string name = Path.GetFileNameWithoutExtension(info.Name);
                    string key = name;
                    if (!caseSensitive)
                    {
                        key = key.ToLower();
                    }
                    FileInfo previewFile = new FileInfo(Path.Combine(Directories.Preview.FullName, name + ".jpg"));
                    if (!previewFile.Exists)
                    {
                        Bitmap bitmap = new Bitmap(info.FullName);
                        Bitmap preview = BitmapHelper_Net.BitmapConverter.ResizeLongEdge(ref bitmap,300);
                        preview.Save(previewFile.FullName, ImageFormat.Jpeg);
                    }
                    newInfo.NftPreviewFiles[key] = previewFile;
                }
            }

        }
    }
}
