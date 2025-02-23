using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Packager
{
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();

    /// <summary>
    /// 生成assetbundle以及lua的file list文件
    /// </summary>
    [MenuItem("Builder/Build AssetBundles and filelist")]
    public static void BuildAssetResource()
    {
        //build assetbundle: Assets/StreamingAssets/assetbundle
        string abPath = Path.Combine(Util.BuildPath, Const.AssetDirname);
        if (!Directory.Exists(abPath))
            Directory.CreateDirectory(abPath);
        BuildPipeline.BuildAssetBundles(abPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        //build lua file list: Assets/StreamingAssets/files.txt
        ///----------------------创建文件列表-----------------------
        string newFilePath = Util.BuildPath + "/files.txt";

        if (File.Exists(newFilePath))
            File.Delete(newFilePath);
        paths.Clear();
        files.Clear();
        Recursive(Util.BuildPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (ext.Equals(".meta")) continue;

            string value = file.Replace(Util.BuildPath, string.Empty);
            sw.WriteLine(value);
        }
        sw.Close(); fs.Close();
        AssetDatabase.Refresh();
        Util.Log("Generated lua files.txt at:" + newFilePath);
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }


    [MenuItem("Builder/Build Protobuf File")]
    public static void BuildProtobufFile()
    {
        string dir = Path.Combine(Util.BuildPath, "Lua/protocol");
        string protoc = Util.BuildPath + "/lua/protocol/BuildProto-lua.bat";

        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = protoc;
        info.WindowStyle = ProcessWindowStyle.Minimized;
        info.UseShellExecute = true;
        info.WorkingDirectory = dir;
        info.ErrorDialog = true;

        Process pro = Process.Start(info);
        pro.WaitForExit();

        AssetDatabase.Refresh();

        Util.Log("Build lua protobuf files success");
    }
}