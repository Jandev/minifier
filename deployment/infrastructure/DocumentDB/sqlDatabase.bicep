param databaseAccountName string
param databaseName string

resource databaseAccount 'Microsoft.DocumentDB/databaseAccounts@2021-10-15' existing = {
  name: databaseAccountName
}

resource sqlDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-04-15' = {
  parent: databaseAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

output databaseName string = sqlDatabase.name
