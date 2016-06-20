cd "C:\Users\veg\Documents\Visual Studio 2015\Projects\GameStorageClientAsset\GameStorageClientAsset\bin\Debug"
cmd /c "c:\Program Files (x86)\Mono\bin\pdb2mdb.bat" ModelAsset.dll
cmd /c "c:\Program Files (x86)\Mono\bin\pdb2mdb.bat" RageAssetManager.dll
cmd /c copy *.dll "C:\Users\Public\Documents\Unity Projects\UITests\Assets"
cmd /c copy *.dll.mdb "C:\Users\Public\Documents\Unity Projects\UITests\Assets"