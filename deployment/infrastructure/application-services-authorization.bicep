param serviceBusNamespaceName string
param cosmosDbAccountName string

param backendPrincipalId string
param frontendPrincipalId string

targetScope = 'resourceGroup'

// Authorization roles
// You can get these by running the command `az role definition list --query "[].{name:name, roleType:roleType, roleName:roleName}" --output tsv`
var serviceBusDataReceiver = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0' // Azure Service Bus Data Receiver
var serviceBusDataSender = '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39' //	Azure Service Bus Data Sender
var cosmosDbDataReader = '00000000-0000-0000-0000-000000000001' // Cosmos DB Data Reader
var cosmosDbDataContributor = '00000000-0000-0000-0000-000000000002' // Cosmos DB Data Contributor

// The repository
resource databaseAccount 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' existing = {
  name: cosmosDbAccountName
}

module repositoryBackendApplicationContributorAuthorization 'DocumentDB/sqlRoleAssignments.bicep' = {
  name: 'repositoryBackendApplicationContributorAuthorization'
  params: {
    accountName: databaseAccount.name
    principalId: backendPrincipalId
    roleDefinitionId: cosmosDbDataContributor
  }
}

module repositoryBackendApplicationReaderAuthorization 'DocumentDB/sqlRoleAssignments.bicep' = {
  name: 'repositoryBackendApplicationReaderAuthorization'
  dependsOn: [ 
    repositoryBackendApplicationContributorAuthorization 
  ]
  params: {
    accountName: databaseAccount.name
    principalId: backendPrincipalId
    roleDefinitionId: cosmosDbDataReader
  }
}

module repositoryFrontendApplicationAuthorization 'DocumentDB/sqlRoleAssignments.bicep' = {
  name: 'repositoryFrontendApplicationAuthorization'
  dependsOn: [ 
    repositoryBackendApplicationContributorAuthorization 
    repositoryBackendApplicationReaderAuthorization
  ]
  params: {
    accountName: databaseAccount.name
    principalId: frontendPrincipalId
    roleDefinitionId: cosmosDbDataReader
  }
}

// The messaging
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' existing = {
  name: serviceBusNamespaceName
}

module serviceBusBackendSenderAuthorization 'Authorization/roleAssignmentsServiceBus.bicep' = {
  name: 'serviceBusBackendSenderAuthorization'
  params: {
    principalId: backendPrincipalId
    roleDefinitionId: serviceBusDataSender
    serviceBusNamespaceName: serviceBusNamespace.name
  }
}
module serviceBusBackendReaderAuthorization 'Authorization/roleAssignmentsServiceBus.bicep' = {
  name: 'serviceBusBackendReaderAuthorization'
  params: {
    principalId: backendPrincipalId
    roleDefinitionId: serviceBusDataReceiver
    serviceBusNamespaceName: serviceBusNamespace.name
  }
}

module serviceBusFrontendAuthorization 'Authorization/roleAssignmentsServiceBus.bicep' = {
  name: 'serviceBusFrontendAuthorization'
  params: {
    principalId: frontendPrincipalId
    roleDefinitionId: serviceBusDataReceiver
    serviceBusNamespaceName: serviceBusNamespace.name
  }
}
