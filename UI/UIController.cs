using System.Collections.Generic;
using EvilMask.Elin.ModOptions;
using System.IO;
using System.Reflection;
using BepInEx;
using EvilMask.Elin.ModOptions.UI;
using QuestPicker.Config;

namespace QuestPicker.UI
{
    public static class UIController
    {
        private static readonly Dictionary<string, string> ToggleMappings = new Dictionary<string, string>
        {
            { "deliver", "Deliver" },
            { "supply", "Supply" },
            { "supplyBulk_resource", "Supply Bulk Resource" },
            { "supplyBulk_junk", "Supply Bulk Junk" },
            { "hunt", "Hunt" },
            { "huntRace", "Hunt Race" },
            { "defenseGame", "Defense Game 1" },
            { "defenseGame2", "Defense Game 2" },
            { "harvest", "Harvest" },
            { "music", "Music" },
            { "subdue", "Subdue" },
            { "meal_meat", "Meat" },
            { "meal_fish", "Fish" },
            { "meal_vegi", "Vegetable" },
            { "meal_fruit", "Fruit" },
            { "meal_bread", "Bread" },
            { "meal_noodle", "Noodle" },
            { "meal_cake", "Cake" },
            { "meal_cookie", "Cookie" },
            { "meal_egg", "Egg" },
            { "meal_rice", "Rice" },
            { "meal_soup", "Soup" },
            { "meal_cat", "Category" },
            { "escort", "Escort 1" },
            { "escort2", "Escort 2" },
            { "escort3", "Escort 3" }
        };
        
        public static void RegisterUI()
        {
            UIEvents.InitializeMappings(ToggleMappings);
            
            foreach (var obj in ModManager.ListPluginObject)
            {
                if (obj is BaseUnityPlugin plugin && plugin.Info.Metadata.GUID == ModInfo.ModOptionsGuid)
                {
                    var controller = ModOptionController.Register(guid: ModInfo.Guid, tooptipId: "mod.tooltip");
                    
                    var assemblyLocation = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location);
                    var xmlPath = Path.Combine(path1: assemblyLocation, path2: "QuestPickerConfig.xml");
                    QuestPickerConfig.InitializeXmlPath(xmlPath: xmlPath);
            
                    var xlsxPath = Path.Combine(path1: assemblyLocation, path2: "translations.xlsx");
                    QuestPickerConfig.InitializeTranslationXlsxPath(xlsxPath: xlsxPath);
                    
                    if (File.Exists(path: QuestPickerConfig.XmlPath))
                    {
                        using (StreamReader sr = new StreamReader(path: QuestPickerConfig.XmlPath))
                            controller.SetPreBuildWithXml(xml: sr.ReadToEnd());
                    }
                    
                    if (File.Exists(path: QuestPickerConfig.TranslationXlsxPath))
                    {
                        controller.SetTranslationsFromXslx(path: QuestPickerConfig.TranslationXlsxPath);
                    }
                    
                    SetTranslations(controller: controller);
                    RegisterEvents(controller: controller);
                }
            }
        }

        private static void SetTranslations(ModOptionController controller)
        {
            foreach (var questId in ToggleMappings.Keys)
            {
                var (name, nameJP) = GetQuestNames(id: questId);

                controller.SetTranslation(id: $"{questId}.toggle.tooltip",
                    en: name,
                    jp: nameJP,
                    cn: name);
            }
        }
        
        public static (string name, string nameJP) GetQuestNames(string id)
        {
            if (EClass.sources.quests.map.TryGetValue(id, out var questRow))
            {
                return (questRow.name, questRow.name_JP);
            }

            return ("Unknown Quest", "不明なクエスト");
        }

        private static void RegisterEvents(ModOptionController controller)
        {
            controller.OnBuildUI += builder =>
            {
                var selectedQuestIds = QuestPickerConfig.SelectedQuestIds;

                foreach (var mapping in ToggleMappings)
                {
                    var questId = mapping.Key;
                    var contentId = mapping.Value;

                    var toggle = builder.GetPreBuild<OptToggle>(id: $"{questId}Toggle");
                    toggle.Checked = selectedQuestIds.Contains(item: questId);
                    toggle.OnValueChanged += isChecked =>
                    {
                        UIEvents.OnToggleValueChanged(contentId: contentId, isChecked: isChecked);
                    };
                }
            };
        }
    }
}