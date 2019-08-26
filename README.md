# FollowingFileStream

A Filestream implementation to follow a file being written.

Any read operation will not report a zero result while the file is locked for writing operations.

It is usually compared to the "tail -f" approach.


[![Build Status](https://dev.azure.com/manandre/manandre/_apis/build/status/manandre.FollowingFileStream?branchName=master)](https://dev.azure.com/manandre/manandre/_build/latest?definitionId=1&branchName=master)

[![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/manandre/manandre/1)](https://dev.azure.com/manandre/manandre/_build/latest?definitionId=1&branchName=master&view=codecoverage-tab)

[![Sonar Violations (long format)](https://img.shields.io/sonar/violations/manandre_FollowingFileStream?format=long&server=https%3A%2F%2Fsonarcloud.io)](https://sonarcloud.io/dashboard?id=manandre_FollowingFileStream)

![Nuget](https://img.shields.io/nuget/v/FollowingFileStream)

![Nuget](https://img.shields.io/nuget/dt/FollowingFileStream)

![GitHub](https://img.shields.io/github/license/manandre/FollowingFileStream)
