using System;

namespace VisualizationDSA.Domain.Entities
{
    /// <summary>Nhiệm vụ (quest) định kỳ — ngày/tuần/tháng. Khớp FE QuestsView (/me/quests).</summary>
    public class Quest
    {
        public Guid Id { get; private set; }
        public string QuestKey { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        /// <summary>Daily | Weekly | Monthly</summary>
        public string Period { get; private set; } = "Daily";
        public string ConditionJson { get; private set; } = "{}";
        public string RewardJson { get; private set; } = "{}";
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Quest() { }

        public Quest(string questKey, string title, string description, string period, string conditionJson, string rewardJson, int sortOrder, bool isActive = true)
        {
            Id = Guid.NewGuid();
            QuestKey = questKey;
            Title = title;
            Description = description ?? string.Empty;
            Period = period;
            ConditionJson = conditionJson ?? "{}";
            RewardJson = rewardJson ?? "{}";
            SortOrder = sortOrder;
            IsActive = isActive;
        }
    }
}
