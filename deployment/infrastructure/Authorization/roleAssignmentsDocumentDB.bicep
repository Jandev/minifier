param roleDefinitionId string
param principalId string

param accountName string

resource documentDbAccount 'Microsoft.DocumentDB/databaseAccounts@2021-10-15' existing = {
  name: accountName
}

resource symbolicname 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(subscription().id, principalId, roleDefinitionId)
  scope: documentDbAccount
  properties: {
    principalId: principalId
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/${roleDefinitionId}'
    principalType: 'ServicePrincipal'
  }
}
