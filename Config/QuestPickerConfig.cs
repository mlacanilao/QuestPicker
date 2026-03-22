using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;

namespace QuestPicker;

internal static class QuestPickerConfig
{
    private static readonly string[] AvailableQuestIdsInternal =
    {
        "deliver",
        "supply",
        "supplyBulk_resource",
        "supplyBulk_junk",
        "hunt",
        "huntRace",
        "defenseGame",
        "defenseGame2",
        "harvest",
        "music",
        "subdue",
        "meal_meat",
        "meal_fish",
        "meal_vegi",
        "meal_fruit",
        "meal_bread",
        "meal_noodle",
        "meal_cake",
        "meal_cookie",
        "meal_egg",
        "meal_rice",
        "meal_soup",
        "meal_cat",
        "escort",
        "escort2",
        "escort3"
    };

    private static ConfigEntry<string>? _selectedQuestIdsEntry;
    private static readonly List<string> _selectedQuestIds = new List<string>();
    private static readonly HashSet<string> _selectedQuestIdSet = new HashSet<string>(comparer: StringComparer.Ordinal);

    internal static IReadOnlyList<string> AvailableQuestIds => AvailableQuestIdsInternal;
    internal static IReadOnlyList<string> SelectedQuestIds => _selectedQuestIds;

    internal static bool IsQuestSelected(string questId)
    {
        return !string.IsNullOrEmpty(value: questId) && _selectedQuestIdSet.Contains(item: questId);
    }

    internal static void UpdateSelectedQuestIds(IEnumerable<string> selectedQuestIds)
    {
        if (_selectedQuestIdsEntry == null)
        {
            return;
        }

        string normalizedValue = NormalizeSelectedQuestIds(selectedQuestIds: selectedQuestIds);
        if (_selectedQuestIdsEntry.Value != normalizedValue)
        {
            _selectedQuestIdsEntry.Value = normalizedValue;
        }
        else
        {
            RefreshSelectedQuestIdsCache(selectedQuestIdsValue: normalizedValue);
        }
    }

    internal static void SetQuestSelected(string questId, bool isSelected)
    {
        if (string.IsNullOrWhiteSpace(value: questId))
        {
            return;
        }

        if (isSelected)
        {
            if (_selectedQuestIdSet.Contains(item: questId))
            {
                return;
            }

            List<string> updatedQuestIds = new List<string>(collection: _selectedQuestIds);
            updatedQuestIds.Add(item: questId);
            UpdateSelectedQuestIds(selectedQuestIds: updatedQuestIds);
            return;
        }

        if (!_selectedQuestIdSet.Contains(item: questId))
        {
            return;
        }

        List<string> filteredQuestIds = new List<string>(capacity: _selectedQuestIds.Count);
        foreach (string selectedQuestId in _selectedQuestIds)
        {
            if (!string.Equals(a: selectedQuestId, b: questId, comparisonType: StringComparison.Ordinal))
            {
                filteredQuestIds.Add(item: selectedQuestId);
            }
        }

        UpdateSelectedQuestIds(selectedQuestIds: filteredQuestIds);
    }

    public static string XmlPath { get; private set; } = string.Empty;

    public static string TranslationXlsxPath { get; private set; } = string.Empty;

    internal static void LoadConfig(ConfigFile config)
    {
        if (_selectedQuestIdsEntry != null)
        {
            _selectedQuestIdsEntry.SettingChanged -= OnSelectedQuestIdsChanged;
        }

        _selectedQuestIdsEntry = config.Bind(
            section: ModInfo.Name,
            key: "SelectedQuestIds",
            defaultValue: string.Join(separator: ",", value: AvailableQuestIdsInternal),
            description: "Comma-separated list of quest IDs to use for rerolling quests on the quest board.\n" +
                         "クエストボードでクエストをリロールするために使用するクエストIDのカンマ区切りリストです。\n" +
                         "用于在任务板上重新刷任务的任务ID的逗号分隔列表。"
        );

        _selectedQuestIdsEntry.SettingChanged += OnSelectedQuestIdsChanged;
        RefreshSelectedQuestIdsCache(selectedQuestIdsValue: _selectedQuestIdsEntry.Value);
    }

    public static void InitializeXmlPath(string xmlPath)
    {
        if (File.Exists(path: xmlPath))
        {
            XmlPath = xmlPath;
        }
        else
        {
            XmlPath = string.Empty;
        }
    }

    public static void InitializeTranslationXlsxPath(string xlsxPath)
    {
        if (File.Exists(path: xlsxPath))
        {
            TranslationXlsxPath = xlsxPath;
        }
        else
        {
            TranslationXlsxPath = string.Empty;
        }
    }

    private static void OnSelectedQuestIdsChanged(object sender, EventArgs args)
    {
        RefreshSelectedQuestIdsCache(selectedQuestIdsValue: _selectedQuestIdsEntry?.Value);
    }

    private static string NormalizeSelectedQuestIds(IEnumerable<string> selectedQuestIds)
    {
        List<string> normalizedQuestIds = new List<string>();
        HashSet<string> seenQuestIds = new HashSet<string>(comparer: StringComparer.Ordinal);

        if (selectedQuestIds != null)
        {
            foreach (string questId in selectedQuestIds)
            {
                string normalizedQuestId = NormalizeQuestId(questId: questId);
                if (!string.IsNullOrEmpty(value: normalizedQuestId) && seenQuestIds.Add(item: normalizedQuestId))
                {
                    normalizedQuestIds.Add(item: normalizedQuestId);
                }
            }
        }

        return string.Join(separator: ",", values: normalizedQuestIds);
    }

    private static void RefreshSelectedQuestIdsCache(string selectedQuestIdsValue)
    {
        _selectedQuestIds.Clear();
        _selectedQuestIdSet.Clear();

        if (string.IsNullOrWhiteSpace(value: selectedQuestIdsValue))
        {
            return;
        }

        string[] questIds = selectedQuestIdsValue.Split(separator: new[] { ',' }, options: StringSplitOptions.RemoveEmptyEntries);
        foreach (string questId in questIds)
        {
            string normalizedQuestId = NormalizeQuestId(questId: questId);
            if (!string.IsNullOrEmpty(value: normalizedQuestId) && _selectedQuestIdSet.Add(item: normalizedQuestId))
            {
                _selectedQuestIds.Add(item: normalizedQuestId);
            }
        }
    }

    private static string NormalizeQuestId(string questId)
    {
        return string.IsNullOrWhiteSpace(value: questId) ? string.Empty : questId.Trim();
    }
}
