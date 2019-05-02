# <img src="./MogglesImages/Logo.png" alt="Moggles logo" height="50" width="150" >  

Feature flag management for non development wizards.  

Web application that centralizes feature flags across all applications in an enterprise. Feature flag management application for developers, BAs and everyone involved in the software development lifecycle process.

Developed together with [MogglesClient](https://github.com/NSIAppDev/MogglesClient). 

## Features

* **Add application.** -> [Go to screenshot](./MogglesImages/AddApplication.PNG)
* **Add environment.** -> [Go to screenshot](./MogglesImages/AddEnv.PNG)
* **Add feature flag.** -> [Go to screenshot](./MogglesImages/AddFeatureToggle.PNG)
* **Turn on/off feature flags on different environments, edit notes and set feature as accepted by the client.** -> [Go to screenshot](./MogglesImages/EditFeatureToggle.PNG)

:heavy_exclamation_mark: *In order to make use of the following features a [Rabbitmq](https://www.rabbitmq.com/configure.html) machine will need to be setup.*

* **Force cache refresh.** -> [Go to screenshot](./MogglesImages/ForceCache.PNG)
  * If the impact of a toggle needs to be immediate, a force cache message can be published by the application. [MogglesClient](https://github.com/NSIAppDev/MogglesClient) will read the message from the queue and it will refresh the cache for the corresponding application. The published message contract can be found [here](./MogglesContracts/RefreshTogglesCache.cs).
* **Show deployed feature toggles.** -> [Go to screenshot](./MogglesImages/ShowDeployedToggles.PNG)  
  * For each environment the application will show the deployed feature toggles such that the team knows when the code was published on each environment.
  * The queue name for this event will need to be provided.
  * The consumer implemented in Moggles will read the message from the queue (published by [MogglesClient](https://github.com/NSIAppDev/MogglesClient)) and it will update the status of each feature toggle. The expected message contract can be found [here](./MogglesContracts/RegisteredTogglesUpdate.cs).
