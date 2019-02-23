# FitNesseFitSharpSelenium
This repo contains a fixture to enable automated testing of web applications using Selenium WebDriver, along with a number of demo FitNesse pages

# Getting Started
1. Download FitNesse (http://fitnesse.org) and install it to C:\Apps\FitNesse
2. Download FitSharp (https://github.com/jediwhale/fitsharp) and install it to C:\Apps\FitNesse\FitSharp.
3. Clone the repo to a local folder (C:\Data\FitNesseDemo)
4. Update plugins.properties to point to the FitSharp folder (if you took other folders than suggested)
5. Open the Seleniumfixture solution
6. Configure App.config: 

	a. If you want to execute the tests, publish the test site in the SeleniumFixtureTestSite project to an azure website, and configure the URL in key TestSite.

	b. If you want to be able to execute remote Selenium tests, install Selenium Server from https://www.seleniumhq.org/download/ and configure the Selenium Server URL in key RemoteSelenium

	c. If you are using Firefox and you want to use integrated authentication, set the key Firefox.IntegratedAuthenticationDomain to the desired domain
7. Install the browser drivers for the browsers you need (see https://www.seleniumhq.org/download/ as well), and ensure that they can be found via the Path
8. Restore the NuGet packages and build all projects (Release)
9. Ensure you have Java installed (1.7 or higher)
10. Update plugins.properties:

	a. Set TESTPAGE to http://your.test.site/Testpage.html (test site URL as per 6a)

	b. Set SeleniumServer to the URL of your Selenium server (as per 6b)

	c. Set BROWSER to the browser you want to run the test on (e.g. Chrome Headless)
11. Start FitNesse with the root repo folder as the data folder as well as the current directory:
	cd /D C:\Data\FitNesseDemo
	java -jar C:\Apps\FitNesse\fitnesse-standalone.jar -d .
12. Open a browser and enter the URL http://localhost:8080/FitSharpDemos.SeleniumSuite?suite

# Contribute
Enter an issue or provide a pull request.