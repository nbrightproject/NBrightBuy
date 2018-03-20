Upgrading NBrightStore (NBStore, NBS) to Open Store
---------------------------------------------------

ONLY UPGRADE IF YOU NEED TOO.

To those that don't know, Open Store is a upgrade to a e-commerce called NBStore.

If you run NBS and want to upgrade to OpenStore, this is posisble but not as simple as installing the new module.
Backward compatibly is broken in some areas.  These areas need special attention on upgrade.

MAKE A BACKUP OF THE LIVE SYSTEM AND RESTORE AS A TEST SYSTEM.  
BEFORE YOU TRY THIS ON A LIVE SYSTEM, TRY IT ON A TEST SYSTEM FIRST.

BEFORE UPGRADE ENSURE YOU HAVE A WORKING PAYMENT GATEWAY FOR OPEN STORE (NBS gateway plugins are NOT compatible with OS gateways plugins.)

Method 1 (RECOMENDED)
--------

The easiest method to upgrade is to create a new store and migrate the data.  
Simply export from NBS and import into Open Store, the import and export of data is compatible.


Method 3
--------

If method 1 is impossible, install the latest verison of Open Source.  

Becuase the module names and templates have changed, you might get some issues with modules.
If you get any issues you should add the new module with settings and delete the old module.

Check the BO>Admin>Plufgins has the correct data, the plugin method has altered, it's been designed to upgrade, but you should check.




