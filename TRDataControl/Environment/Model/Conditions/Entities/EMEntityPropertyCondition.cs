﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMEntityPropertyCondition : BaseEMCondition
{
    public int EntityIndex { get; set; }
    public short? EntityType { get; set; }
    public List<short> EntityTypes { get; set; } // i.e. is the entity one of these types?
    public bool? Invisible { get; set; }
    public bool? ClearBody { get; set; }
    public short? Intensity1 { get; set; }
    public short? Intensity2 { get; set; }
    public ushort? Flags { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Z { get; set; }
    public short? Room { get; set; }

    protected override bool Evaluate(TR1Level level)
    {
        return GetResult(level.Entities[EntityIndex]);
    }

    protected override bool Evaluate(TR2Level level)
    {
        return GetResult(level.Entities[EntityIndex]);
    }

    protected override bool Evaluate(TR3Level level)
    {
        return GetResult(level.Entities[EntityIndex]);
    }

    private bool GetResult(TR1Entity entity)
    {
        bool result = GetResult<TR1Type>(entity);
        if (Intensity1.HasValue)
        {
            result &= entity.Intensity == Intensity1.Value;
        }
        if (Intensity2.HasValue)
        {
            result &= entity.Intensity == Intensity2.Value;
        }
        return result;
    }

    private bool GetResult(TR2Entity entity)
    {
        bool result = GetResult<TR2Type>(entity);
        if (Intensity1.HasValue)
        {
            result &= entity.Intensity1 == Intensity1.Value;
        }
        if (Intensity2.HasValue)
        {
            result &= entity.Intensity2 == Intensity2.Value;
        }
        return result;
    }

    private bool GetResult(TR3Entity entity)
    {
        bool result = GetResult<TR3Type>(entity);
        if (Intensity1.HasValue)
        {
            result &= entity.Intensity1 == Intensity1.Value;
        }
        if (Intensity2.HasValue)
        {
            result &= entity.Intensity2 == Intensity2.Value;
        }
        return result;
    }

    private bool GetResult<T>(TREntity<T> entity)
        where T : Enum
    {
        bool result = true;
        if (EntityType.HasValue)
        {
            result &= EqualityComparer<T>.Default.Equals(entity.TypeID, (T)(object)(uint)EntityType.Value);
        }
        if (EntityTypes != null)
        {
            result &= EntityTypes.Any(e => EqualityComparer<T>.Default.Equals(entity.TypeID, (T)(object)(uint)e));
        }
        if (Invisible.HasValue)
        {
            result &= entity.Invisible == Invisible.Value;
        }
        if (ClearBody.HasValue)
        {
            result &= entity.ClearBody == ClearBody.Value;
        }
        if (Flags.HasValue)
        {
            result &= entity.Flags == Flags.Value;
        }
        if (X.HasValue)
        {
            result &= entity.X == X.Value;
        }
        if (Y.HasValue)
        {
            result &= entity.Y == Y.Value;
        }
        if (Z.HasValue)
        {
            result &= entity.Z == Z.Value;
        }
        if (Room.HasValue)
        {
            result &= entity.Room == Room.Value;
        }
        return result;
    }
}
