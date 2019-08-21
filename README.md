# FollowingFileStream

A Filestream implementation to follow a file being written.

Any read operation will not report a zero result until the file is locked for writing operations.

It is usually compared to the "tail -f" approach.


[![Build Status](https://dev.azure.com/manandre/manandre/_apis/build/status/manandre.FollowingFileStream?branchName=master)](https://dev.azure.com/manandre/manandre/_build/latest?definitionId=1&branchName=master)
