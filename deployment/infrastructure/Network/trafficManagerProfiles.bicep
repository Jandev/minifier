param systemName string
@allowed([
  'lcl'
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param relativeLiveEndpoint string = '/api/Live'

var trafficManagerProfileName = '${systemName}-${environmentName}'

resource trafficManagerProfile 'Microsoft.Network/trafficmanagerprofiles@2018-08-01' = {
  name: trafficManagerProfileName
  location: 'global'
  properties: {
    dnsConfig: {
      relativeName: trafficManagerProfileName
      ttl: 60
    }
    profileStatus: 'Enabled'
    trafficRoutingMethod: 'Performance'
    monitorConfig: {
      profileMonitorStatus: 'Online'
      protocol: 'HTTPS'
      port: 443
      path: relativeLiveEndpoint
      timeoutInSeconds: 10
      intervalInSeconds: 30
      toleratedNumberOfFailures: 3
    }
  }
}

output instanceName string = trafficManagerProfileName
