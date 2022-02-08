param roleDefinitionId string
param principalId string

param storageAccountName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: storageAccountName
}

resource symbolicname 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(principalId)
  scope: storageAccount
  properties: {
    principalId: principalId
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/${roleDefinitionId}'
    principalType: 'ServicePrincipal'
  }
}
