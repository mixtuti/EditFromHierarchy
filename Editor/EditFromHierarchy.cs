using UnityEditor;
using UnityEngine;

public class EditFromHierarchy
{
    private const int ICON_SIZE = 16;

    private static GameObject selectedGameObject;
    private static Component selectedComponent;
    private static Vector2 scrollPosition;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
    }

    private static void OnGUI(int instanceID, Rect selectionRect)
    {
        // instanceID をオブジェクト参照に変換
        var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null)
        {
            return;
        }

        // オブジェクトが所持しているコンポーネント一覧を取得
        var components = go.GetComponents<Component>();
        if (components.Length == 0)
        {
            return;
        }

        selectionRect.x = selectionRect.xMax - ICON_SIZE * components.Length;
        selectionRect.width = ICON_SIZE;

        foreach (var component in components)
        {
            // コンポーネントのアイコン画像を取得
            var texture2D = AssetPreview.GetMiniThumbnail(component);

            // アイコンを描画
            GUI.DrawTexture(selectionRect, texture2D);

            // クリックを検出
            if (Event.current.type == EventType.MouseDown && selectionRect.Contains(Event.current.mousePosition))
            {
                selectedGameObject = go;
                selectedComponent = component;

                // Componentの情報を編集するウィンドウを表示
                EditorWindow.GetWindow<ComponentEditorWindow>("Component Editor");
                Event.current.Use(); // イベントの消費
            }

            selectionRect.x += ICON_SIZE;
        }
    }

    private class ComponentEditorWindow : EditorWindow
    {
        private GUIStyle labelStyle;

        private void OnEnable()
        {
            labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.fontSize = 15; // フォントサイズを変更
        }

        private void OnGUI()
        {
            if (selectedGameObject == null || selectedComponent == null)
            {
                EditorGUILayout.LabelField("No Component selected");
                return;
            }

            // EditorStyles.labelを初期化
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.fontSize = 20; // フォントサイズを変更
            }

            // アイコン画像を取得
            Texture2D componentIcon = AssetPreview.GetMiniThumbnail(selectedComponent);

            // Componentのアイコンと名前を表示
            GUILayout.BeginHorizontal();
            GUILayout.Label(componentIcon, GUILayout.Width(ICON_SIZE + 10), GUILayout.Height(ICON_SIZE + 10));
            GUILayout.Space(5); // 文字と線の間に5ピクセルの隙間を設ける
            EditorGUILayout.LabelField(selectedComponent.GetType().Name, labelStyle);
            GUILayout.EndHorizontal();

            // ホワイトの背景色を持つボックスを描画
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = EditorGUIUtility.whiteTexture;
            GUILayout.Box("", boxStyle, GUILayout.Width(this.position.width), GUILayout.Height(1));


            // スクロールビューを開始
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(5); // 文字と線の間に5ピクセルの隙間を設ける
            
            // Componentのプロパティを編集
            var serializedObject = new SerializedObject(selectedComponent);
            serializedObject.Update();
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                EditorGUILayout.PropertyField(iterator, true);
            }
            serializedObject.ApplyModifiedProperties();

            // スクロールビューを終了
            EditorGUILayout.EndScrollView();
        }
    }
}
