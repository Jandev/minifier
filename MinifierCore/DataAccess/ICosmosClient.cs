using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace MinifierCore.DataAccess
{
    internal interface ICosmosClient
    {
        Uri GetDocumentCollectionUri();

        Task<DocumentClient> Get();
    }
}