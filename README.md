# Cloud Relay

The Cloud Relay is an open-source and cross-platform Web API which allows to access [ECLYPSE Rest API ](http://eclypseapi.distech-controls.com/) available on ECY Series controllers through [Azure IoT Hub](https://azure.microsoft.com/services/iot-hub/).

It can also access any other custom devices connected to Azure IoT Hub with the [Azure IoT Hub device SDK](https://github.com/Azure/azure-iot-sdks). For more information, see the [Understand and invoke direct methods from IoT Hub](https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-direct-methods) documentation.

## Table of Contents

- [Requirements](#requirements)
- [Installation](#installation)
  - [Active Directory App Registrations](#active-directory-app-registrations)
  - [Resources Deployment](#resources-deployment)
  - [Code Deployment](#code-deployment)
- [Configuration](#configuration)
  - [Cloud Relay API](#cloud-relay-api)
  - [Cloud Relay Function App](#cloud-relay-function-app)
- [Usage](#usage)
- [Additional Information](#additional-information)
- [Logging](#logging)
- [Migrations](#migrations)
- [License](#license)

## Requirements

In order to use a device with the Cloud Relay, the following requirements need to be met/installed:

- ECY Server firmware v1.15 or higher (ECLYPSE)
- Azure IoT Hub Connector enabled controller with proper licensing (ECLYPSE)
- Device is connected to the Azure IoT Hub with a dedicated connection string (for ECLYPSE, this is configured in the IoT section of the controller web page).

In order to install the Cloud Relay components, you need to have the [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) installed or use the [Azure Cloud Shell](https://docs.microsoft.com/azure/cloud-shell/overview). Once this is done, sign in to your Azure account using the following command:

```PowerShell
az login
```

## Installation

### Active Directory App Registrations

The Cloud Relay can make use of your Azure subscription Active Directory to authenticate/authorize incoming requests. To achieve this, you need to register the Cloud Relay as an Active Directory application. To enable additional configuration features, you also need to register any another client applications that wants to make requests to the Cloud Relay. For more information about app registrations, see the [Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app) documentation.

Create the Azure Active Directory application registrations

```PowerShell
az ad app create --display-name CloudRelayAPI

az ad app create --display-name CloudRelayClient
az ad app credential reset --display-name AzureCliGeneratedPwd --id <appId from the CloudRelayClient output> --append
```

Take note of both application registrations `appId` and generated `password`. They will be needed later on.

### Resources Deployment

Azure resources are deployed using the Azure Resource Manager (ARM) templates supplied in asset [relay-arm.zip](https://github.com/DistechControls/CloudRelay/releases/latest). For more information about ARM templates and deployment best practices (CI/CD), see the [Azure Resource Manager template](https://docs.microsoft.com/azure/azure-resource-manager/templates/) documentation.

#### Gateway Resources

Gateway resources deployment consists of creating/updating the following Azure resources:
- Azure IoT Hub
- Azure Storage Account to persist files that go through the IoT Hub

You can skip this step and refer to existing resources in your subscription if you already have an Azure IoT Hub ready to go.

> Note that the Azure IoT Hub Basic tier is not supported. For more information, see [IoT Hub quotas, throttling and limitations](https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-quotas-throttling) documentation.

Deploy the `Distech.CloudRelay.Gateway.json` file into a resource group that serves as the container for the deployed resources. The template includes a `Distech.CloudRelay.Gateway.Parameters.json` parameter file that enables you to customize the deployment.

```PowerShell
az group create --name rg-cloudrelaygateway --location "East US"

az deployment group create `
  --handle-extended-json-format `
  --name CloudRelayGatewayDeployment `
  --resource-group rg-cloudrelaygateway `
  --template-file Distech.CloudRelay.Gateway.json `
  --parameters Distech.CloudRelay.Gateway.Parameters.json
```

#### Compute Resources

Compute resources deployment consists of creating/updating the following Azure resources:
- App Service Plan (an existing resource in your subscription can be referred instead)
- App Service / Web App
- Application Insights (an existing resource in your subscription can be referred instead)
- Function App for IoT Hub file maintenance
- Function App Storage Account

Deploy the `Distech.CloudRelay.Compute.json` file into a resource group that serves as the container for the deployed resources. The template includes a `Distech.CloudRelay.Compute.Parameters.json` parameter file that enables you to customize the deployment.

```PowerShell
az group create --name rg-cloudrelaycompute --location "East US"

az deployment group create `
  --handle-extended-json-format `
  --name CloudRelayComputeDeployment `
  --resource-group rg-cloudrelaycompute `
  --template-file Distech.CloudRelay.Compute.json `
  --parameters Distech.CloudRelay.Compute.Parameters.json
```

### Code Deployment

Code deployment consists of pushing compiled code into previously deployed compute resources.

Deploy the [Distech.CloudRelay.API.zip](https://github.com/DistechControls/CloudRelay/releases/latest) and [Distech.CloudRelay.Functions.zip](https://github.com/DistechControls/CloudRelay/releases/latest) code assets.

```PowerShell
az webapp deployment source config-zip `
  --resource-group rg-cloudrelaycompute `
  --name <webSiteName> `
  --src Distech.CloudRelay.API.zip

az functionapp deployment source config-zip `
  --resource-group rg-cloudrelaycompute `
  --name <functionAppName> `
  --src Distech.CloudRelay.Functions.zip
```

## Configuration

Here's the list of all configurations supported by the applications. Required configurations are defined in the ARM templates and automatically created during the Azure resources deployment process.

### Cloud Relay API

#### Application settings

- `Authentication__Provider`: The name of the authentication provider used to authenticate/authorize incoming requests.
- `Authentication__AzureAD__Instance`: The Azure Active Directory URL used for logging in. 
- `Authentication__AzureAD__TenantId`: The ID of the Azure Active Directory tenant.
- `Authentication__AzureAD__ClientId`: The ID assigned to the API once the app is registered.
- `Authentication__AzureAD__Roles`: The [array](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/index?tabs=basicconfiguration&view=aspnetcore-2.1#bind-an-array-to-a-class-1) of application permissions which specify role-based access using the client application's identity (optional).
- `Authentication__AzureAD__Scopes`: The space-separated list of delegated permissions which specify scope-based access using delegated authorization from the signed-in user (optional).
- `APPINSIGHTS_INSTRUMENTATIONKEY`: The instrumentation key of the Azure Application Insights used for monitoring. 
- `DeviceCommunication__IoTHub__MethodName`: The Azure IoT Hub DirectMethod name that will be invoked by the API.
- `DeviceCommunication__IoTHub__ResponseTimeout`: The number of seconds to wait for a response from a DirectMethod invocation. This value can be overriden on a per request basis using the `responseTimeout` URL query string parameter.
- `DeviceCommunication__IoTHub__MessageSizeThreshold`: The size in bytes above which messages are buffered to file storage prior to being forwarded to the device.
- `FileStorage__DeviceFileUploadFolder`: The Azure storage account container name where devices will upload their files.
- `FileStorage__ServerFileUploadFolder`: The Azure storage account container name where the API will upload its files.

> As of today, the only supported authentication provider is `AzureAD`. Any other provider name will not enforce authentication/authorization and therefore allow anonymous access. If you plan to use your own authentication solution, make sure to implement required components around the Cloud Relay API.

#### Connection strings

- `IoTHub__Service`: The Azure IoT Hub used to forward requests to the device. This connection string needs to be granted with the [Service Connect](https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-security#access-control-and-permissions) permissions.
- `StorageAccount`: The Azure IoT Hub underlying Storage Account used to store files uploaded by/intended to the device.

### Cloud Relay Function App

#### Application Settings

- `APPINSIGHTS_INSTRUMENTATIONKEY`: The instrumentation key of the Azure Application Insights used for monitoring. 
- `FileStorage__ConnectionString`: The Azure IoT Hub underlying Storage Account used to store files uploaded by/intended to the device.
- `FileStorage__DeviceFileUploadFolder`: The Azure storage account container name where devices will upload their files.
- `FileStorage__ServerFileUploadFolder`: The Azure storage account container name where the API will upload its files.
- `FileStorage__ServerFileUploadSubFolder`: The Azure storage account virtual folder name where the API will upload its files. This value must match the API `DeviceCommunication__IoTHub__MethodName` application setting.
- `FileStorage__Cleanup__Disabled`: Whether the automatic blob clean-up is enabled for blobs generated by API queries. Optional, default value is `true` if not set.
- `FileStorage__Cleanup__MinutesExpirationDelay`: The delay, in minutes, for blobs to be considered expired and deleted by the automatic clean-up function. Delay is computed based on blob last modified time. Optional, default value is `10080` minutes (7 days) if not set.

## Usage

Routing a request to an ECLYPSE controller through the Cloud Relay can be acheived in 3 simple steps:

- Remove the scheme and authority information from the original URL and copy the remaining `/path[?query][#fragment]` as a custom header named `Remote-Query` which becomes the URL that will be executed by the ECLYPSE Rest API.
- Replace the request URL by `https://{cloudRelayHostname}/api/v1/devices/{deviceId}/request` where `{deviceId}` is the unique ID used  when creating the IoT Device registration in IoT Hub.
- Replace the Basic authentication scheme in the `Authorization` header by the [Bearer authentication](#request-headers) scheme.

> Routing a request to a custom device is also achieved using the same authentication scheme, request URL and `Remote-Query` custom header which can contain any information relevant to the device.

**Example:**

Original request:

```http
GET https://10.0.0.1/api/rest/v1/info HTTP/1.1
Authorization: Basic bmljZTp0cnk=
Host: 10.0.0.1
```

Original response:

```http
HTTP/1.1 200 OK
Server: nginx/1.12.2
Date: Wed, 06 Nov 2019 17:23:41 GMT
Content-Type: application/json;charset=UTF-8
Content-Length: 428
Connection: keep-alive
Set-Cookie: ECLYPSERESTSESSIONID=1vc4erhbezswk1u7n13x9wnpki;Path=/;Secure;HttpOnly
Expires: Thu, 01 Jan 1970 00:00:00 GMT

{"device":{"href":"\/api\/rest\/v1\/info\/device","name":"device"},"apiRevision":"8.0","bacnet":{"href":"\/api\/rest\/v1\/info\/bacnet","name":"bacnet"},"extensionManagement":{"href":"\/api\/rest\/v1\/info\/extension-management","name":"extensionManagement"},"supportedServices":{"href":"\/api\/rest\/v1\/info\/supported-services","name":"supportedServices"},"network":{"href":"\/api\/rest\/v1\/info\/network","name":"network"}}
```

Same request routed through the Cloud Relay:

```http
GET https://{cloudRelayHostname}/api/v1/devices/ECY-S1000-CD4058/request HTTP/1.1
Authorization: Bearer eyJ0eXAiOiJKV1hdfgthYJhbGciOiJSUzI1NiIsIng1dCIr6IkhsdQzB...
Remote-Query: /api/rest/v1/info
Host: {cloudRelayHostname}
```

Response:

```http
HTTP/1.1 200 OK
Content-Length: 368
Content-Type: application/json
Server: Kestrel
Request-Context: appId=cid-v1:71ac2cd5-1f7a-4782-b09c-fc85c32d447b
Strict-Transport-Security: max-age=2592000
X-Powered-By: ASP.NET
Date: Wed, 06 Nov 2019 17:24:21 GMT

{"device":{"href":"\/api\/rest\/v1\/info\/device","name":"device"},"apiRevision":"8.0","bacnet":{"href":"\/api\/rest\/v1\/info\/bacnet","name":"bacnet"},"extensionManagement":{"href":"\/api\/rest\/v1\/info\/extension-management","name":"extensionManagement"},"supportedServices":{"href":"\/api\/rest\/v1\/info\/supported-services","name":"supportedServices"},"network":{"href":"\/api\/rest\/v1\/info\/network","name":"network"}}
```

### Request headers

Only a subset of the request headers are actually routed to the device, all others are being discarded. Here are the headers currently supported:

- `Accept`
- `Content-Type`
- `Content-Length`

The Cloud Relay makes use of OAuth 2.0 Bearer tokens to authenticate/authorize incoming requests. Not providing this token will result in a `401 Unauthorized` error.

The following example demonstrates how to redeem a Cloud Relay API access token using the identity of the previously registered Cloud Relay client application (client credentials flow). For more information about authorization flows, see the [Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/authentication-scenarios) documentation.

Request:

```http
POST https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded
Host: login.microsoftonline.com

grant_type=client_credentials&client_id={cloudRelayClientAppId}&client_secret={azureCliGeneratedPwd}&scope={cloudRelayApiAppId}/.default
```

Response:

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Content-Length: 1128
Strict-Transport-Security: max-age=31536000; includeSubDomains
Date: Fri, 21 Feb 2020 20:40:38 GMT

{
  "token_type": "Bearer",
  "expires_in": 3599,
  "ext_expires_in": 3599,
  "access_token": "{the access token to use}"
}
```

> Consider using the [Microsoft Authentication Library](https://docs.microsoft.com/azure/active-directory/develop/msal-overview) (MSAL) to easily acquire tokens from the Microsoft Identity Platform with consistent APIs for a number of languages and frameworks.

### Error codes

Here's the list of custom errors that can be generated by the Cloud Relay API:

| Error code | Value | HTTP Status Code | Description |
|:---|:---|:---:|---|
| InvalidResponseTimeout | 100 | 400 | `responseTimeout` specified in the request URL is invalid.
| DeviceNotFound | 1000 | 404 | `deviceId` specified in the request URL does not exist.
| DeviceOperationError | 1001 | 504 | The device operation timed out.
| InvalidResult | 1002 | 502 | Invalid response payload received from the device.
| CommunicationError | 5000 | 503 | An error occured while communicating with the gateway.
| GatewayError | 5001 | 503 | An error occured in the gateway which prevents communication with the device.

## Additional Information

### Execution Context

Request execution on the ECLYPSE controller is handled as its default admin user.

### Scheduled Calls Results

The ECLYPSE controller has the ability to execute requests based on schedules. Execution results are sent back to the IoT Hub messages/events built-in endpoint as device-to-cloud messages. Messages and events are retained by default for 1 day, but configurable for up to 7 days, which is the time frame for reading those messages. For more information, see [Read device-to-cloud messages from the built-in endpoint](https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-messages-read-builtin) documentation. You can also use the [Service Bus Explorer](https://github.com/paolosalvatori/ServiceBusExplorer) to test messages arriving in the IoT Hub.

## Logging

The Cloud Relay makes use of [Azure Application Insights](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview) to monitor applications for availability, performance, usage and errors.

You can configure the applications multiple categories with their logging verbosity with `appsetting.json` or equivalent environment variables. For more information, see the [official documentation](https://docs.microsoft.com/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#configuration).

All categories created by the Cloud Relay API asset are prefixed with `Distech.CloudRelay.API`.  
All categories created by the Cloud Relay Function App asset are prefixed with `Distech.CloudRelay.Functions`.

## Migrations

The next sections contain information and instructions about migration and deployment
process between Cloud Relay versions.

### v1.0 to v1.1

The migrations from .NET Core 2.1 to .NET 6.0 and from Azure Functions runtime
2.x to 4.x involved multiple breaking changes. While trying to avoid breaking changes
at the Cloud Relay itself, several of them resulted in mandatory updates in ARM
templates we were unable to avoid, especially at the function app level.  
If you previously deployed your infrastructure using the templates contained in this
repository, then you will need to re-deploy them according to the instructions contained
in [Installation - Compute Resources](#compute-resources).  
If you manually deployed your resources or throught your own templates, then you
will need to handle the breaking changes accordingly (this list might not be
exhaustive based on the additional functionalities you might be using):
- Cloud Relay API
  - If the API is hosted on a linux app service plan, ensure the runtime stack
is properly set to .NET 6 (`linuxFxVersion`).
- Function app
  - Ensure the function app runtime is properly set to 4.x (`FUNCTIONS_EXTENSION_VERSION`).
  - On Windows hosting, ensure the targeted .NET framework is set to v6.0 (`netFrameworkVersion`).
  - On Linux hosting, ensure the runtime stack
is properly set to .NET 6 (`linuxFxVersion`).
  - Ensure you are not relying anymore on the deprecated Webjob dashboard (`AzureWebJobsDashboard`).

## License

The Cloud Relay is licensed under the [MIT](LICENSE.txt) license.
