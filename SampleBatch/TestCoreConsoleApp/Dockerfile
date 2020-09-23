FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY bin/Release/netcoreapp3.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "SampleBatchApi.NETCore.dll"]