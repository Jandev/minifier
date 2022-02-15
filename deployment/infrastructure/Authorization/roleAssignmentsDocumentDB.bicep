param roleDefinitionId string
param principalId string

param accountName string

resource documentDbAccount 'Microsoft.DocumentDB/databaseAccounts@2021-10-15' existing = {
  name: accountName
}

resource sqlRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2021-10-15' = {
  name: guid(roleDefinitionId, principalId, documentDbAccount.id)
  parent: documentDbAccount
  properties: {
    principalId: principalId
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions/${documentDbAccount.name}/${roleDefinitionId}'
    scope: documentDbAccount.id
  }
}
