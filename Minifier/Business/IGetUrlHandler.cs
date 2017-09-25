using System.Threading.Tasks;
using Minifier.Model;

namespace Minifier.Business
{
    internal interface IGetUrlHandler
    {
        Task<MinifiedUrl> Execute(string slug);
    }
}