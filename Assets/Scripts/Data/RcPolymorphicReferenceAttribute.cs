using System;
using UnityEngine;

/// [SerializeReference] 배열/필드에 다형성 타입 드롭다운을 제공하는 PropertyAttribute.
/// 에디터에서 타입 선택 UI를 자동으로 생성한다.
///
/// 사용 예:
///   [SerializeReference, RcPolymorphicReference(typeof(RcTileData))]
///   public RcTileData[] Tiles;
[AttributeUsage(AttributeTargets.Field)]
public class RcPolymorphicReferenceAttribute : PropertyAttribute
{
    public readonly Type BaseType;

    public RcPolymorphicReferenceAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}
