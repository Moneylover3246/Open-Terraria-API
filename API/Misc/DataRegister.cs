﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OTA.Logging;

namespace OTA.Misc
{
    /// <summary>
    /// Key value register.
    /// </summary>
    public class PairFileRegister : DataRegister
    {
        private bool _lowerKeys;

        const Char PrefixKey = '=';

        public PairFileRegister(string path, bool lowerKeys = true, bool autoLoad = true) : base(path, autoLoad)
        {
            _lowerKeys = lowerKeys;
        }

        public override void Load()
        {
            lock (_data)
            {
                if (System.IO.File.Exists(_path))
                {
                    _data = System.IO.File.ReadAllLines(_path)
                        .Select(x => x.Trim())
                        .Distinct()
                        .ToArray();

                    //Some lines may have the key separate.
                    for (var i = 0; i < _data.Length; i++)
                    {
                        var ix = _data[i].IndexOf(PrefixKey);
                        if (ix > -1)
                        {
                            var key = _data[i].Substring(0, ix).Trim();
                            //Don't trim the value, as it may be a string
                            var value = _data[i].Remove(0, ix + 1);
                            _data[i] = key + PrefixKey + value;
                        }
                        else
                        {
                            //Leave line alone.
                            continue;
                        }
                    }
                }
                else
                    System.IO.File.WriteAllText(_path, System.String.Empty);
            }
        }

        public bool Add(string key, string value, bool autoSave = true)
        {
            lock (_data)
            {
                System.Array.Resize(ref _data, _data.Length + 1);
                _data[_data.Length - 1] = (_lowerKeys ? key.ToLower() : key).Trim() + PrefixKey + value;
            }

            if (autoSave)
                return Save();
            return true;
        }

        public bool Update(string key, string value, bool autoSave = true)
        {
            key = (_lowerKeys ? key.ToLower() : key).Trim();
            bool updated = false;
            lock (_data)
            {
                for (var x = 0; x < _data.Length; x++)
                {
                    if ((_lowerKeys ? _data[x].ToLower() : _data[x]).StartsWith(key + PrefixKey))
                    {
                        updated = true;
                        _data[x] = key + PrefixKey + (value ?? String.Empty);
                    }
                }
            }

            if (!updated)
            {
                updated = Add(key, value, autoSave);
            }

            if (autoSave)
                return Save() && updated;
            return updated;
        }

        /// <summary>
        /// Retreives a listing by the key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Find(string key)
        {
            var cleaned = (_lowerKeys ? key.ToLower() : key).Trim();
            var item = _data
                .Where(x => (_lowerKeys ? x.ToLower() : x).StartsWith(cleaned + PrefixKey))
                .ToArray();

            if (item.Length == 1)
            {
                var v = item[0].Remove(0, item[0].IndexOf(PrefixKey) + 1);
                if (!String.IsNullOrEmpty(v))
                    return v;
            }

            return null;
        }
    }

    /// <summary>
    /// Low use data storage
    /// </summary>
    public class DataRegister : IEnumerator<string>
    {
        protected string _path;
        protected string[] _data;

        public DataRegister(string path, bool lowerKeys = true, bool autoLoad = true)
        {
            _path = path;
            _data = new string[0];

            if (autoLoad)
                Load();
        }

        public int Count
        {
            get
            { return _data.Length; }
        }

        public virtual void Load()
        {
            lock (_data)
            {
                if (System.IO.File.Exists(_path))
                {
                    _data = System.IO.File.ReadAllLines(_path)
                            .Select(x => x.Trim())
                            .Distinct()
                            .ToArray();
                }
                else
                    System.IO.File.WriteAllText(_path, System.String.Empty);
            }
        }

        public virtual bool Save()
        {
            try
            {
                lock (_data)
                {
                    System.IO.File.WriteAllLines(_path,
                        _data.Distinct().ToArray()
                    );
                }
                return true;
            }
            catch (System.Exception e)
            {
                ProgramLog.Log(e, "Failure saving data list.");
                return false;
            }
        }

        public virtual bool Clear(bool autoSave = true)
        {
            lock (_data)
            {
                _data = new string[] { };
            }

            if (autoSave)
                return Save();
            return true;
        }

        public virtual bool Add(string item, bool autoSave = true)
        {
            lock (_data)
            {
                System.Array.Resize(ref _data, _data.Length + 1);
                _data[_data.Length - 1] = item;
            }

            if (autoSave)
                return Save();
            return true;
        }

        public virtual bool Remove(string item, bool autoSave = true)
        {
            lock (_data)
            {
                _data = _data.Where(x => x != item).ToArray();
            }

            if (autoSave)
                return Save();
            return true;
        }

        public virtual bool Contains(string item, bool compareAsLowered = false)
        {
            lock (_data)
            {
                if (compareAsLowered) return _data.Contains(item.ToLower());
                return _data.Contains(item);
            }
        }

        //        public bool Remove(string item, bool byKey = false, bool autoSave = true)
        //        {
        //            var cleaned = (_lowerKeys || !byKey ? item.ToLower() : item).Trim();
        //            lock (_data)
        //            {
        //                if (byKey)
        //                    _data = _data.Where(x => !(_lowerKeys ? x.ToLower() : x).StartsWith(cleaned + PrefixKey)).ToArray();
        //                else
        //                    _data = _data.Where(x => x != cleaned).ToArray();
        //            }
        //
        //            if (autoSave)
        //                return Save();
        //            return true;
        //        }
        //
        //        public bool Contains(string item)
        //        {
        //            var cleaned = (_lowerKeys || !byKey ? item.ToLower() : item).ToLower().Trim();
        //            lock (_data)
        //            {
        //                if (byKey)
        //                    return _data.Where(x => x.StartsWith(cleaned + PrefixKey)).Count() > 0;
        //                else
        //                    return _data.Where(x => x == item).Count() > 0;
        //            }
        //        }
        //
        //        public bool Contains(string key, string value)
        //        {
        //            var cleaned = (_lowerKeys ? key.ToLower() : key).Trim();
        //            lock (_data)
        //            {
        //                return _data.Where(x => x == (cleaned + PrefixKey + value)).Count() > 0;
        //            }
        //        }

        public string this [int index]
        {
            get
            { return _data[index]; }
        }

        private int _index;

        public string Current
        {
            get { return _data[_index]; }
        }

        object IEnumerator.Current
        {
            get { return _data[_index]; }
        }

        public bool MoveNext()
        {
            return (++_index) < _data.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
            _data = null;
            _index = 0;
            _path = null;
        }
    }
}
