@allowed([
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string = 'prod'

@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the frontend Function App')
param frontendPackageReferenceLocation string
@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the backend Function App')
param backendPackageReferenceLocation string

var systemName = 'minifier'
var fullSystemPrefix = '${systemName}-${environmentName}'
var regionWestEuropeName = 'weu'

targetScope = 'subscription'

resource rgWestEurope 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionWestEuropeName}'
  location: 'westeurope'
}

module deploymentStorageWestEurope 'Storage/storageAccounts.bicep' = {
  scope: rgWestEurope
  name: 'deploymentStorageWestEurope'
  params: {
    azureRegion: regionWestEuropeName
    environmentName: environmentName
    systemName: systemName
  }
}
module deploymentContainer 'Storage/container.bicep' = {
  scope: rgWestEurope
  name: 'deploymentContainerWestEurope'
  params: {
    containerName: 'deployments'
    storageAccountName: deploymentStorageWestEurope.outputs.storageAccountName
  }
}

module applicationWestEurope 'application-infrastructure.bicep' = {
  name: '${systemName}${regionWestEuropeName}'
  params: {
    azureRegion: regionWestEuropeName
    environmentName: environmentName
    systemName: systemName
    frontendPackageReferenceLocation: frontendPackageReferenceLocation
    backendPackageReferenceLocation: backendPackageReferenceLocation
  }
  scope: rgWestEurope
}
