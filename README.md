# multitool
.NET WPF app to do things that Windows makes you struggle to do.

# Releases
# 1.0.0.6
## Updates
The name (title) of the different windows will be changed to be clearer to the end user. There will most likely be an option to customize the name (title) of each window in the future (+0.0.1.0).

 - User can download and save files with the Downloader utility. The ability to see the source characters is still possible.
 - The preference file will be changed to .XML from .JSON. The app will still be able to use a .JSON file for now but .XML seems a better options when working with C# and .NET.

## Known bugs
 - Downloader utility corrupts .jpg files (and maybe others) upon download or saving. It seems to be an encoding issue.

## Fixed bugs
 - Weird button sizing behavior the bottom of the power tool window ("Power management" window) from v 1.0.0.5.

## 1.0.0.5
### Updates
Finished preference file parsing (see v..0.4), implementing it to every windows.

### Known bugs
Weird button sizing behavior the bottom of the power tool window ("Power management" window).


## 1.0.0.4
### Updates
Starting to save window "user" properties (width, height, color and other settings).
Currently using a JSON file to save infos, but not functionnal.