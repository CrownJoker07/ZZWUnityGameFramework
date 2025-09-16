using System.IO;
using ICSharpCode.SharpZipLib.Zip;


public static class ZipTool
{
    public static void ZipDirectory(string sourceDirectoryPath, string outputZipPath, string passward = null)
    {
        using (FileStream fileStream = File.Create(outputZipPath))
        {
            using (ZipOutputStream zipOutputStream = new ZipOutputStream(fileStream))
            {
                zipOutputStream.SetLevel(6);  // 压缩质量和压缩速度的平衡点

                // 压缩密码
                if (string.IsNullOrEmpty(passward))
                {
                    zipOutputStream.Password = passward;
                }

                // 获取源目录的信息，用于计算相对路径
                DirectoryInfo sourceDir = new DirectoryInfo(sourceDirectoryPath);
                // 调用递归方法添加文件
                CompressDirectoryToZip(sourceDir, sourceDir.Name, zipOutputStream);
            }
        }
    }

    private static void CompressDirectoryToZip(DirectoryInfo directory, string rootPathInZip, ZipOutputStream zipStream)
    {
        byte[] buffer = new byte[4096];

        // 压缩当前目录下的所有文件
        FileInfo[] files = directory.GetFiles();
        foreach (FileInfo file in files)
        {
            // 计算文件在 zip 中的相对路径
            string relativePath = Path.Combine(rootPathInZip, file.Name);
            ZipEntry entry = new ZipEntry(relativePath);
            entry.DateTime = file.LastWriteTime;
            entry.Size = file.Length; // 可选：设置未压缩的大小

            zipStream.PutNextEntry(entry);

            using (FileStream fs = file.OpenRead())
            {
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }

            zipStream.CloseEntry();
        }

        // 递归压缩所有子目录
        DirectoryInfo[] subDirs = directory.GetDirectories();
        foreach (DirectoryInfo subDir in subDirs)
        {
            string newRootPath = Path.Combine(rootPathInZip, subDir.Name);
            CompressDirectoryToZip(subDir, newRootPath, zipStream);
        }
    }
}