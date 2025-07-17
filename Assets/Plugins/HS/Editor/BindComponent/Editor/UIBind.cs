using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class UIBind
{
    private static CollectSetting _setting;
    public static CollectSetting Setting
    {
        get
        {
            if (_setting == null)
            {
                _setting = new CollectSetting();
            }
            return _setting;
        }
    }
    private string fieldNamePrefix;
    private bool fieldNameUseType;
    private string excludeName;
    private Dictionary<string, string> components;
    private Collect collector;
    public string nameSpace;
    private string codeSavePath;

    public static string GetPrefabAssetPath(GameObject gameObject)
    {
        //Project中的gameobject是Asset
        if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            return AssetDatabase.GetAssetPath(gameObject);
        //Scene中的gameobject是Instance
        if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
            return AssetDatabase.GetAssetPath(gameObject);
        }
        //预制体模式中的gameobject两个都不是
        var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
        if (prefabStage != null)
        {
            return prefabStage.assetPath;
        }
        return "";
    }

    public void Bind(GameObject selectedObject)
    {
        if (selectedObject.name.IndexOf("Win") >= 0)
        {
            BindWin(selectedObject);
        }
        else
        {
            BindComponent(selectedObject);
        }
    }

    private void BindWin(GameObject selectedObject)
    {
        SetupDefaultValue();
        string path = GetPrefabAssetPath(selectedObject);
        codeSavePath = path.Replace("GamePackage", "Scripts").Replace(selectedObject.name + ".prefab", "");
        CollectComponentFieldsToUpdate(selectedObject);
        GenerateCode(selectedObject);
    }

    private void BindComponent(GameObject selectedObject, string savePath = "")
    {
        SetupDefaultValue();
        if (savePath == "")
        {
            string path = GetPrefabAssetPath(selectedObject);
            codeSavePath = path.Replace("GamePackage", "Scripts").Replace(selectedObject.name + ".prefab", "");
        }
        else
            codeSavePath = savePath;
        CollectComponentFieldsToUpdate(selectedObject);
        GenerateComponentCode(selectedObject);
    }

    public async void GenerateComponentCode(GameObject selectedObject)
    {
        if (Setting.ComponentCodeTemp == null)
        {
            Debug.LogError("ComponentCodeTemplate is null, Please check 'CollectSetting' asset.");
            return;
        }


        Dictionary<string, string> fieldTypeDict = new Dictionary<string, string>();

        foreach (var item in components)
        {
            string fieldName = item.Key;
            if (string.IsNullOrEmpty(fieldName) || !Setting.FieldNameRegex.IsMatch(fieldName))
            {
                Debug.LogErrorFormat("Field name '{0}' is invalid.", fieldName);
                continue;
            }
            string componentTypeName = item.Value;
            fieldTypeDict.Add(fieldName, componentTypeName);
        }
        string className = selectedObject.name;
        var path = GetPrefabAssetPath(selectedObject);
        //预制体路径处理
        if (path == "")
        {
            //Debug.LogErrorFormat("Please check this prefab '{0}' path.", className);
            return;
        }

        //存在同名文件则放弃模版，读取文件内容只替换字段
        var codeTemplete = Setting.ComponentCodeTemp.text;
        string codeFileName = string.Format("{0}/{1}.cs", codeSavePath, className);
        if (File.Exists(codeFileName))
        {
            codeTemplete = AssetDatabase.LoadAssetAtPath<TextAsset>(codeFileName).text;
        }

        await GenerateBehaviourCode(codeFileName, codeTemplete, nameSpace, className, path, fieldTypeDict);
        //AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        CompilingFinishedCallback.Set(GetPrefabAssetPath(selectedObject));
    }

    public Collect Collect
    {
        get
        {
            if (collector == null)
            {
                collector = new Collect();
            }
            return collector;
        }
    }

    public void SetupDefaultValue()
    {
        components = new Dictionary<string, string>();

        fieldNamePrefix = Setting.FieldNamePrefix;
        fieldNameUseType = Setting.FieldNameUseType;
        nameSpace = Setting.Namespace;
        codeSavePath = Setting.CodeSavePath;
        excludeName = Setting.ExcludeName;
    }

    #region Collect

    public void CollectComponentFieldsToUpdate(GameObject selectedObject)
    {
        if (Collect == null)
        {
            return;
        }

        Transform transform = selectedObject.transform;
        if (transform == null)
            return;
        var children = selectedObject.GetComponentsInChildren<Transform>(true);

        //剔除排除项的组件
        foreach (var child in children)
        {
            if (selectedObject.name != child.name && child.name.IndexOf(excludeName) >= 0 && CheckComp(child, selectedObject.transform))
            {
                components.Add(child.name, child.name);
            }
        }

        Dictionary<string, Component> fieldComponentDict = Collect.CollectComponentFields(transform, Setting);
        foreach (var pair in fieldComponentDict)
        {
            var name = pair.Key;
            components.Add(pair.Key, pair.Value.GetType().Name);
        }
    }

    public bool CheckComp(Transform child, Transform parent)
    {
        if (child.parent.name == parent.name)
            return true;
        if (child.parent.name.IndexOf(excludeName) >= 0)
            return false;
        if (child.parent == null)
            return false;

        return CheckComp(child, child.parent);
    }

    private bool CheckGenerateCondition(GameObject selectedObject)
    {
        string savePath = codeSavePath;
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("Code save Path is invalid.");
            return false;
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        if (string.IsNullOrEmpty(nameSpace) || !Setting.DefaultNameRegex.IsMatch(nameSpace))
        {
            Debug.LogErrorFormat("NameSpace '{0}' is invalid.", nameSpace);
            return false;
        }

        string className = selectedObject.name;
        if (string.IsNullOrEmpty(className) || !Setting.DefaultNameRegex.IsMatch(className))
        {
            Debug.LogErrorFormat("Class name '{0}' is invalid.", className);
            return false;
        }

        return true;
    }

    public async void GenerateCode(GameObject selectedObject)
    {
        if (!CheckGenerateCondition(selectedObject))
        {
            return;
        }

        if (Setting.WindowCodeTemp == null)
        {
            Debug.LogError("BehaviourCodeTemplate is null, Please check 'CollectSetting' asset.");
            return;
        }

        var path = GetPrefabAssetPath(selectedObject);

        Dictionary<string, string> fieldTypeDict = new Dictionary<string, string>();

        foreach (var item in components)
        {
            string fieldName = item.Key;
            if (string.IsNullOrEmpty(fieldName) || !Setting.FieldNameRegex.IsMatch(fieldName))
            {
                Debug.LogErrorFormat("Field name '{0}' is invalid.", fieldName);
                continue;
            }
            string componentTypeName = item.Value;
            fieldTypeDict.Add(fieldName, componentTypeName);
        }
        string className = selectedObject.name;
        //预制体路径处理
        if (path == "")
        {
            Debug.LogErrorFormat("Please check this prefab '{0}' path.", className);
            return;
        }

        //存在同名文件则放弃模版，读取文件内容只替换字段
        var codeTemplete = Setting.WindowCodeTemp.text;
        string codeFileName = string.Format("{0}/{1}.cs", codeSavePath, className);
        if (File.Exists(codeFileName))
        {
            codeTemplete = AssetDatabase.LoadAssetAtPath<TextAsset>(codeFileName).text;
        }

        await GenerateBehaviourCode(codeFileName, codeTemplete, nameSpace, className, path, fieldTypeDict);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        CompilingFinishedCallback.Set(GetPrefabAssetPath(selectedObject));
    }
    #endregion

    #region 构建代码
    public async Task GenerateBehaviourCode(string filePath, string codeTemplate, string nameSpace, string className, string path, Dictionary<string, string> fieldTypeDict)
    {
        if (string.IsNullOrEmpty(codeTemplate))
        {
            Debug.LogError("Component behaviour code template file is invalid.");
            return;
        }

        StringBuilder stringBuilder = new StringBuilder(codeTemplate);

        stringBuilder.Replace("__NAME_SPACE__", nameSpace);
        stringBuilder.Replace("__CLASS_NAME__", className);
        stringBuilder.Replace("__PATH__", path);

        string field, getField;
        GenerateComponentField(fieldTypeDict, out field, out getField);
        ReplaceTextBetween(stringBuilder, "//__FIELD_BEGIN__", "//__FIELD_END__", "\n" + field);
        ReplaceTextBetween(stringBuilder, "//__GET_FIELD_BEGIN__", "//__GET_FIELD_END__", "\n" + getField);

        await SaveFile(filePath, stringBuilder.ToString());
    }

    private StringBuilder ReplaceTextBetween(StringBuilder stringBuilder, string startTag, string endTag, string newText)
    {
        int startIndex = stringBuilder.ToString().IndexOf(startTag);
        int endIndex = stringBuilder.ToString().IndexOf(endTag);

        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            stringBuilder.Remove(startIndex + startTag.Length, endIndex - (startIndex + startTag.Length));
            stringBuilder.Insert(startIndex + startTag.Length, newText);
        }

        return stringBuilder;
    }

    private void GenerateComponentField(Dictionary<string, string> fieldTypeDict, out string field, out string getField)
    {
        StringBuilder fieldBuilder = new StringBuilder();
        StringBuilder getFieldBuilder = new StringBuilder();

        foreach (var pair in fieldTypeDict)
        {
            fieldBuilder.AppendLine(string.Format("        public {0} {1};", pair.Value, pair.Key));
        }

        // 去除最后的换行符
        field = fieldBuilder.ToString();//.TrimEnd();
        getField = getFieldBuilder.ToString();//.TrimEnd();
    }

    private async Task SaveFile(string filePath, string content)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                writer.NewLine = System.Environment.NewLine; // 设置行尾标准为当前操作系统的标准


                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
        }
    }
    #endregion
}