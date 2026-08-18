using System;

namespace VisualizationDSA.Domain.Entities
{
    /// <summary>Trạng thái quest của từng user.</summary>
    public class UserQuest
    {
        public Guid Id { get; private set; }
        public Guid QuestId { get; private set; }
        public Guid UserId { get; private set; }
        public int Progress { get; private set; }
        /// <summary>NotStarted | InProgress | Completed | Claimed</summary>
        public string Status { get; private set; } = "NotStarted";
        public DateTime? ClaimedAt { get; private set; }

        public virtual Quest Quest { get; private set; } = null!;
        public virtual User User { get; private set; } = null!;

        private UserQuest() { }

        public UserQuest(Guid questId, Guid userId, int progress, string status)
        {
            Id = Guid.NewGuid();
            QuestId = questId;
            UserId = userId;
            Progress = Math.Max(0, progress);
            Status = status;
        }

        public void SetProgress(int progress)
        {
            Progress = Math.Max(0, progress);
            if (Status == "NotStarted" && Progress > 0) Status = "InProgress";
        }

        public void MarkClaimed()
        {
            Status = "Claimed";
            ClaimedAt = DateTime.UtcNow;
        }
    }
}
