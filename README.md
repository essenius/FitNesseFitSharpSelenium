# FitNesseFitSharpSelenium
This repo contains a fixture to enable automated testing of web applications using Selenium WebDriver, along with a number of demo FitNesse pages

# Getting Started
1. Download FitNesse (http://fitnesse.org) and install it to C:\Apps\FitNesse
1. Download FitSharp (https://github.com/jediwhale/fitsharp) and install it to C:\Apps\FitNesse\FitSharp. Note: at this point there seems to be a compatibility issue with FitSharp 2.7.1, so use 2.7.0 for now.
1. Clone the repo to a local folder (C:\Data\FitNesseDemo)
1. Update plugins.properties to point to the FitSharp folder (if you took other folders than suggested)
1. Open the Seleniumfixture solution in Visual Studio
1. Configure App.config: 

	a. If you want to execute the tests, publish the test site in the SeleniumFixtureTestSite project to an azure website, and configure the URL in key TestSite.

	b. If you want to be able to execute remote Selenium tests, install Selenium Server from https://www.seleniumhq.org/download/ and if you want to be able to run the unit tests, configure the Selenium Server URL in key RemoteSelenium

	c. If you are using Firefox and you want to use integrated authentication, set the key Firefox.IntegratedAuthenticationDomain to the desired domain
1. Install the drivers for the browsers you need (see https://www.seleniumhq.org/download/ as well), and ensure that they can be found via the Path
1. If tou want to use Appium (https://appium.io) and/or WinAppDriver (https://github.com/microsoft/WinAppDriver), install those.
1. Restore the NuGet packages and build all projects (Release)
1. Ensure you have Java installed (1.7 or higher)
1. Update plugins.properties:

	a. Set TESTPAGE to http://your.test.site/Testpage.html (test site URL as per above)

	b. Set SeleniumServer to the URL of your Selenium server (as per above)

	c. Set BROWSER to the browser you want to run the test on (e.g. Chrome Headless)
1. Start FitNesse with the root repo folder as the data folder as well as the current directory:

	cd /D C:\Data\FitNesseDemo
	
	java -jar C:\Apps\FitNesse\fitnesse-standalone.jar -d .
1. Open a browser and enter the URL http://localhost:8080/FitSharpDemos.SeleniumSuite?suite

# Contribute
Enter an issue or provide a pull request.
