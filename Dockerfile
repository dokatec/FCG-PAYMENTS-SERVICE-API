FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# 1. Declara o argumento que receberá o token
ARG GH_TOKEN

# 2. Copia o nuget.config
COPY ["nuget.config", "./"]

# 3. Substitui o placeholder no nuget.config pelo token real antes do restore
# Isso garante que o dotnet restore tenha a senha correta
RUN sed -i "s/%GH_TOKEN%/$GH_TOKEN/g" nuget.config

# 4. Copia os arquivos de projeto
COPY ["src/FCG.Payments.API/FCG.Payments.API.csproj", "src/FCG.Payments.API/"]
COPY ["src/FCG.Payments.Domain/FCG.Payments.Domain.csproj", "src/FCG.Payments.Domain/"]
COPY ["src/FCG.Payments.Application/FCG.Payments.Application.csproj", "src/FCG.Payments.Application/"]
COPY ["src/FCG.Payments.Infrastructure/FCG.Payments.Infrastructure.csproj", "src/FCG.Payments.Infrastructure/"]

# 5. Agora o restore terá permissão para baixar a FCG.Shared
RUN dotnet restore "src/FCG.Payments.API/FCG.Payments.API.csproj"

# 6. Copiar o restante do código fonte e compilar
COPY . .
WORKDIR "/app/src/FCG.Payments.API"
RUN dotnet build "FCG.Payments.API.csproj" -c Release -o /app/build

# Estágio 2: Publicação
FROM build AS publish
RUN dotnet publish "FCG.Payments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Runtime (Imagem final leve)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "FCG.Payments.API.dll"]