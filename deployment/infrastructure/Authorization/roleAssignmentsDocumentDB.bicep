param roleDefinitionId string
param principalId string

param accountName string

resource documentDbAccount 'Microsoft.DocumentDB/databaseAccounts@2021-10-15' existing = {
  name: accountName
}

resource sqlRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2021-10-15' = {
  name: 'sqlRoleAssignment'
  parent: documentDbAccount
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions', roleDefinitionId)
    scope: documentDbAccount.id
  }
}
