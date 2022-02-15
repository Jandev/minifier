param accountName string
param databaseName string

@description('The throughput for the container')
@minValue(400)
@maxValue(1000000)
param throughput int = 400

var containerName = 'urls'

resource databaseAccount 'Microsoft.DocumentDB/databaseAccounts@2021-10-15' existing = {
  name: accountName
}
resource sqlDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-04-15' existing = {
  name: databaseName
  parent: databaseAccount
}

resource urlContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-04-15' = {
  parent: sqlDatabase
  name: containerName
  properties: {
    resource: {
      id: containerName
      partitionKey: {
        paths: [
          '/slug'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/myPathToNotIndex/*'
          }
        ]
      }
    }
    options: {
      throughput: throughput
    }
  }
}

output name string = containerName
