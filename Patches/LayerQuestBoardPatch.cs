using QuestPicker.Config;

namespace QuestPicker.Patches
{
    public class LayerQuestBoardPatch
    {
        public static void LayerQuestBoardRefreshQuestPrefix(LayerQuestBoard __instance)
        {
            foreach (Chara chara in ELayer._map.charas)
            {
                if (chara.quest != null && !ELayer.game.quests.list.Contains(item: chara.quest) &&
                    !chara.IsPCParty && chara.memberType == FactionMemberType.Default &&
                    chara.quest.IsVisibleOnQuestBoard())
                {
                    var questIds = QuestPickerConfig.SelectedQuestIds;
                    
                    if (questIds.Count > 0 && !questIds.Contains(item: chara.quest.id))
                    {
                        int randomIndex = EClass.rnd(a: questIds.Count);

                        string selectedQuestId = questIds[index: randomIndex];

                        chara.quest = Quest.Create(_id: selectedQuestId, c: chara);
                    }
                }
            }
        }
    }
}