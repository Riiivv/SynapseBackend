using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;

namespace Synapse.Application.Services;

public class PrivateChatMemberService : IPrivateChatMemberService
{
    private readonly IPrivateChatRepository _privateChatRepository;
    private readonly IPrivateChatMemberRepository _privateChatMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;

    public PrivateChatMemberService(
        IPrivateChatRepository privateChatRepository,
        IPrivateChatMemberRepository privateChatMemberRepository,
        IUserRepository userRepository,
        IMessageRepository messageRepository)
    {
        _privateChatRepository = privateChatRepository;
        _privateChatMemberRepository = privateChatMemberRepository;
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }

    public async Task<ServiceResult<PrivateChatMemberResponse>> AddMemberAsync(
        int currentUserId,
        AddPrivateChatMemberRequest request)
    {
        var chat = await _privateChatRepository
            .GetByIdAsync(request.PrivateChatId);

        if (chat == null)
        {
            return ServiceResult<PrivateChatMemberResponse>
                .NotFound("Private chat findes ikke.");
        }

        var currentUserIsMember =
            await _privateChatMemberRepository.IsMemberAsync(
                request.PrivateChatId,
                currentUserId);

        if (!currentUserIsMember)
        {
            return ServiceResult<PrivateChatMemberResponse>
                .Forbidden();
        }

        var user = await _userRepository
            .GetByIdAsync(request.UserId);

        if (user == null)
        {
            return ServiceResult<PrivateChatMemberResponse>
                .NotFound("Bruger findes ikke.");
        }

        var alreadyMember =
            await _privateChatMemberRepository.IsMemberAsync(
                request.PrivateChatId,
                request.UserId);

        if (alreadyMember)
        {
            return ServiceResult<PrivateChatMemberResponse>
                .BadRequest(
                    "Brugeren er allerede medlem af chatten.");
        }

        var member = new PrivateChatMember
        {
            PrivateChatId = request.PrivateChatId,
            UserId = request.UserId
        };

        await _privateChatMemberRepository.AddAsync(member);
        await _privateChatMemberRepository.SaveChangesAsync();

        member.User = user;

        return ServiceResult<PrivateChatMemberResponse>
            .Success(MapMember(member));
    }

    public async Task<ServiceResult<List<PrivateChatMemberResponse>>> GetMembersAsync(
        int privateChatId,
        int currentUserId)
    {
        var chat = await _privateChatRepository
            .GetByIdAsync(privateChatId);

        if (chat == null)
        {
            return ServiceResult<List<PrivateChatMemberResponse>>
                .NotFound("Private chat findes ikke.");
        }

        var currentUserIsMember =
            await _privateChatMemberRepository.IsMemberAsync(
                privateChatId,
                currentUserId);

        if (!currentUserIsMember)
        {
            return ServiceResult<List<PrivateChatMemberResponse>>
                .Forbidden();
        }

        var members =
            await _privateChatMemberRepository
                .GetByChatIdAsync(privateChatId);

        var response = members
            .Select(MapMember)
            .ToList();

        return ServiceResult<List<PrivateChatMemberResponse>>
            .Success(response);
    }

    public async Task<ServiceResult<bool>> LeavePrivateChatAsync(
        int privateChatId,
        int currentUserId)
    {
        var chat = await _privateChatRepository
            .GetByIdAsync(privateChatId);

        if (chat == null)
        {
            return ServiceResult<bool>
                .NotFound("Private chat findes ikke.");
        }

        var membership =
            await _privateChatMemberRepository.GetMembershipAsync(
                privateChatId,
                currentUserId);

        if (membership == null)
        {
            return ServiceResult<bool>
                .BadRequest("Du er ikke medlem af chatten.");
        }

        await _privateChatMemberRepository.DeleteAsync(membership);
        await _privateChatMemberRepository.SaveChangesAsync();

        var members =
            await _privateChatMemberRepository
                .GetByChatIdAsync(privateChatId);

        if (members.Count == 0)
        {
            var messages =
                await _messageRepository
                    .GetPrivateMessagesAsync(privateChatId);

            foreach (var message in messages)
            {
                await _messageRepository.DeleteAsync(message);
            }

            await _privateChatRepository.DeleteAsync(chat);

            await _privateChatRepository.SaveChangesAsync();
        }

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> RemoveMemberAsync(
        int privateChatId,
        int userId,
        int currentUserId)
    {
        var chat = await _privateChatRepository
            .GetByIdAsync(privateChatId);

        if (chat == null)
        {
            return ServiceResult<bool>
                .NotFound("Private chat findes ikke.");
        }

        var currentUserIsMember =
            await _privateChatMemberRepository.IsMemberAsync(
                privateChatId,
                currentUserId);

        if (!currentUserIsMember)
        {
            return ServiceResult<bool>.Forbidden();
        }

        var membership =
            await _privateChatMemberRepository.GetMembershipAsync(
                privateChatId,
                userId);

        if (membership == null)
        {
            return ServiceResult<bool>
                .NotFound(
                    "Brugeren er ikke medlem af chatten.");
        }

        await _privateChatMemberRepository.DeleteAsync(membership);
        await _privateChatMemberRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    private static PrivateChatMemberResponse MapMember(
        PrivateChatMember member)
    {
        return new PrivateChatMemberResponse
        {
            Id = member.Id,
            PrivateChatId = member.PrivateChatId,
            UserId = member.UserId,

            User = member.User == null
                ? null
                : new UserResponse
                {
                    Id = member.User.Id,
                    Username = member.User.Username,
                    Email = member.User.Email
                }
        };
    }
}