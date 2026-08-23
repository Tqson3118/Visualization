using System;

namespace VisualizationDSA.Domain.Entities
{
    public class UserQuest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid QuestId { get; set; }
        public Quest Quest { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int Progress { get; set; }
        public string Status { get; set; } = "InProgress";
        public DateTime? ClaimedAt { get; set; }
    }
}
