using Synapse.Application.Common;
using Synapse.Application.DTOs;

namespace Synapse.Application.Interfaces;

public interface IChannelService
{
    Task<ServiceResult<List<ChannelResponse>>> GetServerChannelsAsync(int serverId,int userId);

    Task<ServiceResult<ChannelResponse>> CreateAsync(int userId,CreateChannelRequest request);

    Task<ServiceResult<ChannelResponse>> UpdateAsync(int channelId,int userId,UpdateChannelRequest request);

    Task<ServiceResult<bool>> DeleteAsync(int channelId,int userId);
}