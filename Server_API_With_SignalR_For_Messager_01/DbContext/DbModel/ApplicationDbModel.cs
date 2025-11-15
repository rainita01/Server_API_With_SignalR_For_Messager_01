using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharpServer.DbContext.Entities;
using WebSocketSharpServer.DbContext.EntityOnConfiguration;

namespace WebSocketSharpServer.DbContext.DbModel
{
    public class ApplicationDbModel : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<TextMessage> TextMessages { get; set; }
        public DbSet<ImageMessage> ImageMessages { get; set; }
        public DbSet<VideoMessage> VideoMessages { get; set; }
        public DbSet<AudioMessage> AudioMessages { get; set; }


        public ApplicationDbModel(DbContextOptions optionsBuilder) : base(optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasDiscriminator<string>("MessageType")
                .HasValue<TextMessage>("Text")
                .HasValue<ImageMessage>("Image")
                .HasValue<VideoMessage>("Video")
                .HasValue<AudioMessage>("Audio")
                .HasValue<FileMessage>("File");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
            base.OnModelCreating(modelBuilder);
        }

    }
}
