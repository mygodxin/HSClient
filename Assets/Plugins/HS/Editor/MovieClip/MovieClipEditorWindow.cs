using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MovieClipEditorWindow : EditorWindow
{
    private HS.MovieClip currentMovieClip;

    private Texture2D previewTexture;
    private Vector2 frameStripScrollPos;
    private double lastFrameTime;   // 上一帧的时间

    // 窗口尺寸常量
    private const float SETTINGS_PANEL_WIDTH = 250f;
    private const float FRAME_STRIP_HEIGHT = 130f;
    private const float TOOLBAR_HEIGHT = 24f;

    [MenuItem("Tools/序列帧编辑器")]
    public static void ShowWindow()
    {
        var window = GetWindow<MovieClipEditorWindow>("MovieClip - 序列帧编辑器");
        window.titleContent = new GUIContent("MovieClip - 序列帧编辑器");
        window.minSize = new Vector2(800, 600);
        window.Show();
    }

    public static void OpenWindow(HS.MovieClip movieClip)
    {
        var window = GetWindow<MovieClipEditorWindow>("MovieClip - 序列帧编辑器");
        window.titleContent = new GUIContent($"MovieClip - {movieClip.gameObject.name}");
        window.currentMovieClip = movieClip;
        window.currentMovieClip.Init();
        window.minSize = new Vector2(800, 600);
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
        previewTexture = CreatePreviewTexture();

        // 如果没有传入MovieClip，尝试从选中对象获取
        if (currentMovieClip == null && Selection.activeGameObject != null)
        {
            currentMovieClip = Selection.activeGameObject.GetComponent<HS.MovieClip>();
        }
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        currentMovieClip.Playing = false;
    }

    private void OnEditorUpdate()
    {
        if (currentMovieClip != null && currentMovieClip.Frames != null)
        {
            // 计算自上一帧以来的时间（使用Editor的增量时间，而不是Time.deltaTime）
            double deltaTime = EditorApplication.timeSinceStartup - lastFrameTime;
            lastFrameTime = EditorApplication.timeSinceStartup;
            currentMovieClip.OnTimer((float)deltaTime);
            Repaint();
        }
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (currentMovieClip == null)
        {
            EditorGUILayout.HelpBox("请选择一个带有 MovieClip 组件的 GameObject", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        {
            DrawPreviewArea();
            DrawAnimationSettings();
        }
        EditorGUILayout.EndHorizontal();

        DrawFrameStrip();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(TOOLBAR_HEIGHT));
        {
            if (GUILayout.Button("导入图片序列", EditorStyles.toolbarButton))
            {
                ImportImageSequence();
            }

            if (GUILayout.Button("导入Sprite表", EditorStyles.toolbarButton))
            {
                ImportSpriteSheet();
            }

            GUILayout.Space(20);

            if (currentMovieClip != null)
            {
                string fileName = currentMovieClip.gameObject.name;
                EditorGUILayout.LabelField($"当前编辑: {fileName}", EditorStyles.miniLabel);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPreviewArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        {
            // 计算预览区域大小
            float previewHeight = position.height - TOOLBAR_HEIGHT - FRAME_STRIP_HEIGHT;
            Rect previewRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.Height(previewHeight));

            // 绘制背景
            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill);
            }

            // 绘制当前帧
            Sprite currentSprite = currentMovieClip.sprite;
            if (currentMovieClip != null)
            {
                Rect textureRect = CalculateTextureRect(previewRect, currentSprite);
                GUI.DrawTexture(textureRect, currentSprite.texture, ScaleMode.StretchToFill);
            }

            // 绘制播放控制栏
            DrawPlaybackControls();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawPlaybackControls()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();

            GUI.enabled = currentMovieClip != null && currentMovieClip.Sprites != null && currentMovieClip.Sprites.Length > 0;

            // 上一帧按钮
            if (GUILayout.Button("◀◀", GUILayout.Width(40), GUILayout.Height(20)))
            {
                if (currentMovieClip.CurrentFrame == 0)
                    currentMovieClip.CurrentFrame = currentMovieClip.Frames.Length - 1;
                else
                    currentMovieClip.CurrentFrame--;
            }

            // 播放/暂停按钮
            string playPauseText = currentMovieClip.Playing ? "❚❚" : "▶";
            if (GUILayout.Button(playPauseText, GUILayout.Width(40), GUILayout.Height(20)))
            {
                currentMovieClip.Playing = !currentMovieClip.Playing;
            }

            // 下一帧按钮
            if (GUILayout.Button("▶▶", GUILayout.Width(40), GUILayout.Height(20)))
            {
                if (currentMovieClip.CurrentFrame == currentMovieClip.Frames.Length - 1)
                    currentMovieClip.CurrentFrame = 0;
                else
                    currentMovieClip.CurrentFrame++;
            }

            GUI.enabled = true;

            // 帧信息显示
            int totalFrames = currentMovieClip != null && currentMovieClip.Sprites != null ? currentMovieClip.Sprites.Length : 0;
            string frameInfo = totalFrames > 0 ? $"{currentMovieClip.CurrentFrame + 1}/{totalFrames}" : "0/0";
            EditorGUILayout.LabelField(frameInfo, GUILayout.Width(40));

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawAnimationSettings()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(SETTINGS_PANEL_WIDTH), GUILayout.ExpandHeight(true));
        {
            EditorGUILayout.LabelField("动画设置", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            currentMovieClip.Interval = EditorGUILayout.FloatField("播放间隔(s)", currentMovieClip.Interval);
            currentMovieClip.Swing = EditorGUILayout.Toggle("摆动播放", currentMovieClip.Swing);
            currentMovieClip.RepeatDelay = EditorGUILayout.FloatField("循环延迟", currentMovieClip.RepeatDelay);

            EditorGUILayout.Space();

            // 高级设置
            currentMovieClip.TimeScale = EditorGUILayout.FloatField("时间缩放", currentMovieClip.TimeScale);
            currentMovieClip.ignoreEngineTimeScale = EditorGUILayout.Toggle("忽略时间缩放", currentMovieClip.ignoreEngineTimeScale);

            EditorGUILayout.Space();

            // 当前帧延迟设置
            if (currentMovieClip.Sprites != null && currentMovieClip.Sprites.Length > 0)
            {
                EditorGUILayout.LabelField("当前帧设置", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"帧 {currentMovieClip.CurrentFrame + 1}", EditorStyles.miniLabel);

                // 这里可以添加每帧的延迟设置
                // 由于您的MovieClip组件没有每帧延迟，这里显示基本信息
                Sprite currentSprite = currentMovieClip.Sprites[currentMovieClip.CurrentFrame];
                if (currentSprite != null)
                {
                    EditorGUILayout.LabelField($"尺寸: {currentSprite.rect.width} x {currentSprite.rect.height}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"名称: {currentSprite.name}", EditorStyles.miniLabel);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(currentMovieClip);
            }

            EditorGUILayout.Space();

            // 统计信息
            if (currentMovieClip.Sprites != null && currentMovieClip.Sprites.Length > 0)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"总帧数: {currentMovieClip.Sprites.Length}", EditorStyles.miniLabel);
                    float totalDuration = currentMovieClip.Sprites.Length * currentMovieClip.Interval;
                    EditorGUILayout.LabelField($"总时长: {totalDuration:F2}s", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"帧率: {1 / currentMovieClip.Interval:F1} FPS", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawFrameStrip()
    {
        if (currentMovieClip == null || currentMovieClip.Sprites == null || currentMovieClip.Sprites.Length == 0)
        {
            EditorGUILayout.LabelField("帧序列", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("暂无帧序列，请导入图片序列", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"帧序列 ({currentMovieClip.Sprites.Length} 帧)", EditorStyles.boldLabel);

        frameStripScrollPos = EditorGUILayout.BeginScrollView(frameStripScrollPos, GUILayout.Height(FRAME_STRIP_HEIGHT));
        EditorGUILayout.BeginHorizontal();
        {
            var currentFrameIndex = currentMovieClip.CurrentFrame;
            for (int i = 0; i < currentMovieClip.Sprites.Length; i++)
            {
                Sprite sprite = currentMovieClip.Sprites[i];
                if (sprite != null)
                {
                    GUIStyle frameStyle = new GUIStyle(GUI.skin.box);
                    if (i == currentFrameIndex)
                    {
                        frameStyle.normal.background = CreateColorTexture(new Color(0.2f, 0.4f, 0.8f, 0.3f));
                    }

                    EditorGUILayout.BeginVertical();
                    if (GUILayout.Button(sprite.texture, frameStyle, GUILayout.Width(60), GUILayout.Height(60)))
                    {
                        currentFrameIndex = i;
                        currentMovieClip.Playing = false;
                        Repaint();
                    }
                    EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(60));
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    // 显示缺失纹理的占位符
                    EditorGUILayout.BeginVertical();
                    if (GUILayout.Button("缺失", GUILayout.Width(60), GUILayout.Height(60)))
                    {
                        currentFrameIndex = i;
                        Repaint();
                    }
                    EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(60));
                    EditorGUILayout.EndVertical();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private void ImportImageSequence()
    {
        if (currentMovieClip == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择或创建一个 MovieClip 组件", "确定");
            return;
        }

        string folderPath = EditorUtility.OpenFolderPanel("选择图片序列所在文件夹", "", "");
        if (string.IsNullOrEmpty(folderPath)) return;

        string[] supportedFormats = new string[] { "*.png", "*.jpg", "*.jpeg", "*.tga" };
        List<string> filePaths = new List<string>();
        foreach (string format in supportedFormats)
        {
            filePaths.AddRange(Directory.GetFiles(folderPath, format));
        }

        // 自然排序
        filePaths = filePaths.OrderBy(p => p, new NaturalSortComparer()).ToList();

        if (filePaths.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "未找到支持的图片文件", "确定");
            return;
        }

        // 询问是否清除现有帧
        bool clearExisting = true;
        if (currentMovieClip.Sprites != null && currentMovieClip.Sprites.Length > 0)
        {
            clearExisting = EditorUtility.DisplayDialog(
                "导入选项",
                "是否清除现有帧序列？",
                "清除", "追加");
        }

        List<Sprite> sprites = new List<Sprite>();
        if (!clearExisting && currentMovieClip.Sprites != null)
        {
            sprites.AddRange(currentMovieClip.Sprites);
        }

        int importedCount = 0;
        foreach (string filePath in filePaths)
        {
            string relativePath = "Assets" + filePath.Replace(Application.dataPath, "");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
            if (sprite != null)
            {
                sprites.Add(sprite);
                importedCount++;
            }
        }

        if (importedCount > 0)
        {
            currentMovieClip.Sprites = sprites.ToArray();
            currentMovieClip.Init();
            currentMovieClip.CurrentFrame = 0;
            currentMovieClip.Playing = false;
            EditorUtility.SetDirty(currentMovieClip);
            Repaint();

            EditorUtility.DisplayDialog("导入完成", $"成功导入 {importedCount} 张图片", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("导入失败", "未能导入任何图片", "确定");
        }
    }

    private void ImportSpriteSheet()
    {
        EditorUtility.DisplayDialog("功能提示", "Sprite表导入功能将在后续版本中实现", "确定");
    }

    private Texture2D CreatePreviewTexture()
    {
        Texture2D tex = new Texture2D(2, 2);
        Color[] pixels = new Color[4] {
            new Color(0.3f, 0.3f, 0.3f),
            new Color(0.25f, 0.25f, 0.25f),
            new Color(0.25f, 0.25f, 0.25f),
            new Color(0.3f, 0.3f, 0.3f)
        };
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private Texture2D CreateColorTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixels(new Color[] { color });
        tex.Apply();
        return tex;
    }

    private Rect CalculateTextureRect(Rect container, Sprite sprite)
    {
        if (sprite == null) return container;

        float aspectRatio = (float)sprite.rect.width / sprite.rect.height;
        float containerAspect = container.width / container.height;

        float width, height;

        if (aspectRatio > containerAspect)
        {
            // 宽度受限
            width = container.width * 0.9f;
            height = width / aspectRatio;
        }
        else
        {
            // 高度受限
            height = container.height * 0.9f;
            width = height * aspectRatio;
        }

        float x = container.x + (container.width - width) / 2;
        float y = container.y + (container.height - height) / 2;

        return new Rect(x, y, width, height);
    }

    // 自然排序比较器
    private class NaturalSortComparer : System.Collections.Generic.IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return EditorUtility.NaturalCompare(x, y);
        }
    }
}