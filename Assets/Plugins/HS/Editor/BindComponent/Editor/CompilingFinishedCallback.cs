using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CompilingFinishedCallback
{
    private static readonly string TmpMethodNamesKey = "CompilingFinishedCallback_TmpMethodName";
    private static CollectSetting Setting => UIBind.Setting;
    public static void Set(string path)
    {
        var pathStr = EditorPrefs.GetString(TmpMethodNamesKey);
        if (string.IsNullOrEmpty(pathStr))
        {
            pathStr = path;
        }
        else
        {
            pathStr += ";" + path;
        }
        EditorPrefs.SetString(TmpMethodNamesKey, pathStr);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        var pathStr = EditorPrefs.GetString(TmpMethodNamesKey);
        if (string.IsNullOrEmpty(pathStr))
            return;
        EditorPrefs.DeleteKey(TmpMethodNamesKey);

        var paths = pathStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            GameObject selectedObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (selectedObject == null) return;

            BindProperty(selectedObject);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void BindProperty(GameObject selectedObject)
    {
        var collectCodeName = Setting.CollectorCodeName;
        var fieldNamePrefix = Setting.FieldNamePrefix;
        var fieldNameUseType = Setting.FieldNameUseType;
        var nameSpace = Setting.Namespace;

        string typeName = string.Format("{0}.{1}", nameSpace, selectedObject.name);

        Type componentType = TypeUtil.GetRuntimeType(typeName);
        if (componentType == null)
        {
            return;
        }

        var target = selectedObject;
        Component script = target.GetComponent(componentType);
        if (script == null)
        {
            script = target.AddComponent(componentType);
        }

        SerializedObject serializedObject = new SerializedObject(script);

        string collectorTypeName = collectCodeName;
        Type collectorType = TypeUtil.GetEditorType(collectorTypeName);
        var collector = (Collect)Activator.CreateInstance(collectorType);

        Dictionary<string, Component> components = new Dictionary<string, Component>();
        //
        //给组件绑定上代码
        var children = selectedObject.GetComponentsInChildren<Transform>(true);
        foreach (var child in children)
        {
            if (selectedObject.name != child.name && child.name.IndexOf(Setting.ExcludeName) >= 0)
            {
                BindProperty(child.gameObject);
                string childTypeName = string.Format("{0}.{1}", nameSpace, child.name);
                Type childCompType = TypeUtil.GetRuntimeType(childTypeName);
                if (childCompType != null)
                {
                    components.Add(child.name, child.GetComponent(childCompType));
                }
            }
        }

        Dictionary<string, Component> fieldComponentDict = collector.CollectComponentFields(target.transform, Setting);
        foreach (var pair in fieldComponentDict)
        {
            components.Add(pair.Key, pair.Value);
        }
        foreach (var item in components)
        {
            string fieldName = item.Key;
            Component component = item.Value;
            SerializedProperty targetProp = serializedObject.FindProperty(fieldName);
            if (targetProp != null && targetProp.objectReferenceValue != component)
            {
                targetProp.objectReferenceValue = component;
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}