# FitNesseFitSharpSelenium
This repo contains a fixture to enable automated testing of web applications using Selenium WebDriver, along with a number of demo FitNesse pages

# Installing the fixture and the examplesn
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo).

Differences are:
* Download the repo code as a zip file and extract the contents of the folder `FitNesseFitSharpSelenium`. 
* Build command becomes: `dotnet build %LOCALAPPDATA%\FitNesse\Selenium\SeleniumFixture.sln`
* Install browser drivers and Selenium Server (see below).
* Go to folder: `cd /D %LOCALAPPDATA%\FitNesse\Selenium\SeleniumFixture\bin\debug\net5.0`
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.SeleniumSuite.FixtureTestPageSuite?suite

# Installing browser drivers
We'll give the instructions for Windows here, for Mac it should be quite similar.

* Choose a suitable folder that is already in the Path, or create new folder %LOCALAPPDATA%\BrowserDrivers and add that to the Path.
* [Download the ChromeDriver version](https://chromedriver.chromium.org/downloads) that corresponds to the version of Chrome that you use. 
* Unblock the ZIP file and extract the contents into that folder.
* Repeat the process to download drivers for [Edge](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/), [Firefox (GeckoDriver)](https://github.com/mozilla/geckodriver/releases) and [Internet Explorer](https://github.com/SeleniumHQ/selenium/wiki/InternetExplorerDriver) (if you still need that)

# Tutorial and Reference
See the [Wiki](../../wiki)

# Contribute
Enter an issue or provide a pull request. 
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
