FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY bin/Release/netcoreapp3.0/ App/
WORKDIR /App
ENV ASPNETCORE_URLS=http://+:44338
ENTRYPOINT ["dotnet", "SampleBatchApi.NETCore.dll"]