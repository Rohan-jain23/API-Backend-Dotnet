using System.Threading;
using System.Threading.Tasks;

namespace WuH.Ruby.FrameworkAPI.Client;

public interface IClientSecretAuthTokenClient
{
    Task<string> GetToken(string apiClientSecret, CancellationToken cancellationToken);
}