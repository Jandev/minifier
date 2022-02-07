param systemName string
@allowed([
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param azureRegion string

param kind string = 'app'

param sku object = {
  name: 'Y1'
  tier: 'Dynamic'
}

var servicePlanName = toLower('${systemName}-${environmentName}-${azureRegion}-plan')

resource appFarm 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: servicePlanName
  location: resourceGroup().location
  kind: kind
  sku: sku
  properties: {
    reserved: true
  }
}

output servicePlanName string = servicePlanName
output id string = appFarm.id
