param systemName string
@allowed([
  'lcl'
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param azureRegion string

var name = toLower('${systemName}-${environmentName}-${azureRegion}-bus')

resource namespace 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' = {
  name: name
  location: resourceGroup().location
}

output name string = namespace.name
