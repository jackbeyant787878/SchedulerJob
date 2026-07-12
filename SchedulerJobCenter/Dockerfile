# 基础镜像：运行时
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5060
ENV ASPNETCORE_URLS=http://+:5060

# 构建镜像：SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 复制所有 .csproj 文件（路径相对于根目录）
COPY ["SchedulerJobCenter/SchedulerJobCenter.Api.csproj", "SchedulerJobCenter.Api/"]
COPY ["SchedulerJobCenter.Application/SchedulerJobCenter.Application.csproj", "SchedulerJobCenter.Application/"]
COPY ["SchedulerJobCenter.Domain/SchedulerJobCenter.Domain.csproj", "SchedulerJobCenter.Domain/"]
COPY ["SchedulerJobCenter.Infrastructure/SchedulerJobCenter.Infrastructure.csproj", "SchedulerJobCenter.Infrastructure/"]
COPY ["SchedulerJobCenter.Shared/SchedulerJobCenter.Shared.csproj", "SchedulerJobCenter.Shared/"]

RUN dotnet restore "SchedulerJobCenter.Api/SchedulerJobCenter.Api.csproj"

# 复制所有源代码（相对于根目录）
COPY . .

# 构建并发布 Api 项目
WORKDIR "/src/SchedulerJobCenter.Api"
RUN dotnet build "SchedulerJobCenter.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "SchedulerJobCenter.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 最终阶段
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchedulerJobCenter.Api.dll"]