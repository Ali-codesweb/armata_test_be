# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["armada_test.csproj", "./"]
RUN dotnet restore "armada_test.csproj"

# Copy the rest of the files and build
COPY . .
RUN dotnet build "armada_test.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "armada_test.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy the published output
COPY --from=publish /app/publish .

# Create wwwroot and subdirectories for PDF storage
RUN mkdir -p wwwroot/Assets/SalesInvoice

ENTRYPOINT ["dotnet", "armada_test.dll"]
