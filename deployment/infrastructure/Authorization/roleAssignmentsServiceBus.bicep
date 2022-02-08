param roleDefinitionId string
param principalId string

param serviceBusNamespaceName string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: serviceBusNamespaceName
}

resource symbolicname 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid('${principalId}${roleDefinitionId}')
  scope: serviceBusNamespace
  properties: {
    principalId: principalId
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/${roleDefinitionId}'
    principalType: 'ServicePrincipal'
  }

}
