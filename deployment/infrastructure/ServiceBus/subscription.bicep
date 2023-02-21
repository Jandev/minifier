param namespaceName string
param topicName string
param name string

resource namespace 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: namespaceName
}

resource topic 'Microsoft.ServiceBus/namespaces/topics@2021-11-01' existing = {
  parent: namespace
  name: topicName
}

resource subscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  name: name
  parent: topic
}

output name string = subscription.name
