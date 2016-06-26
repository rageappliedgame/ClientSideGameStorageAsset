# ClientSideGameStorageAsset
RAGE T2.4c - Client Side Game Storage Asset

This asset allows a developer to define and store (model)data in a tree structure. 

Each node in such model, can define its own the datatype and preferred storage location (or inherited the location).

Structure and data are stored separately to allow restoring a model from multiple storage locations.

As storage locations: the RAGE Game Storage Server, Local Storage and In-Game Storage (read-only) 
are forseen as well as marking data to be transient (so not persisted).

The asset currently supports two storage formats for it's data: Json and Xml. Json is to be used for storage 
at the RAGE Game Storage Server and Local Storage, Xml can be used by Local Storage only.

Both formats are lossless so not only the values are restored but also the exact data type. 

Without this extra code especcially Json makes a guess and turns all integer numbers in 64 bits ones. 
Xml Serialization tends to turn all generic lists into arrays.

Note: Binary serialization was forseen also but has issues with PCL (Portable Class Libraries) 
      and is therefor currently disabled.

Note: The structure is at the moment stored as base64 encoding Xml when using the RAGE Game Storage Server. 
      This might change in the near future.
