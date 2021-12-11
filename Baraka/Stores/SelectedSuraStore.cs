﻿using Baraka.Models;
using Baraka.Models.Quran;
using Baraka.Singletons;
using Baraka.Utils.MVVM.ViewModel;
using System;

namespace Baraka.Stores
{
    public class SelectedSuraStore
    {
        public event Action<SuraModel> ValueChanged;

        public void ChangeSelectedSura(SuraModel value)
        {
            AppStateSingleton.Instance.SelectedSura = value;
            ValueChanged?.Invoke(value);
        }
    }
}