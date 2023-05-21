﻿using System.Text.Json.Serialization;

namespace IO.Milvus.ApiSchema;

internal sealed class HasPartitionResponse
{
    [JsonPropertyName("status")]
    public ResponseStatus Status { get; set; }

    [JsonPropertyName("value")]
    public bool Value { get; set; }
}