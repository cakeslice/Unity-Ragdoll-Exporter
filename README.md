Unity-Ragdoll-Exporter
======================


INSTALLATION:
------------
Download the files.

Place the folder inside the Assets folder.


USAGE:
------------
To export a ragdoll, go to Tools->Ragdoll Exporter, select the **root bone** GameObject of the skeleton and save the file.

To load a ragdoll, go to Tools->Ragdoll Loader, select the **root bone** GameObject of the skeleton and load a ragdoll previously exported.

You can also load a ragdoll at runtime using the script "RagdollLoader.cs". 
Just add the script to a GameObject and then for the field "Ragdoll" select a previously exported ragdoll and for the field "Target" select the root bone GameObject of the target skeleton where you wan't the ragdoll to be loaded.
