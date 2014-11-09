T-Driver
========

__(work in progress)__

![Running](http://zacharykarpinski.com/projects/tdriver/img/tdriver_running.png "T-Driver while running.")

##Purpose
A tool designed to automate the T: Drive with functions such as faxing, emailing and mailing documents created by representatives.

##Features
 * Integration with RightFax to fax out documents.
 * Highly customizable settings using standard INI file.
 * Zero user interaction.
 * Light-weight and portable.

##Requirements
 * [.Net Framework v4.0](http://www.microsoft.com/en-us/download/details.aspx?id=17851)
 * Microsoft Office 2003+
 * Windows XP, Vista, 7, 8 or 8.1
 * RightFax 8.7+

##Usage
 1. Compile or extract the zipped executable `T-Driver.zip`.
 2. Modify the `Settings.ini`, following the supplied template.
 3. Run `T-Driver` and press `Start`.

##Build Requirements
* Visual Studio 2013
* [.Net Framework v4.0](http://www.microsoft.com/en-us/download/details.aspx?id=17851)
* RightFax RFCOMAPI.dll
* [Office 2003 Update: Redistributable Primary Interop](http://support.microsoft.com/kb/897646)
* [Ini File Parser] (https://github.com/rickyah/ini-parser)


##Coming Soon...
 * Email support.
 * Print queuing, for files that are to be printed out manually later.
 * Print mailing labels.
 * Data logging for errors, metrics and general logs.

Copyright (c) 2014 Zachary Karpinski
