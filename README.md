# URL Minifier in Azure

[![Build status][actions build badge]][actions build link]  
[![Build and deploy solution][actions build and deploy badge]][actions build and deploy link]

A URL minifier which works with Azure Functions, and a couple of other Azure services, just because we can!

# Application

## Supported actions

- **Get** : Getting the corresponding url. We either get a `NotFound` code (`404`) or get redirected
- **Create** : Post data to the corresponding url. The response is either `BadRequest` (`400`) or `Created` (`201`)
- **Delete** : Delete data on the corresponding url. The response is either `BadRequest` (`400`), `Ok` (`200`) if the deletion is done, or `ExpectationFailed` (`417`) if anything forbid the deletion to be performed.

## Configuration

There are several configuration values the solution depends upon.

### Local development - local.settings.json

You should add a file called `local.settings.json`, if you want to run the solution yourself. The contents of this file will have to look like the following example.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "CosmosConnectionString": "AccountEndpoint=https://MyTableApiDatabase.documents.azure.com;AccountKey=superSecertKey",
    "CreateNewUrlsServiceBusConnection": "Endpoint=sb://MyServiceBusNamespace.servicebus.windows.net/;SharedAccessKeyName=MyAccessKeyNameWithSendPermission;SharedAccessKey=TheActualAccessKey",
    "GetNewUrlsServiceBusConnection": "Endpoint=sb://minifier.servicebus.windows.net/;SharedAccessKeyName=MyAccessKeyNameWithManagePermission;SharedAccessKey=TheActualAccessKeyForManagePermission"
  }
}
```

### In Azure

There aren't any deployment scripts at the moment, so you have to set everything up manually, including specifying all of the listed settings in the `Configuration` area of your Function App.

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
