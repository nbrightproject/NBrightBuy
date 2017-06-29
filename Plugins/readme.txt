On upgrade any plugin config file which ends with "system.config" will be merged with the existing system menu file (/Themes/config/default/menuplugin.xml).

So on upgrade any new system plugins can be merged into existing system level menu. (/Themes/config/default/menuplugin.xml)

Files
=====

menu.config 
-----------
 Main default system menu (will be used for new systems and should include all system menu options)

*system.config 
--------------
 An upgrade file which will upgrade current existing system menu file (/Themes/config/default/menuplugin.xml) with any new menu options specified in this file.
 This file will be deleted after first install and may not exist for all install zip files.

/Themes/config/default/menuplugin.xml
--------------------------------------
 Sytsem level menu which will be used for each portal, unless a portal has it's own version of the menu at portal level.
 (A manual update may be required if the portal level menu requires any new options.)




 N.B.
======

Because the *system.config is deleted after install, the source code keeps this file named as v*-system.configsave.  (e.g. v030501-system.configsave)
To ensure the install works correctly, in the install package this file should be renamed to "v*-system.config" (e.g. v030501-system.config)
  