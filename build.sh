#!/usr/bin/env sh
# vi: set tw=0

dotnet pack &&\
dotnet test -p:CollectCoverage=true -p:CoverletOutputFormat=opencover
