# FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
# WORKDIR /app
# EXPOSE 13401
# EXPOSE 443

# FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
# WORKDIR /src
# COPY ["payments-api/payments-api.csproj", "./"]
# RUN dotnet restore "./payments-api.csproj" --disable-parallel
# COPY ./payments-api .
# ENV MYSQL_PASSWD="gitCheckout2022!"
# RUN chmod +x ./tools/build.sh
# RUN ./tools/build.sh
# RUN dotnet build "payments-api.csproj" -c Release -o /app/build

# FROM build AS publish
# RUN dotnet publish "payments-api.csproj" -c Release -o /app/publish

# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "/app/payments-api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:6.0

RUN mkdir -p /app
WORKDIR /app
COPY ./payments-api /app

ENV MYSQL_PASSWD="gitCheckout2022!"
RUN chmod +x ./tools/build.sh
RUN ./tools/build.sh

RUN dotnet restore
RUN dotnet build "payments-api.csproj" -c Release -o /app/build
RUN dotnet publish "payments-api.csproj" -c Release -o /app/publish

EXPOSE 13401

CMD ["dotnet", "/app/publish/payments-api.dll"]