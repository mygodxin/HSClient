
using HybridCLR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class Launch : MonoBehaviour
{
    public Text text;
    void Start()
    {
        //ִ�и��²���
        //Addressable Asset Setting��Disable Catalog Update on Startup˵��
        //δ��ѡʱ������Addressables.LoadAssetAsync���Զ�ִ���޸и���
        //��ѡʱ������ִ���������ݺ󣬵���Addressables.LoadAssetAsync�����������ص���Ҫ���µ�����
        StartCoroutine(DoUpdateAddressables());
    }
   
    IEnumerator DoUpdateAddressables()
    {
        //��ʼ��addressables
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        //����Ƿ���Ҫ����
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        if (checkHandle.Status != AsyncOperationStatus.Succeeded)
        {
            yield break;
        }

        if (checkHandle.Result.Count > 0)
        {
            //��ȡ����Ŀ¼
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return updateHandle;

            if (updateHandle.Status != AsyncOperationStatus.Succeeded)
            {
                yield break;
            }

            var locators = updateHandle.Result;
            foreach (var locator in locators)
            {
                var keys = new List<object>();
                keys.AddRange(locator.Keys);
                //��ȡsize��Ϣ
                var sizeHandle = Addressables.GetDownloadSizeAsync(keys.GetEnumerator());
                yield return sizeHandle;
                if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    yield break;
                }
                var downloadSize = sizeHandle.Result;
                this.text.text = "downloadSize:" + downloadSize;
                if (downloadSize > 0)
                {
                    //����
                    var downloadHandle = Addressables.DownloadDependenciesAsync(keys);
                    while (!downloadHandle.IsDone)
                    {
                        if (downloadHandle.Status == AsyncOperationStatus.Failed)
                        {
                            yield break;
                        }

                        var percentage = downloadHandle.PercentComplete;
                        this.text.text += "������:" + percentage;
                        yield return null;
                    }
                    if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        this.text.text += "�������";
                    }
                }
            }
        }
        else
        {
            this.text.text = "��ǰ�Ѿ������°汾";
        }

        LoadDllAsset();
    }

    void StartGame()
    {
        byte[] assemblyData = GetAssetData("Assembly-CSharp.dll");
        Assembly.Load(assemblyData);
        //AssetBundle prefabAb = AssetBundle.LoadFromMemory(GetAssetData("defaultlocalgroup_assets_all_d1990ef8fedf9fe10470645fc4b5d879.bundle"));
        //GameObject testPrefab = Instantiate(prefabAb.LoadAsset<GameObject>("HotUpdatePrefab.prefab"));
        Addressables.LoadSceneAsync("Assets/Scenes/LoginScene.unity");
        //Addressables.InstantiateAsync("Assets/Prefab/HotUpdate.prefab");
        Debug.Log("��ӵ�������");
        this.text.text = "�������";
    }

    private static readonly string AssemblyFile = "Assets/Hotfix/";
    public static List<string> HOTAssemblyNames { get; } = new List<string>()
    {
        "Assembly-CSharp.dll"
    };
    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
    };
    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    public static byte[] GetAssetData(string dllName)
    {
        return s_assetDatas[dllName];
    }

    private void LoadDllAsset()
    {
        var assets = HOTAssemblyNames.Concat(AOTMetaAssemblyNames);
        foreach (var asset in assets)
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>(AssemblyFile + asset + ".bytes");
            handle.Completed += DownloadComplete;
        }
    }

    private void DownloadComplete(AsyncOperationHandle<TextAsset> obj)
    {
        byte[] assetData = obj.Result.bytes;
        Debug.Log($"dll:{obj.Result.name}  size:{assetData.Length}");
        s_assetDatas[obj.Result.name] = assetData;

        if (s_assetDatas.Count == 4)
        {
            LoadMetadataForAOTAssemblies();

            StartGame();
        }
    }

    /// <summary>
    /// Ϊaot assembly����ԭʼmetadata�� ��������aot�����ȸ��¶��С�
    /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
        /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyNames)
        {
            byte[] dllBytes = GetAssetData(aotDllName);
            // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }
}
