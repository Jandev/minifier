using Minifier.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minifier.Business
{
    internal interface IDeleteUrlHandler
    {
        Task<bool> Execute(MinifiedUrl minifiedUrl);
    }
}
