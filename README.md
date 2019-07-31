![Logo](https://raw.githubusercontent.com/NSIAppDev/Moggles/master/MogglesImages/Logo.png)
# Feature flag management for non development wizards

Feature flags are awesome, they help us separate the technical process (deploying a feature) from the business process (activating a feature).  What isn't awesome is having to redeploy code to activate a feature or modifying live code or a database value.  These steps are very technical and sometimes go against CI/CD principals.  There has to be a better way!

Imagine you just nailed your client demo. They are all excited about this new feature, and they ask when it can go live.  Do you want to tell them "let me fire the code, do a new release" or "I need to modify a web.config file"?  What if you or your client could just go to a website, click a couple of buttons and instantly your feature was activated? Wouldn't that be better?  

Moggles is just that solution.  We created a web interface, API and client package that can handle all of that (and more).
![Demo](https://raw.githubusercontent.com/NSIAppDev/Moggles/master/MogglesImages/MogglesDemo.GIF)

Moggles comes in 2 parts, the server/web interface and the client package (available as [NuGet package](https://www.nuget.org/packages/MogglesClient/)).  Using our code, you setup your own private server and configure the Application, Environment and Features.  Next, install the [NuGet package](https://www.nuget.org/packages/MogglesClient/) in your .NET application (ASP.NET and .NET Core are both supported), register the service, and create a new class for your feature (we think class based toggles are the best). Now you can control your feature from anywhere.

Detailed setup and usage instructions are available on the [Wiki](https://github.com/NSIAppDev/Moggles/wiki).
