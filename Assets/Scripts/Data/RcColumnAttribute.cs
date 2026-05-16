using System;

/// <summary>
/// 데이터 테이블 에디터 창의 컬럼 너비/헤더를 지정.
/// 미지정 시 필드명 자동 사용, 너비 기본값 150.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class RcColumnAttribute : Attribute
{
    public readonly float  Width;
    public readonly string Header;

    public RcColumnAttribute(float width, string header = null)
    {
        Width  = width;
        Header = header;
    }
}
