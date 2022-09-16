@allowed([
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string = 'prod'

var systemName = 'minifier'
var fullSystemPrefix = '${systemName}-${environmentName}'
var regionWestEuropeName = 'weu'
var regionWestUsName = 'wus'
var regionAustraliaSouthEastName = 'aus'

targetScope = 'subscription'

// Creating the resource groups for this service
resource rgWestEurope 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionWestEuropeName}'
  location: 'westeurope'
}
resource rgWestUs 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionWestUsName}'
  location: 'westus'
}
resource rgAustraliaSouthEast 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionAustraliaSouthEastName}'
  location: 'australiasoutheast'
}

// The deployment storage account, holding all assets necessary for deployment
module deploymentStorageWestEurope 'Storage/storageAccounts.bicep' = {
  scope: rgWestEurope
  name: 'deploymentStorageWestEurope'
  params: {
    azureRegion: regionWestEuropeName
    environmentName: environmentName
    systemName: '${systemName}deploy'
  }
}
module deploymentContainerWestEurope 'Storage/container.bicep' = {
  scope: rgWestEurope
  name: 'deploymentContainerWestEurope'
  params: {
    containerName: 'deployments'
    storageAccountName: deploymentStorageWestEurope.outputs.storageAccountName
  }
}

module deploymentStorageWestUs 'Storage/storageAccounts.bicep' = {
  scope: rgWestUs
  name: 'deploymentStorageWestUs'
  params: {
    azureRegion: regionWestUsName
    environmentName: environmentName
    systemName: '${systemName}deploy'
  }
}
module deploymentContainerWestUs 'Storage/container.bicep' = {
  scope: rgWestUs
  name: 'deploymentContainerWestUs'
  params: {
    containerName: 'deployments'
    storageAccountName: deploymentStorageWestUs.outputs.storageAccountName
  }
}

module deploymentStorageAustraliaSouthEast 'Storage/storageAccounts.bicep' = {
  scope: rgAustraliaSouthEast
  name: 'deploymentStorageAustraliaSouthEast'
  params: {
    azureRegion: regionAustraliaSouthEastName
    environmentName: environmentName
    systemName: '${systemName}deploy'
  }
}
module deploymentContainerAustraliaSouthEast 'Storage/container.bicep' = {
  scope: rgAustraliaSouthEast
  name: 'deploymentContainerAustraliaSouthEast'
  params: {
    containerName: 'deployments'
    storageAccountName: deploymentStorageAustraliaSouthEast.outputs.storageAccountName
  }
}
