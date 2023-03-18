using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HybridCLR;
using System.IO;
using HybridCLR.Editor;

public class CopyHotFixDll2Assets : Editor
{
    [MenuItem("Tools/CopyHotFixDll2Assets/ActiveBuildTarget")]
    static void CopeByActive()
    {
        Copy(EditorUserBuildSettings.activeBuildTarget);
    }
    [MenuItem("Tools/CopyHotFixDll2Assets/Win32")]
    static void CopeByStandaloneWindows32()
    {
        Copy(BuildTarget.StandaloneWindows);
    }
    [MenuItem("Tools/CopyHotFixDll2Assets/Win64")]
    static void CopeByStandaloneWindows64()
    {
        Copy(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Tools/CopyHotFixDll2Assets/Android")]
    static void CopeByAndroid()
    {
        Copy(BuildTarget.Android);
    }
    [MenuItem("Tools/CopyHotFixDll2Assets/IOS")]
    static void CopeByIOS()
    {
        Copy(BuildTarget.iOS);
    }

    static void Copy(BuildTarget target)
    {
        string outDir = $"{HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir}/{target}";
        string exportDir = Application.dataPath + "/Hotfix";
        if (!Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }
        foreach (var copyDll in Launch.HOTAssemblyNames)
        {
            File.Copy($"{outDir}/{copyDll}", $"{exportDir}/{copyDll}.bytes", true);
        }

        string aotDllDir = $"{HybridCLRSettings.Instance.strippedAOTDllOutputRootDir}/{target}";
        foreach (var dll in Launch.AOTMetaAssemblyNames)
        {
            string dllPath = $"{aotDllDir}/{dll}";
            if (!File.Exists(dllPath))
            {
                Debug.LogError($"ab�����AOT����Ԫ����dll:{dllPath} ʱ��������,�ļ������ڡ���Ҫ����һ��������������ɲü����AOT dll");
                continue;
            }
            string dllBytesPath = $"{exportDir}/{dll}.bytes";
            File.Copy(dllPath, dllBytesPath, true);
        }
        AssetDatabase.Refresh();
        Debug.Log("�ȸ�Dll���Ƴɹ���");
    }
}
