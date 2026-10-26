using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RedMoonGames.Editor
{
    public static class ProjectSetup
    {
        public const string EntryScene = "Assets/RedMoonGames/Scenes/Spillerummet.unity";

        [MenuItem("RedMoonGames/Open Spillerummet")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(EntryScene);
        }

        [MenuItem("RedMoonGames/Configure mobile project")]
        public static void Configure()
        {
            PlayerSettings.companyName = "Red Moon Games";
            PlayerSettings.productName = "Rød Måne · Spillerummet";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "dk.redmaane.games");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "dk.redmaane.games");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(EntryScene, true) }
                .Concat(EditorBuildSettings.scenes.Where(scene => scene.path != EntryScene)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("RedMoonGames: portrait mobile settings and entry scene configured. Use your own application identifier before store release.");
        }

        [MenuItem("RedMoonGames/Validate project")]
        public static void Validate()
        {
            foreach (var name in new[] { "drop", "moon", "flower", "tampon", "pad", "cup", "star", "cat", "crown" })
                if (Resources.Load<Texture2D>("Icons/" + name) == null) throw new InvalidOperationException("Missing icon: " + name);
            if (Resources.Load<TextAsset>("Tips") == null) throw new InvalidOperationException("Tips.json is missing.");
            if (Resources.Load<UnityEngine.UIElements.ThemeStyleSheet>("RuntimeTheme") == null) throw new InvalidOperationException("RuntimeTheme.tss is missing.");
            if (!File.Exists(EntryScene)) throw new InvalidOperationException("Entry scene is missing.");
            if (Application.unityVersion != "6000.6.4f1") Debug.LogWarning("Requested editor is 6000.6.4f1; current editor is " + Application.unityVersion);
            Debug.Log("RedMoonGames project resources validated. Enter Play mode to test the UI.");
        }

        [MenuItem("RedMoonGames/Build/Android development APK")]
        public static void Android() => Build(BuildTarget.Android, BuildTargetGroup.Android, "Builds/Android/RedMoonGames.apk");

        [MenuItem("RedMoonGames/Build/iOS Xcode project")]
        public static void IOS() => Build(BuildTarget.iOS, BuildTargetGroup.iOS, "Builds/iOS");

        static void Build(BuildTarget target, BuildTargetGroup group, string destination)
        {
            Validate();
            if (!BuildPipeline.IsBuildTargetSupported(group, target)) throw new InvalidOperationException("Install the " + target + " build module through Unity Hub first.");
            Configure();
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(group, target)) throw new InvalidOperationException("Could not select " + target);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { EntryScene }, locationPathName = destination, target = target, options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Build failed: " + report.summary.result);
            Debug.Log("Development build saved to " + destination);
        }
    }
}
