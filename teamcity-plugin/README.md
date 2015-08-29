# TeamCity Git Auto Merge Meta-Runner
TeamCity meta-runner for merging Git branches. Similar to the Auto Merge Build Feature but allows for wildcards in destination which allows you 
to merge the current branch into many branches i.e. GitFlow finish release or hotfix.

## How to use

### Installing plugin in TeamCity
Copy the plugin zip (GitAutoMerge-plugin.zip) into the main TeamCity plugins directory, located at _**\<TeamCity Data Directory>**/plugins_. 
It will automatically get unpacked into the Build Agent Tools folder located at _**\<TeamCity Home>**/buildAgent/tools_.

If you are not sure where the home or data directories are located you can find them in the Administration section of TeamCity.

### Add Meta-Runner to Build Configuration

Once the plugin has unpacked you should see _Git Auto Merge_ as an option when you add a new build step in you build configuration.

![New Build Step](https://joncubed.github.io/auto-merge-script/assets/img/teamcity-build-step.png)

If you leave fields empty, the default will be used.

## How to build the plugin

Run build-plugin.ps1 from the teamcity-plugin folder and the plugin will be created in _**./.artifacts**_ as GitAutoMerge-plugin.zip 
````PowerShell
PS c:\source\auto-merge-script\teamcity-plugin>.\build-plugin.ps1
````

