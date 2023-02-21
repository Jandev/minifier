# URL Minifier in Azure

[![Build status][actions build badge]][actions build link]  
[![Build and deploy solution][actions build and deploy badge]][actions build and deploy link]

A URL minifier which works with Azure Functions, and a couple of other Azure services, just because we can!

# Application

## Usage

### Supported actions

| Action | HTTP VERB | Description                                                                                                                                                                                                                         |
| ------ | --------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Get    | GET       | Retrieves the URL matching the slug and redirects, or returns a `NotFound` code (`404`).                                                                                                                                            |
| Create | POST      | Creates a new slug in the repository, along with the redirect URL. The response is either `BadRequest` (`400`) or `Created` (`201`)                                                                                                 |
| Delete | DELETE    | Removes the URL matching the specified slug from the repository. The response is either `BadRequest` (`400`), `Ok` (`200`) if the deletion is done, or `ExpectationFailed` (`417`) if anything forbid the deletion to be performed. |

### GET

Invoke the Azure Function like this.

```
GET http://{yourInstance}/someSlug
```

Replace `{yourInstance}` with your actual host.

### POST

```
POST http://localhost:7071/api/Create?code=yourFunctionKey
{
    "slug": "blog",
    "url": "https://jan-v.nl"
}
```

This will result in the slug being stored in the repository. Make sure you have some authorization in place, like using a Function Key.

### DELETE

_Not implemented, yet_

```
DELETE http://localhost:7071/api/Delete?code=yourFunctionKey
{
    "slug": "blog",
}
```

This will remove the specified slug from the repository. Make sure you have some authorization in place, like using a Function Key.

## Configuration

There are several configuration values the solution depends upon.

### Local development - local.settings.json

You should add a file called `local.settings.json`, if you want to run the solution yourself.

While the deployment templates make sure the appropriate permissions are set for the managed identities, you need to do this for yourself when running on your own machine. Within Visual Studio you can set the used identity in `Tools -> Options -> Azure Service Authentication -> Account Selection`. Most other tools use the identity configured via the Azure CLI.

The Azure Functions are using identity-based bindings, therefore you need to grant yourself the appropriate roles to use these. You can use the following script to grant yourself the appropriate roles for Cosmos DB.

```azcli
$resourceGroupName='<myResourceGroup>'
$accountName='<myCosmosAccount>'
# Cosmos DB Built-in Data Reader: 00000000-0000-0000-0000-000000000001
# Cosmos DB Built-in Data Contributor: 00000000-0000-0000-0000-000000000002
$readOnlyRoleDefinitionId = '<roleDefinitionId>'
$principalId = '<aadPrincipalIdOfYourManagedIdentity>'
az cosmosdb sql role assignment create --account-name $accountName --resource-group $resourceGroupName --scope "/" --principal-id $principalId --role-definition-id $readOnlyRoleDefinitionId
```

The `principalId` is the Object Id of your Managed Identity OR from your own Azure Active Directory account.

The contents of the `Minifier.Backend` project should look similar to the following:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "MinifierIncomingMessages__fullyQualifiedNamespace": "{yourServiceBusNamespaceName}.servicebus.windows.net",
    "IncomingUrlsTopicName": "incoming-minified-urls", // This one is defined in the Bicep template, but you can change it if you want.
    "IncomingUrlsProcessingSubscription": "process", // This one is defined in the Bicep template, but you can change it if you want.
    "UrlMinifierRepository__accountEndpoint": "https://{yourCosmosDbAccountName}.documents.azure.com:443/",
    // Or this if you're running with the local emulator
    "UrlMinifierRepository": "AccountEndpoint=https://localhost:8081/;AccountKey={theEmulatorKey}",
    "UrlMinifierRepository__DatabaseName": "minifier", // This one is defined in the Bicep template, but you can change it if you want.
    "UrlMinifierRepository__CollectionName": "urls" // This one is defined in the Bicep template, but you can change it if you want.
  }
}
```

The contents of the `Minifier.Frontend` project should look similar to the following:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "UrlMinifierRepository__accountEndpoint": "https://{yourCosmosDbAccountName}.documents.azure.com:443/",
    "UrlMinifierRepository__DatabaseName": "minifier", // This one is defined in the Bicep template, but you can change it if you want.
    "UrlMinifierRepository__CollectionName": "urls" // This one is defined in the Bicep template, but you can change it if you want.
  }
}
```

To get the necessary Azure resources, run the following Azure CLI commands:

```azcli
az deployment sub create --location WestEurope --template-file basic-infrastructure.bicep --parameters parameters.lcl.json

az deployment sub create --location WestEurope --template-file main.bicep --parameters parameters.lcl.json
```

### In Azure

This is all being handled by the Bicep template in this repository, so no need to worry about it.

# Deployment

To deploy this solution you need to create a service principal in Azure which has the appropriate roles to create resource groups, all resources and set permissions (RBAC) to all these resources. The easiest way to set this up is by using the `Owner` role, as it has enough permissions to apply roles. However, keep in mind, this grants the service principal a lot of power on the entire subscription.

```azcli
az ad sp create-for-rbac --name "minifier" --role owner --sdk-auth
```

What I'm doing to limit this is to make this service principal a `Contributor`, which still grants it a lot of power, and applying the `Owner` role to the created resource group after the first (failed) deployment.

After executing the command above, you should have an output similar to the following:

```json
{
  "clientId": "d9816ed2-66f2-40f2-bcf0-1b222a910416",
  "clientSecret": "someSuperSecret",
  "subscriptionId": "fe2a1369-754c-4703-ba37-c3a864e1eac8",
  "tenantId": "b491f32d-ecc5-43ba-9699-0860994360d7",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

Store this value in a GitHub secret called `AZURE_DEV` and you're good to go!

<!-- Aliases for URLs: please place here any long urls to keep clean markdown markup -->

[actions build badge]: https://github.com/Jandev/minifier/actions/workflows/build.yml/badge.svg "Build solution"
[actions build link]: https://github.com/Jandev/minifier/actions/workflows/build.yml
[actions build and deploy badge]: https://github.com/Jandev/minifier/actions/workflows/release.yml/badge.svg "Build and deploy solution"
[actions build and deploy link]: https://github.com/Jandev/minifier/actions/workflows/release.yml
