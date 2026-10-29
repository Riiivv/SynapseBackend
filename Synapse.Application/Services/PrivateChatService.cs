using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;

namespace Synapse.Application.Services;

public class PrivateChatService : IPrivateChatService
{
    private readonly IPrivateChatRepository _privateChatRepository;
    private readonly IPrivateChatMemberRepository _privateChatMemberRepository;
    private readonly IUserRepository _userRepository;

    public PrivateChatService(
        IPrivateChatRepository privateChatRepository,
        IPrivateChatMemberRepository privateChatMemberRepository,
        IUserRepository userRepository)
    {
        _privateChatRepository = privateChatRepository;
        _privateChatMemberRepository = privateChatMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<List<PrivateChatOverviewResponse>> GetMyChatsAsync(
        int currentUserId)
    {
        var chats = await _privateChatRepository
            .GetForUserAsync(currentUserId);

        var response = new List<PrivateChatOverviewResponse>();

        foreach (var chat in chats)
        {
            var members = await _privateChatMemberRepository
                .GetByChatIdAsync(chat.Id);

            response.Add(new PrivateChatOverviewResponse
            {
                Id = chat.Id,
                CreatedAt = chat.CreatedAt,

                Members = members
                    .Where(m => m.User != null)
                    .Select(m => new PrivateChatMemberInfoResponse
                    {
                        UserId = m.UserId,
                        Username = m.User!.Username,
                        Email = m.User.Email
                    })
                    .ToList()
            });
        }

        return response;
    }

    public async Task<ServiceResult<PrivateChatResponse>> CreateAsync(
        int currentUserId,
        CreatePrivateChatRequest request)
    {
        if (request.OtherUserId == currentUserId)
        {
            return ServiceResult<PrivateChatResponse>
                .BadRequest(
                    "Du kan ikke oprette en privat chat med dig selv.");
        }

        var otherUser = await _userRepository
            .GetByIdAsync(request.OtherUserId);

        if (otherUser == null)
        {
            return ServiceResult<PrivateChatResponse>
                .NotFound("Brugeren findes ikke.");
        }

        var existingChat =
            await _privateChatRepository.GetBetweenUsersAsync(
                currentUserId,
                request.OtherUserId);

        if (existingChat != null)
        {
            return ServiceResult<PrivateChatResponse>
                .BadRequest(
                    "Der findes allerede en privat chat mellem brugerne.");
        }

        var privateChat = new PrivateChat
        {
            CreatedAt = DateTime.UtcNow
        };

        await _privateChatRepository.AddAsync(privateChat);

        // Skal gemmes først, så chatten får sit Id.
        await _privateChatRepository.SaveChangesAsync();

        var currentUserMember = new PrivateChatMember
        {
            PrivateChatId = privateChat.Id,
            UserId = currentUserId
        };

        var otherUserMember = new PrivateChatMember
        {
            PrivateChatId = privateChat.Id,
            UserId = request.OtherUserId
        };

        await _privateChatMemberRepository
            .AddAsync(currentUserMember);

        await _privateChatMemberRepository
            .AddAsync(otherUserMember);

        await _privateChatMemberRepository.SaveChangesAsync();

        return ServiceResult<PrivateChatResponse>.Success(
            MapChat(privateChat));
    }

    public async Task<ServiceResult<PrivateChatResponse>> GetByIdAsync(
        int privateChatId,
        int currentUserId)
    {
        var privateChat =
            await _privateChatRepository.GetByIdAsync(privateChatId);

        if (privateChat == null)
        {
            return ServiceResult<PrivateChatResponse>
                .NotFound("Private chat findes ikke.");
        }

        var isMember =
            await _privateChatMemberRepository.IsMemberAsync(
                privateChatId,
                currentUserId);

        if (!isMember)
        {
            return ServiceResult<PrivateChatResponse>.Forbidden();
        }

        return ServiceResult<PrivateChatResponse>
            .Success(MapChat(privateChat));
    }

    public async Task<ServiceResult<PrivateChatCryptoInfoResponse>>
        GetCryptoInfoAsync(
            int privateChatId,
            int currentUserId)
    {
        var privateChat =
            await _privateChatRepository.GetByIdAsync(privateChatId);

        if (privateChat == null)
        {
            return ServiceResult<PrivateChatCryptoInfoResponse>
                .NotFound("Private chat findes ikke.");
        }

        var isMember =
            await _privateChatMemberRepository.IsMemberAsync(
                privateChatId,
                currentUserId);

        if (!isMember)
        {
            return ServiceResult<PrivateChatCryptoInfoResponse>
                .Forbidden();
        }

        var members =
            await _privateChatMemberRepository
                .GetByChatIdAsync(privateChatId);

        if (members.Count != 2)
        {
            return ServiceResult<PrivateChatCryptoInfoResponse>
                .BadRequest(
                    "E2EE crypto-info understøtter kun chats mellem 2 brugere.");
        }

        var otherMember = members
            .FirstOrDefault(m =>
                m.UserId != currentUserId);

        if (otherMember?.User == null)
        {
            return ServiceResult<PrivateChatCryptoInfoResponse>
                .NotFound("Den anden bruger findes ikke.");
        }

        if (string.IsNullOrWhiteSpace(
                otherMember.User.PublicKey))
        {
            return ServiceResult<PrivateChatCryptoInfoResponse>
                .BadRequest(
                    "Den anden bruger har ikke registreret en public key.");
        }

        var response = new PrivateChatCryptoInfoResponse
        {
            PrivateChatId = privateChatId,

            OtherUserId =
                otherMember.User.Id,

            Username =
                otherMember.User.Username,

            PublicKey =
                otherMember.User.PublicKey,

            PublicKeyFingerprint =
                otherMember.User.PublicKeyFingerprint
                ?? string.Empty,

            PublicKeyVersion =
                otherMember.User.PublicKeyVersion,

            PublicKeyUpdatedAt =
                otherMember.User.PublicKeyUpdatedAt
        };

        return ServiceResult<PrivateChatCryptoInfoResponse>
            .Success(response);
    }

    private static PrivateChatResponse MapChat(
        PrivateChat chat)
    {
        return new PrivateChatResponse
        {
            Id = chat.Id,
            CreatedAt = chat.CreatedAt
        };
    }
}