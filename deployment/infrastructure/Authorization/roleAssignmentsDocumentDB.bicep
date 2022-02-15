param roleDefinitionId string
param principalId string

param accountName string
param databaseName string
param containerName string

resource documentDbAccount 'Microsoft.DocumentDB/databaseAccounts@2021-10-15' existing = {
  name: accountName
}
resource sqlDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-10-15' existing = {
  name: databaseName
}
resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-10-15' existing = {
  name: containerName
}

resource symbolicname 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(subscription().id, principalId, roleDefinitionId)
  scope: container
  properties: {
    principalId: principalId
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/${roleDefinitionId}'
    principalType: 'ServicePrincipal'
  }
}
