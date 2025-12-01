using System;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

public class BuildGame : Editor
{
    public static string CopyAssetsDir => Application.dataPath + "/HotfixPackage/HotfixDll";

    [MenuItem("Tools/BuildApp")]
    public static void BuildApp()
    {
        AssetDatabase.Refresh();
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
        Debug.Log($"开始构建 : {buildTarget}");
        var exportPath = $"Build/";
        var exportPackageName = $"com.{PlayerSettings.companyName}.{PlayerSettings.productName}";
        // 设置不同平台的导出设置
        switch (targetGroup)
        {
            case BuildTargetGroup.Standalone:
                exportPath += "Window";
                exportPackageName += ".exe";
                break;

            case BuildTargetGroup.Android:
                exportPath += "Android";
                exportPackageName += ".apk";
                break;

            case BuildTargetGroup.iOS:
                exportPath += "IOS";
                exportPackageName += "";
                break;
            case BuildTargetGroup.WebGL:
                exportPath += "WebGL";
                exportPackageName += "";
                break;
            default:
                break;
        }

        //设置安卓证书密码
        PlayerSettings.Android.keystorePass = "";
        PlayerSettings.Android.keyaliasPass = "";

        // 设置构建的参数
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Main/Main.unity" };
        buildPlayerOptions.locationPathName = exportPath + "/" + exportPackageName;
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = BuildOptions.None;
        // 执行构建
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("BuildGame completed.");
    }
    [MenuItem("Tools/BuildHotfixAll")]
    static void BuildHotfixAll()
    {
        BuildAndCopyDll();
        BuildBundle();
    }
    [MenuItem("Tools/BuildBundle")]
    static void BuildBundle()
    {
        static string GetDefaultPackageVersion()
        {
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }
        var version = GetDefaultPackageVersion();
        var packageName = "DefaultPackage";
        var buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();

        var buildParameters = new ScriptableBuildParameters();

        buildParameters.BuildOutputRoot = buildoutputRoot;
        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
        buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
        buildParameters.PackageName = packageName;
        buildParameters.PackageVersion = version;
        buildParameters.EnableSharePackRule = true;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.FileNameStyle = EFileNameStyle.HashName;
        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
        buildParameters.BuildinFileCopyParams = string.Empty;
        buildParameters.CompressOption = ECompressOption.LZ4;
        buildParameters.ClearBuildCacheFiles = true;
        buildParameters.UseAssetDependencyDB = true;
        buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName(packageName);
        // buildParameters.EncryptionServices = new TestFileStreamEncryption();
        // buildParameters.ManifestProcessServices = new TestProcessManifest();
        // buildParameters.ManifestRestoreServices = new TestRestoreManifest();

        var pipeline = new ScriptableBuildPipeline();
        BuildResult buildResult = pipeline.Run(buildParameters, false);
        if (buildResult.Success)
        {
            Debug.Log($"Build Success:{buildResult.OutputPackageDirectory}");
        }
        else
        {
            Debug.LogError($"Build Fail:{buildResult.ErrorInfo}");
        }
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 内置着色器资源包名称
    /// 注意：和自动收集的着色器资源包名保持一致！
    /// </summary>
    private static string GetBuiltinShaderBundleName(string packageName)
    {
        var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
        var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
        return packRuleResult.GetBundleName(packageName, uniqueBundleName);
    }
    [MenuItem("Tools/BuildAndCopyDll")]
    public static void BuildAndCopyDll()
    {
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        CompileDllCommand.CompileDll(buildTarget, false);
        MakeFolder(CopyAssetsDir);
        CopyAOTAssembliesToAssetsPath();
        CopyHotUpdateAssembliesToAssetsPath();
        AssetDatabase.Refresh();
    }

    private static void CopyAOTAssembliesToAssetsPath()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);

        foreach (var dll in SettingsUtil.AOTAssemblyNames)
        {
            string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
            if (!File.Exists(srcDllPath))
            {
                Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                continue;
            }
            string dllBytesPath = $"{CopyAssetsDir}/{dll}.dll.bytes";
            File.Copy(srcDllPath, dllBytesPath, true);
            Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
        }
    }

    public static void CopyHotUpdateAssembliesToAssetsPath()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;

        string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotfixDllSrcDir}/{dll}";
            string dllBytesPath = $"{CopyAssetsDir}/{dll}.bytes";
            File.Copy(dllPath, dllBytesPath, true);
            Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
        }
    }

    public static void MakeFolder(String folder)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folder);
        if (directoryInfo.Exists == false)
        {
            directoryInfo.Create();
        }
    }
}