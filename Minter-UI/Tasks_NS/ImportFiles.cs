using Chia_Metadata;
using Minter_UI.CollectionInformation_ns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Minter_UI.Tasks_NS
{
    /// <summary>
    /// this class is used to import files in the apropriate folder and update the collection information
    /// </summary>
    internal class ImportFiles
    {
        /// <summary>
        /// importing Task
        /// </summary>
        /// <param name="files">list of files to import (from folder browser)</param>
        /// <param name="progressInterface"></param>
        /// <returns></returns>
        internal async static Task Import(string[] files, IProgress<float> progressInterface)
        {
            // Get the selected file's path
            List<FileInfo> filesToImport = new List<FileInfo>();
            // Merge File and directory infos
            foreach (string filePath in files)
            {
                if (System.IO.File.Exists(filePath))
                {
                    FileInfo testFile = new FileInfo(filePath);
                    if (testFile.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        continue;
                    }
                    filesToImport.Add(testFile);
                }
            }
            // save directory path for next opening
            if (filesToImport.Count > 0)
            {
                Settings_NS.Settings.All.LastImportPath = filesToImport[0].Directory.FullName;
                Settings_NS.Settings.Save();
            }
            // import Files
            float progress = 0;
            FileInfo file;
            for (int i = 0; i < filesToImport.Count; i++)
            {
                file = filesToImport[i];
                string type = file.Extension;
                string key = CollectionInformation.GetKeyFromFile(file);
                // file type cant be specified
                if (file.Extension == null || file.Extension == "")
                {
                    continue;
                }
                // move files to the apropriate directories and update collection information
                else if (file.Extension == ".nft")
                {
                    file.CopyTo(Path.Combine(Directories.Minted.FullName, file.Name), overwrite: true);
                    FileInfo newfile = new FileInfo(Path.Combine(Directories.Minted.FullName, file.Name));
                    CollectionInformation.Information.MintedFiles[key] = newfile;
                }
                else if (file.Extension == ".mint")
                {
                    file.CopyTo(Path.Combine(Directories.PendingTransactions.FullName, file.Name), overwrite: true);
                    FileInfo newfile = new FileInfo(Path.Combine(Directories.PendingTransactions.FullName, file.Name));
                    CollectionInformation.Information.PendingTransactions[key] = newfile;
                }
                else if (file.Extension == ".offer")
                {
                    file.CopyTo(Path.Combine(Directories.Offered.FullName, file.Name), overwrite: true);
                    FileInfo newfile = new FileInfo(Path.Combine(Directories.Offered.FullName, file.Name));
                    CollectionInformation.Information.OfferedFiles[key] = newfile;
                }
                else if (file.Extension == ".rpc")
                {
                    file.CopyTo(Path.Combine(Directories.Rpcs.FullName, file.Name), overwrite: true);
                    FileInfo newfile = new FileInfo(Path.Combine(Directories.Rpcs.FullName, file.Name));
                    CollectionInformation.Information.RpcFiles[key] = newfile;
                }
                else if (file.Extension == ".metadata" || file.Name == "CollectionInfo.json")
                {
                    file.CopyTo(Path.Combine(Directories.Metadata.FullName, file.Name), overwrite: true);
                    FileInfo newfile = new FileInfo(Path.Combine(Directories.Metadata.FullName, file.Name));
                    CollectionInformation.Information.MetadataFiles[key] = newfile;
                }
                else if (file.Extension == ".json")
                {
                    // could be metadata or another file type. validate by trying to load the metadata
                    try
                    {
                        Metadata test = Chia_Metadata.IO.Load(file.FullName);
                        file.CopyTo(Path.Combine(Directories.Metadata.FullName, file.Name), overwrite: true);
                        FileInfo newfile = new FileInfo(Path.Combine(Directories.Metadata.FullName, file.Name));
                        CollectionInformation.Information.MetadataFiles[key] = newfile;
                    }
                    catch
                    {
                        MessageBox.Show($"the file {file.Name} could not be imported! It does not seem to be a valid metadata file!");
                        return;
                    }
                }
                else
                {
                    file.CopyTo(Path.Combine(Directories.Nfts.FullName, file.Name), overwrite: true);
                    FileInfo newfile = new FileInfo(Path.Combine(Directories.Nfts.FullName, file.Name));
                    CollectionInformation.Information.NftFiles[key] = newfile;
                }
                // report progress
                progress = (float)(i+1) / filesToImport.Count;
                progress *= 100;
                progressInterface.Report(progress);
            }
        }
    }
}
