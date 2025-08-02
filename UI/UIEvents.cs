using System.Collections.Generic;
using System.Linq;
using QuestPicker.Config;

namespace QuestPicker.UI
{
    internal static class UIEvents
    {
        private static Dictionary<string, string> _reverseQuestTypeMappings;

        public static void InitializeMappings(Dictionary<string, string> questTypeMappings)
        {
            _reverseQuestTypeMappings = questTypeMappings.ToDictionary(keySelector: kvp => kvp.Value, elementSelector: kvp => kvp.Key);
        }

        public static void OnToggleValueChanged(string contentId, bool isChecked)
        {
            if (!_reverseQuestTypeMappings.TryGetValue(key: contentId, value: out var questId))
            {
                return;
            }

            var selectedQuestIds = QuestPickerConfig.SelectedQuestIds;

            if (isChecked)
            {
                if (!selectedQuestIds.Contains(item: questId))
                    selectedQuestIds.Add(item: questId);
            }
            else
            {
                selectedQuestIds.Remove(item: questId);
            }

            QuestPickerConfig.UpdateSelectedQuestIds(selectedQuestIds: selectedQuestIds);
        }
    }
}