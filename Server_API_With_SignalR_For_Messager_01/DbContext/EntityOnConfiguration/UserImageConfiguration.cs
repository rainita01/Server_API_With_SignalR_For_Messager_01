using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSocketSharpServer.DbContext.Entities;

namespace Server_API_With_SignalR_For_Messager_01.DbContext.EntityOnConfiguration
{
    public class UserImageConfiguration :IEntityTypeConfiguration<UserImage>
    {
        public void Configure(EntityTypeBuilder<UserImage> builder)
        {
            builder.Property(e => e.Title).HasMaxLength(500);
            builder.HasOne(e => e.User)
                .WithOne(e => e.Image)
                .HasForeignKey<UserImage>(e => e.UserId);

        }
    }
}
