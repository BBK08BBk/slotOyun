using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CokmeAkisServisi için context arayüzü. OyunYoneticisi implement eder.
/// </summary>
public interface ICokmeAkisBaglami
{
    int GetSutun();
    int GetSatir();
    int[,] GetGrid();
    int[,] GetCarpanDegerGrid();
    Vector2[] GetCellPos();
    RectTransform[] GetCellRT();
    float GetSpawnFromTopOffset();
    float GetFallDuration();
    bool GetBonusAktif();
    int GetCarpanSembol();
    IzgaraServisi GetIzgaraServisi();
    TumbleServisi GetTumbleServisi();
    CarpanServisi GetCarpanServisi();
    SenaryoServisi GetSenaryoServisi();
    void ApplyNewGridAndSync(int[,] newGrid, int[,] newCarpanGrid);
}

/// <summary>
/// Çökme + doldurma + animasyon akışı. CokmeDoldurVeCanlandir gövdesi burada.
/// </summary>
public class CokmeAkisServisi
{
    private ICokmeAkisBaglami _ctx;

    public void SetBaglam(ICokmeAkisBaglami ctx)
    {
        _ctx = ctx;
    }

    public IEnumerator CokmeDoldurVeCanlandir()
    {
        if (_ctx == null) yield break;

        int sutun = _ctx.GetSutun();
        int satir = _ctx.GetSatir();
        int[,] grid = _ctx.GetGrid();
        int[,] carpanDegerGrid = _ctx.GetCarpanDegerGrid();
        var izgaraServisi = _ctx.GetIzgaraServisi();
        var cellPos = _ctx.GetCellPos();
        var cellRT = _ctx.GetCellRT();
        float spawnFromTopOffset = _ctx.GetSpawnFromTopOffset();
        float fallDuration = _ctx.GetFallDuration();
        bool bonusAktif = _ctx.GetBonusAktif();
        int CARPAN_SEMBOL = _ctx.GetCarpanSembol();

        int[,] oldGrid = new int[sutun, satir];
        for (int x = 0; x < sutun; x++)
            for (int y = 0; y < satir; y++)
                oldGrid[x, y] = grid[x, y];

        int[,] newGrid = new int[sutun, satir];
        int[,] newCarpanGrid = new int[sutun, satir];

        int hucreSayisi = izgaraServisi != null ? izgaraServisi.HucreSayisi() : 0;
        Vector2[] startPos = new Vector2[hucreSayisi];
        Vector2[] targetPos = new Vector2[hucreSayisi];
        bool[] willMove = new bool[hucreSayisi];
        List<Vector2Int> newlySpawnedCells = new List<Vector2Int>();

        for (int i = 0; i < hucreSayisi; i++)
        {
            startPos[i] = cellPos != null && i < cellPos.Length ? cellPos[i] : Vector2.zero;
            targetPos[i] = startPos[i];
            willMove[i] = false;
            if (cellRT != null && i < cellRT.Length && cellRT[i] != null)
                cellRT[i].anchoredPosition = cellPos[i];
        }

        for (int x = 0; x < sutun; x++)
        {
            List<int> col = new List<int>();
            List<int> colCarpanVal = new List<int>();
            List<int> oldYs = new List<int>();

            for (int y = 0; y < satir; y++)
            {
                int v = oldGrid[x, y];
                if (v != -1)
                {
                    col.Add(v);
                    colCarpanVal.Add(v == CARPAN_SEMBOL ? carpanDegerGrid[x, y] : 0);
                    oldYs.Add(y);
                }
            }

            int emptyCount = satir - col.Count;

            for (int y = 0; y < satir; y++)
            {
                if (y < emptyCount)
                {
                    newGrid[x, y] = -1;
                    newCarpanGrid[x, y] = 0;
                }
                else
                {
                    int ii = y - emptyCount;
                    newGrid[x, y] = col[ii];
                    newCarpanGrid[x, y] = colCarpanVal[ii];
                }
            }

            for (int y = 0; y < satir; y++)
            {
                if (newGrid[x, y] == -1)
                {
                    newGrid[x, y] = izgaraServisi != null ? izgaraServisi.RandomSymbolWithScatterChance(newGrid, bonusAktif) : -1;
                    newCarpanGrid[x, y] = 0;
                }
            }

            for (int y = 0; y < emptyCount; y++)
            {
                int idxTarget = izgaraServisi != null ? izgaraServisi.XYToIndex(x, y) : 0;
                newlySpawnedCells.Add(new Vector2Int(x, y));
                startPos[idxTarget] = cellPos[idxTarget] + new Vector2(0f, spawnFromTopOffset);
                targetPos[idxTarget] = cellPos[idxTarget];
                willMove[idxTarget] = true;
                if (cellRT != null && idxTarget < cellRT.Length && cellRT[idxTarget] != null)
                    cellRT[idxTarget].anchoredPosition = startPos[idxTarget];
            }

            for (int y = emptyCount; y < satir; y++)
            {
                int fromIndexInCol = y - emptyCount;
                int oldY = oldYs[fromIndexInCol];
                int newY = y;
                if (oldY != newY)
                {
                    int idxTarget = izgaraServisi != null ? izgaraServisi.XYToIndex(x, newY) : 0;
                    int idxSource = izgaraServisi != null ? izgaraServisi.XYToIndex(x, oldY) : 0;
                    startPos[idxTarget] = cellPos[idxSource];
                    targetPos[idxTarget] = cellPos[idxTarget];
                    willMove[idxTarget] = true;
                    if (cellRT != null && idxTarget < cellRT.Length && cellRT[idxTarget] != null)
                        cellRT[idxTarget].anchoredPosition = startPos[idxTarget];
                }
            }
        }

        var carpanServisi = _ctx.GetCarpanServisi();
        var senaryoServisi = _ctx.GetSenaryoServisi();
        var pendingCarpan = carpanServisi != null ? carpanServisi.GetPendingDrops() : null;
        if (pendingCarpan != null && pendingCarpan.Count > 0 && newlySpawnedCells.Count > 0 &&
            senaryoServisi != null && senaryoServisi.IsCarpanUretimiAktif() && (!senaryoServisi.IsCarpanSadeceBonus() || bonusAktif))
        {
            for (int i = 0; i < newlySpawnedCells.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, newlySpawnedCells.Count);
                var tmp = newlySpawnedCells[i];
                newlySpawnedCells[i] = newlySpawnedCells[j];
                newlySpawnedCells[j] = tmp;
            }
            int carpanKalan = carpanServisi.GetCarpanKalanBuSpin();
            int placeCount = Mathf.Min(pendingCarpan.Count, newlySpawnedCells.Count, carpanKalan);
            var placedCarpan = new List<int>();
            for (int i = 0; i < placeCount; i++)
            {
                var p = newlySpawnedCells[i];
                int carpan = pendingCarpan[i];
                newGrid[p.x, p.y] = CARPAN_SEMBOL;
                newCarpanGrid[p.x, p.y] = carpan;
                placedCarpan.Add(carpan);
            }
            if (placedCarpan.Count > 0)
                carpanServisi.RecordPlacedCarpanlar(placedCarpan);
        }

        _ctx.ApplyNewGridAndSync(newGrid, newCarpanGrid);
        if (izgaraServisi != null)
            izgaraServisi.RenderAllSprites(setAlphaOne: true, resetScale: true);

        float t = 0f;
        while (t < fallDuration)
        {
            float u = t / fallDuration;
            float eased = 1f - Mathf.Pow(1f - u, 5f);
            for (int i = 0; i < hucreSayisi; i++)
            {
                if (!willMove[i]) continue;
                if (cellRT != null && i < cellRT.Length && cellRT[i] != null)
                    cellRT[i].anchoredPosition = Vector2.Lerp(startPos[i], targetPos[i], eased);
            }
            t += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < hucreSayisi; i++)
        {
            if (!willMove[i]) continue;
            if (cellRT != null && i < cellRT.Length && cellRT[i] != null)
                cellRT[i].anchoredPosition = targetPos[i];
        }
    }

    /// <summary>Simülasyon için: çökme + refill + çarpan yerleştirme mantığı, animasyon yok. Grid context'te güncellenir; kayıt doldurulur.</summary>
    public void CokmeDoldurSadeceMantik(TumbleAdimKaydi kayit)
    {
        if (_ctx == null || kayit == null) return;

        int sutun = _ctx.GetSutun();
        int satir = _ctx.GetSatir();
        int[,] grid = _ctx.GetGrid();
        int[,] carpanDegerGrid = _ctx.GetCarpanDegerGrid();
        var izgaraServisi = _ctx.GetIzgaraServisi();
        bool bonusAktif = _ctx.GetBonusAktif();
        int CARPAN_SEMBOL = _ctx.GetCarpanSembol();

        int[,] oldGrid = new int[sutun, satir];
        for (int x = 0; x < sutun; x++)
            for (int y = 0; y < satir; y++)
                oldGrid[x, y] = grid[x, y];

        int[,] newGrid = new int[sutun, satir];
        int[,] newCarpanGrid = new int[sutun, satir];
        List<Vector2Int> newlySpawnedCells = new List<Vector2Int>();

        kayit.DusenHucreFrom.Clear();
        kayit.DusenHucreTo.Clear();

        for (int x = 0; x < sutun; x++)
        {
            List<int> col = new List<int>();
            List<int> colCarpanVal = new List<int>();
            List<int> colOldY = new List<int>();

            for (int y = 0; y < satir; y++)
            {
                int v = oldGrid[x, y];
                if (v != -1)
                {
                    col.Add(v);
                    colCarpanVal.Add(v == CARPAN_SEMBOL ? carpanDegerGrid[x, y] : 0);
                    colOldY.Add(y);
                }
            }
            int emptyCount = satir - col.Count;

            for (int y = 0; y < satir; y++)
            {
                if (y < emptyCount)
                {
                    newGrid[x, y] = -1;
                    newCarpanGrid[x, y] = 0;
                }
                else
                {
                    int ii = y - emptyCount;
                    newGrid[x, y] = col[ii];
                    newCarpanGrid[x, y] = colCarpanVal[ii];
                    int oldY = colOldY[ii];
                    if (oldY != y)
                    {
                        kayit.DusenHucreFrom.Add(new Vector2Int(x, oldY));
                        kayit.DusenHucreTo.Add(new Vector2Int(x, y));
                    }
                }
            }
            for (int y = 0; y < satir; y++)
            {
                if (newGrid[x, y] == -1)
                {
                    newGrid[x, y] = izgaraServisi != null ? izgaraServisi.RandomSymbolWithScatterChance(newGrid, bonusAktif) : -1;
                    newCarpanGrid[x, y] = 0;
                }
            }
            for (int y = 0; y < emptyCount; y++)
                newlySpawnedCells.Add(new Vector2Int(x, y));
        }

        var carpanServisi = _ctx.GetCarpanServisi();
        var senaryoServisi = _ctx.GetSenaryoServisi();
        var pendingCarpan = carpanServisi != null ? carpanServisi.GetPendingDrops() : null;
        if (pendingCarpan != null && pendingCarpan.Count > 0 && newlySpawnedCells.Count > 0 &&
            senaryoServisi != null && senaryoServisi.IsCarpanUretimiAktif() && (!senaryoServisi.IsCarpanSadeceBonus() || bonusAktif))
        {
            for (int i = 0; i < newlySpawnedCells.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, newlySpawnedCells.Count);
                var tmp = newlySpawnedCells[i];
                newlySpawnedCells[i] = newlySpawnedCells[j];
                newlySpawnedCells[j] = tmp;
            }
            int carpanKalan = carpanServisi.GetCarpanKalanBuSpin();
            int placeCount = Mathf.Min(pendingCarpan.Count, newlySpawnedCells.Count, carpanKalan);
            var placedCarpan = new List<int>();
            for (int i = 0; i < placeCount; i++)
            {
                var p = newlySpawnedCells[i];
                int carpan = pendingCarpan[i];
                newGrid[p.x, p.y] = CARPAN_SEMBOL;
                newCarpanGrid[p.x, p.y] = carpan;
                placedCarpan.Add(carpan);
            }
            kayit.CarpanDegerleriBuTur.Clear();
            kayit.CarpanDegerleriBuTur.AddRange(placedCarpan);
            if (placedCarpan.Count > 0)
                carpanServisi.RecordPlacedCarpanlar(placedCarpan);
        }
        else
            kayit.CarpanDegerleriBuTur.Clear();

        kayit.GridRefillSonrasi = new int[sutun, satir];
        kayit.CarpanGridRefillSonrasi = new int[sutun, satir];
        for (int x = 0; x < sutun; x++)
            for (int y = 0; y < satir; y++)
            {
                kayit.GridRefillSonrasi[x, y] = newGrid[x, y];
                kayit.CarpanGridRefillSonrasi[x, y] = newCarpanGrid[x, y];
            }
        kayit.YeniSpawnEdilenHucreler.Clear();
        kayit.YeniSpawnEdilenHucreler.AddRange(newlySpawnedCells);

        _ctx.ApplyNewGridAndSync(newGrid, newCarpanGrid);
    }

    /// <summary>Kayıttaki bir tumble adımını ekranda oynat: kayıt grid uygulanır, sadece yeni hücreler düşme animasyonu alır.</summary>
    public IEnumerator CokmeDoldurOynat(TumbleAdimKaydi adim)
    {
        if (_ctx == null || adim == null) yield break;

        int sutun = _ctx.GetSutun();
        int satir = _ctx.GetSatir();
        var cellPos = _ctx.GetCellPos();
        var cellRT = _ctx.GetCellRT();
        float spawnFromTopOffset = _ctx.GetSpawnFromTopOffset();
        float fallDuration = _ctx.GetFallDuration();
        var izgaraServisi = _ctx.GetIzgaraServisi();
        int hucreSayisi = izgaraServisi != null ? izgaraServisi.HucreSayisi() : 0;

        _ctx.ApplyNewGridAndSync(adim.GridRefillSonrasi, adim.CarpanGridRefillSonrasi);

        var willMove = new bool[hucreSayisi];
        var startPos = new Vector2[hucreSayisi];
        var targetPos = new Vector2[hucreSayisi];
        for (int i = 0; i < hucreSayisi; i++)
        {
            startPos[i] = cellPos != null && i < cellPos.Length ? cellPos[i] : Vector2.zero;
            targetPos[i] = startPos[i];
            willMove[i] = false;
        }
        foreach (var p in adim.YeniSpawnEdilenHucreler)
        {
            int idx = izgaraServisi != null ? izgaraServisi.XYToIndex(p.x, p.y) : 0;
            if (idx >= 0 && idx < hucreSayisi)
            {
                willMove[idx] = true;
                startPos[idx] = (cellPos != null && idx < cellPos.Length ? cellPos[idx] : Vector2.zero) + new Vector2(0f, spawnFromTopOffset);
                targetPos[idx] = cellPos != null && idx < cellPos.Length ? cellPos[idx] : Vector2.zero;
            }
        }
        if (adim.DusenHucreFrom != null && adim.DusenHucreTo != null && izgaraServisi != null)
        {
            for (int i = 0; i < adim.DusenHucreFrom.Count && i < adim.DusenHucreTo.Count; i++)
            {
                var from = adim.DusenHucreFrom[i];
                var to = adim.DusenHucreTo[i];
                int idxTo = izgaraServisi.XYToIndex(to.x, to.y);
                int idxFrom = izgaraServisi.XYToIndex(from.x, from.y);
                if (idxTo >= 0 && idxTo < hucreSayisi && idxFrom >= 0 && idxFrom < (cellPos != null ? cellPos.Length : 0))
                {
                    willMove[idxTo] = true;
                    startPos[idxTo] = cellPos[idxFrom];
                    targetPos[idxTo] = cellPos != null && idxTo < cellPos.Length ? cellPos[idxTo] : Vector2.zero;
                }
            }
        }
        for (int i = 0; i < hucreSayisi; i++)
        {
            if (cellRT == null || i >= cellRT.Length || cellRT[i] == null) continue;
            if (willMove[i])
                cellRT[i].anchoredPosition = startPos[i];
            else
                cellRT[i].anchoredPosition = cellPos != null && i < cellPos.Length ? cellPos[i] : Vector2.zero;
        }
        // Refill sonrası grid'e göre tüm sprite'ları güncelle (düşen meyveler doğru sembolü göstersin, yerine meyve gelsin).
        if (izgaraServisi != null)
            izgaraServisi.RenderAllSprites(setAlphaOne: true, resetScale: true);

        float t = 0f;
        while (t < fallDuration)
        {
            float u = t / fallDuration;
            float eased = 1f - Mathf.Pow(1f - u, 5f);
            for (int i = 0; i < hucreSayisi; i++)
            {
                if (!willMove[i]) continue;
                if (cellRT != null && i < cellRT.Length && cellRT[i] != null)
                    cellRT[i].anchoredPosition = Vector2.Lerp(startPos[i], targetPos[i], eased);
            }
            t += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < hucreSayisi; i++)
        {
            if (!willMove[i]) continue;
            if (cellRT != null && i < cellRT.Length && cellRT[i] != null)
                cellRT[i].anchoredPosition = targetPos[i];
        }
        // Kayma olmasın: tüm hücreleri tam konuma kilitle.
        if (cellPos != null && cellRT != null)
        {
            for (int i = 0; i < hucreSayisi && i < cellPos.Length && i < cellRT.Length; i++)
            {
                if (cellRT[i] != null)
                    cellRT[i].anchoredPosition = cellPos[i];
            }
        }
        if (izgaraServisi != null)
            izgaraServisi.ForceRefreshCarpanTextsFromGrid();
    }
}
