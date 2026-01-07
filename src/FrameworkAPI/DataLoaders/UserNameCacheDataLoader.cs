using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using GreenDonut;
using WuH.Ruby.Supervisor.Client;

namespace FrameworkAPI.DataLoaders;

public class UserNameCacheDataLoader : CacheDataLoader<string, DataResult<string>>
{
    private readonly ISupervisorHttpClient _supervisorHttpClient;

    public UserNameCacheDataLoader(
        ISupervisorHttpClient supervisorHttpClient,
        DataLoaderOptions? options = null)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(supervisorHttpClient);
        _supervisorHttpClient = supervisorHttpClient;
    }

    protected override async Task<DataResult<string>> LoadSingleAsync(string userId, CancellationToken cancellationToken)
    {
        var response = await _supervisorHttpClient.ResolveNames([Guid.Parse(userId)], cancellationToken);

        if (response.HasError)
        {
            return new DataResult<string>(null, new InternalServiceException(response.Error));
        }

        return new DataResult<string>(response.Items.First().Name, null);
    }
}