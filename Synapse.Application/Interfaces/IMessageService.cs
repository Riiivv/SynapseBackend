using Synapse.Application.Common;
using Synapse.Application.DTOs;

namespace Synapse.Application.Interfaces;

public interface IMessageService
{
    Task<ServiceResult<List<MessageResponse>>> GetChannelMessagesAsync(int channelId,int currentUserId);

    Task<ServiceResult<List<MessageResponse>>> GetPrivateMessagesAsync(int privateChatId,int currentUserId);

    Task<ServiceResult<MessageResponse>> SendChannelMessageAsync(int currentUserId,SendMessageRequest request);

    Task<ServiceResult<MessageResponse>> SendPrivateMessageAsync(int currentUserId,SendPrivateMessageRequest request);

    Task<ServiceResult<MessageResponse>> UpdateMessageAsync(int messageId,int currentUserId,EditMessageRequest request);

    Task<ServiceResult<bool>> DeleteMessageAsync(int messageId,int currentUserId);
}