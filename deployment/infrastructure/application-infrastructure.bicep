@allowed([
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param azureRegion string
param systemName string

var webAppName = '${systemName}-${environmentName}-${azureRegion}-app'
var backendSystemName = '${systemName}backend'
var webAppNameBackend = '${backendSystemName}-${environmentName}-${azureRegion}-app'

targetScope = 'resourceGroup'


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
    ]
  }
}

output webApplicationName string = functionApp.outputs.webAppName
output webApplicationNameBackend string = functionAppBackend.outputs.webAppName
output resourceGroupLocation string = resourceGroup().location
