using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// How the Glyph Editor generates a pack's symbol enum: its name and namespace, empty for the defaults, and the GUID
    /// of the script it wrote last, so a moved script is found again. Only the Editor reads them.
    /// </summary>
    [Serializable]
    internal struct GlyphEnumSettings
    {
        internal const string NameField = nameof(_name);
        internal const string NamespaceField = nameof(_namespace);
        internal const string ScriptGuidField = nameof(_scriptGuid);

        [Tooltip("Name of the generated enum. Empty uses the pack's name.")]
        [SerializeField] private string _name;

        [Tooltip("Namespace of the generated enum. Empty uses the project's root namespace, if it has one.")]
        [SerializeField] private string _namespace;

        [SerializeField] private string _scriptGuid;

        internal GlyphEnumSettings(string name, string namespaceName, string scriptGuid)
        {
            _name = name;
            _namespace = namespaceName;
            _scriptGuid = scriptGuid;
        }

        internal string Name
        {
            get => _name ?? string.Empty;
        }

        internal string Namespace
        {
            get => _namespace ?? string.Empty;
        }

        internal string ScriptGuid
        {
            get => _scriptGuid ?? string.Empty;
        }
    }
}
