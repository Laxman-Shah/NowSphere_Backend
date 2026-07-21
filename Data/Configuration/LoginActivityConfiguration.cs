using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configuration
{




    public class LoginActivityConfiguration
     : IEntityTypeConfiguration<LoginActivity>
    {
        public void Configure(EntityTypeBuilder<LoginActivity> builder)
        {
            builder.HasOne(x => x.User)
        .WithMany(x => x.LoginActivities)
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.UserSession)
        .WithMany(x => x.LoginActivities)
        .HasForeignKey(x => x.UserSessionId)
        .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.UserDevice)
        .WithMany(x => x.LoginActivities)
        .HasForeignKey(x => x.UserDeviceId)
        .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.LoginChallenge)
        .WithMany(x => x.LoginActivities)
        .HasForeignKey(x => x.LoginChallengeId)
        .OnDelete(DeleteBehavior.SetNull);
        }
    }
}



