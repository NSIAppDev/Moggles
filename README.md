# <img src="./MogglesImages/Logo.png" alt="Moggles logo" height="50" width="150" >  

Feature flag management for non development wizards.  

Web application that centralizes feature flags across all applications in an enterprise. Feature flag management application for developers, BAs and everyone involved in the software development lifecycle process.

Developed together with [MogglesClient](https://github.com/NSIAppDev/MogglesClient). 

1. [Features](#features)
2. [Technologies used](#technologies-used)
3. [Setup](#setup)  
 3.1 [Running the application](#running-the-application)  
 3.2 [Deploying the application](#deploying-the-application)  
 3.3 [Tests](#tests)  
 3.4 [Troubleshooting](#troubleshooting)  
4. [License](#license)

## Features

* **Add application.** -> [Go to screenshot](./MogglesImages/AddApplication.PNG)
* **Add environment.** -> [Go to screenshot](./MogglesImages/AddEnv.PNG)  
  * The environment is added to a specific application.
  * A default feature toggle value for the environment can be set.
* **Add feature flag.** -> [Go to screenshot](./MogglesImages/AddFeatureToggle.PNG)  
  * The feature flag is added to a specific application in all environments.
* **Turn on/off feature flags on different environments, edit notes and set feature as accepted by the client.** -> [Go to screenshot](./MogglesImages/EditFeatureToggle.PNG)

:heavy_exclamation_mark: *In order to make use of the following features a [Rabbitmq](https://www.rabbitmq.com/configure.html) machine will need to be setup and ```UseMessaging``` key will need to be set in the application configuration file.* 

 The **message bus url**, **user** and **password** will need to be provided.  
 
#### **Force cache refresh.** -> [Go to screenshot](./MogglesImages/ForceCache.PNG)
  * If the impact of a toggle needs to be visible prior to the new refresh time of the cache, a force cache message can be published by the application. [MogglesClient](https://github.com/NSIAppDev/MogglesClient#force-cache-refresh) will read the message from the queue and it will refresh the cache for the corresponding application and environment. The published message contract can be found [here](./MogglesContracts/RefreshTogglesCache.cs) (*the namespace of the contract class will also have to match*).
  * The cache will be refreshed as soon as the message is published and read from the queue by [MogglesClient](https://github.com/NSIAppDev/MogglesClient#force-cache-refresh).
#### **Show deployed feature toggles.** -> [Go to screenshot](./MogglesImages/ShowDeployedToggles.PNG)  
  * For each environment the application will show the deployed feature toggles such that the team knows when the code was published on each environment (visible in green).
  * The consumer implemented in Moggles will read the message from the queue (published by [MogglesClient](https://github.com/NSIAppDev/MogglesClient#show-deployed-feature-toggles)) and it will update the status of each feature toggle. The expected message contract can be found [here](./MogglesContracts/RegisteredTogglesUpdate.cs) (*the namespace of the contract class will also have to match*).
  * The queue name for this event will need to be provided.

## Technologies used  
* ASP.NET Core, Microsoft SQL Server, MasssTransit, RabbitMQ, Vue.js.

## Setup  
#### Running the application:     
* Node.js needs to be installed.  
* Run ```npm install``` from the project directory (where package.json is located).    
* Run ```npm run dev-full``` from the project directory (where package.json is located).   
* Add the **configuration keys** in appsettings.json.
    
#### **Deploying the application:**  
The application needs to be deployed on a web server, built in ```Release``` mode. 
    
#### **Configuration keys**   
 * Database connection string  
   The application uses a Microsoft SQL Server database created with the Code First approach.
   ```C#
   "ConnectionStrings": {
      "FeatureTogglesConnection": "Server=ServerName;Database=Moggles;Integrated Security=true;Application Name=Moggles"
   }
   ```  
 * Messaging
   ```C#
   "Messaging": {
      "UseMessaging": "true",
      "Url": "rabbitmq://messageBusUrl",
      "Username": "user",
      "Password": "password",
      "QueueName": "my_moggles_deploy_status_queue"
   }
   ```
   If ```UseMessaging``` is set to false the messaging features will not be available.

 * Application insights instrumentation key  
   ```C#
   "ApplicationInsights": {
      "InstrumentationKey": "instrumentation key"
   }
   ```  
 * Custom roles  
   The application uses by default windows authentication (**form based authentication is not implemented**) with role based authorization. This part can be removed by removing the ```ConfigureAuthServices``` method from ```Startup``` and the authorize policy from the ```HomeController```, resulting in anonymous authentication. 
   ```C#
   "CustomRoles": {
      "Admins": "MY_DOMAIN\myAdminGroup"
   }
   ```
#### Tests  
Running the unit tests and API tests don't require any additional setup.  
Running UI tests:
To be added.

#### Troubleshooting  
If ```InvalidOperationException: No authenticationScheme was specified, and there was no DefaultChallengeScheme found.``` exception is thrown when running the application: right click on Moggles project -> Properties -> Debug -> check 'Enable Windows Authentication'.  
  
## License
The project is licensed under the [GNU Affero General Public License v3.0](./LICENSE) 
