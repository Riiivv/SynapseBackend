using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;

namespace Synapse.Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IServerMemberRepository _serverMemberRepository;
    private readonly IPrivateChatRepository _privateChatRepository;
    private readonly IPrivateChatMemberRepository _privateChatMemberRepository;

    public MessageService(
        IMessageRepository messageRepository,
        IChannelRepository channelRepository,
        IServerMemberRepository serverMemberRepository,
        IPrivateChatRepository privateChatRepository,
        IPrivateChatMemberRepository privateChatMemberRepository)
    {
        _messageRepository = messageRepository;
        _channelRepository = channelRepository;
        _serverMemberRepository = serverMemberRepository;
        _privateChatRepository = privateChatRepository;
        _privateChatMemberRepository = privateChatMemberRepository;
    }

    public async Task<ServiceResult<List<MessageResponse>>> GetChannelMessagesAsync(
        int channelId,
        int currentUserId)
    {
        var channel = await _channelRepository.GetByIdAsync(channelId);

        if (channel == null)
        {
            return ServiceResult<List<MessageResponse>>
                .NotFound("Channel findes ikke.");
        }

        var isMember = await _serverMemberRepository.IsMemberAsync(
            channel.ServerId,
            currentUserId);

        if (!isMember)
        {
            return ServiceResult<List<MessageResponse>>.Forbidden();
        }

        var messages = await _messageRepository
            .GetChannelMessagesAsync(channelId);

        var response = messages
            .Select(MapChannelMessage)
            .ToList();

        return ServiceResult<List<MessageResponse>>
            .Success(response);
    }

    public async Task<ServiceResult<List<MessageResponse>>> GetPrivateMessagesAsync(
        int privateChatId,
        int currentUserId)
    {
        var chat = await _privateChatRepository
            .GetByIdAsync(privateChatId);

        if (chat == null)
        {
            return ServiceResult<List<MessageResponse>>
                .NotFound("Private chat findes ikke.");
        }

        var isMember = await _privateChatMemberRepository
            .IsMemberAsync(
                privateChatId,
                currentUserId);

        if (!isMember)
        {
            return ServiceResult<List<MessageResponse>>.Forbidden();
        }

        var messages = await _messageRepository
            .GetPrivateMessagesAsync(privateChatId);

        var response = messages
            .Select(message =>
                MapPrivateMessage(message, currentUserId))
            .ToList();

        return ServiceResult<List<MessageResponse>>
            .Success(response);
    }

    public async Task<ServiceResult<MessageResponse>> SendChannelMessageAsync(
        int currentUserId,
        SendMessageRequest request)
    {
        if (request.ChannelId == null)
        {
            return ServiceResult<MessageResponse>.BadRequest(
                "Dette endpoint bruges kun til channel-beskeder.");
        }

        if (request.PrivateChatId != null)
        {
            return ServiceResult<MessageResponse>.BadRequest(
                "Private beskeder skal sendes krypteret via /api/Messages/private.");
        }

        if (string.IsNullOrWhiteSpace(request.MessageContent))
        {
            return ServiceResult<MessageResponse>.BadRequest(
                "Beskeden må ikke være tom.");
        }

        var channel = await _channelRepository
            .GetByIdAsync(request.ChannelId.Value);

        if (channel == null)
        {
            return ServiceResult<MessageResponse>
                .NotFound("Channel findes ikke.");
        }

        var isMember = await _serverMemberRepository
            .IsMemberAsync(
                channel.ServerId,
                currentUserId);

        if (!isMember)
        {
            return ServiceResult<MessageResponse>.Forbidden();
        }

        var message = new Message
        {
            UserId = currentUserId,

            ChannelId = request.ChannelId,
            PrivateChatId = null,

            MessageContent = request.MessageContent,

            Ciphertext = null,
            Nonce = null,
            EncryptedKeyForSender = null,
            EncryptedKeyForRecipient = null,

            SentAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();

        var response = new MessageResponse
        {
            Id = message.Id,
            UserId = message.UserId,

            ChannelId = message.ChannelId,
            PrivateChatId = null,

            MessageContent = message.MessageContent,

            Ciphertext = null,
            Nonce = null,
            EncryptedKey = null,

            SentAt = message.SentAt
        };

        return ServiceResult<MessageResponse>
            .Success(response);
    }

    public async Task<ServiceResult<MessageResponse>> SendPrivateMessageAsync(
        int currentUserId,
        SendPrivateMessageRequest request)
    {
        var chat = await _privateChatRepository
            .GetByIdAsync(request.PrivateChatId);

        if (chat == null)
        {
            return ServiceResult<MessageResponse>
                .NotFound("Private chat findes ikke.");
        }

        var isMember = await _privateChatMemberRepository
            .IsMemberAsync(
                request.PrivateChatId,
                currentUserId);

        if (!isMember)
        {
            return ServiceResult<MessageResponse>.Forbidden();
        }

        var members = await _privateChatMemberRepository
            .GetByChatIdAsync(request.PrivateChatId);

        if (members.Count != 2)
        {
            return ServiceResult<MessageResponse>.BadRequest(
                "Denne E2EE-implementering understøtter kun private chats mellem 2 brugere.");
        }

        var message = new Message
        {
            UserId = currentUserId,

            ChannelId = null,
            PrivateChatId = request.PrivateChatId,

            // Serveren gemmer ikke plaintext.
            MessageContent = null,

            Ciphertext = request.Ciphertext,
            Nonce = request.Nonce,

            EncryptedKeyForSender =
                request.EncryptedKeyForSender,

            EncryptedKeyForRecipient =
                request.EncryptedKeyForRecipient,

            SentAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();

        var response = new MessageResponse
        {
            Id = message.Id,
            UserId = message.UserId,

            ChannelId = null,
            PrivateChatId = message.PrivateChatId,

            MessageContent = null,

            Ciphertext = message.Ciphertext,
            Nonce = message.Nonce,

            EncryptedKey =
                message.EncryptedKeyForSender,

            SentAt = message.SentAt
        };

        return ServiceResult<MessageResponse>
            .Success(response);
    }

    public async Task<ServiceResult<MessageResponse>> UpdateMessageAsync(
        int messageId,
        int currentUserId,
        EditMessageRequest request)
    {
        var message = await _messageRepository
            .GetByIdAsync(messageId);

        if (message == null)
        {
            return ServiceResult<MessageResponse>
                .NotFound("Besked findes ikke.");
        }

        if (message.UserId != currentUserId)
        {
            return ServiceResult<MessageResponse>.Forbidden();
        }

        if (message.PrivateChatId != null)
        {
            return ServiceResult<MessageResponse>.BadRequest(
                "Krypterede private beskeder kan ikke redigeres via dette endpoint.");
        }

        if (string.IsNullOrWhiteSpace(request.MessageContent))
        {
            return ServiceResult<MessageResponse>.BadRequest(
                "Beskeden må ikke være tom.");
        }

        message.MessageContent = request.MessageContent;

        await _messageRepository.UpdateAsync(message);
        await _messageRepository.SaveChangesAsync();

        var response = MapChannelMessage(message);

        return ServiceResult<MessageResponse>
            .Success(response);
    }

    public async Task<ServiceResult<bool>> DeleteMessageAsync(
        int messageId,
        int currentUserId)
    {
        var message = await _messageRepository
            .GetByIdAsync(messageId);

        if (message == null)
        {
            return ServiceResult<bool>
                .NotFound("Besked findes ikke.");
        }

        if (message.UserId != currentUserId)
        {
            return ServiceResult<bool>.Forbidden();
        }

        await _messageRepository.DeleteAsync(message);
        await _messageRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    private static MessageResponse MapChannelMessage(
        Message message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            UserId = message.UserId,

            User = message.User == null
                ? null
                : new UserResponse
                {
                    Id = message.User.Id,
                    Username = message.User.Username,
                    Email = message.User.Email
                },

            ChannelId = message.ChannelId,
            PrivateChatId = null,

            MessageContent = message.MessageContent,

            Ciphertext = null,
            Nonce = null,
            EncryptedKey = null,

            SentAt = message.SentAt
        };
    }

    private static MessageResponse MapPrivateMessage(
        Message message,
        int currentUserId)
    {
        return new MessageResponse
        {
            Id = message.Id,
            UserId = message.UserId,

            User = message.User == null
                ? null
                : new UserResponse
                {
                    Id = message.User.Id,
                    Username = message.User.Username,
                    Email = message.User.Email
                },

            ChannelId = null,
            PrivateChatId = message.PrivateChatId,

            MessageContent = null,

            Ciphertext = message.Ciphertext,
            Nonce = message.Nonce,

            EncryptedKey =
                message.UserId == currentUserId
                    ? message.EncryptedKeyForSender
                    : message.EncryptedKeyForRecipient,

            SentAt = message.SentAt
        };
    }
}