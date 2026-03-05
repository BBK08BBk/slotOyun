using System;
using UnityEngine;

/// <summary>
/// Kasa / ödül havuzu ödeme ve bahis girişi katmanı. State tutmaz; tüm erişim delegate ile.
/// </summary>
public class OdemeServisi
{
    private Func<long> _getHavuzTL;
    private Action<int> _paraGirisiBolVeEkle;
    private Func<int, int> _odemeYapOdulHavuzundan;

    public void SetGetHavuzTL(Func<long> fn) => _getHavuzTL = fn;
    public void SetParaGirisiBolVeEkle(Action<int> fn) => _paraGirisiBolVeEkle = fn;
    public void SetOdemeYapOdulHavuzundan(Func<int, int> fn) => _odemeYapOdulHavuzundan = fn;

    public long GetHavuzTL() => _getHavuzTL != null ? _getHavuzTL.Invoke() : 0L;

    public void AddBahisToKasa(int tl)
    {
        if (tl > 0) _paraGirisiBolVeEkle?.Invoke(tl);
    }

    public int PayFromHavuz(int istenenTL) => (istenenTL <= 0) ? 0 : (_odemeYapOdulHavuzundan?.Invoke(istenenTL) ?? 0);

    private int? _odenebilirLimitOverride;
    private Func<int> _getOdenebilirLimitDynamic;

    /// <summary>Dinamik ödenebilir limit (senaryo: ödedikçe azalır, ödemedikçe artar). Dönen değer >= 0 ise kullanılır.</summary>
    public void SetGetOdenebilirLimitDynamic(Func<int> fn) => _getOdenebilirLimitDynamic = fn;

    /// <summary>Sabit override (eski davranış). null = havuzun %10'u.</summary>
    public void SetOdenebilirLimitOverride(int? tl) => _odenebilirLimitOverride = tl;

    public int GetSpinOdenebilirLimit()
    {
        // Finale aşamasında (7) kazanç tamamen kapalı: her spin net kayıp olacak şekilde ödenebilir limit 0.
        if (SenaryoYoneticisi.I != null && SenaryoYoneticisi.I.mevcutAsama == SenaryoYoneticisi.SenaryoAsama.Asama7_Finale)
            return 0;

        if (_getOdenebilirLimitDynamic != null)
        {
            int dyn = _getOdenebilirLimitDynamic();
            if (dyn >= 0) return dyn;
        }
        if (_odenebilirLimitOverride.HasValue && _odenebilirLimitOverride.Value > 0)
            return _odenebilirLimitOverride.Value;
        return GetSpinOdenebilirLimitRaw();
    }

    /// <summary>Override olmadan havuzun %10'u; bonus içinde panelde güncel değer göstermek için.</summary>
    public int GetSpinOdenebilirLimitRaw()
    {
        long havuz = GetHavuzTL();
        long limit = (long)Mathf.Floor(havuz * 0.10f);
        if (limit < 0) limit = 0;
        if (limit > int.MaxValue) return int.MaxValue;
        return (int)limit;
    }
}
