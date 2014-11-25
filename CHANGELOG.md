## T-Drive 1.2 2014-11-24

Features:

  - Email functionality added using Collaboration Data Objects (CDO)
  - Ability to disable delivery methods for each individual DPA type either by settings or if a method fails.
  - Added simple error loggging.
  - Missing settings file, generates a template file.

Code Changes:

  - Settings class is now a static class (globally accessible).
  - Missing settings keys throw an error or use default values.
  - Improved error catching and handling.
  - Watchers and queue list is preserved when user stops and restarts.
  - Settings file checks and error handles.

