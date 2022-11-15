@allowed([
  'lcl'
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param azureRegion string
param systemName string

targetScope = 'resourceGroup'

var serviceBusIncomingMinifiedUrlsTopicName = 'incoming-minified-urls'
var serviceBusProcessSubscriptionName = 'process'

// The repository
module databaseAccount 'DocumentDB/databaseAccount.bicep' = {
  name: 'databaseAccount'
  params: {
    azureRegion: azureRegion
    environmentName: environmentName
    systemName: systemName
  }
}

module sqlDatabase 'DocumentDB/sqlDatabase.bicep' = {
  name: 'sqlDatabase'
  params: {
    databaseAccountName: databaseAccount.outputs.accountName
    databaseName: systemName
  }
}

module slugContainer 'DocumentDB/minifierContainer.bicep' = {
  name: 'slugContainer'
  params: {
    accountName: databaseAccount.outputs.accountName
    databaseName: sqlDatabase.outputs.databaseName
  }
}

// The messaging
module serviceBusNamespace 'ServiceBus/namespace.bicep' = {
  name: 'serviceBusNamespace'
  params: {
    azureRegion: azureRegion
    environmentName: environmentName
    systemName: systemName
  }
}

module topic 'ServiceBus/topic.bicep' = {
  name: 'serviceBusTopic'
  params: {
    name: serviceBusIncomingMinifiedUrlsTopicName
    namespaceName: serviceBusNamespace.outputs.name
  }
}

module processSubscription 'ServiceBus/subscription.bicep' = {
  name: 'processSubscription'
  params: {
    name: serviceBusProcessSubscriptionName
    namespaceName: serviceBusNamespace.outputs.name
    topicName: topic.outputs.name
  }
}

module invalidateSubscription 'ServiceBus/subscription.bicep' = {
  name: 'invalidateSubscription'
  params: {
    name: 'invalidate${azureRegion}'
    namespaceName: serviceBusNamespace.outputs.name
    topicName: topic.outputs.name
  }
}

output databaseAccountName string = databaseAccount.outputs.accountName
output slugContainerName string = slugContainer.outputs.name
output sqlDatabaseName string = sqlDatabase.outputs.databaseName
output serviceBusNamespaceName string = serviceBusNamespace.outputs.name
