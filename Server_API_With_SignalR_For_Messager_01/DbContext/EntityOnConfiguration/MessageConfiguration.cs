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
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasOne(p => p.Conversation)
                .WithMany(p => p.Messages)
                .HasForeignKey(e => e.ConversationId);
        }
    }
}
