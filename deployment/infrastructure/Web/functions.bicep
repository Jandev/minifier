param systemName string
@allowed([
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param azureRegion string

param appServicePlanId string

param fullDomainName string = 'skip'

var webAppName = toLower('${systemName}-${environmentName}-${azureRegion}-app')
var subdomainPrefix = (environmentName == 'prod') ? '' : '${environmentName}-'

resource webApp 'Microsoft.Web/sites@2020-12-01' = {
  name: webAppName
  location: resourceGroup().location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      ftpsState: 'Disabled'
      http20Enabled: true
      linuxFxVersion: 'DOTNET|6.0'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource webAppNewCname 'Microsoft.Web/sites/hostNameBindings@2021-02-01' = if (fullDomainName != 'skip') {
  name: '${webAppName}/${subdomainPrefix}${fullDomainName}'
  dependsOn: [
    webApp
  ]
  
  properties: {
    customHostNameDnsRecordType: 'CName'
    siteName: '${subdomainPrefix}${fullDomainName}}'
    hostNameType: 'Verified'
    sslState: 'Disabled'
  }
}

output servicePrincipal string = webApp.identity.principalId
output webAppName string = webAppName
