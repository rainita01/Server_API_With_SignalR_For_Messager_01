using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using demo_158.MVVM.Model;
using Microsoft.EntityFrameworkCore;
using Server_API_With_SignalR_For_Messager_01.Models;
using WebSocketSharpServer.DbContext.DbModel;
using WebSocketSharpServer.DbContext.Entities;
using WebSocketSharpServer.Models;

namespace WebSocketSharpServer.Services
{
    public class MessageServices(ApplicationDbModel dbModel)
    {

        public  MessageModelFromServer ConvertMessageFromUserToMessageFromServer(MessageModelFromUser message)
        {
            var messageToSend = new MessageModelFromServer()
            {
                ConversationId = message.ConversationId,
                SendDate = DateTime.Now,
                Object = message.Object,
                MessageType = message.MessageType,
                Text = message.Text,
                UserId = message.UserId,
                Username = message.Username
            };
            
            return messageToSend;
        }

        public async Task SaveMessageToDataBase(MessageModelFromServer message)
        {
            Message messageToAdd = message.MessageType switch
            {
                MessageTypes.Text => new TextMessage { Text = message.Text },      
                MessageTypes.Video => new VideoMessage { VideoData = message.Object },
                MessageTypes.Image => new ImageMessage { ImageData = message.Object },
                MessageTypes.Audio => new AudioMessage { AudioData = message.Object },
                MessageTypes.File => new FileMessage { FileData = message.Object},
                _ => throw new ArgumentException("Unknown MessageType")
            };
            messageToAdd.SentTime = message.SendDate;
            messageToAdd.ConversationId = message.ConversationId;
            messageToAdd.UserId = message.UserId;
            dbModel.Messages.Add(messageToAdd);
            await dbModel.SaveChangesAsync();
        }
        public async Task<List<Message>> UploadMessagesAsync(ConversationModel conversation, Message? message)
        {
            var messages = new List<Message>();

            if (message != null)
            {

                messages = await dbModel.Messages
                    .AsNoTracking()
                    .Where(i => i.ConversationId == conversation.Id)
                    .Where(e => e.SentTime > message.SentTime)
                    .OrderByDescending(c => c.SentTime)
                    .Take(30)
                    .OrderBy(e => e.SentTime)
                    .ToListAsync();
            }
            else
            {
                messages = await dbModel.Messages
                    .AsNoTracking()
                    .Where(i => i.ConversationId == conversation.Id)
                    .OrderByDescending(c => c.SentTime)
                    .Take(30)
                    .OrderBy(e=>e.SentTime)
                    .ToListAsync();

            }

            return messages;

        }
        public Task<List<MessageModelFromServer>> ConvertMessagesToMessagesModelFromUserAsync(List<Message> messages)
        {
          var convertedMessage =  messages.Select(e => new MessageModelFromServer()
            {
              UserId = e.UserId,
              Id = e.Id,
              ConversationId = e.ConversationId,
              Text = e is TextMessage message ? message.Text : null,
               Username = dbModel.Users
              .AsNoTracking()
              .Where(u => u.Id == e.UserId)
              .Select(u => u.Username)
              .FirstOrDefault() ?? "Unknown",
              MessageType = e switch
              {
                  AudioMessage => MessageTypes.Audio,
                  ImageMessage => MessageTypes.Image,
                  VideoMessage => MessageTypes.Video,
                  FileMessage => MessageTypes.File,
                  _ => MessageTypes.Text
              },
              Object = e switch
              {
                  AudioMessage audio => audio.AudioData, 
                  ImageMessage image => image.ImageData,
                  VideoMessage video=> video.VideoData ,
                  FileMessage  file=> file.FileData,
                  _ => null
              }

          }).ToList();
          return Task.FromResult(convertedMessage);
        }
    }
}
