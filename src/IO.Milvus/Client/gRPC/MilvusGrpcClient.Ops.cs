﻿using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using IO.Milvus.Diagnostics;
using IO.Milvus.ApiSchema;

namespace IO.Milvus.Client.gRPC;

public partial class MilvusGrpcClient
{
    ///<inheritdoc/>
    public async Task<long> ManualCompactionAsync(
        long collectionId, 
        DateTime? timetravel = null,
        CancellationToken cancellationToken = default)
    {
        this._log.LogDebug("Manual compaction {1}", collectionId);

        Grpc.ManualCompactionRequest request = ManualCompactionRequest
            .Create(collectionId)
            .WithTimetracel(timetravel)
            .BuildGrpc();

        Grpc.ManualCompactionResponse response = await _grpcClient.ManualCompactionAsync(request);

        if (response.Status.ErrorCode != Grpc.ErrorCode.Success)
        {
            this._log.LogError("Manual compaction failed: {0}, {1}", response.Status.ErrorCode, response.Status.Reason);
            throw new MilvusException(response.Status);
        }

        return response.CompactionID;
    }

    ///<inheritdoc/>
    public async Task<CompactionState> GetCompactionStateAsync(
        long compactionId, 
        CancellationToken cancellationToken = default)
    {
        this._log.LogDebug("Get compaction state: {1}", compactionId);

        Grpc.GetCompactionStateRequest request = GetCompactionStateRequest
            .Create(compactionId)
            .BuildGrpc();

        Grpc.GetCompactionStateResponse response = await _grpcClient.GetCompactionStateAsync(request);

        if (response.Status.ErrorCode != Grpc.ErrorCode.Success)
        {
            this._log.LogError("Failed get compaction state: {0}, {1}", response.Status.ErrorCode, response.Status.Reason);
            throw new MilvusException(response.Status);
        }

        return (CompactionState)response.State;
    }
}
