using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// The Glyph Editor: a gallery of a glyph pack's characters and symbols, which splits to edit the glyph opened from
    /// it. Edits go straight into the pack, with undo, and every text and symbol drawn with the pack follows at once.
    /// </summary>
    internal sealed class GlyphEditorWindow : EditorWindow
    {
        private const string Title = "Glyph Editor";
        private const float EditorPaneWidth = 560f;
        private const float MinimumTileSize = 40f;
        private const float MaximumTileSize = 128f;
        private const string DefaultPreviewText = "The quick brown fox jumps over the lazy dog. 0123456789";

        [SerializeField] private WireframeGlyphPack _pack;
        [SerializeField] private bool _isSymbolOpen;
        [SerializeField] private int _openCharacter = -1;
        [SerializeField] private string _openKeyword;
        [SerializeField] private float _tileSize = 64f;
        [SerializeField] private bool _sortsSymbolsByName;
        [SerializeField] private string _previewText = DefaultPreviewText;
        [SerializeField] private bool _previewsMonospace;

        private GlyphPackEditing _editing;
        // Lists only the open pack, so the preview shows what the pack itself draws.
        private WireframeGlyphs _previewGlyphs;
        private bool _isReadOnly;
        private int _seenVersion = -1;

        private ObjectField _packField;
        private ToolbarButton _settingsButton;
        private ToolbarButton _generateButton;
        private ToolbarSearchField _search;
        private ToolbarToggle _sortToggle;
        private VisualElement _readOnlyBanner;
        private VisualElement _emptyState;
        private TwoPaneSplitView _split;
        private GlyphGallery _gallery;
        private GlyphEditorPane _pane;
        private GlyphPreviewStrip _previewStrip;

        /// <summary>The glyph open in the editor pane, or none while only the gallery shows.</summary>
        internal GlyphReference OpenGlyph { get; private set; } = GlyphReference.None;

        internal WireframeGlyphPack Pack
        {
            get => _pack;
        }

        [MenuItem("Window/Wireframes/Glyph Editor")]
        internal static GlyphEditorWindow ShowWindow()
        {
            GlyphEditorWindow window = GetWindow<GlyphEditorWindow>();
            window.titleContent = new GUIContent(Title);
            window.minSize = new Vector2(420f, 320f);
            window.Show();
            return window;
        }

        /// <summary>Shows the Glyph Editor with <paramref name="pack"/> in its gallery.</summary>
        internal static GlyphEditorWindow Open(WireframeGlyphPack pack)
        {
            GlyphEditorWindow window = ShowWindow();
            window.SetPack(pack);
            return window;
        }

#if UNITY_6000_3_OR_NEWER
        [OnOpenAsset]
        private static bool OnOpenAsset(EntityId entityId, int line)
        {
            return OpenIfPack(EditorUtility.EntityIdToObject(entityId));
        }
#else
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            return OpenIfPack(EditorUtility.InstanceIDToObject(instanceId));
        }
#endif

        [Shortcut("Wireframes/Glyph Editor/Save Pack", typeof(GlyphEditorWindow), KeyCode.S, ShortcutModifiers.Action)]
        private static void SavePack(ShortcutArguments arguments)
        {
            ((GlyphEditorWindow)arguments.context).SaveChanges();
        }

        /// <summary>Puts <paramref name="pack"/> in the gallery and closes the glyph open from the previous one.</summary>
        internal void SetPack(WireframeGlyphPack pack)
        {
            if (pack == _pack && _editing != null)
            {
                return;
            }
            _pack = pack;
            _packField?.SetValueWithoutNotify(pack);
            OpenGlyph = GlyphReference.None;
            _openCharacter = -1;
            _openKeyword = null;
            Load();
        }

        /// <summary>Opens <paramref name="glyph"/> in the editor pane, or closes the pane for none.</summary>
        internal void Open(GlyphReference glyph)
        {
            OpenGlyph = glyph.IsNone || _pack == null || !glyph.IsIn(_pack) ? GlyphReference.None : glyph;
            _isSymbolOpen = OpenGlyph.IsSymbol;
            _openCharacter = !OpenGlyph.IsNone && !OpenGlyph.IsSymbol ? _pack.Characters[OpenGlyph.Index].Character : -1;
            _openKeyword = !OpenGlyph.IsNone && OpenGlyph.IsSymbol ? _pack.Symbols[OpenGlyph.Index].Keyword : null;
            _gallery.Selected = OpenGlyph;
            if (OpenGlyph.IsNone)
            {
                _split.CollapseChild(1);
                return;
            }
            _split.UnCollapse();
            _pane.Show(_editing, OpenGlyph, _isReadOnly);
        }

        public override void SaveChanges()
        {
            if (_pack != null)
            {
                AssetDatabase.SaveAssetIfDirty(_pack);
            }
            base.SaveChanges();
        }

        public override void DiscardChanges()
        {
            // Reading the pack from disk again drops what wasn't saved, in every scene that draws with it too.
            if (_pack != null && EditorUtility.IsDirty(_pack))
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_pack), ImportAssetOptions.ForceUpdate);
            }
            base.DiscardChanges();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            if (GlyphAssets.StyleSheet != null)
            {
                root.styleSheets.Add(GlyphAssets.StyleSheet);
            }
            root.AddToClassList("glyph-editor");
            root.Add(CreateToolbar());

            _readOnlyBanner = new VisualElement();
            _readOnlyBanner.AddToClassList("glyph-editor__banner");
            _readOnlyBanner.Add(new Label(
                "This pack is part of an installed package, so it can't be changed. Duplicate it to edit a copy."));
            _readOnlyBanner.Add(new Button(DuplicateToAssets) { text = "Duplicate to Assets" });
            root.Add(_readOnlyBanner);

            _emptyState = new VisualElement();
            _emptyState.AddToClassList("glyph-editor__empty");
            _emptyState.Add(new Label("Open a glyph pack: double-click one in the Project window, or pick one above."));
            _emptyState.Add(new Button(CreatePack) { text = "New Glyph Pack…" });
            root.Add(_emptyState);

            _gallery = new GlyphGallery { TileSize = _tileSize, SortsSymbolsByName = _sortsSymbolsByName };
            _gallery.GlyphClicked += Open;
            _gallery.AddCharacterClicked += AskForNewCharacter;
            _gallery.AddMissingAsciiClicked += () => Edit(() => _editing.AddMissingAscii());
            _gallery.AddSymbolClicked += AskForNewSymbol;
            _gallery.SymbolMoved += (from, index) => Edit(() => _editing.MoveSymbol(from, index));
            _gallery.TileMenuBuilding += BuildTileMenu;

            _pane = new GlyphEditorPane();
            _pane.CloseClicked += () => Open(GlyphReference.None);
            _pane.Edited += OnGlyphEdited;
            _pane.GlyphMoved += glyph =>
            {
                Open(glyph);
                Refresh();
            };

            _split = new TwoPaneSplitView(1, EditorPaneWidth, TwoPaneSplitViewOrientation.Horizontal);
            _split.AddToClassList("glyph-editor__split");
            _split.Add(_gallery);
            _split.Add(_pane);
            root.Add(_split);

            _previewStrip = new GlyphPreviewStrip(_previewText, _previewsMonospace);
            _previewStrip.TextChanged += text => _previewText = text;
            _previewStrip.MonospaceChanged += isMonospace => _previewsMonospace = isMonospace;
            root.Add(_previewStrip);

            root.RegisterCallback<KeyDownEvent>(OnKeyDown);
            Load();
        }

        private VisualElement CreateToolbar()
        {
            Toolbar toolbar = new();
            _packField = new ObjectField { objectType = typeof(WireframeGlyphPack), allowSceneObjects = false, value = _pack };
            _packField.AddToClassList("glyph-editor__pack-field");
            _packField.RegisterValueChangedCallback(change => SetPack(change.newValue as WireframeGlyphPack));
            toolbar.Add(_packField);

            _settingsButton = new ToolbarButton(() => GlyphPackSettingsPopup.Show(_settingsButton, _pack, _isReadOnly))
            {
                text = "Pack Settings", tooltip = "The pack's enum, space width, guides and grid."
            };
            toolbar.Add(_settingsButton);
            _generateButton = new ToolbarButton(GenerateEnum) { text = "Generate Enum" };
            toolbar.Add(_generateButton);

            toolbar.Add(new ToolbarSpacer { flex = true });
            _search = new ToolbarSearchField();
            _search.AddToClassList("glyph-editor__search");
            _search.RegisterValueChangedCallback(change => _gallery.Filter = change.newValue);
            toolbar.Add(_search);
            _sortToggle = new ToolbarToggle { text = "A–Z", tooltip = "List symbols by keyword instead of pack order.", value = _sortsSymbolsByName };
            _sortToggle.RegisterValueChangedCallback(change =>
            {
                _sortsSymbolsByName = change.newValue;
                _gallery.SortsSymbolsByName = change.newValue;
            });
            toolbar.Add(_sortToggle);
            Slider tileSize = new(MinimumTileSize, MaximumTileSize) { value = _tileSize, tooltip = "Size of the tiles." };
            tileSize.AddToClassList("glyph-editor__tile-size");
            tileSize.RegisterValueChangedCallback(change =>
            {
                _tileSize = change.newValue;
                _gallery.TileSize = change.newValue;
            });
            toolbar.Add(tileSize);
            return toolbar;
        }

        /// <summary>Starts editing <see cref="_pack"/> and shows it, opening the glyph that was open before a reload.</summary>
        private void Load()
        {
            if (_gallery == null)
            {
                return;
            }
            _editing = _pack != null ? new GlyphPackEditing(_pack) : null;
            _isReadOnly = _pack != null && GlyphAssets.IsReadOnly(_pack);
            SetShown(_emptyState, _pack == null);
            SetShown(_split, _pack != null);
            SetShown(_previewStrip, _pack != null);
            _previewGlyphs.SetPacks(_pack);
            _previewStrip.Show(_previewGlyphs);
            _gallery.Show(_pack, _isReadOnly);
            Open(FindOpenGlyph());
            Refresh();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += Refresh;
            _previewGlyphs = CreateInstance<WireframeGlyphs>();
            _previewGlyphs.hideFlags = HideFlags.HideAndDontSave;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Refresh;
            DestroyImmediate(_previewGlyphs);
        }

        private void OnInspectorUpdate()
        {
            // The pack can change outside the window too: in the Inspector, by a reimport, or in another window.
            if (_seenVersion != GlyphEdits.Version)
            {
                Refresh();
            }
            hasUnsavedChanges = _pack != null && EditorUtility.IsDirty(_pack);
            saveChangesMessage = _pack != null ? $"The glyph pack '{_pack.name}' has changes that aren't saved." : null;
        }

        /// <summary>Brings everything in line with the pack after an edit, undo or change made elsewhere.</summary>
        private void Refresh()
        {
            _seenVersion = GlyphEdits.Version;
            if (_gallery == null)
            {
                return;
            }
            if (_pack == null)
            {
                if (_editing != null)
                {
                    Load();
                }
                return;
            }
            _editing?.Serialized.Update();
            _gallery.Refresh();
            GlyphReference open = FindOpenGlyph();
            if (!open.Equals(OpenGlyph))
            {
                Open(open);
            }
            else if (!open.IsNone)
            {
                _pane.Refresh();
            }
            SetShown(_readOnlyBanner, _isReadOnly);
            _previewStrip.Refresh();
            RefreshGenerateButton();
        }

        private void RefreshGenerateButton()
        {
            bool hasSymbols = _pack.Symbols.Length > 0 || !string.IsNullOrEmpty(_pack.EnumSettings.ScriptGuid);
            _generateButton.SetEnabled(hasSymbols && !_isReadOnly);
            bool isOutOfDate = hasSymbols && !_isReadOnly && GlyphEnumGenerator.IsOutOfDate(_pack);
            _generateButton.EnableInClassList("glyph-editor__generate--out-of-date", isOutOfDate);
            _generateButton.tooltip = isOutOfDate
                ? "The symbols changed since the enum was generated, so the enum is out of date."
                : "Writes the pack's symbol enum, which names each symbol in code.";
        }

        /// <summary>Finds the glyph that was open by its character or keyword, which edits and undo can move.</summary>
        private GlyphReference FindOpenGlyph()
        {
            if (_editing == null)
            {
                return GlyphReference.None;
            }
            if (_isSymbolOpen)
            {
                return string.IsNullOrEmpty(_openKeyword) ? GlyphReference.None : _editing.FindSymbol(_openKeyword);
            }
            return _openCharacter < 0 ? GlyphReference.None : _editing.FindCharacter(_openCharacter);
        }

        /// <summary>Runs an edit, then refreshes, so the gallery and the pane show its result at once.</summary>
        private void Edit(Action edit)
        {
            if (_editing == null || _isReadOnly)
            {
                return;
            }
            edit();
            Refresh();
        }

        /// <summary>Follows an edit the pane made to the open glyph, which changes nothing outside its tile.</summary>
        private void OnGlyphEdited()
        {
            _seenVersion = GlyphEdits.Version;
            _gallery.RepaintGlyph(OpenGlyph);
            _pane.Refresh();
            _previewStrip.Refresh();
            hasUnsavedChanges = EditorUtility.IsDirty(_pack);
        }

        private void AskForNewCharacter(VisualElement anchor)
        {
            GlyphKeyPopup.Show(anchor, "Add a character, or its code", string.Empty, "Add", FindNewCharacterProblem, text =>
            {
                GlyphNames.Parse(text, out int character);
                GlyphReference added = GlyphReference.None;
                Edit(() => added = _editing.AddCharacter(character));
                Open(added);
            });
        }

        private void AskForNewSymbol(VisualElement anchor)
        {
            GlyphKeyPopup.Show(anchor, "Add a symbol named", string.Empty, "Add", FindNewKeywordProblem, keyword =>
            {
                GlyphReference added = GlyphReference.None;
                Edit(() => added = _editing.AddSymbol(keyword));
                Open(added);
            });
        }

        private void BuildTileMenu(GlyphReference glyph, VisualElement tile, DropdownMenu menu)
        {
            DropdownMenuAction.Status editable = _isReadOnly ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
            menu.AppendAction("Open", _ => Open(glyph));
            menu.AppendSeparator();
            if (glyph.IsSymbol)
            {
                string keyword = _pack.Symbols[glyph.Index].Keyword;
                menu.AppendAction("Rename…", _ => GlyphKeyPopup.Show(tile, "Rename the symbol to", keyword, "Rename",
                    text => text == keyword ? null : FindNewKeywordProblem(text),
                    text => Edit(() => _editing.Rename(glyph, text))), editable);
                menu.AppendAction("Duplicate…", _ => GlyphKeyPopup.Show(tile, "Name the copy", UniqueKeyword(keyword + "Copy"),
                    "Duplicate", FindNewKeywordProblem, text =>
                    {
                        GlyphReference copy = GlyphReference.None;
                        Edit(() => copy = _editing.DuplicateSymbol(glyph, text));
                        Open(copy);
                    }), editable);
            }
            else
            {
                menu.AppendAction("Change Character…", _ => GlyphKeyPopup.Show(tile, "Give the glyph to the character",
                    string.Empty, "Change", FindNewCharacterProblem, text =>
                    {
                        GlyphNames.Parse(text, out int character);
                        GlyphReference moved = GlyphReference.None;
                        Edit(() => moved = _editing.ChangeCharacter(glyph, character));
                        Open(moved);
                    }), editable);
                menu.AppendAction("Duplicate…", _ => GlyphKeyPopup.Show(tile, "Copy the glyph to the character",
                    string.Empty, "Duplicate", FindNewCharacterProblem, text =>
                    {
                        GlyphNames.Parse(text, out int character);
                        GlyphReference copy = GlyphReference.None;
                        Edit(() => copy = _editing.DuplicateCharacter(glyph, character));
                        Open(copy);
                    }), editable);
            }
            menu.AppendAction("Copy Strokes", _ => GlyphClipboard.Copy(glyph.StrokesIn(_pack)),
                glyph.StrokesIn(_pack).Length > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            menu.AppendAction("Paste Strokes", _ =>
            {
                if (GlyphClipboard.TryPaste(out GlyphStroke[] strokes))
                {
                    Edit(() => _editing.AddStrokes(glyph, strokes));
                }
            }, action => !_isReadOnly && GlyphClipboard.TryPaste(out _) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            menu.AppendSeparator();
            menu.AppendAction("Delete", _ =>
            {
                if (glyph.Equals(OpenGlyph))
                {
                    Open(GlyphReference.None);
                }
                Edit(() => _editing.Delete(glyph));
            }, editable);
        }

        private string FindNewCharacterProblem(string text)
        {
            string problem = GlyphNames.Parse(text, out int character);
            if (problem != null)
            {
                return problem;
            }
            return _editing.FindCharacter(character).IsNone ? null : $"The pack already has {GlyphNames.CharacterOf(character)}.";
        }

        private string FindNewKeywordProblem(string keyword)
        {
            string problem = SymbolKeys.FindProblem(keyword);
            if (problem != null)
            {
                return problem;
            }
            return _editing.FindSymbol(keyword).IsNone ? null : $"The pack already has {keyword}.";
        }

        private string UniqueKeyword(string keyword)
        {
            string unique = keyword;
            for (int number = 2; !_editing.FindSymbol(unique).IsNone; number++)
            {
                unique = keyword + number;
            }
            return unique;
        }

        private void GenerateEnum()
        {
            string problem = GlyphEnumGenerator.FindProblem(_pack);
            if (problem != null)
            {
                EditorUtility.DisplayDialog("Can't generate the enum", problem, "OK");
                return;
            }
            try
            {
                GlyphEnumGenerator.Write(_pack);
            }
            catch (Exception exception) when (exception is InvalidOperationException or System.IO.IOException
                                                  or UnauthorizedAccessException)
            {
                EditorUtility.DisplayDialog("Can't generate the enum", exception.Message, "OK");
            }
            RefreshGenerateButton();
        }

        private void DuplicateToAssets()
        {
            WireframeGlyphPack copy = GlyphAssets.DuplicateToAssets(_pack);
            if (copy != null)
            {
                EditorGUIUtility.PingObject(copy);
                SetPack(copy);
            }
        }

        private void CreatePack()
        {
            string path = EditorUtility.SaveFilePanelInProject("New Glyph Pack", "Glyph Pack", "asset", "Where to save the glyph pack.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            WireframeGlyphPack pack = CreateInstance<WireframeGlyphPack>();
            AssetDatabase.CreateAsset(pack, path);
            SetPack(pack);
        }

        private void OnKeyDown(KeyDownEvent keyEvent)
        {
            if (keyEvent.keyCode == KeyCode.Escape && !OpenGlyph.IsNone)
            {
                Open(GlyphReference.None);
                keyEvent.StopPropagation();
            }
        }

        private static bool OpenIfPack(Object asset)
        {
            if (asset is not WireframeGlyphPack pack)
            {
                return false;
            }
            Open(pack);
            return true;
        }

        private static void SetShown(VisualElement element, bool isShown)
        {
            element.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
