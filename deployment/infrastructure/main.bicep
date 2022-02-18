@allowed([
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string = 'prod'

param primaryLocation string = 'westeurope'

@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the frontend Function App')
param frontendPackageReferenceLocation string
@description('The absolute location where the package (zip-file) is located which will be used in the `RUN_FROM_PACKAGE` setting of the backend Function App')
param backendPackageReferenceLocation string
param hostname string
param subdomain string

var systemName = 'minifier'
var fullSystemPrefix = '${systemName}-${environmentName}'
var regionWestEuropeName = 'weu'
var fullDomainName = '${subdomain}.${hostname}'

targetScope = 'subscription'

resource rgInfrastructure 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-infra'
  location: primaryLocation
}

resource rgWestEurope 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${fullSystemPrefix}-${regionWestEuropeName}'
  location: primaryLocation
}

module applicationWestEurope 'application-infrastructure.bicep' = {
  name: '${systemName}${regionWestEuropeName}'
  params: {
    azureRegion: regionWestEuropeName
    environmentName: environmentName
    systemName: systemName
    frontendPackageReferenceLocation: frontendPackageReferenceLocation
    backendPackageReferenceLocation: backendPackageReferenceLocation
  }
  scope: rgWestEurope
}

module trafficManagerProfile 'Network/trafficManagerProfiles.bicep' = {
  name: 'trafficManagerProfileModule'
  params: {
    environmentName: environmentName
    systemName: systemName
  }
  scope: rgInfrastructure
}

module endpoints 'Network/trafficManagerProfilesEndpoint.bicep' = {
  name: 'trafficManagerProfileEndpoints'
  dependsOn: [
    trafficManagerProfile
    applicationWestEurope
  ]
  params: {
    trafficManagerProfileName: trafficManagerProfile.outputs.instanceName
    webAppEndpoints: [
      {
        webAppNameToAdd: applicationWestEurope.outputs.frontendFunctionName
        webAppResourceGroupName: rgWestEurope.name
        priority: 1
      }
    ]
  }
  scope: rgInfrastructure
}
