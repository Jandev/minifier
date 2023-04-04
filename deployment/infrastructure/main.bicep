@allowed([
  'lcl'
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string = 'prod'

param primaryLocation string = 'westeurope'

@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the frontend Function App')
param frontendPackageReferenceLocationWeu string
@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the backend Function App')
param backendPackageReferenceLocationWeu string
@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the frontend Function App')
param frontendPackageReferenceLocationWus string
@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the backend Function App')
param backendPackageReferenceLocationWus string
@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the frontend Function App')
param frontendPackageReferenceLocationAus string
@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the backend Function App')
param backendPackageReferenceLocationAus string

param hostname string
param subdomain string

var systemName = 'minifier'
var fullSystemPrefix = '${systemName}-${environmentName}'
var regionWestEuropeName = 'weu'
var regionWestUsName = 'wus'
var regionAustraliaSouthEastName = 'aus'
var fullDomainName = '${subdomain}.${hostname}'
var serviceBusUpdateFrontendTopicSubscriptionNamePrefix = 'updatefrontend'

targetScope = 'subscription'

resource rgInfrastructure 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-infra'
  location: primaryLocation
}

resource rgWestEurope 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionWestEuropeName}'
  location: primaryLocation
}

resource rgWestUs 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionWestUsName}'
  location: 'westus'
}
resource rgAustraliaSouthEast 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionAustraliaSouthEastName}'
  location: 'australiasoutheast'
}

module applicationServices 'application-services.bicep' = {
  scope: rgWestEurope
  name: '${systemName}${regionWestEuropeName}-services'
  params: {
    azureRegion: regionWestEuropeName
    environmentName: environmentName
    systemName: systemName
  }
}

module servicebusSubscriptions 'ServiceBus/subscription.bicep' = [for region in [regionAustraliaSouthEastName, regionWestEuropeName, regionWestUsName]: {
  scope: rgWestEurope
  name: '${systemName}${region}-update-cache-subscriptions'
  params: {
    name: '${serviceBusUpdateFrontendTopicSubscriptionNamePrefix}${region}'
    namespaceName: applicationServices.outputs.serviceBusNamespaceName
    topicName: applicationServices.outputs.serviceBusIncomingUrlTopicName
  }
}]

module applicationWestEurope 'application-infrastructure.bicep' = {
  name: '${systemName}${regionWestEuropeName}-apps'
  params: {
    azureRegion: regionWestEuropeName
    environmentName: environmentName
    systemName: systemName
    frontendPackageReferenceLocation: frontendPackageReferenceLocationWeu
    backendPackageReferenceLocation: backendPackageReferenceLocationWeu
    fullDomainName: fullDomainName
    databaseAccountName: applicationServices.outputs.databaseAccountName
    serviceBusNamespaceName: applicationServices.outputs.serviceBusNamespaceName
    serviceBusUpdateFrontendTopicSubscriptionNamePrefix: serviceBusUpdateFrontendTopicSubscriptionNamePrefix
    slugContainerName: applicationServices.outputs.slugContainerName
    sqlDatabaseName: applicationServices.outputs.sqlDatabaseName
  }
  scope: rgWestEurope
}

module applicationAuthorizationWestEurope 'application-services-authorization.bicep' = {
  scope: rgWestEurope
  name: '${systemName}${regionWestEuropeName}-authorization'
  params: {
    backendPrincipalId: applicationWestEurope.outputs.backendPrincipalId
    cosmosDbAccountName: applicationServices.outputs.databaseAccountName
    frontendPrincipalId: applicationWestEurope.outputs.frontendPrincipalId
    serviceBusNamespaceName: applicationServices.outputs.serviceBusNamespaceName
  }
}

module applicationWestUs 'application-infrastructure.bicep' = {
  name: '${systemName}${regionWestUsName}-apps'
  params: {
    azureRegion: regionWestUsName
    environmentName: environmentName
    systemName: systemName
    frontendPackageReferenceLocation: frontendPackageReferenceLocationWus
    backendPackageReferenceLocation: backendPackageReferenceLocationWus
    fullDomainName: fullDomainName
    databaseAccountName: applicationServices.outputs.databaseAccountName
    serviceBusNamespaceName: applicationServices.outputs.serviceBusNamespaceName
    serviceBusUpdateFrontendTopicSubscriptionNamePrefix: serviceBusUpdateFrontendTopicSubscriptionNamePrefix
    slugContainerName: applicationServices.outputs.slugContainerName
    sqlDatabaseName: applicationServices.outputs.sqlDatabaseName
  }
  scope: rgWestUs
}

module applicationAuthorizationWestUs 'application-services-authorization.bicep' = {
  scope: rgWestEurope
  dependsOn: [
    applicationAuthorizationWestEurope
  ]
  name: '${systemName}${regionWestUsName}-authorization'
  params: {
    backendPrincipalId: applicationWestUs.outputs.backendPrincipalId
    cosmosDbAccountName: applicationServices.outputs.databaseAccountName
    frontendPrincipalId: applicationWestUs.outputs.frontendPrincipalId
    serviceBusNamespaceName: applicationServices.outputs.serviceBusNamespaceName
  }
}

module applicationAustraliaSouthEast 'application-infrastructure.bicep' = {
  name: '${systemName}${regionAustraliaSouthEastName}-apps'
  params: {
    azureRegion: regionAustraliaSouthEastName
    environmentName: environmentName
    systemName: systemName
    frontendPackageReferenceLocation: frontendPackageReferenceLocationAus
    backendPackageReferenceLocation: backendPackageReferenceLocationAus
    fullDomainName: fullDomainName
    databaseAccountName: applicationServices.outputs.databaseAccountName
    serviceBusNamespaceName: applicationServices.outputs.serviceBusNamespaceName
    serviceBusUpdateFrontendTopicSubscriptionNamePrefix: serviceBusUpdateFrontendTopicSubscriptionNamePrefix
    slugContainerName: applicationServices.outputs.slugContainerName
    sqlDatabaseName: applicationServices.outputs.sqlDatabaseName
  }
  scope: rgAustraliaSouthEast
}

module applicationAuthorizationAustraliaSouthEast 'application-services-authorization.bicep' = {
  scope: rgWestEurope
  dependsOn: [
    applicationAuthorizationWestUs
    applicationAuthorizationWestEurope
  ]
  name: '${systemName}${regionAustraliaSouthEastName}-authorization'
  params: {
    backendPrincipalId: applicationAustraliaSouthEast.outputs.backendPrincipalId
    cosmosDbAccountName: applicationServices.outputs.databaseAccountName
    frontendPrincipalId: applicationAustraliaSouthEast.outputs.frontendPrincipalId
    serviceBusNamespaceName: applicationServices.outputs.serviceBusNamespaceName
  }
}

module trafficManagerProfile 'Network/trafficManagerProfiles.bicep' = {
  name: 'trafficManagerProfileModule'
  params: {
    environmentName: environmentName
    systemName: systemName
  }
  scope: rgInfrastructure
}

module endpoints 'Network/trafficManagerProfilesEndpoint.bicep' = {
  name: 'trafficManagerProfileEndpoints'
  dependsOn: [
    trafficManagerProfile
    applicationWestEurope
  ]
  params: {
    trafficManagerProfileName: trafficManagerProfile.outputs.instanceName
    webAppEndpoints: [
      {
        webAppNameToAdd: applicationWestEurope.outputs.frontendFunctionName
        webAppResourceGroupName: rgWestEurope.name
        priority: 1
      }
      {
        webAppNameToAdd: applicationWestUs.outputs.frontendFunctionName
        webAppResourceGroupName: rgWestUs.name
        priority: 2
      }
      {
        webAppNameToAdd: applicationAustraliaSouthEast.outputs.frontendFunctionName
        webAppResourceGroupName: rgAustraliaSouthEast.name
        priority: 3
      }
    ]
  }
  scope: rgInfrastructure
}
