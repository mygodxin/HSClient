using HS;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MovieClipMenuItems : Editor
{
    private static string selectAssetPath;
    private static Action<string, DirectoryInfo> excute;

    [MenuItem("GameObject/UI/MovieClip", false, 2000)]
    public static void CreateMovieClip()
    {
        UnityEngine.Object[] selectedObjects = Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets);
        UnityEngine.Object obj = selectedObjects[0];

        // 设置要处理的文件夹路径
        string folderPath = AssetDatabase.GetAssetPath(obj);

        selectAssetPath = folderPath;
        DirectoryInfo raw = new DirectoryInfo(folderPath);
        excute = (_selectAssetPath, raw) =>
        {
            if (raw.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo dictorys in raw.GetDirectories())
                {
                    Build(_selectAssetPath + "/" + dictorys.Name, dictorys);
                    if (dictorys.GetDirectories().Length > 0)
                    {
                        excute.Invoke(_selectAssetPath + "/" + dictorys.Name, dictorys);
                    }
                }
            }
            else
            {
                Build(_selectAssetPath, raw);
            }
        };
        excute.Invoke(selectAssetPath, raw);
    }

    [MenuItem("Assets/Create/MovieClip", false, 301)]
    public static void CreateMovieClipAsset()
    {
        // 创建空的 GameObject 并添加 MovieClip 组件
        CreateMovieClip();
    }

    private static void Build(string path, DirectoryInfo dictorys)
    {
        //图片数量大于1才会生成
        FileInfo[] images = dictorys.GetFiles("*.png");
        if (images.Length <= 0)
        {
            Debug.LogErrorFormat("操作文件或文件夹为空，构建MovieClip失败.");
            return;
        }
        //最后生成程序用的Prefab文件
        BuildPrefab(dictorys, path);
    }


    private static void BuildPrefab(DirectoryInfo dictorys, string path)
    {
        FileInfo[] images = dictorys.GetFiles("*.png");
        GameObject go = new GameObject("MovieClipCreator");
        go.name = dictorys.Name;

        //Image image = go.AddComponent<Image>();
        //image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images[0].FullName));
        //image.SetNativeSize();

        var movieClip = go.AddComponent<MovieClip>();
        movieClip.Sprites = new Sprite[images.Length];

        for (int i = 0; i < images.Length; i++)
        {
            var url = DataPathToAssetPath(images[i].FullName);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(url);
            movieClip.Sprites[i] = sprite;
        }
        movieClip.Interval = 0.05f;

        PrefabUtility.SaveAsPrefabAsset(go, path + "/" + go.name + ".prefab");
        DestroyImmediate(go);
    }

    public static string DataPathToAssetPath(string path)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return path.Substring(path.IndexOf("Assets\\"));
        else
            return path.Substring(path.IndexOf("Assets/"));
    }
}