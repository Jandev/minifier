param namespaceName string
param name string

resource namespace 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: namespaceName
}

resource topic 'Microsoft.ServiceBus/namespaces/topics@2021-06-01-preview' = {
  parent: namespace
  name: name
}
output name string = topic.name
