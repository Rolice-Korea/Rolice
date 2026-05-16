using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 데이터 테이블 SO의 추상 베이스. flat row 배열만 보유.
/// </summary>
public abstract class RcDataTableSO<TRow> : RcDataTableSOBase where TRow : struct
{
    [SerializeField] private TRow[] rows = Array.Empty<TRow>();

    public int    Count => rows?.Length ?? 0;
    public TRow[] Rows  => rows;

    protected void SetRows(TRow[] newRows) => rows = newRows;

    private void OnValidate() => OnTableChanged();
    private void OnEnable()   => OnTableChanged();

    /// <summary>
    /// rows 변경 시 호출. 서브클래스에서 Dictionary 빌드 등 처리.
    /// </summary>
    protected virtual void OnTableChanged() { }

    /// <summary>
    /// rows를 keySelector 기준으로 Dictionary로 빌드.
    /// </summary>
    protected Dictionary<TKey, TRow> BuildLookup<TKey>(Func<TRow, TKey> keySelector)
    {
        var dict = new Dictionary<TKey, TRow>();
        if (rows == null) return dict;
        foreach (var row in rows)
            dict[keySelector(row)] = row;
        return dict;
    }
}
