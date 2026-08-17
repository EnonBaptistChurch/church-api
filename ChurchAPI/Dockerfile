FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/out .

ENV DOTNET_GCRegionServer=0
ENV DOTNET_gcServer=0

ENTRYPOINT ["dotnet", "ChurchApi.dll"]