﻿using Google.Protobuf;
using IO.Milvus.Exception;
using IO.Milvus.Param;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IO.Milvus.ApiSchema;

namespace IO.Milvus;

/// <summary>
/// Represents a milvus field/
/// </summary>
public abstract class Field
{
    #region Properties
    /// <summary>
    /// Field name
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// Row count
    /// </summary>
    public abstract int RowCount { get; }

    /// <summary>
    /// <see cref="MilvusDataType"/>
    /// </summary>
    public MilvusDataType DataType { get; protected set; }

    /// <summary>
    /// Convert to a grpc generated field.
    /// </summary>
    /// <returns></returns>
    public abstract Grpc.FieldData ToGrpcFieldData();
    #endregion

    #region Creation
    /// <summary>
    /// Create a field
    /// </summary>
    /// <typeparam name="TData">Data type</typeparam>
    /// <param name="fieldName">Field name</param>
    /// <param name="data">Data in this field</param>
    /// <returns></returns>
    public static Field Create<TData>(
        string fieldName,
        IList<TData> data
        )
    {
        return new Field<TData>()
        {
            FieldName = fieldName,
            Data = data
        };
    }

    /// <summary>
    /// Create a field from <see cref="byte"/> array.
    /// </summary>
    /// <param name="fieldName">Field name</param>
    /// <param name="bytes">Byte array data</param>
    /// <returns></returns>
    public static Field CreateFromBytes(string fieldName, byte[] bytes)
    {
        ParamUtils.CheckNullEmptyString(fieldName, nameof(FieldName));
        var field = new ByteStringField()
        {
            FieldName = fieldName,
            ByteString = ByteString.CopyFrom(bytes)
        };

        return field;
    }

    /// <summary>
    /// Create a binary vectors
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public static Field CreateBinaryVectors(string fieldName, IList<IList<float>> datas)
    {
        return new BinaryVectorField()
        {
            FieldName = fieldName,
            Data = datas,
        };
    }

    /// <summary>
    /// Create a field from <see cref="ByteString"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="byteString"><see cref="ByteString"/></param>
    /// <returns></returns>
    public static Field CreateFromByteString(string name, ByteString byteString)
    {
        ParamUtils.CheckNullEmptyString(name, nameof(FieldName));
        var field = new ByteStringField()
        {
            FieldName = name,
            ByteString = byteString
        };

        return field;
    }

    /// <summary>
    /// Create a field from stream
    /// </summary>
    /// <param name="fieldName">Field name</param>
    /// <param name="stream"></param>
    /// <returns>New created field</returns>
    public static Field CreateFromStream(string fieldName, Stream stream)
    {
        ParamUtils.CheckNullEmptyString(fieldName, nameof(FieldName));
        var field = new ByteStringField()
        {
            FieldName = fieldName,
            ByteString = ByteString.FromStream(stream)
        };

        return field;
    }
    #endregion
}

/// <summary>
/// Milvus Field
/// </summary>
/// <typeparam name="TData"></typeparam>
public class Field<TData> : Field
{
    /// <summary>
    /// Construct a field
    /// </summary>
    public Field()
    {
        CheckDataType();
    }

    /// <summary>
    /// Vector data
    /// </summary>
    public IList<TData> Data { get; set; }

    /// <summary>
    /// Row count
    /// </summary>
    public override int RowCount => Data?.Count ?? 0;

    ///<inheritdoc/>
    public override Grpc.FieldData ToGrpcFieldData()
    {
        Check();

        var fieldData = new Grpc.FieldData()
        {
            FieldName = FieldName,
            Type = (Grpc.DataType)DataType
        };

        switch (DataType)
        {
            case MilvusDataType.None:
                throw new MilvusException($"DataType Error:{DataType}");
            case MilvusDataType.Bool:
                {
                    var boolData = new Grpc.BoolArray();
                    boolData.Data.AddRange(Data as List<bool>);

                    fieldData.Scalars = new Grpc.ScalarField()
                    {
                        BoolData = boolData
                    };
                }
                break;
            case MilvusDataType.Int8:
                throw new NotSupportedException("not support in .net");
            case MilvusDataType.Int16:
                {
                    var intData = new Grpc.IntArray();
                    intData.Data.AddRange((Data as List<Int16>).Select(p => (int)p));

                    fieldData.Scalars = new Grpc.ScalarField()
                    {
                        IntData = intData
                    };
                }
                break;
            case MilvusDataType.Int32:
                {
                    var intData = new Grpc.IntArray();
                    intData.Data.AddRange(Data as List<int>);

                    fieldData.Scalars = new Grpc.ScalarField()
                    {
                        IntData = intData
                    };
                }
                break;
            case MilvusDataType.Int64:
                {
                    var longData = new Grpc.LongArray();
                    longData.Data.AddRange(Data as IList<long>);

                    fieldData.Scalars = new Grpc.ScalarField()
                    {
                        LongData = longData
                    };
                }
                break;
            case MilvusDataType.Float:
                {
                    var floatData = new Grpc.FloatArray();
                    floatData.Data.AddRange(Data as IList<float>);

                    fieldData.Scalars = new Grpc.ScalarField()
                    {
                        FloatData = floatData
                    };
                }
                break;
            case MilvusDataType.Double:
                {
                    var doubleData = new Grpc.DoubleArray();
                    doubleData.Data.AddRange(Data as IList<double>);

                    fieldData.Scalars = new Grpc.ScalarField()
                    {
                        DoubleData = doubleData
                    };
                }
                break;
            case MilvusDataType.String:
                {
                    var stringData = new Grpc.StringArray();
                    stringData.Data.AddRange(Data as IList<string>);

                    fieldData.Scalars = new Grpc.ScalarField()
                    {
                        StringData = stringData
                    };
                }
                break;
            default:
                throw new MilvusException($"DataType Error:{DataType}");
        }

        return fieldData;
    }

    internal void Check()
    {
        ParamUtils.CheckNullEmptyString(FieldName, nameof(FieldName));
        if (Data?.Any() != true)
        {
            throw new MilvusException($"{nameof(Field)}.{nameof(Data)} is empty");
        }
    }

    /// <summary>
    /// Check data type
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    protected void CheckDataType()
    {
        var type = typeof(TData);

        if (type == typeof(bool))
        {
            DataType = MilvusDataType.Bool;
        }
        else if (type == typeof(Int16))
        {
            DataType = MilvusDataType.Int16;
        }
        else if (type == typeof(int) || type == typeof(Int32))
        {
            DataType = MilvusDataType.Int32;
        }
        else if (type == typeof(Int64) || type == typeof(long))
        {
            DataType = MilvusDataType.Int64;
        }
        else if (type == typeof(float))
        {
            DataType = MilvusDataType.Float;
        }
        else if (type == typeof(double))
        {
            DataType = MilvusDataType.Double;
        }
        else if (type == typeof(string))
        {
            DataType = MilvusDataType.String;
        }
        else if (type == typeof(List<float>) || type == typeof(Grpc.FloatArray))
        {
            DataType = MilvusDataType.FloatVector;
        }
        else
        {
            throw new NotSupportedException($"Not Support DataType:{DataType}");
        }
    }
}