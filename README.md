# milvus-sdk-csharp

<div class="column" align="middle">
  <img src="https://img.shields.io/nuget/v/io.milvus"/>
  <img src="https://img.shields.io/nuget/dt/io.milvus"/>
</div>

<div align="middle">
    <img src="milvussharp.png"/>
</div>

C# SDK for [Milvus](https://github.com/milvus-io/milvus).

## Getting Started

**Visual Studio**

Visual Studio 2019  or higher

**NuGet**

IO.Milvus is delivered via NuGet package manager. You can find the package here:
https://www.nuget.org/packages/IO.Milvus/

### Prerequisites

 ![.netstandard-2.0](https://img.shields.io/badge/.netstandard-2.0-red)

### Installing

```
Install-Package IO.Milvus
```

### Examples

Connect to a Milvus server.

```csharp
var milvusClient = new MilvusServiceClient(
    ConnectParam.Create(
    host: "192.168.100.139",
    port: 19531));
```
Disconnect from a Milvus server.

```csharp
milvusClient.Close();
```

Please refer to [Test Project](https://github.com/weianweigan/milvus-sdk-csharp/tree/develop/src/IO.MilvusTests) for more examples.

### Grpc client

You can find code that auto generated by grpc tools in namespace of IO.Milvus.Grpc,then use auto-generated serviceclient to connect server and send request.
```csharp
var defaultClient = MilvusServiceClient.CreateGrpcDefaultClient(
    ConnectParam.Create(
        host: "192.168.100.139",
        port: 19531));
```
### How to use in .net framework

* Windows 11 or later, Windows Server 2022 or later.
* A reference to System.Net.Http.WinHttpHandler version 6.0.1 or later.
* Configure WinHttpHandler on the channel using GrpcChannelOptions.HttpHandler.

```c#
        public GrpcChannel CreateChannel(ConnectParam connectParam)
        {
#if NET461_OR_GREATER
            return GrpcChannel.ForAddress(connectParam.GetAddress(),new GrpcChannelOptions
            {
                HttpHandler = new WinHttpHandler()
            });
#else
            return GrpcChannel.ForAddress(connectParam.GetAddress());
#endif
        }

        public MilvusServiceClient DefaultClient()
        {
             var connectParam = ConnectParam.Create(
                host: "Your Host",
                port: 19531)
             var defaultClient = MilvusServiceClient.CreateGrpcDefaultClient(connectParam,CreateChannel(connectParam));
             return defaultClient;
        }
```

[Document](https://learn.microsoft.com/en-us/aspnet/core/grpc/netstandard?view=aspnetcore-7.0)