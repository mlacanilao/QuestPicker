using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;

namespace QuestPicker
{
    /// <summary>
    /// Configuration handler for QuestPicker.
    /// Manages both BepInEx configuration and XML layout paths.
    /// </summary>
    internal static class QuestPickerConfig
    {
        private static ConfigEntry<string> _selectedQuestIdsEntry;

        /// <summary>
        /// Gets the list of selected quest IDs from the configuration.
        /// </summary>
        internal static List<string> SelectedQuestIds =>
            _selectedQuestIdsEntry?.Value.Split(separator: ',')
                                        .Select(selector: id => id.Trim())
                                        .Where(predicate: id => !string.IsNullOrEmpty(value: id))
                                        .ToList() ?? new List<string>();
        
        internal static void UpdateSelectedQuestIds(List<string> selectedQuestIds)
        {
            _selectedQuestIdsEntry.Value = string.Join(separator: ",", values: selectedQuestIds);
        }

        /// <summary>
        /// Path to the XML layout configuration file.
        /// </summary>
        public static string XmlPath { get; private set; }
        
        /// <summary>
        /// Path to the XLSX translations file.
        /// </summary>
        public static string TranslationXlsxPath { get; private set; }

        /// <summary>
        /// Loads BepInEx configuration values.
        /// </summary>
        /// <param name="config">BepInEx ConfigFile instance.</param>
        internal static void LoadConfig(ConfigFile config)
        {
            _selectedQuestIdsEntry = config.Bind(
                section: ModInfo.Name,
                key: "SelectedQuestIds",
                defaultValue: "deliver,supply,supplyBulk_resource,supplyBulk_junk,hunt,huntRace,defenseGame,defenseGame2,harvest,music,subdue,meal_meat,meal_fish,meal_vegi,meal_fruit,meal_bread,meal_noodle,meal_cake,meal_cookie,meal_egg,meal_rice,meal_soup,meal_cat,escort,escort2,escort3",
                description: "Comma-separated list of quest IDs to use for rerolling quests on the quest board.\n" +
                             "クエストボードでクエストをリロールするために使用するクエストIDのカンマ区切りリストです。\n" +
                             "用于在任务板上重新刷任务的任务ID的逗号分隔列表。"
            );
        }

        /// <summary>
        /// Initializes the path to the XML layout configuration file.
        /// </summary>
        /// <param name="xmlPath">Path to the XML layout file.</param>
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
    }
}
