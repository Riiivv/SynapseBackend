using Synapse.Application.Common;
using Synapse.Application.DTOs;

namespace Synapse.Application.Interfaces;

public interface IPrivateChatService
{
    Task<List<PrivateChatOverviewResponse>> GetMyChatsAsync(int currentUserId);

    Task<ServiceResult<PrivateChatResponse>> CreateAsync(int currentUserId,CreatePrivateChatRequest request);

    Task<ServiceResult<PrivateChatResponse>> GetByIdAsync(int privateChatId,int currentUserId);

    Task<ServiceResult<PrivateChatCryptoInfoResponse>> GetCryptoInfoAsync(int privateChatId,int currentUserId);
}