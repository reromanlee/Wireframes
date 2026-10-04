using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using PackageSource = UnityEditor.PackageManager.PackageSource;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>The Glyph Editor's dealings with assets: which ones it can edit, copies to edit, and its stylesheet.</summary>
    internal static class GlyphAssets
    {
        private const string StyleSheetPath = "Packages/com.reromanlee.wireframes/Editor/Glyphs/GlyphEditor.uss";

        private static StyleSheet _styleSheet;

        /// <summary>The Glyph Editor's stylesheet, or null if it's missing, which leaves the editor unstyled but working.</summary>
        internal static StyleSheet StyleSheet
        {
            get
            {
                if (_styleSheet == null)
                {
                    _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
                }
                return _styleSheet;
            }
        }

        /// <summary>
        /// True when <paramref name="asset"/> can't be changed: it's in a package installed from a registry, git or a
        /// tarball, which Unity keeps read-only, or version control won't let it be edited.
        /// </summary>
        internal static bool IsReadOnly(Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            PackageInfo package = PackageInfo.FindForAssetPath(path);
            if (package != null && package.source != PackageSource.Embedded && package.source != PackageSource.Local)
            {
                return true;
            }
            return !AssetDatabase.IsOpenForEdit(path, StatusQueryOptions.UseCachedIfPossible);
        }

        /// <summary>
        /// Copies <paramref name="pack"/> into the Assets folder as "My" followed by its name, to edit it there. The copy
        /// forgets the original's enum settings, so generating its enum never clashes with the original's.
        /// </summary>
        internal static WireframeGlyphPack DuplicateToAssets(WireframeGlyphPack pack)
        {
            string source = AssetDatabase.GetAssetPath(pack);
            string target = AssetDatabase.GenerateUniqueAssetPath($"Assets/My {Path.GetFileName(source)}");
            if (!AssetDatabase.CopyAsset(source, target))
            {
                return null;
            }
            WireframeGlyphPack copy = AssetDatabase.LoadAssetAtPath<WireframeGlyphPack>(target);
            SerializedObject serialized = new(copy);
            SerializedProperty settings = serialized.FindProperty(WireframeGlyphPack.EnumSettingsField);
            settings.FindPropertyRelative(GlyphEnumSettings.NameField).stringValue = string.Empty;
            settings.FindPropertyRelative(GlyphEnumSettings.NamespaceField).stringValue = string.Empty;
            settings.FindPropertyRelative(GlyphEnumSettings.ScriptGuidField).stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(copy);
            return copy;
        }
    }
}
