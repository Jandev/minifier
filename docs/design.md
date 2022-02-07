# Solution design

This page will describe how the solution is working and how all services work with eachother. The goal is to create a cheap solution for URL minification using several Azure services.  
I want the solution being able to scale globally and having a very low latency for every user retrieving a minified URL. Creating new minified URLs only has to be done in a single location/region, but we might need this functionality on a global scale later on also.

## Software

From a compute perspective, the software is built using Azure Functions.  
This Azure service offers great flexibility & scaling. Because of the triggers and bindings, it isn't necessary to built a lot of plumbing code.

## Design

A Function App is created to retrieve the redirects for the minified slugs and redirect the user. This Function App can be deployed in multiple regions to achieve the global scale with low latency. This Function App will contain a local cache with minified URLs, so the repository doesn't has to be queried for every request.  
A Function App is created for the management operations, like creation and deletion, of minified URLs. This one only has to be deployed to a single region.  
For messaging, the Azure Service Bus is used and specifically a Topic. A topic allows us to subscribe to the incoming messages from multiple subscribers, which is necessary to invalidate cache of the retrieval Function Apps.  
The repository of choice will be Azure Cosmos DB. This database solution offers a wide range of features which are nice to use, like having the possibility to scale worldwide, have multi-master write support and the change feed is useful to send notifications whenever a record has been persisted.
