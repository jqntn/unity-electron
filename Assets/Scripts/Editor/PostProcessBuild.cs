using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;

internal sealed class PostProcessBuild
{
    [PostProcessBuild(0)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.StandaloneWindows64)
        {
            CleanBuildDirectory_Windows(pathToBuiltProject);
        }
        else if (target == BuildTarget.WebGL)
        {
            CopyDirectory(pathToBuiltProject, "electron");
        }
    }

    private static void CleanBuildDirectory_Windows(string pathToBuiltProject)
    {
        string directoryPath = Path.GetDirectoryName(pathToBuiltProject);

        File.Delete(Path.Combine(directoryPath, "UnityCrashHandler64.exe"));
        File.Delete(Path.Combine(directoryPath, "MonoBleedingEdge/EmbedRuntime/MonoPosixHelper.dll"));

        Directory.Delete(Path.Combine(directoryPath, "MonoBleedingEdge/etc"), true);

        string dataPath = Directory.EnumerateDirectories(directoryPath, "*_Data").Single();

        File.Delete(Path.Combine(dataPath, "RuntimeInitializeOnLoads.json"));

        Directory.GetFiles(Path.Combine(dataPath, "Managed"), "*.pdb")
            .ToList()
            .ForEach(file => File.Delete(file));
    }

    private static void CopyDirectory(string sourcePath, string destinationPath)
    {
        DirectoryInfo sourceDirectory = new(sourcePath);

        _ = Directory.CreateDirectory(destinationPath);

        sourceDirectory.GetFiles()
            .ToList()
            .ForEach(file => file.CopyTo(Path.Combine(destinationPath, file.Name), true));

        sourceDirectory.GetDirectories()
            .ToList()
            .ForEach(subDir => CopyDirectory(subDir.FullName, Path.Combine(destinationPath, subDir.Name)));
    }
}