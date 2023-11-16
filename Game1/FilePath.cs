using System.IO;

namespace Game1
{
    [Serializable]
    public readonly struct FilePath(DirectoryPath directoryPath, string fileNameWithExtension)
    {
        public enum FileAccess
        {
            Read,
            Write
        }

        public readonly string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);

        private readonly string filePath = Path.Combine(directoryPath.directoryPath, fileNameWithExtension);

        public bool Exists()
            => File.Exists(filePath);

        public string ReadAllText()
            => File.ReadAllText(filePath);

        public void WriteAllText(string contents)
        {
            directoryPath.CreateDirectory();
            File.WriteAllText(path: filePath, contents: contents);
        }

        public FileStream CreateFileStream(FileAccess fileAccess)
        {
            if (fileAccess == FileAccess.Write)
                directoryPath.CreateDirectory();
            return new
            (
                path: filePath,
                mode: fileAccess switch
                {
                    FileAccess.Read => FileMode.Open,
                    FileAccess.Write => FileMode.Create
                },
                access: fileAccess switch
                {
                    FileAccess.Read => System.IO.FileAccess.Read,
                    FileAccess.Write => System.IO.FileAccess.Write
                }
            );
        }

        public void CopyFileToDirectory(DirectoryPath directoryPath)
            => File.Copy
            (
                sourceFileName: filePath,
                destFileName: Path.Combine(directoryPath.directoryPath, fileNameWithExtension)     
            );

        public override string ToString()
            => filePath;
    }
}
