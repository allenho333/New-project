FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj src/backend/InterviewShowcase.Api/
RUN dotnet restore src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj

COPY src/backend/InterviewShowcase.Api/ src/backend/InterviewShowcase.Api/
RUN dotnet publish src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet InterviewShowcase.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
