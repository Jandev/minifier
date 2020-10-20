using MinifierCore.Model;

namespace MinifierCore.Business
{
    internal interface ICreateUrlHandler
    {
        MinifiedUrl Execute(MinifiedUrl minifiedUrl);
    }
}