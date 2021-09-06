# FitNesseFitSharpSelenium
This repo contains a fixture to enable automated testing of web applications using Selenium WebDriver, along with a number of demo FitNesse pages.

# Getting Started
1. Download FitNesse (http://fitnesse.org) and install it to ```C:\Apps\FitNesse```
2. Install the FitSharp NuGet package with target output directory specified:  
   ```
   nuget install FitSharp -OutputDirectory C:\Apps -ExcludeVersion
   ```
   After executing above command, FitSharp will be installed to ```C:\Apps\FitSharp```.
   
3. Clone the repo to a local folder, e.g. ```C:\Data\FitNesseDemo```. This is the folder where ```plugins.properties``` should be.
```
mkdir c:\data\FitNesseDemo
cd /d c:\data\FitNesseDemo
git clone  https://github.com/essenius/FitNesseFitSharpSelenium .
```
4. Build all projects in the solution SeleniumFixture in Visual Studio Code:
   ```
   cd Selenium
   dotnet build SeleniumFixture.sln -c release
   ```
5. Ensure you have Java installed (1.7 or higher)
6. If required, install Appium and a suitable android emulator. The demo uses KitKat 4.4 with x86, 1GB, 720x1280 Xh-DPI. Ensure Appium is up and running and listening. 
7. If required, install WinAppDriver from https://github.com/microsoft/WinAppDriver and run it. Ensure that it listens to a different port than Appium (by default they use the same port) - e.g. 4727.
8. If required, install Selenium Server (https://www.selenium.dev/downloads/), and let it listen to a unique port (e.g. 6667).
9. Install the necessary browser drivers (https://chromedriver.chromium.org, https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/, https://github.com/mozilla/geckodriver/releases) and ensure they are in the path.
10. If you want to use the demo test site, publish it to e.g. an Azure website
11. Configure the settings in plugins.properties
    1. If you use a different port for Appium than 4723, update ```AppiumServer```.
    2. If you use a different port for WinApp than 4727, update ```WinAppServer```.
    3. If you use a different port for Selenium Server than 6667, update ```SeleniumServer```.
    4. Configure the name of your Android device  in ```AndroidDevice```.
    5. Configure the URL of the demo test page in ```TESTSITE```.
    6. If you want to run the demo on a different default browser than Chrome Headless, update ```BROWSER```.
    7. If you installed FitSharp somwewhere else, Update ```FITSHARP_HOME``` accordingly.
   
    All these settings can also be configured as variables in FitNesse pages. e.g. ```!define BROWSER {chrome}```.
   
12. Start FitNesse with the root repo folder as the data folder, and the test assembly folder as the current directory:

	```
	cd /D C:\Data\FitNesseDemo\Selenium\SeleniumFixtureTest\bin\Release\net5.0

	java -jar C:\Apps\FitNesse\fitnesse-standalone.jar -d c:\data\FitNesseDemo -e 0
	```
    
13. Open a browser and enter the URL http://localhost:8080/FitSharpDemos.SeleniumSuite?suite

14. If you want to run the unit tests, configure appsettings.json:
    1. Similarly to plugins.properties for ```TestSite``` and ```RemoteSelenium```.
    2. If you are using Firefox and you want to use integrated authentication, set the key ```Firefox.IntegratedAuthenticationDomain``` to the desired domain.


# Contribute
Enter an issue or provide a pull request.
