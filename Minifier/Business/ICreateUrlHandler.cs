using System.Threading.Tasks;
using Minifier.Model;

namespace Minifier.Business
{
    internal interface ICreateUrlHandler
    {
        Task Execute(MinifiedUrl minifiedUrl);
    }
}