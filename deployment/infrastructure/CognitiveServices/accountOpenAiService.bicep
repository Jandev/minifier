param systemName string
@allowed([
  'lcl'
  'dev'
  'test'
  'acc'
  'prod'
])
param environmentName string
param azureRegion string
param location string = resourceGroup().location

@allowed([
  'S0'
])
param skuName string = 'S0'

targetScope = 'resourceGroup'

var accountName = '${systemName}${environmentName}${azureRegion}aai'
var deploymentName = '${systemName}${environmentName}${azureRegion}ai'

var llmModel = 'text-davinci-003'
var llmModelVersion = '1'

resource openAiService 'Microsoft.CognitiveServices/accounts@2022-12-01' = {
  name: accountName
  location: location
  kind: 'OpenAI'
  sku: {
    name: skuName
  }
}

resource llmDeployment 'Microsoft.CognitiveServices/accounts/deployments@2022-12-01' = {
  name: deploymentName
  parent: openAiService
  properties: {
    model: {
      format: 'OpenAI'
      name: llmModel
      version: llmModelVersion
    }
    scaleSettings: {
      scaleType: 'Standard'
    }
  }
}

output serviceName string = openAiService.name
