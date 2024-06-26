﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace HLab.Mvvm.Annotations;

public interface ILocalizationService
{
    public class Design : ILocalizationService
    {
        public string Localize(string lang, string value) => value;
        public string Localize(string value) => value; 
        public Task<string> LocalizeAsync(string lang, string value, CancellationToken token = default) => new(()=>value);
        public Task<string> LocalizeAsync(string value, CancellationToken token = default) => new(()=>value);

        public Task<ILocalizeEntry> GetLocalizeEntryAsync(string lang, string value) => new(()=>new LocalizeEntryDesign(value));

        public async IAsyncEnumerable<ILocalizeEntry> GetLocalizeEntriesAsync(string value)
        {
            yield return new LocalizeEntryDesign(value);
        }

        public void Set(CultureInfo info){}
        public void Register(ILocalizationProvider service){}

        public Task<ILocalizeEntry> GetLocalizeEntryAsync(string lang, string value, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<ILocalizeEntry> GetLocalizeEntriesAsync(string value, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
    }


    string  Localize(string lang, string value);
    string  Localize(string value);
    Task<string>  LocalizeAsync(string lang, string value, CancellationToken token = default);
    Task<string>  LocalizeAsync(string value, CancellationToken token = default);
    Task<ILocalizeEntry> GetLocalizeEntryAsync(string lang, string value, CancellationToken token = default);
    IAsyncEnumerable<ILocalizeEntry> GetLocalizeEntriesAsync(string value, CancellationToken token = default);
    void Set(CultureInfo info);
    void Register(ILocalizationProvider service);
}