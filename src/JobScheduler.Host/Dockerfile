FROM mcr.microsoft.com/dotnet/sdk:8.0 AS net-builder

# install diagnostics tools
RUN mkdir /tools
RUN dotnet tool install --tool-path /tools dotnet-trace
RUN dotnet tool install --tool-path /tools dotnet-counters
RUN dotnet tool install --tool-path /tools dotnet-dump

WORKDIR /src
# Copy the main source project files
COPY ["jag-jobscheduler.sln", ".editorconfig", "Directory.Build.props", "Directory.Packages.props", "global.json", "./"]
COPY */*.csproj ./
RUN cat jag-jobscheduler.sln \
| grep "\.csproj" \
| awk '{print $4}' \
| sed -e 's/[",]//g' \
| sed 's#\\#/#g' \
| xargs -I {} sh -c 'mkdir -p $(dirname {}) && mv $(basename {}) $(dirname {})/'

RUN dotnet restore "JobScheduler.Host/JobScheduler.Host.csproj"
COPY . .
RUN dotnet publish "JobScheduler.Host/JobScheduler.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
FROM registry.access.redhat.com/ubi8/dotnet-80-runtime AS final
WORKDIR /app
# copy diagnostics tools
WORKDIR /tools
COPY --from=net-builder /tools .
# copy app
WORKDIR /app
COPY --from=net-builder /app/publish .
ENTRYPOINT ["dotnet", "JobScheduler.Host.dll"]
