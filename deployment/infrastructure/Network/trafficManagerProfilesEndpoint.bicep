param trafficManagerProfileName string
param webAppEndpoints array

resource webAppResources 'Microsoft.Web/sites@2021-02-01' existing = [for i in webAppEndpoints: {
  name: i.webAppNameToAdd
  scope: resourceGroup(i.webAppResourceGroupName)
}]

resource endpoints 'Microsoft.Network/trafficManagerProfiles/azureEndpoints@2018-08-01' = [for (webAppEndpoint, index) in webAppEndpoints: {
  name: '${trafficManagerProfileName}/${webAppResources[index].name}'
  properties: {
    endpointStatus: 'Enabled'
    endpointMonitorStatus: 'Online'
    targetResourceId: webAppResources[index].id
    target: 'https://${webAppResources[index].name}.azurewebsites.net/'
    endpointLocation: webAppResources[index].location
    weight: 1
    priority: webAppEndpoint.priority
  }
}]
