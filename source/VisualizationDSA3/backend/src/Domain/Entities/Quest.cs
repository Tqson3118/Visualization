using System;
using System.Collections.Generic;

namespace VisualizationDSA.Domain.Entities
{
    public class Quest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string QuestKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Period { get; set; } = "Daily";
        public string? ConditionJson { get; set; }
        public string? RewardJson { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<UserQuest> UserQuests { get; set; } = new List<UserQuest>();
    }
}
