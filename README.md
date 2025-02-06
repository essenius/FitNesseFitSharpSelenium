# Selenium Fixture ![workflow badge](../../actions/workflows/ci.yml/badge.svg) [![CodeScene Code Health](https://codescene.io/projects/57095/status-badges/code-health)](https://codescene.io/projects/57095)
This repo contains a fixture to enable automated testing of web applications using Selenium WebDriver, along with a number of demo FitNesse pages.

As Selenium 3 is considered legacy, Selenium 4 is now used. This implies that the support for WinAppDriver is limited as that can't cope with Selenium 4 a this stage.

## Installing the fixture and the examples
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo).

Differences are:
* Download the repo code as a zip file and extract the contents of the folder `FitNesseFitSharpSelenium-branch`. 
* Go to solution folder: `cd /D %LOCALAPPDATA%\FitNesse\Selenium`
* If you have .NET 8 SDK installed:
    * Build fixture solution: `dotnet build --configuration release SeleniumFixture.sln`
    * Go to fixture folder: `cd SeleniumFixture`
    * Publish fixture: `dotnet publish SeleniumFixture.csproj --output bin\Deploy\net8.0 --framework net8.0 --configuration release`
* If you don't have .NET 8 SDK installed: download `SeleniumFixture.zip` from the latest [release](../../releases) and extract it into `Selenium\SeleniumFixture`
* Install browser drivers and other dependencies that you need (see below).
* Go to the assembly folder: `cd /d %LOCALAPPDATA%\FitNesse\Selenium\SeleniumFixture\bin\Deploy\net8.0`
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.SeleniumSuite.FixtureTestPageSuite?suite

## Installing dependencies

All the settings mentioned in this section can be configured both in `plugins.properties` and on FitNesse pages (e.g. `!define BROWSER {chrome}`).
Settings on test pages overrule the settings in `plugins.properties`.

We'll give the instructions for Windows here, for Mac it should be quite similar.

### Browser Drivers

As of Selenium 4, there is a Selenium Manager which takes care of downloading the right browser drivers. Unfortunately, it doesn't play nice with FitSharp, as it expects the selenium-manager folder under the FitSharp assembly folder rather than under the fixture assembly folder (where it is).

There are three options to work around this:
1. Simplest: set the environment variable `SE_MANAGER_PATH` to the full path of selenium-manager (including the executable itself).
2. Create a symbolic link in the FitSharp assembly folder(s) to the selenium-manager folder in the fixture assembly
3. Prevent that Selenium Manager gets used. You can do that by downloading the drivers yourself into a folder of your choice,
and setting the `DriverFolder` environment variable (or app setting) accordingly. A good location would be `%LOCALAPPDATA%\BrowserDrivers`.
    * [Download the ChromeDriver version](https://googlechromelabs.github.io/chrome-for-testing/) that corresponds to the version of Chrome that you use. 
    * Unblock the ZIP file and extract the contents into that folder.
    * Repeat the process to download drivers for [Edge](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/), [Firefox (GeckoDriver)](https://github.com/mozilla/geckodriver/releases) and [Internet Explorer](https://github.com/SeleniumHQ/selenium/wiki/InternetExplorerDriver) (if you still need that).

The default browser for the test pages is Chrome Headless. If you want to use a different browser set `BROWSER` in `plugins.properties`.
```
BROWSER=Chrome Headless
```

### Test site

If you want to execute the tests, publish the test site in the `SeleniumFixtureTestSite` project to e.g. an azure website, and configure the URL in `TESTSITE`.
```
TESTSITE=http://mytestsite.azurewebsites.net.
```

### Selenium Server

If you want to be able to execute remote Selenium tests, install Selenium Server from https://www.selenium.dev/downloads/ (you will need version 4.11 or a newer version 4 patch if that exists) and if you want to be able to run the unit tests, configure the Selenium Server URL in key `SeleniumServer` in `plugins.properties`. 

```
SeleniumServer=!-http://127.0.0.1:6667-!
```

### Appium

If you want to use Appium Desktop (https://appium.io) install that. Only Appium 2 is supported. You might also need to configure an emulated Android device; Two variables of use here:
* `AppiumServer`: the URL for Appium
* `AndroidDevice`: the ID of the Android device you want to test with.

The demo uses: 
* Lollipop 5.1 with x86, 512MB, 720x1280 WXGA, API 22. 
* Pie 9.0 Pixel 2, API 28

Ensure Appium is up and running and listening before you run the tests

```
AppiumServer=!-http://127.0.0.1:4723-!
AndroidDevice=!-4.7 WXGA API 22-!
```

also make sure that the right driver is installed:
```
appium driver install uiautomator2
```
### WinAppDriver

Install WinAppDriver (https://github.com/microsoft/WinAppDriver) if required, and make sure that Appium has the Windows driver installed.
WinAppDriver support is not complete as Microsoft hasn't upgraded WinAppDriver to use the W3C protocol, so it isn't fully functional under Selenium 4.
For the same reason, you will not be able to run via WinAppDriver directly.

```
appium driver install --source=npm appium-windows-driver
```

### Firefox integrated authentication

If you want to enable Windows Integrated authentication in Firefox, set `Firefox.IntegratedAuthenticationDomain` to the domain you what to enable it for.

```
Firefox.IntegratedAuthentication=mydomain.com
```

## Running the unit tests
If you want to run the unit tests, configure `appsettings.json`:

1. Similarly to plugins.properties for `TestSite` and `RemoteSelenium`.
2. If you are using Firefox and you want to use integrated authentication, set the key `Firefox.IntegratedAuthenticationDomain` to the desired domain.

## Tutorial and Reference
See the [Wiki](../../wiki)

## Contribute
Enter an [issue](../../issues) or provide a [pull request](../../pulls). 
