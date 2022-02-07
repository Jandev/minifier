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

resource rgWestEurope 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionWestEuropeName}'
  location: 'westeurope'
}

module applicationWestEurope 'application-infrastructure.bicep' = {
  name: '${systemName}${regionWestEuropeName}'
  params: {
    azureRegion: regionWestEuropeName
    environmentName: environmentName
    systemName: systemName
  }
  scope: rgWestEurope
}
