## T-Driver 1.1.2   2015-01-09

Features:

  - Added support for NR DPAs.

Code Changes:

   - Medicals are skipped when no zip code is found.
   - Default rightfax comment no longer shows revision number. ie: This would show 1.1.2 instead of 1.1.2.42873
   - Added separate parser for NR DPAs.
   - Other minor code changes added to fully support NR DPAs.

Bug Fix:

   - No longer crashes when file is in use when the parser loads the file.

## T-Driver 1.1.1   2015-01-07

Features:

  - Added Visual list of the user defined settings including section name, server name, user id, folder watched and the move folder.
  - Added support for Medical CME documents.
  - Watchers re-establish after a connection to folder is lost.
  - Added region specific handles.
  - CMEs are checked for zip code validity based on region.

Code Changes:

  - Used WatcherEx, see README for link, to handle the multi-event problem and connection issues.
  - Watcher and initial directory queuing skip hidden files.
  - Restructured the classes where DPA is no longer the base class instead AP_Document. Changed functions, names and comments to reflect this.
  - Removed fax, email and mail as derived classes. Redesigned as DeliveryMethod type and stored in base class.
  - WorkFactory and AP_Factory (previously DPA_Factory) were updated to handle these changes.
  - Reworked parser to handle each AP_Document derivatives and return the respective constructed object
  - Used ReSharper to re-factor, cleanup and restructure the solution.
  - Increased work queue size to 100, up from 50.
  - Added new region class with Regions as static container of each valid region.
  - Spell check =)
  

## T-Driver 1.0.1 2014-11-24

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

