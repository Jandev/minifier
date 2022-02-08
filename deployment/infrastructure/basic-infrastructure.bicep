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

targetScope = 'subscription'

// Creating the resource groups for this service
resource rgWestEurope 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionWestEuropeName}'
  location: 'westeurope'
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
module deploymentContainer 'Storage/container.bicep' = {
  scope: rgWestEurope
  name: 'deploymentContainerWestEurope'
  params: {
    containerName: 'deployments'
    storageAccountName: deploymentStorageWestEurope.outputs.storageAccountName
  }
}

output containerName string = deploymentContainer.outputs.containerName
output storageAccountName string = deploymentContainer.outputs.storageAccountName
