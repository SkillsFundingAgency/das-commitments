

This file must be deployed to production so that we get the AppData folder.
The reason for this is this bug in NSB 7.0
https://github.com/Particular/NServiceBus/issues/5117

Even if the default LoggerFactory is switched out (see https://docs.particular.net/nservicebus/logging/)
the start host diagnostic logger in NSB will still call the GetOutputDirectory which will fail if there
is no AppData folder and result in this error message:

	Detected running in a website and attempted to use HostingEnvironment.MapPath("~/App_Data/") to derive 
	the logging path. Failed since path returned (F:\sitesroot\0\App_Data\) does not exist. Ensure this 
	directory is created and restart the endpoint.. To avoid using HostingEnvironment.MapPath to derive 
	the logging directory you can instead configure it to a specific path using LogManager.Use<DefaultFactory>().Directory("pathToLoggingDirectory");

