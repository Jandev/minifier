# URL Minifier in Azure

[![Build status][actions build badge]][actions build link]  
[![Build and deploy solution][actions build and deploy badge]][actions build and deploy link]

A URL minifier which works with Azure Functions, and a couple of other Azure services, just because we can!

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

<!-- Aliases for URLs: please place here any long urls to keep clean markdown markup -->

[actions build badge]: https://github.com/Jandev/minifier/actions/workflows/build.yml/badge.svg "Build solution"
[actions build link]: https://github.com/Jandev/minifier/actions/workflows/build.yml
[actions build and deploy badge]: https://github.com/Jandev/minifier/actions/workflows/release.yml/badge.svg "Build and deploy solution"
[actions build and deploy link]: https://github.com/Jandev/minifier/actions/workflows/release.yml
