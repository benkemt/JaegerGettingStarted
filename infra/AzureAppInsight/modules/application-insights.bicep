@description('The name of the Application Insights resource')
param name string

@description('The location of the Application Insights resource')
param location string = resourceGroup().location

@description('Tags to apply to the Application Insights resource')
param tags object = {}

@description('The resource ID of the Log Analytics workspace')
param logAnalyticsWorkspaceId string

@description('The type of Application Insights resource')
@allowed([
  'web'
  'other'
])
param kind string = 'web'

@description('Disable IP masking for Application Insights')
param disableIpMasking bool = false

@description('Sampling percentage for Application Insights')
@minValue(0)
@maxValue(100)
param samplingPercentage int = 100

@description('Data retention in days')
@minValue(30)
@maxValue(730)
param retentionInDays int = 90

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: kind
  properties: {
    Application_Type: kind
    WorkspaceResourceId: logAnalyticsWorkspaceId
    DisableIpMasking: disableIpMasking
    SamplingPercentage: samplingPercentage
    RetentionInDays: retentionInDays
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

output id string = applicationInsights.id
output name string = applicationInsights.name
output connectionString string = applicationInsights.properties.ConnectionString
output instrumentationKey string = applicationInsights.properties.InstrumentationKey
