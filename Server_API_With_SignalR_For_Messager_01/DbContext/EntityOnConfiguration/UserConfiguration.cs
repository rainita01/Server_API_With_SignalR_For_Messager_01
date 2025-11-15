using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSocketSharpServer.DbContext.Entities;

namespace WebSocketSharpServer.DbContext.EntityOnConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {

            builder.Property(p => p.Username).HasMaxLength(100);
            builder.Property(p => p.Password).HasMaxLength(100).IsUnicode(false);
            builder.Property(p => p.Email).HasMaxLength(100);
            builder.Property(p => p.BioCaption).HasMaxLength(400);
            builder.HasOne(e => e.Image).WithOne(e => e.User)
                .HasForeignKey<UserImage>(e => e.UserId);
                
        }
    }
}
