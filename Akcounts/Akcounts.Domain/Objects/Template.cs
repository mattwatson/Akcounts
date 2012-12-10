﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Akcounts.Domain.Interfaces;

namespace Akcounts.Domain.Objects
{
    [Serializable]
    public class Template : EntityIdentifiedByInt<Template>, IDomainObject
    {
        private readonly List<Journal> _journals = new List<Journal>();

        public ReadOnlyCollection<Journal> Journals
        {
            get {
                return _journals.AsReadOnly();
            }
        }

        public void AddJournal(Journal journal)
        {
            _journals.Remove(journal);
            _journals.Add(journal);
        }

        public void RemoveJournal(Journal journal)
        {
            _journals.Remove(journal);
        }

        public XElement EmitXml()
        {
            return new XElement("template", 
                from journal in Journals
                select journal.EmitXml()
                );
        }
    }
}
