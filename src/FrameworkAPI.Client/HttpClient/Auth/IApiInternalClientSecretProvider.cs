using System.Threading;
using System.Threading.Tasks;

namespace WuH.Ruby.FrameworkAPI.Client;

public interface IApiInternalClientSecretProvider
{
    Task<string> GetApiInternalClientSecret(CancellationToken cancellationToken);
}