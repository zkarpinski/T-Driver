## T-Drive 1.1   2015-01-05

Features:

  - Visual list of the user defined settings including section name, server name, user id, folder watched and the move folder.
  - Added support for Medical CME documents.
  - Watchers re-establish after a connection to folder is lost.

Code Changes:

  - Used WatcherEx, see README for link, to handle the multi-event problem and connection issues.
  - Watcher and initial directory queuing skip hidden files.
  - Restructured the classes where DPA is no longer the base class instead AP_Document. Changed functions, names and comments to reflect this.
  - Removed fax, email and mail as derived classes. Redesigned as DeliveryMethod type and stored in base class.
  - WorkFactory and AP_Factory (previously DPA_Factory) were updated to handle these changes.
  - Reworked parser to handle each AP_Document derivatives and return the respective constructed object
  - Used ReSharper to re-factor, cleanup and restructure the solution.
  - Increased work queue size to 100, up from 50.
  - Spell check =)
  

## T-Drive 1.0.1 2014-11-24

Features:

  - Email functionality added using Collaboration Data Objects (CDO)
  - Ability to disable delivery methods for each individual DPA type either by settings or if a method fails.
  - Added simple error logging.
  - Missing settings file, generates a template file.

Code Changes:

  - Settings class is now a static class (globally accessible).
  - Missing settings keys throw an error or use default values.
  - Improved error catching and handling.
  - Watchers and queue list is preserved when user stops and restarts.
  - Settings file checks and error handles.

