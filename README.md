# FitNesseFitSharpSelenium
This repo contains a fixture to enable automated testing of web applications using Selenium WebDriver, along with a number of demo FitNesse pages.
For the moment it still depends on Selenium 3 as it seems WinAppDriver can't cope with Selenium 4 just yet.

# Installing the fixture and the examplesn
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo).

Differences are:
* Download the repo code as a zip file and extract the contents of the folder `FitNesseFitSharpSelenium`. 
* You will need to use Visual Studio Enterprise to build the solution since the test prooject uses Fakes. You can work around that by building SeleniumFixture only, and manually copying in the package libraries to the dll folder (and then using that one as "go to folder" below).
* Install browser drivers and other dependencies that you need (see below).
* Go to folder: `cd /D %LOCALAPPDATA%\FitNesse\Selenium\SeleniumFixtureTest\bin\debug\net5.0`
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.SeleniumSuite.FixtureTestPageSuite?suite

# Installing dependencies

All the settings mentioned in this section can be configured both in `plugins.properties` and on FitNesse pages (e.g. `!define BROWSER {chrome}`).
Settings on test pages overrule the settings in `plugins.properties`.

We'll give the instructions for Windows here, for Mac it should be quite similar.

## Browser Drivers

* Choose a suitable folder that is already in the Path, or create new folder `%LOCALAPPDATA%\BrowserDrivers` and add that to the `Path`.
* [Download the ChromeDriver version](https://chromedriver.chromium.org/downloads) that corresponds to the version of Chrome that you use. 
* Unblock the ZIP file and extract the contents into that folder.
* Repeat the process to download drivers for [Edge](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/), [Firefox (GeckoDriver)](https://github.com/mozilla/geckodriver/releases) and [Internet Explorer](https://github.com/SeleniumHQ/selenium/wiki/InternetExplorerDriver) (if you still need that)

The default browser for the test pages is Chrome Headless. If you want to use a different browser set `BROWSER` in `plugins.properties`.
```
BROWSER=Chrome Headless
```

## Test site

If you want to execute the tests, publish the test site in the `SeleniumFixtureTestSite` project to e.g. an azure website, and configure the URL in `TESTSITE`.
```
TESTSITE=http://mytestsite.azurewebsites.net.
```

## Selenium Server

If you want to be able to execute remote Selenium tests, install Selenium Server from https://www.selenium.dev/downloads/ (you will need version 3.141.59 or a newer version 3 patch if that exists) and if you want to be able to run the unit tests, configure the Selenium Server URL in key `SeleniumServer` in `plugins.properties`. 

```
SeleniumServer=!-http://127.0.0.1:6667-!
```

## Appium

If tou want to use Appium Desktop (https://appium.io) install that. You might also need to configure an emulated Android device; Three variables of use here:
* `AppiumServer`: the URL for Appium
* `AndroidDevice`: the ID of the Android device you want to test with.

The demo uses KitKat 4.4 with x86, 1GB, 720x1280 Xh-DPI. Ensure Appium is up and running and listening before you run the tests

```
AppiumServer=!-http://127.0.0.1:4723-!
AndroidDevice=!-XH-DPI 4.65in Kit Kat 4.4-!
```

## WinAppDriver

Install WinAppDriver (https://github.com/microsoft/WinAppDriver) if required. Make sure that it listens to a different port than Appium (by default they listen to the same port).

```
WinAppServer=!-http://127.0.0.1:4727-!
```

## Firefox integrated authentication

If you want to enable Windows Integrated authentication in Firefox, set `Firefox.IntegratedAuthenticationDomain` to the domain you wnat to enable it for.

```
Firefox.IntegratedAuthentication=mydomain.com
```

# Running the unit tests
If you want to run the unit tests, configure `appsettings.json`:
    1. Similarly to plugins.properties for `TestSite` and `RemoteSelenium`.
    2. If you are using Firefox and you want to use integrated authentication, set the key `Firefox.IntegratedAuthenticationDomain` to the desired domain.

# Tutorial and Reference
See the [Wiki](../../wiki)

# Contribute
Enter an [issue](../../issues) or provide a [pull request](../../pulls). 
