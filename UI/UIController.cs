using EvilMask.Elin.ModOptions;
using EvilMask.Elin.ModOptions.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QuestPicker;

public static class UIController
{
    private const string RandomQuestGroupId = "random";
    private const string UnknownQuestGroupKey = "unknown";

    public static void RegisterUI()
    {
        var assemblyLocation = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var xmlPath = Path.Combine(path1: assemblyLocation, path2: "QuestPickerConfig.xml");
        QuestPickerConfig.InitializeXmlPath(xmlPath: xmlPath);

        var xlsxPath = Path.Combine(path1: assemblyLocation, path2: "translations.xlsx");
        QuestPickerConfig.InitializeTranslationXlsxPath(xlsxPath: xlsxPath);
        var controller = ModOptionController.Register(guid: ModInfo.Guid, tooptipId: "mod.tooltip");
        if (controller == null)
        {
            QuestPicker.LogError(message: "Failed to register Mod Options controller.");
            return;
        }

        if (File.Exists(path: QuestPickerConfig.XmlPath))
        {
            controller.SetPreBuildWithXml(xml: File.ReadAllText(path: QuestPickerConfig.XmlPath));
        }
        else
        {
            QuestPicker.LogError(message: $"Mod Options XML not found: {xmlPath}");
        }

        if (File.Exists(path: QuestPickerConfig.TranslationXlsxPath))
        {
            controller.SetTranslationsFromXslx(path: QuestPickerConfig.TranslationXlsxPath);
        }
        else
        {
            QuestPicker.LogError(message: $"Mod Options translations not found: {xlsxPath}");
        }

        RegisterEvents(controller: controller);
    }

    private static void SetTranslations(ModOptionController controller)
    {
        foreach (var questId in QuestPickerConfig.AvailableQuestIds)
        {
            var (name, nameJP, nameCN) = GetQuestNames(id: questId);
            controller.SetTranslation(id: $"{questId}.toggle.tooltip",
                en: name,
                jp: nameJP,
                cn: nameCN);
        }
    }

    public static (string name, string nameJP, string nameCN) GetQuestNames(string id)
    {
        if (EClass.sources.quests.map.TryGetValue(key: id, value: out var questRow))
        {
            string localizedName = GetLocalizedQuestName(questRow: questRow);
            return (name: questRow.name, nameJP: questRow.name_JP, nameCN: localizedName);
        }

        return (name: "Unknown Quest", nameJP: "Unknown Quest", nameCN: "Unknown Quest");
    }

    private static string GetLocalizedQuestName(SourceQuest.Row questRow)
    {
        if (!string.IsNullOrWhiteSpace(value: questRow.name_L))
        {
            return questRow.name_L;
        }

        string localizedName = questRow.GetText(id: "name", returnNull: false);
        if (!string.IsNullOrWhiteSpace(value: localizedName))
        {
            return localizedName;
        }

        return questRow.name;
    }

    private static void RegisterEvents(ModOptionController controller)
    {
        controller.OnBuildUI += builder =>
        {
            SetTranslations(controller: controller);
            foreach (var questId in QuestPickerConfig.AvailableQuestIds)
            {
                var toggle = GetRequiredPreBuild<OptToggle>(builder: builder, id: $"{questId}Toggle");
                if (toggle == null)
                {
                    continue;
                }

                // Prebuilt XML toggles resolve tooltip IDs before OnBuildUI, so refresh the
                // live tooltip after injecting runtime quest-name translations.
                RefreshToggleTooltip(toggle: toggle, controller: controller, questId: questId);
                toggle.Checked = QuestPickerConfig.IsQuestSelected(questId: questId);
                toggle.OnValueChanged += isChecked =>
                {
                    UIEvents.OnToggleValueChanged(questId: questId, isChecked: isChecked);
                };
            }

            AddDiscoveredQuestSection(builder: builder);
        };
    }

    private static void AddDiscoveredQuestSection(OptionUIBuilder builder)
    {
        List<DiscoveredQuestGroup> discoveredQuestGroups = GetDiscoveredQuestGroups();
        if (discoveredQuestGroups.Count < 1)
        {
            return;
        }

        foreach (DiscoveredQuestGroup discoveredQuestGroup in discoveredQuestGroups)
        {
            OptVLayout section = builder.Root.AddVLayoutWithBorder(title: discoveredQuestGroup.Title);
            OptHLayout columns = section.AddHLayout();
            OptVLayout leftColumn = columns.AddVLayout(height: null);
            OptVLayout rightColumn = columns.AddVLayout(height: null);
            leftColumn.PrefferedWidth = 1f;
            rightColumn.PrefferedWidth = 1f;

            for (int index = 0; index < discoveredQuestGroup.QuestRows.Count; index++)
            {
                SourceQuest.Row questRow = discoveredQuestGroup.QuestRows[index: index];
                OptVLayout column = index % 2 == 0 ? leftColumn : rightColumn;
                AddDiscoveredQuestToggle(parent: column, questRow: questRow);
            }
        }
    }

    private static void AddDiscoveredQuestToggle(OptLayout parent, SourceQuest.Row questRow)
    {
        string displayName = GetQuestDisplayName(questRow: questRow);
        string tooltipText = GetLocalizedQuestName(questRow: questRow);

        OptToggle toggle = parent.AddToggle(text: displayName,
            isChecked: QuestPickerConfig.IsQuestSelected(questId: questRow.id),
            tooltip: tooltipText);
        toggle.OnValueChanged += isChecked =>
        {
            UIEvents.OnToggleValueChanged(questId: questRow.id, isChecked: isChecked);
        };
    }

    private static List<DiscoveredQuestGroup> GetDiscoveredQuestGroups()
    {
        Dictionary<string, DiscoveredQuestGroup> discoveredQuestGroups = new Dictionary<string, DiscoveredQuestGroup>(comparer: StringComparer.Ordinal);
        foreach (SourceQuest.Row questRow in EClass.sources.quests.map.Values)
        {
            if (CanShowDiscoveredQuest(questRow: questRow))
            {
                ModPackage? modPackage = ModUtil.FindSourceRowPackage(row: questRow);
                string groupKey = GetDiscoveredQuestGroupKey(modPackage: modPackage);
                if (discoveredQuestGroups.TryGetValue(key: groupKey, value: out DiscoveredQuestGroup? discoveredQuestGroup) == false)
                {
                    discoveredQuestGroup = new DiscoveredQuestGroup(
                        key: groupKey,
                        title: GetDiscoveredQuestGroupTitle(modPackage: modPackage));
                    discoveredQuestGroups[groupKey] = discoveredQuestGroup;
                }

                discoveredQuestGroup.QuestRows.Add(item: questRow);
            }
        }

        List<DiscoveredQuestGroup> result = discoveredQuestGroups.Values.ToList();
        foreach (DiscoveredQuestGroup discoveredQuestGroup in result)
        {
            discoveredQuestGroup.QuestRows.Sort(comparison: CompareDiscoveredQuestRows);
        }

        result.Sort(comparison: CompareDiscoveredQuestGroups);
        return result;
    }

    private static bool CanShowDiscoveredQuest(SourceQuest.Row questRow)
    {
        return questRow != null &&
               !string.IsNullOrWhiteSpace(value: questRow.id) &&
               string.Equals(a: questRow.group, b: RandomQuestGroupId, comparisonType: StringComparison.Ordinal) &&
               questRow.chance > 0 &&
               QuestPickerConfig.IsVanillaQuestId(questId: questRow.id) == false;
    }

    private static int CompareDiscoveredQuestRows(SourceQuest.Row a, SourceQuest.Row b)
    {
        int nameComparison = StringComparer.CurrentCultureIgnoreCase.Compare(
            x: GetQuestDisplayName(questRow: a),
            y: GetQuestDisplayName(questRow: b));
        if (nameComparison != 0)
        {
            return nameComparison;
        }

        return StringComparer.Ordinal.Compare(x: a.id, y: b.id);
    }

    private static int CompareDiscoveredQuestGroups(DiscoveredQuestGroup a, DiscoveredQuestGroup b)
    {
        bool aIsUnknown = string.Equals(a: a.Key, b: UnknownQuestGroupKey, comparisonType: StringComparison.Ordinal);
        bool bIsUnknown = string.Equals(a: b.Key, b: UnknownQuestGroupKey, comparisonType: StringComparison.Ordinal);
        if (aIsUnknown != bIsUnknown)
        {
            return aIsUnknown ? 1 : -1;
        }

        int titleComparison = StringComparer.CurrentCultureIgnoreCase.Compare(x: a.Title, y: b.Title);
        if (titleComparison != 0)
        {
            return titleComparison;
        }

        return StringComparer.Ordinal.Compare(x: a.Key, y: b.Key);
    }

    private static string GetDiscoveredQuestGroupKey(ModPackage? modPackage)
    {
        if (modPackage != null && string.IsNullOrWhiteSpace(value: modPackage.id) == false)
        {
            return modPackage.id;
        }

        return UnknownQuestGroupKey;
    }

    private static string GetDiscoveredQuestGroupTitle(ModPackage? modPackage)
    {
        if (modPackage != null && string.IsNullOrWhiteSpace(value: modPackage.title) == false)
        {
            return modPackage.title;
        }

        if (string.Equals(a: Lang.langCode, b: "JP", comparisonType: StringComparison.Ordinal))
        {
            return "\u305d\u306e\u4ed6\u306e\u30af\u30a8\u30b9\u30c8";
        }

        if (string.Equals(a: Lang.langCode, b: "CN", comparisonType: StringComparison.Ordinal))
        {
            return "\u5176\u4ed6\u6a21\u7ec4\u4efb\u52a1";
        }

        return "Other Mod Quests";
    }

    private static string GetQuestDisplayName(SourceQuest.Row questRow)
    {
        string localizedName = GetLocalizedQuestName(questRow: questRow);
        string? firstLine = localizedName
            .Split(separator: new[] { "\r\n", "\n", "\r" }, options: StringSplitOptions.None)
            .FirstOrDefault(predicate: line => string.IsNullOrWhiteSpace(value: line) == false)?
            .Trim();

        if (string.IsNullOrWhiteSpace(value: firstLine) == false)
        {
            return firstLine!;
        }

        return string.IsNullOrWhiteSpace(value: questRow.id) ? "Unknown Quest" : questRow.id;
    }

    private static void RefreshToggleTooltip(OptToggle toggle, ModOptionController controller, string questId)
    {
        string tooltipText = controller.Tr(contentId: $"{questId}.toggle.tooltip");
        if (string.IsNullOrWhiteSpace(value: tooltipText))
        {
            return;
        }

        toggle.Base.tooltip.text = tooltipText;
        toggle.Base.tooltip.enable = true;
    }

    private static T? GetRequiredPreBuild<T>(OptionUIBuilder builder, string id) where T : OptUIElement
    {
        T? element = builder.GetPreBuild<T>(id: id);
        if (element == null)
        {
            QuestPicker.LogError(message: $"Missing Mod Options prebuilt element: {id}");
        }

        return element;
    }

    private sealed class DiscoveredQuestGroup
    {
        internal DiscoveredQuestGroup(string key, string title)
        {
            Key = key;
            Title = title;
        }

        internal string Key { get; }

        internal string Title { get; }

        internal List<SourceQuest.Row> QuestRows { get; } = new List<SourceQuest.Row>();
    }
}
