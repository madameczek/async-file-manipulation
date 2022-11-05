# async-file-manipulation

Two methods for copying or moving files asynchronously and safely.

Buffer size is doubled comparing to default 81920 bytes, wchich is give optimal results for files about 5-6MB. You can experiment with other values and compare results to file copying methods from `System.IO` namespace.

If overwrite flag is set `false` and destination file exists, copying or moving will throw.
