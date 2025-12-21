

## Host Builder 擴充方法

```csharp
IHostBuilderExtensions.UseNMQv3
```

<br>
<br>

## 服務註冊

```csharp
services.AddSingleton<IHostedService, WorkerProcess>();
```

<br>
<br>

## Worker 處理類別

```csharp
internal class WorkerProcess : BackgroundService
```

<br>
<br>

## Worker 處理任務

<br>

```csharp
ask workerProcessTask = GetWorkerProcessTask();
```

<br>
<br>

## 任務執行輸出

<br>

```csharp
Console.WriteLine(DoJob(text).ToWorkerState().GetWorkerStateCode());
```

<br>
<br>

## 處理器執行

```csharp
return _processExecutor.Execute(service, task, _stoppingToken);
```