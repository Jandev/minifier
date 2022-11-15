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

var accountName = toLower('${systemName}-${environmentName}-${azureRegion}-repository')
var locations = [
  {
    locationName: resourceGroup().location
    failoverPriority: 0
    isZoneRedundant: false
  }
]

resource databaseAccount 'Microsoft.DocumentDB/databaseAccounts@2021-04-15' = {
  name: accountName
  kind: 'GlobalDocumentDB'
  location: resourceGroup().location
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: locations
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
  }
}

output accountName string = databaseAccount.name
