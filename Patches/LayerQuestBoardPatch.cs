using System;
using System.Collections.Generic;

namespace QuestPicker;

public class LayerQuestBoardPatch
{
    private const string NeedDestinationTag = "needDestZone";
    private static readonly HashSet<string> LoggedInvalidSelectedQuestIds = new HashSet<string>(comparer: StringComparer.Ordinal);

    public static void LayerQuestBoardRefreshQuestPrefix()
    {
        var selectedQuestIds = QuestPickerConfig.SelectedQuestIds;
        if (selectedQuestIds.Count < 1)
        {
            return;
        }

        bool hasEnoughDestinationZones = Quest.ListDeliver().Count >= 2;
        List<string> eligibleReplacementQuestIds = GetEligibleReplacementQuestIds(
            selectedQuestIds: selectedQuestIds,
            hasEnoughDestinationZones: hasEnoughDestinationZones
        );
        if (eligibleReplacementQuestIds.Count < 1)
        {
            return;
        }

        foreach (Chara chara in ELayer._map.charas)
        {
            Quest quest = chara.quest;
            if (CanReplaceBoardQuest(chara: chara, quest: quest))
            {
                int randomIndex = EClass.rnd(a: eligibleReplacementQuestIds.Count);
                string selectedQuestId = eligibleReplacementQuestIds[index: randomIndex];
                chara.quest = Quest.Create(_id: selectedQuestId, c: chara);
            }
        }
    }

    private static List<string> GetEligibleReplacementQuestIds(IReadOnlyList<string> selectedQuestIds, bool hasEnoughDestinationZones)
    {
        List<string> eligibleReplacementQuestIds = new List<string>(capacity: selectedQuestIds.Count);

        foreach (string selectedQuestId in selectedQuestIds)
        {
            if (TryGetQuestRow(questId: selectedQuestId, questRow: out SourceQuest.Row questRow) &&
                CanUseQuestRowAsReplacement(questRow: questRow, hasEnoughDestinationZones: hasEnoughDestinationZones))
            {
                eligibleReplacementQuestIds.Add(item: selectedQuestId);
            }
        }

        return eligibleReplacementQuestIds;
    }

    private static bool CanReplaceBoardQuest(Chara chara, Quest quest)
    {
        return quest != null &&
               !ELayer.game.quests.list.Contains(item: quest) &&
               !chara.IsPCParty &&
               chara.memberType == FactionMemberType.Default &&
               quest.IsVisibleOnQuestBoard() &&
               !QuestPickerConfig.IsQuestSelected(questId: quest.id);
    }

    private static bool TryGetQuestRow(string questId, out SourceQuest.Row questRow)
    {
        if (string.IsNullOrWhiteSpace(value: questId) ||
            !EClass.sources.quests.map.TryGetValue(key: questId, value: out questRow))
        {
            questRow = null!;

            if (!string.IsNullOrWhiteSpace(value: questId) && LoggedInvalidSelectedQuestIds.Add(item: questId))
            {
                Plugin.LogError(message: $"Invalid quest ID in selected quest list: {questId}");
            }

            return false;
        }

        return true;
    }

    private static bool CanUseQuestRowAsReplacement(SourceQuest.Row questRow, bool hasEnoughDestinationZones)
    {
        return !HasTag(tags: questRow.tags, tag: NeedDestinationTag) || hasEnoughDestinationZones;
    }

    private static bool HasTag(string[] tags, string tag)
    {
        if (tags == null)
        {
            return false;
        }

        foreach (string currentTag in tags)
        {
            if (string.Equals(a: currentTag, b: tag, comparisonType: StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
