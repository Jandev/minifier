param systemName string
@allowed([
  'lcl'
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string = 'prod'
param azureRegion string = 'weu'

var applicationInsightsName = '${systemName}-${environmentName}-${azureRegion}-ai'

resource applicationInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  kind: 'web'
  location: resourceGroup().location
  name: applicationInsightsName
  properties: {
    Application_Type: 'web'
  }
}

output applicationInsightsName string = applicationInsightsName
output instrumentationKey string = applicationInsights.properties.InstrumentationKey
