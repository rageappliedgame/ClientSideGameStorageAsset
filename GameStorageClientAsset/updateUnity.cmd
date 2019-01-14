cd "C:\Rage\ClientSideGameStorageAsset\GameStorageClientAsset\bin\Debug"
cmd /c "C:\Program Files (x86)\Mono\bin\pdb2mdb.bat" GameStorageClientAsset.dll
cmd /c "C:\Program Files (x86)\Mono\bin\pdb2mdb.bat" RageAssetManager.dll
cmd /c copy *.dll "C:\Unity\CliensSideGameStorage Demo\Assets"
cmd /c copy *.dll.mdb "C:\Unity\CliensSideGameStorage Demo\Assets"