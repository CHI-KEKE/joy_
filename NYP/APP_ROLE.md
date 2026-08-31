
#### 5.1 CronJob

**deployments.json 的 APP_ROLE**：scheduler

<br>

**DockerFile**：
```dockerfile
WORKDIR /worker
COPY ./src/Nine1.Promotion.CreateTask .
```

<br>

#### 5.2 WebAPI

**deployments.json 的 APP_ROLE**：api

<br>

**DockerFile**：

Building：
```dockerfile
WORKDIR /src
COPY --from=buildTranslation ./src .
COPY ./workdir/${NYP_JOBNAME}/releng.json .
RUN cat ./releng.json
```

<br>

Deploy：
```dockerfile
WORKDIR /app
COPY --from=dotnet-build-env /out .
```

<br>

**檔案結構**：
```
./src/Web/{{FQDN}}/csproj
./src/sln
```

<br>

#### 5.3 Worker

**deployments.json 的 APP_ROLE**：worker

<br>

**DockerFile**：
```dockerfile
WORKDIR /worker
COPY --from=dotnet-build-env /app/out .
```

<br>

**檔案結構**：
```
./src/sln
```

<br>

#### 5.4 Console

**deployments.json 的 APP_ROLE**：scheduler

<br>

**DockerFile**：

Build：
```dockerfile
COPY ./Nine1.Payment.Middleware.ConsoleApp.sln ./
COPY ./src ./src
```

<br>

Deploy：
```dockerfile
WORKDIR /app
COPY --from=dotnet-build-env /out .
```

<br>

**檔案結構**：
```
./src/{{FQDB}}/csproj
sln
```