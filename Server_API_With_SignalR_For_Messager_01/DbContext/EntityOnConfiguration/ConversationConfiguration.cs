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
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.Property(c => c.CreatedBy).HasMaxLength(100).IsUnicode(false);
            builder.HasMany(p => p.Users)
                .WithMany(p => p.Conversations)
                .UsingEntity("UsersConversations");
        }
    }
}
