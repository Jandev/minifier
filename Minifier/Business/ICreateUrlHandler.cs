using Minifier.Model;

namespace Minifier.Business
{
    internal interface ICreateUrlHandler
    {
        MinifiedUrl Execute(MinifiedUrl minifiedUrl);
    }
}