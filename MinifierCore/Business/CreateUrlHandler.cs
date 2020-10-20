using System;
using MinifierCore.Model;

namespace MinifierCore.Business
{
    internal class CreateUrlHandler : ICreateUrlHandler
    {
        public MinifiedUrl Execute(MinifiedUrl minifiedUrl)
        {
            if (!IsValid(minifiedUrl))
            {
                throw new ArgumentException();
            }
            minifiedUrl.Created = DateTime.UtcNow;

            return minifiedUrl;
        }

        private bool IsValid(MinifiedUrl minifiedUrl)
        {
            return !string.IsNullOrWhiteSpace(minifiedUrl.FullUrl) &&
                   !string.IsNullOrWhiteSpace(minifiedUrl.MinifiedSlug);
        }
    }
}
