# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /helloapp
COPY helloapp/*.csproj .
RUN dotnet restore
COPY helloapp/ .
RUN dotnet publish helloapp.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 80
ENTRYPOINT [ "dotnet", "helloapp.dll" ]
