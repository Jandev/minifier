using System.Threading.Tasks;
using MinifierCore.Model;

namespace MinifierCore.Business
{
    internal interface IDeleteUrlHandler
    {
        Task<bool> Execute(MinifiedUrl minifiedUrl);
    }
}
