﻿//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
//
//    OleViewDotNet is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    OleViewDotNet is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.

using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet
{
    public class COMProxyInstance
    {
        public IEnumerable<NdrComProxyDefinition> Entries { get; private set; }

        public IEnumerable<NdrComplexTypeReference> ComplexTypes { get; private set; }

        internal COMProxyInstance(IEnumerable<NdrComProxyDefinition> entries, 
                                  IEnumerable<NdrComplexTypeReference> complex_types)
        {
            Entries = new List<NdrComProxyDefinition>(entries).AsReadOnly();
            ComplexTypes = new List<NdrComplexTypeReference>(complex_types).AsReadOnly();
        }

        private COMProxyInstance(string path, Guid clsid, ISymbolResolver resolver)
        {
            NdrParser parser = new NdrParser(resolver);
            Entries = parser.ReadFromComProxyFile(path, clsid);
            ComplexTypes = parser.ComplexTypes;
        }

        private COMProxyInstance(string path, ISymbolResolver resolver) : this(path, Guid.Empty, resolver)
        {
        }

        private static Dictionary<Guid, COMProxyInstance> m_proxies = new Dictionary<Guid, COMProxyInstance>();
        private static Dictionary<string, COMProxyInstance> m_proxies_by_file = new Dictionary<string, COMProxyInstance>(StringComparer.OrdinalIgnoreCase);

        public static COMProxyInstance GetFromCLSID(COMCLSIDEntry clsid, ISymbolResolver resolver)
        {
            if (m_proxies.ContainsKey(clsid.Clsid))
            {
                return m_proxies[clsid.Clsid];
            }
            else
            {
                COMProxyInstance proxy = new COMProxyInstance(clsid.DefaultServer, clsid.Clsid, resolver);
                m_proxies[clsid.Clsid] = proxy;
                return proxy;
            }
        }

        public static COMProxyInstance GetFromFile(string path, ISymbolResolver resolver)
        {
            if (m_proxies_by_file.ContainsKey(path))
            {
                return m_proxies_by_file[path];
            }
            else
            {
                COMProxyInstance proxy = new COMProxyInstance(path, resolver);
                m_proxies_by_file[path] = proxy;
                return proxy;
            }
        }
    }
}
