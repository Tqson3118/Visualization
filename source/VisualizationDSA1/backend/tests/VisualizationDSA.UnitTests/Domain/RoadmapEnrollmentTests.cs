using System;
using Xunit;
using VisualizationDSA.Domain.Entities;

namespace VisualizationDSA.UnitTests.Domain
{
    public class RoadmapEnrollmentTests
    {
        [Fact]
        public void CreateEnrollment_ShouldBeActive()
        {
            var enrollment = new RoadmapEnrollment(Guid.NewGuid(), Guid.NewGuid());

            Assert.Equal("Active", enrollment.Status);
            Assert.True(enrollment.EnrolledAt > DateTime.UtcNow.AddSeconds(-10));
        }

        [Fact]
        public void MarkCompleted_ShouldSetCompletedStatus()
        {
            var enrollment = new RoadmapEnrollment(Guid.NewGuid(), Guid.NewGuid());

            enrollment.MarkCompleted();

            Assert.Equal("Completed", enrollment.Status);
            Assert.NotNull(enrollment.CompletedAt);
        }

        [Fact]
        public void Drop_ShouldSetDroppedStatus()
        {
            var enrollment = new RoadmapEnrollment(Guid.NewGuid(), Guid.NewGuid());

            enrollment.Drop();

            Assert.Equal("Dropped", enrollment.Status);
            Assert.Null(enrollment.CompletedAt);
        }

        [Fact]
        public void ResetToken_ShouldValidateWithinTtl()
        {
            var user = new User("test@example.com", "TestUser", "hash");
            var token = "abc123token";

            user.SetPasswordResetToken(token, TimeSpan.FromMinutes(15));

            Assert.True(user.IsPasswordResetTokenValid(token));
            Assert.False(user.IsPasswordResetTokenValid("wrong-token"));
        }

        [Fact]
        public void ClearResetToken_ShouldInvalidate()
        {
            var user = new User("test@example.com", "TestUser", "hash");
            user.SetPasswordResetToken("token123", TimeSpan.FromMinutes(15));

            user.ClearPasswordResetToken();

            Assert.False(user.IsPasswordResetTokenValid("token123"));
        }
    }
}
