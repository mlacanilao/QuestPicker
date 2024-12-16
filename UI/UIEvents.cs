using System.Collections.Generic;
using System.Linq;

namespace QuestPicker.UI
{
    internal static class UIEvents
    {
        private static Dictionary<string, string> _reverseQuestTypeMappings;

        public static void InitializeMappings(Dictionary<string, string> questTypeMappings)
        {
            _reverseQuestTypeMappings = questTypeMappings.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }

        public static void OnToggleValueChanged(string contentId, bool isChecked)
        {
            if (!_reverseQuestTypeMappings.TryGetValue(contentId, out var questId))
            {
                return;
            }

            var selectedQuestIds = QuestPickerConfig.SelectedQuestIds;

            if (isChecked)
            {
                if (!selectedQuestIds.Contains(questId))
                    selectedQuestIds.Add(questId);
            }
            else
            {
                selectedQuestIds.Remove(questId);
            }

            QuestPickerConfig.UpdateSelectedQuestIds(selectedQuestIds);
        }
    }
}