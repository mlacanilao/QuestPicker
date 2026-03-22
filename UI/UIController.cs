using EvilMask.Elin.ModOptions;
using System.IO;
using System.Reflection;
using BepInEx;
using EvilMask.Elin.ModOptions.UI;

namespace QuestPicker;

public static class UIController
{
    public static void RegisterUI()
    {
        var assemblyLocation = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location);
        var xmlPath = Path.Combine(path1: assemblyLocation, path2: "QuestPickerConfig.xml");
        QuestPickerConfig.InitializeXmlPath(xmlPath: xmlPath);

        var xlsxPath = Path.Combine(path1: assemblyLocation, path2: "translations.xlsx");
        QuestPickerConfig.InitializeTranslationXlsxPath(xlsxPath: xlsxPath);

        foreach (var obj in ModManager.ListPluginObject)
        {
            if (obj is BaseUnityPlugin plugin && plugin.Info.Metadata.GUID == ModInfo.ModOptionsGuid)
            {
                var controller = ModOptionController.Register(guid: ModInfo.Guid, tooptipId: "mod.tooltip");

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
                return;
            }
        }
    }

    private static void SetTranslations(ModOptionController controller)
    {
        foreach (var questId in QuestPickerConfig.AvailableQuestIds)
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
        if (EClass.sources.quests.map.TryGetValue(key: id, value: out var questRow))
        {
            return (questRow.name, questRow.name_JP);
        }

        return ("Unknown Quest", "ä¸æ˜Žãªã‚¯ã‚¨ã‚¹ãƒˆ");
    }

    private static void RegisterEvents(ModOptionController controller)
    {
        controller.OnBuildUI += builder =>
        {
            foreach (var questId in QuestPickerConfig.AvailableQuestIds)
            {
                var toggle = builder.GetPreBuild<OptToggle>(id: $"{questId}Toggle");
                toggle.Checked = QuestPickerConfig.IsQuestSelected(questId: questId);
                toggle.OnValueChanged += isChecked =>
                {
                    UIEvents.OnToggleValueChanged(questId: questId, isChecked: isChecked);
                };
            }
        };
    }
}
