@allowed([
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param azureRegion string
param systemName string
param frontendPackageReferenceLocation string
param backendPackageReferenceLocation string

var webAppName = '${systemName}-${environmentName}-${azureRegion}-app'
var backendSystemName = '${systemName}backend'
var webAppNameBackend = '${backendSystemName}-${environmentName}-${azureRegion}-app'

targetScope = 'resourceGroup'

// Authorization roles
var storageAccountBlobDataReaderAuthorizationRoleId = '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1' // Storage Blob Data Reader
// Deployment Storage Account details
var deploymentStorageAccountName = '${systemName}deploy${environmentName}${azureRegion}sa'
resource deploymentStorageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: deploymentStorageAccountName
}

module applicationInsights 'Insights/components.bicep' = {
  name: 'applicationInsightsDeploy'
  params: {
    environmentName: environmentName
    systemName: systemName
    azureRegion: azureRegion
  }
}

module webApiStorageAccount 'Storage/storageAccounts.bicep' = {
  name: 'storageAccountAppDeploy'
  params: {
    environmentName: environmentName
    systemName: systemName
    azureRegion: azureRegion
  }
}

module appServicePlan 'Web/serverfarms.bicep' = {
  name: 'appServicePlanModule'
  params: {
    environmentName: environmentName
    systemName: systemName
    azureRegion: azureRegion
    kind: 'linux'
  }
}

module functionApp 'Web/functions.bicep' = {
  dependsOn: [
    appServicePlan
    webApiStorageAccount
  ]
  name: 'functionAppModule'
  params: {
    environmentName: environmentName
    systemName: systemName
    azureRegion: azureRegion
    appServicePlanId: appServicePlan.outputs.id
  }
}

module authorizationDeployStorageAccount 'Authorization/roleAssignmentsStorageAccount.bicep' = {
  name: 'deploymentStorageAccountReaderAuthorization'
  params: {
    principalId: functionApp.outputs.servicePrincipal
    roleDefinitionId: storageAccountBlobDataReaderAuthorizationRoleId
    storageAccountName: deploymentStorageAccountName
  }
}
module authorizationWebjobStorageAccount 'Authorization/roleAssignmentsStorageAccount.bicep' = {
  name: 'authorizationWebjobStorageAccount'
  params: {
    principalId: functionApp.outputs.servicePrincipal
    roleDefinitionId: storageAccountBlobDataReaderAuthorizationRoleId
    storageAccountName: webApiStorageAccount.outputs.storageAccountName
  }
}

resource config 'Microsoft.Web/sites/config@2020-12-01' = {
  dependsOn: [
    functionApp
  ]
  name: '${webAppName}/web'
  properties: {
    cors: {
      allowedOrigins: [
      ]
    }
    appSettings: [
      {
        name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
        value: applicationInsights.outputs.instrumentationKey
      }
      {
        name: 'FUNCTIONS_EXTENSION_VERSION'
        value: '~4'
      }
      {
        name: 'FUNCTIONS_WORKER_RUNTIME'
        value: 'dotnet'
      }
      {
        name: 'WEBSITE_CONTENTSHARE'
        value: 'azure-function'
      }
      {
        name: 'AzureWebJobsStorage'
        value: webApiStorageAccount.outputs.connectionString
      }
      {
        name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
        value: webApiStorageAccount.outputs.connectionString
      }
      {
        name: 'RUN_FROM_PACKAGE'
        value: frontendPackageReferenceLocation
      }
    ]
  }
}

module webApiStorageAccountBackend 'Storage/storageAccounts.bicep' = {
  name: 'storageAccountAppBackend'
  params: {
    environmentName: environmentName
    systemName: backendSystemName
    azureRegion: azureRegion
  }
}

module appServicePlanBackend 'Web/serverfarms.bicep' = {
  name: 'appServicePlanBackend'
  params: {
    environmentName: environmentName
    systemName: backendSystemName
    azureRegion: azureRegion
    kind: 'linux'
  }
}

module functionAppBackend 'Web/functions.bicep' = {
  dependsOn: [
    appServicePlanBackend
    webApiStorageAccountBackend
  ]
  name: 'functionAppBackend'
  params: {
    environmentName: environmentName
    systemName: backendSystemName
    azureRegion: azureRegion
    appServicePlanId: appServicePlanBackend.outputs.id
  }
}

module authorizationDeployStorageAccountBackend 'Authorization/roleAssignmentsStorageAccount.bicep' = {
  name: 'deploymentStorageAccountReaderAuthorizationBackend'
  params: {
    principalId: functionAppBackend.outputs.servicePrincipal
    roleDefinitionId: storageAccountBlobDataReaderAuthorizationRoleId
    storageAccountName: deploymentStorageAccountName
  }
}
module authorizationWebjobStorageAccountBackend 'Authorization/roleAssignmentsStorageAccount.bicep' = {
  name: 'authorizationWebjobStorageAccountBackend'
  params: {
    principalId: functionAppBackend.outputs.servicePrincipal
    roleDefinitionId: storageAccountBlobDataReaderAuthorizationRoleId
    storageAccountName: webApiStorageAccountBackend.outputs.storageAccountName
  }
}

resource configBackend 'Microsoft.Web/sites/config@2020-12-01' = {
  dependsOn: [
    functionAppBackend
  ]
  name: '${webAppNameBackend}/web'
  properties: {
    cors: {
      allowedOrigins: [
      ]
    }
    appSettings: [
      {
        name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
        value: applicationInsights.outputs.instrumentationKey
      }
      {
        name: 'FUNCTIONS_EXTENSION_VERSION'
        value: '~4'
      }
      {
        name: 'FUNCTIONS_WORKER_RUNTIME'
        value: 'dotnet'
      }
      {
        name: 'WEBSITE_CONTENTSHARE'
        value: 'azure-function'
      }
      {
        name: 'AzureWebJobsStorage'
        value: webApiStorageAccountBackend.outputs.connectionString
      }
      {
        name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
        value: webApiStorageAccountBackend.outputs.connectionString
      }
      {
        name: 'RUN_FROM_PACKAGE'
        value: backendPackageReferenceLocation
      }
    ]
  }
}

module databaseAccount 'DocumentDB/databaseAccount.bicep' = {
  name: 'databaseAccount'
  params: {
    azureRegion: azureRegion
    environmentName: environmentName
    systemName: systemName
  }
}

module sqlDatabase 'DocumentDB/sqlDatabase.bicep' = {
  name: 'sqlDatabase'
  params: {
    databaseAccountName: databaseAccount.outputs.accountName
    databaseName: systemName
  }
}

module slugContainer 'DocumentDB/minifierContainer.bicep' = {
  name: 'slugContainer'
  params: {
    accountName: databaseAccount.outputs.accountName
    databaseName: sqlDatabase.outputs.databaseName
  }
}

module serviceBusNamespace 'ServiceBus/namespace.bicep' = {
  name: 'serviceBusNamespace'
  params: {
    azureRegion: azureRegion
    environmentName: environmentName
    systemName: systemName
  }
}

module topic 'ServiceBus/topic.bicep' = {
  name: 'serviceBusTopic'
  params: {
    name: 'incoming-minified-urls'
    namespaceName: serviceBusNamespace.outputs.name
  }
}

module processSubscription 'ServiceBus/subscription.bicep' = {
  name: 'processSubscription'
  params: {
    name: 'process'
    namespaceName: serviceBusNamespace.outputs.name
    topicName: topic.outputs.name
  }
}

module invalidateSubscription 'ServiceBus/subscription.bicep' = {
  name: 'invalidateSubscription'
  params: {
    name: 'invalidate${azureRegion}'
    namespaceName: serviceBusNamespace.outputs.name
    topicName: topic.outputs.name
  }
}
