using System.IO;
using System.Runtime.InteropServices;

namespace Game1
{
    [Serializable]
    public readonly struct DirectoryPath
    {
        /// <summary>
        /// Used to save both games and maps
        /// </summary>
        private static readonly DirectoryPath savePath = new
            (
                Path.Combine
                (
                    // Paths copied from Godot https://docs.godotengine.org/en/stable/tutorials/io/data_paths.html#accessing-persistent-user-data-user
                    // On windows it is $@"%APPDATA%\{GameName}";
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? @"~/Library/Application Support"
                        : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? @"~/.local/share"
                        : throw new InvalidStateException("Unrecognised operating system"),
                    GameConfig.gameName
                )
            );
        public static readonly DirectoryPath editableMapsPath = savePath.Combine(mapFolderName);
        public static readonly DirectoryPath nonEditableMapsPath = new DirectoryPath(C.ContentManager.RootDirectory).Combine(mapFolderName);
        public static readonly DirectoryPath gameSavesPath = savePath.Combine("Saves");

        public static DirectoryPath GetMapsPath(bool editable)
            => editable ? editableMapsPath : nonEditableMapsPath;

        private const string mapFolderName = "Maps";

        public readonly string directoryPath;

        private DirectoryPath(string directoryPath)
            => this.directoryPath = directoryPath;

        private DirectoryPath Combine(string directory)
            => new(Path.Combine(directoryPath, directory));

        public bool Exists()
            => Directory.Exists(directoryPath);

        public void CreateDirectory()
            => Directory.CreateDirectory(directoryPath);

        public IEnumerable<FilePath> GetFilePaths()
            => Directory.GetFiles(path: directoryPath).Select
            (
                // This is a little inefficient, as this splits the path into directory and file, then FilePath combines them again
                // However, this allows me to not have public FilePath constructor which takes path string, thus reducing possibility of mistakes
                filePath => new FilePath
                (
                    directoryPath: new(Path.GetDirectoryName(filePath)!),
                    fileNameWithExtension: Path.GetFileName(filePath)
                )
            );
    }
}
