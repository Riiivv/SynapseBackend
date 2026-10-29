using Synapse.Application.Common;
using Synapse.Application.DTOs;

namespace Synapse.Application.Interfaces;

public interface IPrivateChatMemberService
{
    Task<ServiceResult<PrivateChatMemberResponse>> AddMemberAsync(int currentUserId,AddPrivateChatMemberRequest request);

    Task<ServiceResult<List<PrivateChatMemberResponse>>> GetMembersAsync(int privateChatId,int currentUserId);

    Task<ServiceResult<bool>> LeavePrivateChatAsync(int privateChatId,int currentUserId);

    Task<ServiceResult<bool>> RemoveMemberAsync(int privateChatId,int userId,int currentUserId);
}