using System.Threading.Tasks;

namespace Minifier
{
    internal interface ISecret
    {
        Task<string> Get(string secretKey);
    }
}