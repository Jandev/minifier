@allowed([
  'lcl'
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param azureRegion string
param systemName string

module  webApiStorageAccount 'Storage/storageAccounts.bicep' = {
  name: 'storageAccountAppDeployStaticSite'
  params: {
    environmentName: environmentName
    systemName: '${systemName}static'
    azureRegion: azureRegion
  }
}
