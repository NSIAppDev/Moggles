# Moggles
Feature flag management for non development wizards.  
<img src="https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesImages/Logo.png" alt="Moggles logo" height="100" width="300" >  

Web application that centralizes feature flags across all applications in an enterprise. Feature flag management application for developers, BAs and everyone involved in the software development lifecycle process.

Developed together with [MogglesClient](https://github.com/NSIAppDev/MogglesClient/blob/dev/README.md). 

## Features

* **Add application.** -> [Go to screenshot](https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesImages/AddApplication.PNG)
* **Add environment.** -> [Go to screenshot](https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesImages/AddEnv.PNG)
* **Add feature flag.** -> [Go to screenshot](https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesImages/AddFeatureToggle.PNG)
* **Turn on/off feature flags on different environments, edit notes and set feature as accepted by the client.** -> [Go to screenshot](https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesImages/EditFeatureToggle.PNG)

:heavy_exclamation_mark: *In order to make use of the following features a [Rabbitmq](https://www.rabbitmq.com/configure.html) machine will need to be setup.*

* **Force cache refresh.** -> [Go to screenshot](https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesImages/ForceCache.PNG)
  * If the impact of a toggle needs to be immediate, a force cache message can be published by the application. [MogglesClient](https://github.com/NSIAppDev/MogglesClient/blob/dev/README.md) will read the message from the queue and it will refresh the cache for the corresponding application. The published message contract can be found [here](https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesContracts/RefreshTogglesCache.cs).
* **Show deployed feature toggles.** -> [Go to screenshot](https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesImages/ShowDeployedToggles.PNG)  
  * For each environment the application will show the deployed feature toggles such that the team knows when the code was published on each environment.
  * The queue name for this event will need to be provided.
  * The consumer implemented in Moggles will read the message from the queue and it will update the status of each feature toggle. The expected message contract can be found [here](https://github.com/NSIAppDev/Moggles/blob/PBI54747/MogglesContracts/RegisteredTogglesUpdate.cs).
