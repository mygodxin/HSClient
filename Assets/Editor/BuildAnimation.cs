using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

//�Զ�����animation
public class BuildAnimation : Editor
{
    //���ɳ���Prefab��·��
    private static string PrefabPath = "Assets/Textures/Effects/Prefabs";
    //���ɳ���AnimationController��·��
    private static string AnimationControllerPath = "Assets/Textures/Effects/AnimationController";
    //���ɳ���Animation��·��
    private static string AnimationPath = "Assets/Textures/Effects/Animation";
    //ԭʼͼƬ·��
    private static string ImagePath = "Assets/Textures/EffectTextures";
    //Application.dataPath
    [MenuItem("Tools/BuildAnimaiton")]
    static void BuildAniamtion()
    {
        DirectoryInfo raw = new DirectoryInfo(ImagePath);
        foreach (DirectoryInfo dictorys in raw.GetDirectories())
        {
            //ÿ���ļ��о���һ��֡�����������ÿ���ļ����µ�����ͼƬ���ɳ�һ�������ļ�
            AnimationClip clip = BuildAnimationClip(dictorys);
            //�����еĶ����ļ�������һ��AnimationController��
            AnimatorController controller = BuildAnimationController(clip, dictorys.Name);
            //������ɳ����õ�Prefab�ļ�
            BuildPrefab(dictorys, controller);
        }
    }

    static AnimationClip BuildAnimationClip(DirectoryInfo dictorys)
    {
        string animationName = dictorys.Name;
        FileInfo[] images = dictorys.GetFiles("*.png");
        AnimationClip clip = new AnimationClip();
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(Image);
        //curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";
        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[images.Length];
        //���������ǰ���Ϊ��λ��1/10�ͱ�ʾ1����10��ͼƬ��������Ŀ����������Լ�����
        float frameTime = 1 / 10f;
        for (int i = 0; i < images.Length; i++)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images[i].FullName));
            keyFrames[i] = new ObjectReferenceKeyframe();
            keyFrames[i].time = frameTime * i;
            keyFrames[i].value = sprite;
        }
        //����֡�ʣ�30�ȽϺ���
        clip.frameRate = 30;

        SerializedObject serializedClip = new SerializedObject(clip);
        AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
        clipSettings.loopTime = true;
        serializedClip.ApplyModifiedProperties();

        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
        AssetDatabase.CreateAsset(clip, AnimationPath + "/" + animationName + ".anim");
        AssetDatabase.SaveAssets();
        return clip;
    }

    static AnimatorController BuildAnimationController(AnimationClip clip, string name)
    {
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(AnimationControllerPath + "/" + name + ".controller");
        AnimatorControllerLayer layer = animatorController.layers[0];
        AnimatorStateMachine sm = layer.stateMachine;
        AnimatorState state = sm.AddState(clip.name);
        state.motion = clip;
        //sm.defaultState = state;
        AssetDatabase.SaveAssets();
        return animatorController;
    }

    static void BuildPrefab(DirectoryInfo dictorys, AnimatorController animatorCountorller)
    {
        FileInfo images = dictorys.GetFiles("*.png")[0];
        GameObject go = new GameObject();
        go.name = dictorys.Name;

        //SpriteRenderer spriteRender = go.AddComponent<SpriteRenderer>();
        //spriteRender.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images.FullName));
        Image image = go.AddComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images.FullName));

        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorCountorller;
        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath + "/" + go.name + ".prefab");
        DestroyImmediate(go);
    }

    public static string DataPathToAssetPath(string path)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return path.Substring(path.IndexOf("Assets\\"));
        else
            return path.Substring(path.IndexOf("Assets/"));
    }

    class AnimationClipSettings
    {
        SerializedProperty m_Property;

        private SerializedProperty Get(string property) { return m_Property.FindPropertyRelative(property); }

        public AnimationClipSettings(SerializedProperty prop) { m_Property = prop; }

        public float startTime { get { return Get("m_StartTime").floatValue; } set { Get("m_StartTime").floatValue = value; } }
        public float stopTime { get { return Get("m_StopTime").floatValue; } set { Get("m_StopTime").floatValue = value; } }
        public float orientationOffsetY { get { return Get("m_OrientationOffsetY").floatValue; } set { Get("m_OrientationOffsetY").floatValue = value; } }
        public float level { get { return Get("m_Level").floatValue; } set { Get("m_Level").floatValue = value; } }
        public float cycleOffset { get { return Get("m_CycleOffset").floatValue; } set { Get("m_CycleOffset").floatValue = value; } }

        public bool loopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
        public bool loopBlend { get { return Get("m_LoopBlend").boolValue; } set { Get("m_LoopBlend").boolValue = value; } }
        public bool loopBlendOrientation { get { return Get("m_LoopBlendOrientation").boolValue; } set { Get("m_LoopBlendOrientation").boolValue = value; } }
        public bool loopBlendPositionY { get { return Get("m_LoopBlendPositionY").boolValue; } set { Get("m_LoopBlendPositionY").boolValue = value; } }
        public bool loopBlendPositionXZ { get { return Get("m_LoopBlendPositionXZ").boolValue; } set { Get("m_LoopBlendPositionXZ").boolValue = value; } }
        public bool keepOriginalOrientation { get { return Get("m_KeepOriginalOrientation").boolValue; } set { Get("m_KeepOriginalOrientation").boolValue = value; } }
        public bool keepOriginalPositionY { get { return Get("m_KeepOriginalPositionY").boolValue; } set { Get("m_KeepOriginalPositionY").boolValue = value; } }
        public bool keepOriginalPositionXZ { get { return Get("m_KeepOriginalPositionXZ").boolValue; } set { Get("m_KeepOriginalPositionXZ").boolValue = value; } }
        public bool heightFromFeet { get { return Get("m_HeightFromFeet").boolValue; } set { Get("m_HeightFromFeet").boolValue = value; } }
        public bool mirror { get { return Get("m_Mirror").boolValue; } set { Get("m_Mirror").boolValue = value; } }
    }
}