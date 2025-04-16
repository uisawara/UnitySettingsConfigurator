using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuildConfigEditor : EditorWindow
{
    private BuildConfig[] buildConfigs;
    private BuildConfig selectedConfig;

    private void OnEnable()
    {
        // `Assets/Settings/` フォルダ内の全ての BuildConfig をロード
        var settingsPath = "Assets/Settings/";
        var guids = AssetDatabase.FindAssets("t:BuildConfig", new[] { settingsPath });
        buildConfigs = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<BuildConfig>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();
    }

    private void OnGUI()
    {
        if (buildConfigs == null || buildConfigs.Length == 0)
        {
            EditorGUILayout.LabelField("No Build Config found in Assets/Settings/");
            return;
        }

        // ビルド設定の一覧を表示
        EditorGUILayout.LabelField("Select Build Config:");
        foreach (var config in buildConfigs)
        {
            if (GUILayout.Button(config.buildName))
            {
                // クリックされたらそのビルド設定を選択
                selectedConfig = config;
                Debug.Log($"Selected config: {selectedConfig.buildName}");
            }
        }

        if (selectedConfig != null)
        {
            // 選択されたビルド設定の情報を表示
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selected Build Config:");
            EditorGUILayout.LabelField("Build Name", selectedConfig.buildName);
            EditorGUILayout.LabelField("Build Target", selectedConfig.buildTarget.ToString());
            EditorGUILayout.LabelField("Scripting Define Symbols", string.Join(';', selectedConfig.scriptingDefineSymbols));
            EditorGUILayout.LabelField("Scripting Backend", selectedConfig.scriptingBackend.ToString());

            // 各LogTypeごとのStackTrace設定を表示
            EditorGUILayout.LabelField("Log StackTrace", selectedConfig.logStackTraceLogType.ToString());
            EditorGUILayout.LabelField("Error StackTrace", selectedConfig.errorStackTraceLogType.ToString());
            EditorGUILayout.LabelField("Exception StackTrace", selectedConfig.exceptionStackTraceLogType.ToString());
            EditorGUILayout.LabelField("Warning StackTrace", selectedConfig.warningStackTraceLogType.ToString());

            // Product Name と Bundle Identifier の表示
            EditorGUILayout.LabelField("Product Name", selectedConfig.productName);
            EditorGUILayout.LabelField("Bundle Identifier", selectedConfig.bundleIdentifier);

            // 現在のUnity Editor設定に反映するボタン
            if (GUILayout.Button("Apply to Current Editor Settings"))
            {
                ApplyToEditorSettings();
            }

            // ビルドボタン
            if (GUILayout.Button("Build with Selected Config"))
            {
                BuildWithSelectedConfig();
            }
        }
    }

    [MenuItem("Build/Build Configurator")]
    public static void ShowWindow()
    {
        GetWindow<BuildConfigEditor>("Build Configurator");
    }

    private void ApplyToEditorSettings()
    {
        if (selectedConfig == null)
        {
            Debug.LogError("No Build Config selected!");
            return;
        }

        // スクリプト定義シンボルを現在のエディタ設定に反映
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(selectedConfig.buildTarget),
            selectedConfig.scriptingDefineSymbols);
        Debug.Log($"Applied scripting define symbols: {selectedConfig.scriptingDefineSymbols}");

        // エディタのビルドターゲットを反映
        EditorUserBuildSettings.selectedBuildTargetGroup =
            BuildPipeline.GetBuildTargetGroup(selectedConfig.buildTarget);
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(selectedConfig.buildTarget),
            selectedConfig.buildTarget);
        Debug.Log($"Switched build target to: {selectedConfig.buildTarget}");

        // シーンの設定を現在のビルド設定に反映
        var scenePaths = selectedConfig.GetScenePaths();
        EditorBuildSettings.scenes = scenePaths
            .Select(scenePath => new EditorBuildSettingsScene(scenePath, true))
            .ToArray();
        Debug.Log("Applied scenes to build settings");

        // Scripting Backendを反映
        PlayerSettings.SetScriptingBackend(BuildPipeline.GetBuildTargetGroup(selectedConfig.buildTarget),
            selectedConfig.scriptingBackend);
        Debug.Log($"Applied scripting backend: {selectedConfig.scriptingBackend}");

        // LogTypeごとのStackTraceLogTypeを反映
        PlayerSettings.SetStackTraceLogType(LogType.Log, selectedConfig.logStackTraceLogType);
        PlayerSettings.SetStackTraceLogType(LogType.Error, selectedConfig.errorStackTraceLogType);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, selectedConfig.exceptionStackTraceLogType);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, selectedConfig.warningStackTraceLogType);
        Debug.Log("Applied stack trace log types");

        // Product Name と Bundle Identifier の設定を反映
        PlayerSettings.productName = selectedConfig.productName;
        PlayerSettings.SetApplicationIdentifier(BuildPipeline.GetBuildTargetGroup(selectedConfig.buildTarget),
            selectedConfig.bundleIdentifier);
        Debug.Log($"Applied product name: {selectedConfig.productName}");
        Debug.Log($"Applied bundle identifier: {selectedConfig.bundleIdentifier}");
        
        // 現在のシーンを保存確認
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 最初のシーンは Single ロード（新規開き）
            var firstScene = EditorSceneManager.OpenScene(scenePaths[0], OpenSceneMode.Single /* | (selectedConfig.sceneAssets[0].load? OpenSceneMode.AdditiveWithoutLoading : 0) */);

            // 残りは Additive（追加でロード）
            for (int i = 1; i < scenePaths.Length; i++)
            {
                EditorSceneManager.OpenScene(scenePaths[i], selectedConfig.sceneAssets[i].load? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading);
            }

            Debug.Log("Loaded scenes into Hierarchy");
        }
    }

    private void BuildWithSelectedConfig()
    {
        if (selectedConfig == null)
        {
            Debug.LogError("No Build Config selected!");
            return;
        }

        // シーンのパスを取得
        var scenePaths = selectedConfig.GetScenePaths();

        // スクリプト定義シンボルを設定
        var originalDefineSymbols =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(
                BuildPipeline.GetBuildTargetGroup(selectedConfig.buildTarget));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(selectedConfig.buildTarget),
            selectedConfig.scriptingDefineSymbols);

        try
        {
            // BuildPipelineを使用してビルドを実行
            var buildPath = $"Builds/{selectedConfig.buildName}";
            BuildPipeline.BuildPlayer(scenePaths, buildPath, selectedConfig.buildTarget, BuildOptions.None);
            Debug.Log($"Build completed with config: {selectedConfig.buildName}");
        }
        finally
        {
            // ビルド後に元の定義シンボルを復元
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                BuildPipeline.GetBuildTargetGroup(selectedConfig.buildTarget), originalDefineSymbols);
        }
    }
}