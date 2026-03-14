using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(HS.MovieClip))]
public class MovieClipInspector : Editor
{
    private HS.MovieClip movieClip;
    private SerializedProperty spritesProp;
    private SerializedProperty Interval;
    private SerializedProperty swingProp;
    private SerializedProperty repeatDelayProp;
    private SerializedProperty timeScaleProp;
    private SerializedProperty ignoreEngineTimeScaleProp;
    private SerializedProperty playing;


    private void OnEnable()
    {
        movieClip = (HS.MovieClip)target;

        // 获取序列化属性
        spritesProp = serializedObject.FindProperty("Sprites");
        Interval = serializedObject.FindProperty("Interval");
        swingProp = serializedObject.FindProperty("Swing");
        repeatDelayProp = serializedObject.FindProperty("RepeatDelay");
        timeScaleProp = serializedObject.FindProperty("TimeScale");
        ignoreEngineTimeScaleProp = serializedObject.FindProperty("ignoreEngineTimeScale");
        playing = serializedObject.FindProperty("_playing");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBasicSettings();
        DrawActionButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBasicSettings()
    {
        EditorGUILayout.LabelField("动画设置", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(Interval, new GUIContent("帧频"));
            EditorGUILayout.PropertyField(swingProp, new GUIContent("摆动播放"));
            EditorGUILayout.PropertyField(repeatDelayProp, new GUIContent("循环延迟"));
            EditorGUILayout.PropertyField(timeScaleProp, new GUIContent("时间缩放"));
            EditorGUILayout.PropertyField(ignoreEngineTimeScaleProp, new GUIContent("忽略时间缩放"));
            EditorGUILayout.PropertyField(playing, new GUIContent("播放"));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginVertical("box");
        {
            // 主要操作按钮
            if (GUILayout.Button("打开序列帧编辑器", GUILayout.Height(40)))
            {
                OpenMovieClipEditor();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void OpenMovieClipEditor()
    {
        // 打开序列帧编辑器窗口
        MovieClipEditorWindow.OpenWindow(movieClip);
    }
}